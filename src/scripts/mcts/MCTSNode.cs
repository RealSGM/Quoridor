using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MCTSNode(MCTSNode parent, BoardState state, int currentplayer, ParsedMove lastMove = null)
{
	public MCTSNode Parent = parent;
	public BoardState State = state;
	public List<MCTSNode> Children = [];
	public int Wins = 0;
	public int Visits = 0;
	public int CurrentPlayer = currentplayer;
	public ParsedMove LastMove = lastMove;

	public bool IsLeaf() => Children.Count == 0;

	public MCTSNode SelectChild(double explorationConstant = 1.41)
	{
		if (IsLeaf() || State.IsGameOver()) return this;

		double totalVisits = Math.Max(1, Visits);

		return Children.MaxBy(child =>
		{
			if (child.Visits == 0)
				return double.PositiveInfinity;

			double winRate = (double)child.Wins / child.Visits;
			double explorationTerm = explorationConstant * Math.Sqrt(Math.Log(totalVisits) / child.Visits);
			double movementBias = (child.LastMove != null && child.LastMove.MoveType == 'm') ? 0.1 : 0; // Movement bias for pawn moves
			return winRate + explorationTerm + movementBias;
		});
	}

	public MCTSNode Expand()
	{
		IllegalFenceCheck.GetIllegalFences(State);
		string[] allMoves = State.GetAllMoves(CurrentPlayer);

		HashSet<StateKey> exploredKeys = [.. Children.Select(c => c.State.GetStateKey())];
		Helper.Shuffle(allMoves, Helper.Random); // Shuffle moves to ensure randomness

		foreach (string move in allMoves) // Add all moves as children
		{
			BoardState simState = State.Clone();
			simState.AddMove(move);
			StateKey simKey = simState.GetStateKey();

			if (exploredKeys.Contains(simKey)) continue; // Ignore explored states

			ParsedMove parsedMove = ParsedMove.Create(move);
			MCTSNode child = new(this, simState, 1 - CurrentPlayer, parsedMove);
			Children.Add(child);
		}
		return Children.FirstOrDefault() ?? this;
	}

	public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 100)
	{
		BoardState tempState = State.Clone();
		int depth = 0;
		int currentPlayer = CurrentPlayer;

		// Perform a random simulation until the game is over or max depth is reached
		while (!tempState.IsGameOver() && depth < maxPlayoutDepth)
		{
			IllegalFenceCheck.GetIllegalFences(tempState);
			List<string> moves = Helper.Random.NextDouble() < 0.70
				? [.. tempState.GetReachableTilesSmart(currentPlayer).Select(tile => Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tile))]
				: [.. tempState.GetAllFences(currentPlayer)
				.SelectMany((fence, index) => Helper.GetOnesInBitBoard(fence)
					.Select(bit => Helper.GetMoveCodeAsString(currentPlayer, "f", index, bit)))];

			if (moves.Count == 0) break;

			string selectedMove = moves[Helper.Random.Next(moves.Count)];
			tempState.AddMove(selectedMove);
			currentPlayer = 1 - currentPlayer;
			depth++;
		}

		int result = tempState.GetGameResult(simulatingPlayer);
		return result;
	}

	public void Backpropagate(int result)
	{
		MCTSNode node = this;
		while (node != null)
		{
			node.Visits++;
			if (result == int.MaxValue) node.Wins += 1;
			node = node.Parent;
		}
	}
}
