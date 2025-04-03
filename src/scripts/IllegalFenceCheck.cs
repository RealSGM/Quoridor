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
		if (board.GetFenceCount(currentPlayer) == Helper.MaxFences) return;
	
		List<int> possibleFences = [.. board.GetPlacedFences()
			.SelectMany(board.GetAllSurroundingFences)
			.Where(fence => fence != -1)
			.Distinct()];

		
		Parallel.ForEach(possibleFences, fence =>
		{
			int index = Math.Abs(fence);
			int direction = fence < 0 ? 1 : 0;

			board.SetIllegalFence(index, direction, false);

			if (!board.GetFenceEnabled(index, direction)) return;

			Parallel.ForEach(Helper.Bits, player =>
			{
				if (!IsFenceIllegal(board, index, direction, player)) return;
				board.SetIllegalFence(index, direction, true);
			});
		});
	}

	private static bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();

		boardClone.PlaceFence(direction, fence, player);

		int start = boardClone.GetPawnPosition(player);

		HashSet<int> goalTiles = [.. boardClone.GetGoalTiles(player)];

		return Algorithms.RecursiveDFS(boardClone, start, goalTiles, [], player);
	}
}
