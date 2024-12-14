using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class IllegalFenceCheck : Node
{
	[Signal] public delegate void CheckCompletedEventHandler();
	private readonly int[] _bits = { 0, 1 };

	public void GetIllegalFences(BoardState board)
	{
		if (!board.IsFenceAvailable(board.CurrentPlayer)) return;

		for (int fence = 0; fence < board.GetFenceAmount(); fence++)
		{
			board.SetDFSDisabled(fence, 0, false);
			board.SetDFSDisabled(fence, 1, false);

			if (board.GetFencePlaced(fence)) continue;

			foreach (int direction in _bits)
			{
				if (board.GetDirDisabled(fence, direction)) continue;

				foreach (int player in _bits)
				{
					if (IsFenceIllegal(board, fence, direction, player))
					{
						board.SetDFSDisabled(fence, direction, true);
					}
				}
			}
		}
	}

	private bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();
		int mappedFence = BoardState.GetMappedFenceIndex(fence, direction);

		boardClone.PlaceFence(mappedFence, player, true);

		int start = boardClone.PawnPositions[player];
		HashSet<int> goalTiles = boardClone.WinPositions[player].ToHashSet();

		return IterativeDFS(boardClone, player, start, goalTiles);
	}

	private bool IterativeDFS(BoardState board, int player, int start, HashSet<int> goalTiles)
	{
		Stack<int> stack = new();
		HashSet<int> visited = new();

		stack.Push(start);

		while (stack.Count > 0)
		{
			int current = stack.Pop();

			if (goalTiles.Contains(current)) return false;

			if (visited.Contains(current)) continue;

			visited.Add(current);

			foreach (int connectedTile in board.Tiles[current])
			{
				if (!visited.Contains(connectedTile) && connectedTile != -1)
				{
					stack.Push(connectedTile);
				}
			}
		}
		return true;
	}
}
