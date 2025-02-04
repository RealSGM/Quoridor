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
		int[] bits = { 0, 1 };

		if (board.GetFenceCount(board.CurrentPlayer) == 0) return;

		Parallel.For(0, board.GetFenceAmount(), fence =>
		{
			board.SetDFSDisabled(fence, 0, false);
			board.SetDFSDisabled(fence, 1, false);

			Parallel.ForEach(bits, direction =>
			{
				if (board.GetFencePlaced(fence, direction)) return;

				Parallel.ForEach(bits, player =>
				{
					if (!IsFenceIllegal(board, fence, direction, player)) return;
					board.SetDFSDisabled(fence, direction, true);
				});
			});
		});
	}

	private bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();
		int mappedFence = BoardState.GetMappedFenceIndex(fence, direction);

		boardClone.PlaceFence(mappedFence, player, true);

		int start = boardClone.GetPawnPosition(player);
		HashSet<int> goalTiles = boardClone.GetGoalTiles(player).ToHashSet();

		return RecursiveBFS(boardClone, start, goalTiles, new HashSet<int>());
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

	public bool RecursiveDFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetTileConnections(current).Reverse())
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveDFS(board, connectedTile, goalTiles, visited)) return false;
		}

		return true;
	}

	public bool IterativeBFS(BoardState board, int start, HashSet<int> goalTiles)
	{
		Queue<int> queue = new();
		HashSet<int> visited = new();

		queue.Enqueue(start);

		while (queue.Count > 0)
		{
			int current = queue.Dequeue();

			if (goalTiles.Contains(current)) return false;

			if (visited.Contains(current)) continue;

			visited.Add(current);

			foreach (int connectedTile in board.GetTileConnections(current))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				queue.Enqueue(connectedTile);
			}
		}
		return true;
	}

	public bool RecursiveBFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetTileConnections(current))
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveBFS(board, connectedTile, goalTiles, visited)) return false;
		}

		return true;
	}

	public bool IterativeAStar(BoardState board, int start, HashSet<int> goalTiles, int player)
	{
		HashSet<int> visited = new();
		PriorityQueue<int, int> queue = new();

		queue.Enqueue(start, 0);

		while (queue.Count > 0)
		{
			int current = queue.Dequeue();

			if (goalTiles.Contains(current)) return false;

			if (visited.Contains(current)) continue;

			visited.Add(current);

			foreach (int connectedTile in board.GetTileConnections(current))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				queue.Enqueue(connectedTile, board.GetDistanceToGoal(connectedTile, player));
			}
		}
		return true;
	}

	public bool RecursiveAStar(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetTileConnections(current))
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveAStar(board, connectedTile, goalTiles, visited, player)) return false;
		}

		return true;
	}
}
