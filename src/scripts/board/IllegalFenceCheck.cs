using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class IllegalFenceCheck : Node
{
	public static void GetIllegalFences(BoardState board, int currentPlayer)
	{
		// Ignore if player has no more fences
		if (!board.HasFences(currentPlayer)) return;

		ulong[] possibleFences = board.GetEnabledFences(false);

		ulong[] surroundingFences = board.LastMove.MoveType == 'm'
			? Helper.GetFencesSurroundingTile(board.LastMove.Index)
			: board.GetAllSurroundingFences();

		// Perform bitwise AND operation on each pair
		possibleFences = [.. possibleFences.Zip(surroundingFences, (pf, sf) => pf & sf)];

		Parallel.ForEach(Helper.Bits, dir =>
		{
			Parallel.ForEach(Helper.GetOnesInBitBoard(possibleFences[dir]), index =>
			{
				Parallel.For(0, Helper.Bits.Length, player =>
				{
					board.GetIllegalFences()[dir].UndoSetPlaced(index);
					if (IsFenceIllegal(board, player, dir, index))
					{
						board.GetIllegalFences()[dir].SetPlaced(index);
					}
				});
			});
		});
	}

	public static bool IsFenceIllegal(BoardState board, int player, int direction, int fence)
	{
		BoardState boardClone = board.Clone();

		boardClone.PlaceFence(player, direction, fence);

		int start = boardClone.GetPawnTile(player);

		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)];

		return !Algorithms.IsValidPath(boardClone, start, goalTiles, player);
	}
}
