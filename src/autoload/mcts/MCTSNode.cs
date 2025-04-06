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
        Dictionary<string, float> weightedMoves = State.GetAllMovesWeighted(CurrentPlayer);
        HashSet<BoardState> exploredStates = [.. Children.Select(c => c.State)];

		foreach (var move in weightedMoves.OrderByDescending(m => m.Value))
		{
            string moveString = move.Key;
            float moveWeight = move.Value;

            // Clone the current state to simulate the move
            BoardState newState = State.Clone();
            newState.AddMove(moveString);

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
    public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 200)
    {
        BoardState tempState = State.Clone();
        int depth = 0;
        Random random = new();

        // While the game is not over and the simulation depth is not reached
        while (!tempState.IsGameOver() && depth < maxPlayoutDepth)
        {
            // Get all possible weighted moves
            Dictionary<string, float> rawMoves = tempState.GetAllMovesWeighted(CurrentPlayer);

			if (rawMoves.Count == 0) break;

			// Normalize weights
			float minWeight = rawMoves.Values.Min();
			float offset = Math.Abs(minWeight) + 0.001f;
			var weightedMoves = rawMoves.ToDictionary(kv => kv.Key, kv => kv.Value + offset);

            if (weightedMoves.Count == 0) break;

            // Select a random move based on its weight
            float totalWeight = weightedMoves.Values.Sum();
            float randomValue = (float)(random.NextDouble() * totalWeight);
            float cumulativeWeight = 0f;
            string selectedMove = null;

            foreach (var move in weightedMoves)
            {
                cumulativeWeight += move.Value;
                if (randomValue <= cumulativeWeight)
                {
                    selectedMove = move.Key;
                    break;
                }
            }

            // Apply the selected move
			try
			{
				tempState.AddMove(selectedMove);
			}
			catch (Exception e)
			{
				GD.PrintErr($"Error applying move {selectedMove}: {e}");
				GD.Print($"Current Player: {CurrentPlayer}");
				GD.Print($"State: {tempState}");
				GD.Print($"Move: {selectedMove}");
				GD.Print($"Depth: {depth}");
				GD.Print($"Simulating Player: {CurrentPlayer}");
				GD.Print($"Weighted Moves: {string.Join(", ", weightedMoves.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
				GD.Print($"Random Value: {randomValue}");
				GD.Print($"Cumulative Weight: {cumulativeWeight}");
				GD.Print($"Total Weight: {totalWeight}");
				GD.Print($"Selected Move: {selectedMove}");
				GD.Print("--------------------");
				break;
			}

            CurrentPlayer = 1 - CurrentPlayer;
            depth++;
        }

        // Return the result of the simulation (win: 1, lose: 0, draw: 0.5, etc.)
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
