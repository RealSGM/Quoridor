using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Algorithms: Node
{
	public static bool IsValidPath(BoardState board, int startTile, HashSet<int> goalTiles, int player)
	{
		HashSet<int> visited = [startTile];
		Stack<int> stack = new();
		stack.Push(startTile);
		while (stack.Count > 0)
		{
			int currentTile = stack.Pop();

			if (goalTiles.Contains(currentTile)) return true;

			int[] adjTiles = board.GetAdjacentTiles(currentTile);

			foreach (int connectedTile in Helper.OrderConnections(adjTiles, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				stack.Push(connectedTile);
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
				// Reconstruct the path from the goal tile to the start tile
				List<int> resultPath = [];
				for (int tile = currentTile; tile != -1; tile = path[tile]) resultPath.Add(tile);
				resultPath.Reverse();
				return [.. resultPath];
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
