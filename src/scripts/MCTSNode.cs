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
        
        HashSet<BoardState> exploredStates = [.. Children.Select(c => c.State)];

        List<string> allMoves = [.. State.GetAllMovesWeighted(CurrentPlayer).Select(kvp => kvp.Key)];

        // Separate and shuffle pawn and fence moves
        List<string> pawnMoves = [.. State.GetReachableTilesWeighted(CurrentPlayer).Keys
            .OrderBy(_ => Helper.Random.Next())]; // Randomize the order of pawn moves
        List<string> fenceMoves = [.. State.GetFenceMovesWeighted(CurrentPlayer).Keys  
            .OrderBy(_ => Helper.Random.Next())]; // Randomize the order of fence moves

        List<string> biasedMoves = Helper.Random.NextDouble() < 0.25
            ? [.. pawnMoves, .. fenceMoves]
            : [.. fenceMoves, .. pawnMoves];

        // Try adding the first unvisited state
        foreach (string move in biasedMoves)
        {
            BoardState newState = State.Clone();
            newState.AddMove(move);

            if (exploredStates.Contains(newState)) continue;

            MCTSNode childNode = new(this, newState, 1 - CurrentPlayer);
            Children.Add(childNode);
            return childNode;
        }

        return this; // All states already explored
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
            // Randomly select a move, prioritizing pawn moves 60% of the time
            string selectedMove = random.NextDouble() <= 0.75
                ? tempState.GetReachableTilesWeighted(CurrentPlayer).Keys.FirstOrDefault()
                : tempState.GetFenceMovesWeighted(CurrentPlayer).Keys.OrderBy(_ => random.Next()).FirstOrDefault();
            
            if (selectedMove == null) break; // No valid moves available

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
