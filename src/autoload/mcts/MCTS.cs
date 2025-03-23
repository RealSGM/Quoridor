using Godot;
using System;

public partial class MCTS : Node
{
	private const int SIMULATIONS = 1000;  // Number of MCTS iterations
	private const double EXPLORATION_CONSTANT = 1.41; // sqrt(2), UCB1 exploration factor
	private readonly Random random = new();

	Window Console;

	public string GetMove(BoardState board, int currentPlayer, bool isDebugging = false)
	{
		// Setup console
		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Running Monte Carlo Tree Search...", 0);
		ulong startTime = Time.GetTicksMsec();

		MCTSNode root = new(null, board, currentPlayer);

		for (int i = 0; i < SIMULATIONS; i++)
		{
			MCTSNode selectedNode = Select(root);
			MCTSNode expandedNode = Expand(selectedNode);
			int result = Simulate(expandedNode.State, expandedNode.CurrentPlayer);
			Backpropagate(expandedNode, result);
		}

		MCTSNode bestChild = root.GetBestChild(0); // Choose best move (without exploration factor)
		string bestMove = bestChild.LastMove;

		Console.Call("add_entry", "MCTS completed in " + (Time.GetTicksMsec() - startTime) + " ms", 0);
		Console.Call("add_entry", $"Best Move: {bestMove}, Simulations: {SIMULATIONS}", 0);

		return bestMove;
	}

	private static MCTSNode Select(MCTSNode node)
	{
		while (!node.IsLeaf() && !node.State.IsGameOver())
		{
			node = node.GetBestChild(EXPLORATION_CONSTANT);
		}
		return node;
	}

	private MCTSNode Expand(MCTSNode node)
	{
		if (!node.State.IsGameOver() && node.Children.Count == 0)
		{
			foreach (string move in node.State.GetAllMoves(node.CurrentPlayer))
			{
				BoardState newState = node.State.Clone();
				newState.AddMove(move);
				MCTSNode childNode = new(node, newState, 1 - node.CurrentPlayer, move);
				node.Children.Add(childNode);
			}
		}
		return node.Children.Count > 0 ? node.Children[random.Next(node.Children.Count)] : node;
	}

	private int Simulate(BoardState state, int currentPlayer)
	{
		BoardState simState = state.Clone();
		int player = currentPlayer;

		while (!simState.IsGameOver())
		{
			var moves = simState.GetAllMoves(player);
			if (moves.Length == 0) break;
			simState.AddMove(moves[random.Next(moves.Length)]);
			player = 1 - player;
		}

		return simState.GetWinner(0) ? 1 : simState.GetWinner(1) ? -1 : 0;
	}

	private static void Backpropagate(MCTSNode node, int result)
	{
		while (node != null)
		{
			node.Visits++;
			node.Wins += result;
			node = node.Parent;
		}
	}
}
