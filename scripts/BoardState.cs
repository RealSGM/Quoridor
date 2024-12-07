using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[GlobalClass]
public partial class BoardState : Node
{
	[Export]
	public int[] FenceCounts { get; set; }
	[Export]
	public int[] PawnPositions { get; set; }
	public int[][][][] Fences { get; set; }
	public int[][] Tiles { get; set; }
	[Export]
	public int[] AdjacentOffsets { get; set; }
	public int[][] WinPositions { get; set; }
	[Export]
	public int CurrentPlayer { get; set; }

	#region Initialization
	public void SetFenceCounts(int fencesPerPlayer, int playerCount)
	{
		FenceCounts = Enumerable.Repeat(fencesPerPlayer, playerCount).ToArray();
	}

	public void SetPawnPositions(int playerCount, int boardSize)
	{
		PawnPositions = new int[playerCount];
		PawnPositions[0] = (int)(boardSize * (boardSize - 0.5));
		PawnPositions[1] = (int) boardSize / 2;
	}

	public void SetAdjacentOffsets(int boardSize)
	{
		AdjacentOffsets = new int[4] { -boardSize, 1, boardSize, -1 };
	}

	public void SetWinPositions(int boardSize, int playerCount)
	{
		int totalTiles = boardSize * boardSize;
		WinPositions = new int[playerCount][];
		WinPositions[0] = Enumerable.Range(0, boardSize).ToArray(); // Player 1's winning positions
		WinPositions[1] = Enumerable.Range(totalTiles - boardSize, boardSize).ToArray(); // Player 2's winning positions
	}

	public void GenerateTiles(int boardSize)
	{
		int totalTiles = boardSize * boardSize;
		
		// Generate empty tiles in the form of a 2D array
		Tiles = Enumerable.Range(0, totalTiles)
			.Select(_ => new int[4])
			.ToArray();

		// Loop through tiles, and apply SetTileConnections function to each tile
		Enumerable.Range(0, totalTiles)
			.ToList()
			.ForEach(i => Tiles[i] = GetConnections(i, boardSize));
	}

	public void GenerateFences(int fenceRows)
	{
		int totalFences = fenceRows * fenceRows;

		// Generate empty fences in the form of a 3D array
		Fences = Enumerable.Range(0, totalFences)
			.Select(_ => new int[2][][])
			.ToArray();

		// Loop through fences, and apply SetFenceTileConnections function to each fence
		Enumerable.Range(0, totalFences)
			.ToList()
			.ForEach(i => SetFenceTileConnections(i, fenceRows));
	}

	public void SetFenceTileConnections(int index, int fenceRows)
	{
		// Match the fence button index to the index of the Top-Left Tile in 2x2 Grid
		int modifiedIndex = index + index / fenceRows;

		int boardSize = fenceRows + 1;
		int topLeft = modifiedIndex;
		int topRight = modifiedIndex + 1;
		int bottomLeft = modifiedIndex + boardSize;
		int bottomRight = modifiedIndex + 1 + boardSize;
		
		Fences[index][0] = new int[2][] { new int[2] { topLeft, bottomLeft }, new int[2] { topRight, bottomRight } }; // Horizontal Fences
		Fences[index][1] = new int[2][] { new int[2] { topLeft, topRight }, new int[2] { bottomLeft, bottomRight } }; // Vertical Fences
	}
	
	public BoardState Clone()
	{
		BoardState boardState = new BoardState();
		boardState.FenceCounts = FenceCounts.ToArray();
		boardState.PawnPositions = PawnPositions.ToArray();
		boardState.Fences = Fences.Select(fence => fence.Select(direction => direction.Select(connection => connection.ToArray()).ToArray()).ToArray()).ToArray();
		boardState.Tiles = Tiles.Select(tile => tile.ToArray()).ToArray();
		boardState.AdjacentOffsets = AdjacentOffsets.ToArray();
		boardState.WinPositions = WinPositions.Select(winPosition => winPosition.ToArray()).ToArray();
		boardState.CurrentPlayer = CurrentPlayer;
		return boardState;
	}
	#endregion

