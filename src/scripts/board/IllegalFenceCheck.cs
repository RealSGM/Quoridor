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
		if (board.HasFences(currentPlayer)) return;

		// Get the last move type
		char moveType = board.LastMove.MoveType;
		
		ulong[] possibleFences = board.GetEnabledFences();

		ulong[] surroundingFences = moveType == 'm'
			? Helper.GetFencesSurroundingTile(board.LastMove.Index)
			: board.GetAllSurroundingFences();

		// Ensure possible fences are enabled
		foreach (int direction in Helper.Bits)
		{
			possibleFences[direction] &= surroundingFences[direction];
		}

		// FIXME - Parallel not done for each fence as its using a generator
		Parallel.ForEach(Helper.Bits, dir =>
		{
			Parallel.ForEach(Helper.GetOnesInBitBoard(possibleFences[dir]), index =>
			{
				board.GetIllegalFences()[dir].UndoSetPlaced(index);

				if (IsFenceIllegal(board, index, dir, currentPlayer))
				{
					board.GetIllegalFences()[dir].SetPlaced(index);
				}
			});
		});
	}

	private static bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();

		boardClone.PlaceFence(direction, fence, player);

		int start = boardClone.GetPawnTile(player);

		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)];

		return Algorithms.IsValidPath(boardClone, start, goalTiles);
	}
}
