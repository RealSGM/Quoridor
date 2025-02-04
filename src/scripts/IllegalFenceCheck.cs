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
		var Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Checking for illegal fences...", 0);
		long startTime = DateTime.Now.Ticks;

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
					if (!IsFenceIllegal(board, fence, direction, 1 - player)) return;
					board.SetDFSDisabled(fence, direction, true);
				});
			});
		});

		Console.Call("add_entry", "Illegal fence check took: " + (DateTime.Now.Ticks - startTime) / 100 + "ns", 0);
	}

	private bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();
		int mappedFence = BoardState.GetMappedFenceIndex(fence, direction);

		boardClone.PlaceFence(mappedFence, player, true);

		int start = boardClone.GetPawnPosition(player);
		HashSet<int> goalTiles = boardClone.GetGoalTiles(player).ToHashSet();

		return RecursiveBFS(boardClone, start, goalTiles, new HashSet<int>(), player);
	}

	private bool IterativeDFS(BoardState board, int start, HashSet<int> goalTiles, int player)
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

			foreach (int connectedTile in board.GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				stack.Push(connectedTile);
			}
		}
		return true;
	}

	private bool RecursiveDFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetPathConnections(current, player))
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveDFS(board, connectedTile, goalTiles, visited, player)) return false;
		}

		return true;
	}

	private bool IterativeBFS(BoardState board, int start, HashSet<int> goalTiles, int player)
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

			foreach (int connectedTile in board.GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				queue.Enqueue(connectedTile);
			}
		}
		return true;
	}

	private bool RecursiveBFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetPathConnections(current, player))
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveBFS(board, connectedTile, goalTiles, visited, player)) return false;
		}

		return true;
	}

	private bool IterativeAStar(BoardState board, int start, HashSet<int> goalTiles, int player)
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

			foreach (int connectedTile in board.GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				queue.Enqueue(connectedTile, board.GetDistanceToGoal(connectedTile, player));
			}
		}
		return true;
	}

	private bool RecursiveAStar(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetPathConnections(current, player))
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveAStar(board, connectedTile, goalTiles, visited, player)) return false;
		}

		return true;
	}
}
