using Godot;
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

		string lastMove = board.GetLastMove();
		var (player, moveType, _, _, _) = Helper.GetMoveCodeAsTuple(lastMove);

		List<string> possibleFences = moveType == "m"
			? board.GetTileAdjacentFences(player)
			: [.. board.GetPlacedFences()
				.SelectMany(board.GetAllSurroundingFences)
				.Where(fence => fence != "")
				.Distinct()];

		Parallel.ForEach(possibleFences, fence =>
		{
			int index = int.Parse(fence[1..]);
			int direction = fence[0] == '+' ? 0 : 1;
			board.GetFences()[index].SetIllegal(direction, false);

			if (!board.IsFenceEnabled(index, direction)) return;

			Parallel.ForEach(Helper.Bits, player =>
			{
				if (!IsFenceIllegal(board, index, direction, player)) return;
				board.GetFences()[index].SetIllegal(direction, true);
			});
		});
	}

	private static bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();

		boardClone.PlaceFence(direction, fence, player);

		int start = boardClone.GetPawnPosition(player);

		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)];

		return Algorithms.RecursiveDFS(boardClone, start, goalTiles, [], player);
	}
}
