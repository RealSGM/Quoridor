using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class IllegalFenceCheck : Node
{
	public void GetIllegalFences(BoardState board)

	{
		// Ignore if player has no more fences
		if (board.GetFenceCount(1 - board.CurrentPlayer) == 0) return;

		int[] placedFences = board.GetPlacedFences();

		List<int> possibleFences = placedFences
			.Select((value, fenceIndex) => new { value, fenceIndex })
			.Where(fence => fence.value != -1) // Skip fences that are not placed
			.SelectMany(fence => board.GetSurroundingFences(fence.fenceIndex)
				.Where(adjacentFence => adjacentFence < placedFences.Length && placedFences[Math.Abs(adjacentFence)] == -1))
			.Distinct()
			.ToList();


		possibleFences.ForEach(fence => board.SetIllegalFence(Math.Abs(fence), -1));

		Parallel.ForEach(possibleFences, fence =>
		{
			// Separate the fence index and direction
			int direction = fence < 0 ? 1 : 0;
			fence = Math.Abs(fence);
			
			if (!board.GetFenceEnabled(fence, direction)) return;

			Parallel.ForEach(Helper.Bits, player =>
			{
				if (!IsFenceIllegal(board, fence, direction, player)) return;

				board.SetIllegalFence(fence, direction);
			});
		});
	}

	private static bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();
		int mappedFence = Helper.GetMappedFenceIndex(fence, direction);

		boardClone.PlaceFence(mappedFence, player, true);

		int start = boardClone.GetPawnPosition(player);
		HashSet<int> goalTiles = boardClone.GetGoalTiles(player).ToHashSet();

		return Algorithms.RecursiveDFS(boardClone, start, goalTiles, new(), player);
	}
}
