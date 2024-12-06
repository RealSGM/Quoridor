using Godot;
using System;
using System.Linq;

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

	public void SetFenceTileConnections(int index, int boardSize)
	{
		// Match the fence button index to the index of the Top-Left Tile in 2x2 Grid
		index += index / (boardSize - 1);

		int topLeft = index;
		int topRight = index + 1;
		int bottomLeft = index + boardSize;
		int bottomRight = index + 1 + boardSize;

		Fences[index][0] = new int[2][] { new int[2] { topLeft, bottomLeft }, new int[2] { topRight, bottomRight } }; // Horizontal Fences
		Fences[index][1] = new int[2][] { new int[2] { topLeft, topRight }, new int[2] { bottomLeft, bottomRight } }; // Vertical Fences
	}

	public bool CheckWin(int playerIndex, int currentTileIndex)
	{
		return WinPositions[playerIndex].Contains(currentTileIndex);
	}

	public bool IsFenceAvailable(int playerIndex)
	{
		return FenceCounts[playerIndex] > 0;
	}

	public int[] GetSelectableTiles(int playerIndex)
	{
		int playerPawnPosition = PawnPositions[playerIndex];
		int[] playerPawnTile = Tiles[playerPawnPosition];
		int[] tilesToSelect = new int[0];

		GD.Print("Player Pawn Tile: " + playerPawnTile);
		// Loop through all tiles
		foreach (int playerTile in playerPawnTile)
		{
			// Check if the tile is not empty
			if (playerTile != -1)
			{
				tilesToSelect = tilesToSelect.Concat(GetAdjacentTiles(playerTile)).ToArray();
			}
		}
		return tilesToSelect;
	}

	public int[] GetAdjacentTiles(int tileIndex)
	{
		// Check enemy pawn is not on the tile
		if (PawnPositions[1-CurrentPlayer] != tileIndex)
		{
			return new int[] { tileIndex };
		}

		// Get the direction of the enemy pawn
		int dirIndex = Array.IndexOf(Tiles[tileIndex], PawnPositions[1-CurrentPlayer]);
		// Check if leaped tile is in boundary
		int leapedTileIndex = tileIndex + AdjacentOffsets[dirIndex] * 2;

		if (leapedTileIndex < 0 || leapedTileIndex >= Tiles.Length)
		{
			return Array.Empty<int>();
		}

		return GetLeapedTiles(PawnPositions[CurrentPlayer], leapedTileIndex, tileIndex, dirIndex);
	}

	public int[] GetLeapedTiles(int playerIndex, int leapedTileIndex, int tileIndex, int dirIndex)
	{
		int[] leapedTile = Tiles[leapedTileIndex];

		// Check no fence blocking it
		if (leapedTile[dirIndex] == tileIndex)
		{
			return new int[] { leapedTileIndex };
		}

		// Fence blocking it, check adjacent tiles over the enemy pawn
		var leapedTiles = Tiles[tileIndex]
			.Where(tile => tile != -1 && tile != playerIndex)
			.ToArray();

		return leapedTiles;
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
