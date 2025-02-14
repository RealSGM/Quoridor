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

		List<int> possibleFences = placedFences.SelectMany((fence, fenceIndex) =>
			board.GetSurroundingFences(fenceIndex)
				.Where(adjacentFence => Math.Abs(adjacentFence) < placedFences.Length && placedFences[Math.Abs(adjacentFence)] == -1)
		).ToList();

		possibleFences.ForEach(fence => board.SetIllegalFence(Math.Abs(fence), -1));

		Parallel.ForEach(possibleFences, fence =>
		{
			fence = Math.Abs(fence);

			Parallel.ForEach(Helper.Bits, direction =>
			{
				if (!board.GetFenceEnabled(fence, direction)) return;

				Parallel.ForEach(Helper.Bits, player =>
				{
					if (!IsFenceIllegal(board, fence, direction, player)) return;

					board.SetIllegalFence(Math.Abs(fence), direction);
				});
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
