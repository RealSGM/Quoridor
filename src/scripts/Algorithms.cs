using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Algorithms : Node
{
	public static bool IterativeDFS(BoardState board, int start, HashSet<int> goalTiles, int player)
	{
		Stack<int> stack = new();
		HashSet<int> visited = new();

		stack.Push(start);

		while (stack.Count > 0)
		{
			int current = stack.Pop();

			if (goalTiles.Contains(current))
				return false;

			if (visited.Contains(current))
				continue;

			visited.Add(current);

			foreach (int connectedTile in board.GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1)
					continue;

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

			if (goalTiles.Contains(current))
				return false;

			if (visited.Contains(current))
				continue;

			visited.Add(current);

			foreach (int connectedTile in board.GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1)
					continue;

				queue.Enqueue(connectedTile);
			}
		}

		return true;
	}

	public static bool RecursiveBFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
	{
		if (goalTiles.Contains(current))
			return false;

		if (visited.Contains(current))
			return true;

		visited.Add(current);

		foreach (int connectedTile in board.GetPathConnections(current, player))
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;

			if (!RecursiveBFS(board, connectedTile, goalTiles, visited, player)) return false;
		}

		return true;
	}

	public static List<int> GetShortestPath(BoardState board, int start, HashSet<int> goalTiles, int player)
	{
		Queue<int> queue = new();
		Dictionary<int, int> previous = new();
		HashSet<int> visited = new();

		queue.Enqueue(start);

		while (queue.Count > 0)
		{
			int current = queue.Dequeue();

			if (goalTiles.Contains(current))
			{
				List<int> path = new() { current };

				while (previous.ContainsKey(current))
				{
					current = previous[current];
					path.Add(current);
				}

				path.Reverse();
				return path;
			}

			if (visited.Contains(current))
				continue;

			visited.Add(current);

			foreach (int connectedTile in board.GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1)
					continue;

				queue.Enqueue(connectedTile);
				previous[connectedTile] = current;
			}
		}

		return new();
	}

}
