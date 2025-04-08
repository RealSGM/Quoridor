using Godot;
using System.Linq;

public partial class MCTS : Node
{
	private const int SIMULATIONS = 1000;  // Increased simulations for better decision-making
	private const double EXPLORATION_CONSTANT = 1.41; // sqrt(2), UCB1 exploration factor

	Window Console;

	public string GetMove(BoardState board, int currentPlayer, bool _isMaximising = false, bool _isDebugging = false)
	{
		MCTSNode root = new(null, board, currentPlayer);

		for (int i = 0; i < SIMULATIONS; i++)
		{
			MCTSNode selectedNode = root.SelectChild(EXPLORATION_CONSTANT);
			MCTSNode expandedNode = selectedNode.Expand();
			int simulationResult = expandedNode.Simulate(currentPlayer);
			expandedNode.Backpropagate(simulationResult);
		}

		MCTSNode bestChild = root.Children.OrderByDescending(c => c.Visits).First();

		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", $"Best Move: {bestChild.State.GetLastMove()}, Wins: {bestChild.Wins}, Visits: {bestChild.Visits}", 0);
		return bestChild.State.GetLastMove().Split('_')[0];
	}
}
