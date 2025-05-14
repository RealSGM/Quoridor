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

			// Movement bias (gradually reduces as the node's visits increase)
			double movementBias = (child.LastMove != null && child.LastMove.MoveType == 'm') ? 0.1 : 0;

			return winRate + explorationTerm + movementBias;
		});
	}

	public MCTSNode Expand()
	{
		IllegalFenceCheck.GetIllegalFences(State);
		HashSet<StateKey> exploredKeys = [.. Children.Select(c => c.State.GetStateKey())];

		List<string> allMoves = [];
		int[] pawnMoves = State.GetReachableTilesSmart(CurrentPlayer);
		ulong[] fencesMoves = State.GetAllFences(CurrentPlayer);

		allMoves.AddRange(pawnMoves.Select(tile => Helper.GetMoveCodeAsString(CurrentPlayer, "m", 0, tile)));
		
		for (int index = 0; index < fencesMoves.Length; index++)
		{
			allMoves.AddRange(Helper.GetOnesInBitBoard(fencesMoves[index]).Select(bit => Helper.GetMoveCodeAsString(CurrentPlayer, "f", index, bit)));
		}

		Helper.Shuffle(allMoves, Helper.Random); // Fisher–Yates shuffle

		foreach (string move in allMoves)
		{
			BoardState simState = State.Clone();
			simState.AddMove(move);
			StateKey simKey = simState.GetStateKey();

			if (exploredKeys.Contains(simKey))  continue;

			MCTSNode child = new(this, simState, 1 - CurrentPlayer, ParsedMove.Create(move));
			Children.Add(child);
		}

		return Children.FirstOrDefault() ?? this;
	}

	public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 100)
	{
		BoardState tempState = State.Clone();
		int depth = 0;

		// Use shared random (should be class-level or injected)
		Random rng = Helper.Random;

		int currentPlayer = CurrentPlayer;

		while (!tempState.IsGameOver() && depth < maxPlayoutDepth)
		{
			IllegalFenceCheck.GetIllegalFences(tempState);
			List<string> moves = rng.NextDouble() < 0.70 
			? [.. tempState.GetReachableTilesSmart(currentPlayer).Select(tile => Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tile))]
            : [.. tempState.GetAllFences(currentPlayer)
				.SelectMany((fence, index) => Helper.GetOnesInBitBoard(fence)
					.Select(bit => Helper.GetMoveCodeAsString(currentPlayer, "f", index, bit)))];

			if (moves.Count == 0) break;

			string selectedMove = moves[rng.Next(moves.Count)];
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

		// Propagate the result back to the root node
		while (node != null)
		{
			node.Visits++;
			// If the result is a win, increment the wins for this node
			if (result == int.MaxValue) node.Wins += 1;
			// Move up to the parent node
			node = node.Parent;
		}
	}
}
