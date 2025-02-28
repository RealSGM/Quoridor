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
		
		possibleFences.ForEach(fence => board.SetIllegalFence(fence[0], -1));

		possibleFences = possibleFences
			.Distinct(new Helper.FenceEqualityComparer())
			.ToList();

		Parallel.ForEach(possibleFences, fence =>
		{
			if (!board.GetFenceEnabled(fence[0], fence[1])) return;

			Parallel.ForEach(Helper.Bits, player =>
			{
				if (!IsFenceIllegal(board, fence[0], fence[1], player)) return;

				board.SetIllegalFence(fence[0], fence[1]);
			});
		});
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
