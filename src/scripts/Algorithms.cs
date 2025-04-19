using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

[GlobalClass]
public partial class Algorithms: Node
{
	public static bool IsValidPath(BoardState board, int startTile, HashSet<int> goalTiles)
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

	public static int[] GetPathToGoal(BoardState board, int player)
	{
		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)];
		int startTile = board.GetPawnTile(player);
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
