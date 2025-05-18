using Godot;
using System.Linq;

[GlobalClass]
public partial class IllegalFenceCheck : Node
{

	public static void GetIllegalFences(BoardWrapper wrapper)
	{
		BoardState board = wrapper.State;
		GetIllegalFences(board);
	}

	public static void GetIllegalFences(BoardState board)
	{
		ulong[] possibleFences = board.GetEnabledFences(false); // Get the enabled fences, ignoring illegal ones
		// If the last move was a pawn movement, get the fences surrounding that tile
		// Otherwise, get all surrounding fences
		ulong[] surroundingFences = board.LastMove != null && board.LastMove.MoveType == 'm'
			? Helper.GetFencesSurroundingTile(board.LastMove.Index)
			: board.GetAllSurroundingFences();
		
		possibleFences = [.. possibleFences.Zip(surroundingFences, (pf, sf) => pf & sf)]; // Perform bitwise AND operation on each pair

		foreach (int dir in Helper.Bits)
		{
			foreach (int index in Helper.GetOnesInBitBoard(possibleFences[dir]))
			{
				foreach (int player in Helper.Bits)
				{
					// Reset Illegal Fence and check
					board.GetIllegalFences()[dir].UndoSetPlaced(index);
					if (!IsFenceIllegal(board, player, dir, index)) continue;
					board.GetIllegalFences()[dir].SetPlaced(index);
					break;
				}
			}
		}
	}

	public static void CheckAllIllegalFences(BoardWrapper wrapper)
	{
		BoardState board = wrapper.State;
		ulong[] enabledFences = board.GetEnabledFences(false);

		foreach (int dir in Helper.Bits)
		{
			foreach (int index in Helper.GetOnesInBitBoard(enabledFences[dir]))
			{
				foreach (int player in Helper.Bits)
				{
					board.GetIllegalFences()[dir].UndoSetPlaced(index);
					if (!IsFenceIllegal(board, player, dir, index)) continue;
					board.GetIllegalFences()[dir].SetPlaced(index);
					break;
				}
			}
		}
	}

	public static bool IsFenceIllegal(BoardState board, int player, int direction, int fence)
	{
		BoardState boardClone = board.Clone();
		boardClone.PlaceFence(player, direction, fence); // Place the fence on the clone
		return !Algorithms.IsValidPath(boardClone, player); // Run a DFS to check if the path is valid
	}
}
