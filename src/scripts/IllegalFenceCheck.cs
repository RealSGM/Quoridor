using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class IllegalFenceCheck : Node
{
	public void GetIllegalFences(BoardState board, int currentPlayer)
	{
		// Ignore if player has no more fences
		if (board.GetFenceCount(currentPlayer) == 0) return;
	
		int[] placedFences = board.GetPlacedFences();

		List<int[]> possibleFences = new();

		for (int fenceIndex = 0; fenceIndex < placedFences.Length; fenceIndex++)
		{
			// Ignore if fence is not placed
			if (placedFences[fenceIndex] == -1) continue;

			possibleFences.AddRange(board.GetSurroundingFences(fenceIndex));
		}

		possibleFences = possibleFences
			.Where(fence => Math.Abs(fence[0]) < placedFences.Length && placedFences[Math.Abs(fence[0])] == -1)
			.ToList();
		
		List<int[]> searchedFences = new();

		possibleFences.ForEach(fence => board.SetIllegalFence(fence[0], -1));

		foreach (var fence in possibleFences)
		{
			if (!board.GetFenceEnabled(fence[0], fence[1])) continue;

			foreach (var player in Helper.Bits)
			{
				if (searchedFences.Any(f => f.SequenceEqual(new[] {fence[0], fence[1], player}))) continue;

				searchedFences.Add(new[] {fence[0], fence[1], player});

				if (!IsFenceIllegal(board, fence[0], fence[1], player)) continue;

				board.SetIllegalFence(fence[0], fence[1]);
			}
		}
	}

	private bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();

		boardClone.PlaceFence(direction, fence, player);

		int start = boardClone.GetPawnPosition(player);

		HashSet<int> goalTiles = boardClone.GetGoalTiles(player).ToHashSet();

		return Algorithms.RecursiveDFS(boardClone, start, goalTiles, new(), player);
	}
}
