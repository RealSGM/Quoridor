using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class Algorithms : Node
{
	public static bool IsValidPath(BoardState board, int player)
	{
		int startTile = board.GetPawnTile(player); // Get the tile where the pawn is located
		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)]; // Get the goal tiles for the player
		HashSet<int> visited = [startTile]; // Initialize the visited set with the start tile
		Stack<int> stack = new([startTile]); // Initialize the stack with the start tile

		while (stack.Count > 0)
		{
			int currentTile = stack.Pop(); // Get the current tile from the stack
			if (goalTiles.Contains(currentTile)) return true; // Check if the current tile is a goal tile

			int[] adjTiles = board.GetAdjacentTiles(currentTile); // Get the adjacent tiles of the current tile

			foreach (int connectedTile in Helper.OrderConnections(adjTiles, player)) // Loop through the ordered tiles
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue; // Ignore if visited or blocked
				stack.Push(connectedTile);
				visited.Add(connectedTile);
			}
		}
		return false;
	}

	public static int[] GetPathToGoal(BoardState board, int player)
	{
		HashSet<int> goalTiles = [.. Helper.GetGoalTiles(player)]; // Get the goal tiles for the player
		int startTile = board.GetPawnTile(player); // Get the tile where the pawn is located
		HashSet<int> visited = [startTile]; // Initialize the visited set with the start tile
		Queue<int> queue = new([startTile]); // Initialize the queue with the start tile
		int[] path = [.. Enumerable.Repeat(-1, 81)]; // Initialize the path array with -1

		while (queue.Count > 0)
		{
			int currentTile = queue.Dequeue(); // Get the current tile from the queue

			if (goalTiles.Contains(currentTile)) // Check if the current tile is a goal tile
			{
				// Reconstruct the path from the goal tile to the start tile
				List<int> resultPath = [];
				for (int tile = currentTile; tile != -1; tile = path[tile]) resultPath.Add(tile);
				resultPath.Reverse();
				return [.. resultPath];
			}

			foreach (int connectedTile in board.GetAdjacentTiles(currentTile))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue; // Ignore if visited or blocked
				queue.Enqueue(connectedTile);
				visited.Add(connectedTile); 
				path[connectedTile] = currentTile; // Store the path to reconstruct later
			}
		}
		return [];
	}
}
