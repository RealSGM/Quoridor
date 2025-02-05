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
		// var Console = GetNode<Window>("/root/Console");
		// Console.Call("add_entry", "Checking for illegal fences...", 0);
		// long startTime = DateTime.Now.Ticks;

		// Ignore if player has no more fences
		if (board.GetFenceCount(1 - board.CurrentPlayer) == 0) return;
		
		int[] placedFences = board.GetPlacedFences();

		List<int> possibleFences = placedFences
			.Select((value, fenceIndex) => new { value, fenceIndex })
			.Where(fence => fence.value != -1) // Skip fences that are not placed
			.SelectMany(fence => board.GetSurroundingFences(fence.fenceIndex)
				.Where(adjacentFence => adjacentFence != -1 && placedFences[adjacentFence] == -1))
			.Distinct()
			.ToList();

		possibleFences.ForEach(fence => board.SetIllegalFence(fence, -1));

		Parallel.ForEach(possibleFences, fence =>
		{
			Parallel.ForEach(Helper.Bits, direction =>
			{
				if (!board.GetFenceEnabled(fence, direction)) return;

				Parallel.ForEach(Helper.Bits, player =>
				{
					if (!IsFenceIllegal(board, fence, direction, 1 - player)) return;

					board.SetIllegalFence(fence, direction);
				});
			});
		});

		// Console.Call("add_entry", "Illegal fence check took: " + (DateTime.Now.Ticks - startTime) / 100 + "ns", 0);
	}

	private static bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
	{
		BoardState boardClone = board.Clone();
		int mappedFence = Helper.GetMappedFenceIndex(fence, direction);

		boardClone.PlaceFence(mappedFence, player, true);

		int start = boardClone.GetPawnPosition(player);
		HashSet<int> goalTiles = boardClone.GetGoalTiles(player).ToHashSet();

		return RecursiveDFS(boardClone, start, goalTiles, new(), player);
	}

	public static bool IterativeDFS(BoardState board, int start, HashSet<int> goalTiles, int player)
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

	public static bool RecursiveDFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
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

	public static bool IterativeBFS(BoardState board, int start, HashSet<int> goalTiles, int player)
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

	public static bool RecursiveBFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
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
}