	#region Selectable Tiles
	public int[] GetSelectableTiles(int playerIndex)
	{
		int playerPawnPosition = PawnPositions[playerIndex];
		int[] playerPawnTile = Tiles[playerPawnPosition];
		int[] tilesToSelect = new int[0];

		// Loop through all tiles
		foreach (int connectedTile in playerPawnTile)
		{
			// Check if the tile is not empty
			if (connectedTile != -1)
			{
				tilesToSelect = tilesToSelect.Concat(CheckForEnemy(connectedTile)).ToArray();
			}
		}
		return tilesToSelect;
	}

	public int[] CheckForEnemy(int connectedTile)
	{
		int enemyPawnPosition = PawnPositions[1-CurrentPlayer];
		// Check enemy pawn is not on the tile
		if (enemyPawnPosition != connectedTile)
		{
			return new int[] { connectedTile };
		}

		// Get the direction of the enemy pawn
		int dirIndex = Array.IndexOf(Tiles[PawnPositions[CurrentPlayer]], enemyPawnPosition);
		
		// Check if leaped tile is in boundary
		int leapedTileIndex = connectedTile + AdjacentOffsets[dirIndex];
		if (leapedTileIndex < 0 || leapedTileIndex >= Tiles.Length)
		{
			// Return empty array, as leaped tile is out of boundary, pawn is taken
			return Array.Empty<int>();
		}

		// return Array.Empty<int>();

		return GetLeapedTiles(PawnPositions[CurrentPlayer], leapedTileIndex, connectedTile, dirIndex);
	}

	public int[] GetLeapedTiles(int playerIndex, int leapedTileIndex, int tileIndex, int dirIndex)
	{
		int[] leapedTile = Tiles[leapedTileIndex];

		// Check no fence blocking it
		if (Tiles[tileIndex][dirIndex] == leapedTileIndex)
		{
			return new int[] { leapedTileIndex };
		}

		// Fence blocking it, check adjacent tiles over the enemy pawn
		var leapedTiles = Tiles[tileIndex]
			.Where(tile => tile != -1 && tile != playerIndex)
			.ToArray();

		return leapedTiles;
	}
	#endregion

	#region Illegal Fence Check
	public bool CheckIllegalFence(int fenceIndex, int direction, int playerIndex)
	{
		// Create a duplicate of the current board state
		BoardState boardState = Clone();
		boardState.PlaceFence(fenceIndex, direction, playerIndex);

		int startIndex = PawnPositions[playerIndex];
		int[] goalTiles = WinPositions[playerIndex];

		return RecursiveDFS(startIndex, goalTiles, new HashSet<int>(), boardState);
	}

	public bool RecursiveDFS(int currentIndex, int[] goalTiles, HashSet<int> visitedTiles, BoardState boardState)
	{
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
				// Recursively call the function with the connected tile
				if (RecursiveDFS(connectedTile, goalTiles, visitedTiles, boardState))
				{
					return true;
				}
			}
		}
		return false;
	}

	#endregion
	public bool CheckWin(int currentTileIndex)
	{
		return WinPositions[CurrentPlayer].Contains(currentTileIndex);
	}

	public bool IsFenceAvailable(int playerIndex)
	{
		return FenceCounts[playerIndex] > 0;
	}

	public void PlaceFence(int fenceIndex, int direction, int currentPlayer)
	{
		foreach (var connection in Fences[fenceIndex][direction])
		{
			RemoveTileConnection(connection, 0);
			RemoveTileConnection(connection, 1);
		}

		FenceCounts[currentPlayer]--;
	}

	public void RemoveTileConnection(int[] connection, int index)
	{
		int tileIndex = connection[index];
		int removingIndex = connection[1 - index];
		Tiles[tileIndex][Array.IndexOf(Tiles[tileIndex], removingIndex)] = -1;
	}

	public void MovePawn(int tileIndex)
	{
		PawnPositions[CurrentPlayer] = tileIndex;
	}

	public int[] GetConnections(int index, int size)
	{
		int[] connections = new int[4];
		connections[0] = index >= size ? index - size : -1; // North Tile
		connections[1] = (index + 1) % size != 0 ? index + 1 : -1; // East Tile
		connections[2] = index < size * (size - 1) ? index + size : -1; // South Tile
		connections[3] = index % size != 0 ? index - 1 : -1; // West Tile
		return connections;
	}
}
