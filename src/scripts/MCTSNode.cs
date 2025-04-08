using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

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

    // Expands the node by adding all moves that have not been explored yet, considering weighted moves
    public MCTSNode Expand()
    {
        // Get all possible moves with their weights
        List<string> allMoves = [.. State.GetAllMovesWeighted(CurrentPlayer).Select(kvp => kvp.Key)];
        allMoves = [.. allMoves.OrderBy(_ => Helper.Random.Next())];
        HashSet<BoardState> exploredStates = [.. Children.Select(c => c.State)];

		foreach (string move in allMoves)
		{
            // Clone the current state to simulate the move
            BoardState newState = State.Clone();
            newState.AddMove(move);

            // If the move leads to a state that hasn't been explored yet, add it as a child
            if (!exploredStates.Contains(newState))
            {
                MCTSNode childNode = new(this, newState, 1 - CurrentPlayer);
                Children.Add(childNode);
                return childNode; // Return the first unvisited move
            }
        }

        return this; // No new moves, return the current node
    }

	// Simulates a game from the current state until it reaches a terminal state
    public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 50)
    {
        BoardState tempState = State.Clone();
        int depth = 0;
        Random random = new();

        // While the game is not over and the simulation depth is not reached
        while (!tempState.IsGameOver() && depth < maxPlayoutDepth)
        {
            List<string> allMoves = [.. State.GetAllMovesWeighted(CurrentPlayer).Select(kvp => kvp.Key)];
            allMoves = [.. allMoves.OrderBy(_ => Helper.Random.Next())];

			if (allMoves.Count == 0) break;

            string selectedMove = allMoves[0];
			tempState.AddMove(selectedMove);
            CurrentPlayer = 1 - CurrentPlayer;
            depth++;
        }

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
