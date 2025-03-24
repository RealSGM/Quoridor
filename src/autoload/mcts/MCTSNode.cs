using System;
using System.Collections.Generic;
using System.Linq;

public class MCTSNode(MCTSNode parent, BoardState state, int player)
{
	public MCTSNode Parent = parent;
	public BoardState State = state;
	public List<MCTSNode> Children = [];
	public int Wins = 0;
	public int Visits = 0;
	public int CurrentPlayer = player;

    public bool IsLeaf() => Children.Count == 0;

	public MCTSNode SelectChild(double explorationConstant = 1.41)
    {
		if (IsLeaf() || State.IsGameOver()) return this;

        return Children.OrderByDescending(c =>
			(double)c.Wins / (c.Visits + 1) +
			explorationConstant * Math.Sqrt(Math.Log(Visits + 1) / (c.Visits + 1))
		).First();
    }

	public MCTSNode Expand()
	{
		string[] possibleMoves = State.GetAllMoves(CurrentPlayer);
		HashSet<BoardState> exploredStates = new(Children.Select(c => c.State));

		foreach (string move in possibleMoves)
		{
			BoardState newState = State.Clone();
			newState.AddMove(move);

			if (!exploredStates.Contains(newState)) // Ensure we only expand unexplored moves
			{
				MCTSNode childNode = new(this, newState, 1 - CurrentPlayer);
				Children.Add(childNode);
				return childNode; // Return the new child node
			}
		}

		return this; // Shouldn't happen unless all moves are expanded
	}

	public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 200)
	{
		BoardState tempState = State.Clone();  // Clone the current game state to simulate without modifying the original
		bool isGameOver = false;
		int depth = 0;

		while (!isGameOver && depth < maxPlayoutDepth)
		{
			string[] possibleMoves = tempState.GetAllMoves(CurrentPlayer);

			if (possibleMoves.Length == 0) break;

			// Select a random move and apply it
			string randomMove = possibleMoves[new Random().Next(possibleMoves.Length)];
			tempState.AddMove(randomMove);

			// Check if the game is over (e.g., player has won)
			isGameOver = tempState.IsGameOver();

			CurrentPlayer = 1 - CurrentPlayer;
			depth++;
		}

		// Return the result (win: 1, lose: 0, draw: 0.5, etc.)
		return tempState.GetGameResult(simulatingPlayer);
	}


	public void Backpropagate(int result)
	{
		MCTSNode node = this;
		
		// Propagate the result back to the root node
		while (node != null)
		{
			node.Visits += 1;
			
			// If the result is a win, increment the wins for this node
			if (result == int.MaxValue)
			{
				node.Wins += 1;
			}

			// Move up to the parent node
			node = node.Parent;
		}
	}

}
