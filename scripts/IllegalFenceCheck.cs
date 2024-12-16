using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[GlobalClass]
public partial class IllegalFenceCheck : Node
{
	private readonly int[] _bits = { 0, 1 };
	public void GetIllegalFences(BoardState board)

	{
		var Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Checking for illegal fences...", 0);
		long startTime = DateTime.Now.Ticks;

		if (board.GetFenceCount(board.CurrentPlayer) == 0) return;

		Parallel.For(0, board.GetFenceAmount(), fence =>
		{
			board.SetDFSDisabled(fence, 0, false);
			board.SetDFSDisabled(fence, 1, false);

			Parallel.ForEach(_bits, direction =>
			{
				if (board.GetFencePlaced(fence, direction)) return;

				Parallel.ForEach(_bits, player =>
				{
					if (!IsFenceIllegal(board, fence, direction, player)) return;
					board.SetDFSDisabled(fence, direction, true);
				});
			});
		});

		Console.Call("add_entry", "Illegal fence check took: " + (DateTime.Now.Ticks - startTime) / 10000 + "ms", 0);
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

			foreach (int connectedTile in board.GetTileConnections(current).Reverse())
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				stack.Push(connectedTile);
			}
		}
		return true;
	}
}
