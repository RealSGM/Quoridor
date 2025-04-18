using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

[GlobalClass]
public partial class Algorithms: Node
{
	public static bool RecursiveDFS(BoardState board, int current, HashSet<int> goalTiles, HashSet<int> visited, int player)
	{
		if (goalTiles.Contains(current)) return false;

		if (visited.Contains(current)) return true;

		visited.Add(current);

		Tile tile = board.GetTile(current);
		int[] connections = tile.GetOrderedConnections(player);

		foreach (int connectedTile in connections)
		{
			if (visited.Contains(connectedTile) || connectedTile == -1) continue;
			if (!RecursiveDFS(board, connectedTile, goalTiles, visited, player)) return false;
		}

		return true;
	}

	public static int[] GetShortestPath(int player, BoardState board)
	{
		Queue<int> queue = new();
		Dictionary<int, int> previous = [];
		HashSet<int> visited = [];
		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)];

		queue.Enqueue(board.GetPawnPosition(player));

		while (queue.Count > 0)
		{
			int current = queue.Dequeue();

			if (goalTiles.Contains(current))
			{
				List<int> path = [current];

				while (previous.ContainsKey(current))
				{
					current = previous[current];
					path.Add(current);
				}

				path.Reverse();
				return [.. path];
			}

			if (visited.Contains(current)) continue;

			visited.Add(current);

			Tile tile = board.GetTile(current);
			int[] connections = tile.GetOrderedConnections(player);

			foreach (int connectedTile in connections)
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;
				queue.Enqueue(connectedTile);
				previous[connectedTile] = current;
			}
		}

		return [];
	}

	public static bool IsValidPath(Board board, int startTile, HashSet<int> goalTiles)
	{
		HashSet<int> visited = [startTile];
		Queue<int> queue = new([startTile]);

		while (queue.Count > 0)
		{
			int currentTile = queue.Dequeue();

			if (goalTiles.Contains(currentTile)) return true;

			foreach (int connectedTile in board.GetAdjacentTiles(currentTile))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				queue.Enqueue(connectedTile);
				visited.Add(connectedTile);
			}
		}
		return false;
	}

	public static int[] GetPathToGoal(Board board, int player)
	{
		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)];
		int startTile = board.GetPawnPosition(player);
		HashSet<int> visited = [startTile];
		Queue<int> queue = new([startTile]);
		
		int[] path = [.. Enumerable.Repeat(-1, 81)];

		while (queue.Count > 0)
		{
			int currentTile = queue.Dequeue();

			// Check if the current tile is a goal tile
			if (goalTiles.Contains(currentTile))
			{
				// If we reached a goal tile, reconstruct the path
				return [.. Enumerable.Range(0, currentTile + 1)
					.Reverse()
					.TakeWhile(tile => tile != -1)
					.Select(tile => path[tile])
					.Reverse()];
			}

			foreach (int connectedTile in board.GetAdjacentTiles(currentTile))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				queue.Enqueue(connectedTile);
				visited.Add(connectedTile);
				path[connectedTile] = currentTile;
			}
		}

		return [];
	}
}
