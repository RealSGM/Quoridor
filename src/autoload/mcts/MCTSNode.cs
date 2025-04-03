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

	// Returns a child node with the highest UCT value
	public MCTSNode SelectChild(double explorationConstant = 1.41)
    {
		if (IsLeaf() || State.IsGameOver()) return this;

        return Children.OrderByDescending(c =>
			(double)c.Wins / (c.Visits + 1) +
			explorationConstant * Math.Sqrt(Math.Log(Visits + 1) / (c.Visits + 1))
		).First();
    }

	// Expands the node by adding all moves that have not been explored yet
	public MCTSNode Expand()
	{
		// TODO Run Illegal Fence Check
		string[] possibleMoves = State.GetAllMoves(CurrentPlayer);
		HashSet<BoardState> exploredStates = [.. Children.Select(c => c.State)];

		foreach (string move in possibleMoves)
		{
			BoardState newState = State.Clone();
			newState.AddMove(move);

			if (!exploredStates.Contains(newState))
			{
				MCTSNode childNode = new(this, newState, 1 - CurrentPlayer);
				Children.Add(childNode);
				return childNode;
			}
		}

		return this; 
	}

	// Simulates a game from the current state until it reaches a terminal state
	public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 200)
	{
		BoardState tempState = State.Clone();
		int depth = 0;

		while (!tempState.IsGameOver() && depth < maxPlayoutDepth)
		{
			string[] possibleMoves = tempState.GetAllMoves(CurrentPlayer);

			if (possibleMoves.Length == 0) break;

			// Select a random move and apply it
			string randomMove = possibleMoves[new Random().Next(possibleMoves.Length)];
			tempState.AddMove(randomMove);
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
