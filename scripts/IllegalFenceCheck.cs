using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class IllegalFenceCheck : Node
{
	[Signal] public delegate void CheckCompletedEventHandler();
	private readonly int[] _bits = { 0, 1 };

	public void GetIllegalFences(BoardState board)
	{
		GD.Print("Checking illegal fences");
		long startTime = DateTime.Now.Ticks;

		if (board.GetFenceCount(board.CurrentPlayer) == 0) return;

		for (int fence = 0; fence < board.GetFenceAmount(); fence++)
		{
			board.SetDFSDisabled(fence, 0, false);
			board.SetDFSDisabled(fence, 1, false);

			foreach (int direction in _bits)
			{
				if (board.GetFencePlaced(fence, direction)) continue;

				foreach (int player in _bits)
				{
					if (IsFenceIllegal(board, fence, direction, player))
					{
						GD.Print($"Illegal Fence Found: {fence} {direction} {player}");
						board.SetDFSDisabled(fence, direction, true);
					}
					break;
				}
			// break;
			}
		// break;
		}

		GD.Print("Illegal fence check took: " + (DateTime.Now.Ticks - startTime) / 10000 + "ms");
	}

	private bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{	
		BoardState boardClone = board.Clone();
		int mappedFence = BoardState.GetMappedFenceIndex(fence, direction);

		boardClone.PlaceFence(mappedFence, player, true);

		int start = boardClone.GetPawnPosition(player);
		HashSet<int> goalTiles = boardClone.GetWinPositions(player).ToHashSet();

		return IterativeDFS(boardClone, start, goalTiles);
	}

	private bool IterativeDFS(BoardState board, int start, HashSet<int> goalTiles)
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

			foreach (int connectedTile in board.GetTileConnections(current))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				stack.Push(connectedTile);
			}
		}
		return true;
	}
}
