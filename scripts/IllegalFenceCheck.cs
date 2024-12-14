using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class IllegalFenceCheck : Node
{
	[Signal] public delegate void CheckCompletedEventHandler();

	public void GetIllegalFences(BoardState board)
	{
		// Stop if current player has no more fences
		if (board.FenceCounts[board.CurrentPlayer] <= 0) return;

		List<Task<List<int>>> tasks = new();
		int[] bits = { 0, 1 };
		Dictionary<int, HashSet<int>> illegalFences = new()
		{
			{ 0, new HashSet<int>() },
			{ 1, new HashSet<int>() }
		};

		// Check each fence button, to see if it is possible
		for (int fence = 0; fence < board.GetFenceAmount(); fence++)
		{
			// Reset DFS Array
			board.SetDFSDisabled(fence, 0, false);
			board.SetDFSDisabled(fence, 1, false);

			// Ignore any placed fences
			if (board.GetFencePlaced(fence)) continue;

			// Loop for both, horizontal and vertical fences
			foreach (int direction in bits)
			{
				// Ignore fences adjacent to placed fences
				if (board.GetDirDisabled(fence, direction)) continue;

				// Loop for each player
				foreach (int player in bits)
				{
					// Run DFS check in parallel
					var task = Task.Run(() => IllegalFenceCheckThreaded(fence, direction, player, board));
					tasks.Add(task);
				}
			}
		}

		// Store results from tasks into dictionary
		Task.WhenAll(tasks).ContinueWith(t =>
		{
			foreach (var task in tasks)
			{
				var result = task.Result;
				if (result.Count == 0)
				{
					continue;
				}
				illegalFences[result[0]].Add(result[1]);
			}

			// Set results into fence buttons
			foreach (var direction in illegalFences.Keys)
			{
				foreach (var fence in illegalFences[direction])
				{
					board.SetDFSDisabled(fence, direction, true);
				}
			}
		}).Wait();
	}

	private List<int> IllegalFenceCheckThreaded(int fence, int direction, int player, BoardState currentBoard)
	{
		if (CheckIllegalFence(BoardState.GetMappedFenceIndex(fence, direction), player, currentBoard)) return new List<int>();
		return new List<int> { direction, fence };
	}

	public bool CheckIllegalFence(int fenceIndex, int playerIndex, BoardState board)
	{
		// Create a duplicate of the current board state
		BoardState boardState = board.Clone();
		boardState.PlaceFence(fenceIndex, playerIndex);

		int startIndex = boardState.PawnPositions[playerIndex];
		HashSet<int> goalTiles = boardState.WinPositions[playerIndex].ToHashSet();

		return IterativeDFS(startIndex, goalTiles, boardState);
	}

	public bool IterativeDFS(int startIndex, HashSet<int> goalTiles, BoardState boardState)
	{
		Stack<int> stack = new();
		HashSet<int> visitedTiles = new();

		stack.Push(startIndex);

		while (stack.Count > 0)
		{
			int currentIndex = stack.Pop();

			// Check if the current index is in the goal tiles
			if (goalTiles.Contains(currentIndex))
			{
				return true;
			}

			// Add the current index to the visited tiles
			visitedTiles.Add(currentIndex);

			// Loop through all connected tiles
			foreach (int connectedTile in boardState.Tiles[currentIndex])
			{
				// Check if the connected tile is not empty and not visited
				if (connectedTile != -1 && !visitedTiles.Contains(connectedTile))
				{
					stack.Push(connectedTile);
				}
			}
		}
		return false;
	}
}
