using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class IllegalFenceCheck : Node
{
	[Signal] public delegate void CheckCompletedEventHandler();
	int[] bits = { 0, 1 };

	public void GetIllegalFences(BoardState board)
	{
		// Check if the current player has any fences left
		if (!board.IsFenceAvailable(board.CurrentPlayer)) return;
		
		// Loop through all the fences on the board
		for (int fence = 0; fence < board.GetFenceAmount(); fence++)
		{
			// Reset the DFS Enabled Array
			board.SetDFSDisabled(fence, 0, false);
			board.SetDFSDisabled(fence, 1, false);

			// Ignore any placed fences
			if (board.GetFencePlaced(fence)) continue;

			// Loop through both horizontal and vertical directions
			foreach (int direction in bits)
			{
				// Ignore any disabled directions
				if (board.GetDirDisabled(fence, direction)) continue;

				// Loop for each player
				foreach (int player in bits)
				{
					if (!IsFenceIllegal(board, fence, direction, player)) continue;

					board.SetDFSDisabled(fence, direction, true);
				}
			}
		}
	}

    private bool IsFenceIllegal(BoardState board, int fence, int direction, int player)
    {
		// Clone the boardState so it can't be modified
		BoardState boardClone = board.Clone();
		int mappedFence = BoardState.GetMappedFenceIndex(fence, direction);
		
		// Place the fence on the cloned board
		boardClone.PlaceFence(mappedFence, player, true);
		
		int start = boardClone.PawnPositions[player];
		HashSet<int> goalTiles = boardClone.WinPositions[player].ToHashSet();

        return IterativeDFS(boardClone, player, start, goalTiles);
    }

    private bool IterativeDFS(BoardState board, int player, int start, HashSet<int> goalTiles)
    {
		Stack<int> stack = new();
		HashSet<int> visited = new();

		stack.Push(start);

		while (stack.Count > 0)
		{
			// Pop the current tile
			int current = stack.Pop();

			// Check if the current tile is a goal node
			if (goalTiles.Contains(current)) return false;
			
			// Check if the current tile has been visited
			if (visited.Contains(current)) continue;

			visited.Add(current);

			// Loop through all connected tiles
			foreach (int connectedTile in board.Tiles[current])
			{
				// Check if the connected tile has been visited
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;

				// Push the connected tile to the stack
				stack.Push(connectedTile);
			}
		}
        return true;
    }
}