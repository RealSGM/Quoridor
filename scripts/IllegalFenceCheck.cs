using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public partial class IllegalFenceCheck : Node
{
	public Dictionary<int, List<int>> illegalFences;
	public List<Thread> threads;

	public void GetIllegalFences(BoardState board) 
	{
		if  (board.FenceCounts[board.CurrentPlayer] <= 0) return;

		int[] bits = {0, 1};

		illegalFences = new();
		threads = new();

		foreach (int direction in bits)
		{
			illegalFences[direction] = new();
			
			for (int fence = 0; fence < board.GetFenceAmount(); fence++)
			{
				// Ignore placed fences
				if (board.GetFencePlaced(fence)) continue;

				
				// Reset DFS Disabled
				board.SetDFSDisabled(fence, direction, false);

				// Ignore Adjacently Dir Disabled Fences
				if (board.GetDirDisabled(fence, direction)) continue;

				// Loop through each player
				foreach (int player in bits)
				{
					// Create new thread and run
					Thread thread = new(() => IllegalFenceCheckThread(fence, direction, player, board));
					threads.Add(thread);
					thread.Start();
				}
			}
		}

		foreach (Thread thread in threads)
		{
			thread.Join();
		}
		threads.Clear();

		foreach (var (direction, fences) in illegalFences)
		{
			foreach (int fence in fences)
			{
				GD.Print("Fence: ", fence);
				board.SetDFSDisabled(fence, direction, true);
			}
		}
	}

	public void IllegalFenceCheckThread(int fence, int direction, int player, BoardState board)
	{
		if (CheckIllegalFence(fence, direction, player, board)) return;
		_ = illegalFences[direction].Append(fence);
	}

	// Check for a specfic illegal fence
	public bool CheckIllegalFence(int fence, int direction, int player, BoardState currentBoard)
	{
		// Create a duplicate of the current board state
		BoardState boardState = currentBoard.Clone();
		boardState.PlaceFence(fence, direction, player);

		int startIndex = currentBoard.PawnPositions[player];
		HashSet<int> goalTiles = new(currentBoard.WinPositions[player]);

		return IterativeDFS(startIndex, goalTiles, boardState);
	}

	public bool IterativeDFS(int startIndex, HashSet<int> goalTiles, BoardState boardState)
	{
		// Use a HashSet to track visited tiles
		HashSet<int> visitedTiles = new();

		// Use a stack for iterative DFS
		Stack<int> stack = new();
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
			if (!visitedTiles.Contains(currentIndex))
			{
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
		}
		return false;
	}
}
