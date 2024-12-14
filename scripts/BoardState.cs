using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;


[GlobalClass]
public partial class BoardState : Node
{
	public bool boardReady { get; set; } = false;
	public int boardSize { get; set; }
	public int[] PawnPositions { get; set; }
	public int[] AdjacentOffsets { get; set; }
	public int CurrentPlayer { get; set; }
	public int[] FenceCounts { get; set; }
	// Stores if the button should be disabled for each direction based off latest DFS
	public bool[][] DFSDisabledFences { get; set;}
	// Stores if the button should be disabled for each direction based off it the adjacent fence is placed or not
	public bool[][] DirDisabledFences { get; set;}
	public bool[] PlacedFences { get; set;}
	public int[][][][] Fences { get; set; }
	public int[][] Tiles { get; set; }
	public int[][] WinPositions { get; set; }
	public StringBuilder MoveHistory { get; set; } = new();
	
	// Signals
	[Signal] public delegate void BoardUpdatedEventHandler();

	#region Initialization
	
	public BoardState Clone()
	{
		BoardState boardState = new()
		{
			FenceCounts = (int[])FenceCounts.Clone(),
			PawnPositions = (int[])PawnPositions.Clone(),
			Fences = Fences.Select(fence => fence.Select(direction => direction.Select(connection => (int[])connection.Clone()).ToArray()).ToArray()).ToArray(),
			Tiles = Tiles.Select(tile => (int[])tile.Clone()).ToArray(),
			AdjacentOffsets = (int[])AdjacentOffsets.Clone(),
			WinPositions = WinPositions.Select(winPosition => (int[])winPosition.Clone()).ToArray(),
			CurrentPlayer = CurrentPlayer,
			DirDisabledFences = DirDisabledFences.Select(dir => (bool[])dir.Clone()).ToArray(),
			DFSDisabledFences = DFSDisabledFences.Select(dfs => (bool[])dfs.Clone()).ToArray(),
			PlacedFences = (bool[])PlacedFences.Clone(),
			MoveHistory = new StringBuilder(MoveHistory.ToString())
		};
		return boardState;
	}

	public void InitialiseFenceCounts(int fencesPerPlayer, int playerCount)
	{
		FenceCounts = Enumerable.Repeat(fencesPerPlayer, playerCount).ToArray();
	}

	public void InitialisePawnPositions(int playerCount, int boardSize)
	{
		PawnPositions = new int[playerCount];
		PawnPositions[0] = (int)(boardSize * (boardSize - 0.5));
		PawnPositions[1] = boardSize / 2;
	}

	public void InitialiseAdjacentOffsets(int boardSize)
	{
		AdjacentOffsets = new int[4] { -boardSize, 1, boardSize, -1 };
	}

	public void InitialiseWinPositions(int boardSize, int playerCount)
	{
		int totalTiles = boardSize * boardSize;
		WinPositions = new int[playerCount][];
		WinPositions[0] = Enumerable.Range(0, boardSize).ToArray(); // Player 1's winning positions
		WinPositions[1] = Enumerable.Range(totalTiles - boardSize, boardSize).ToArray(); // Player 2's winning positions
	}

	public void InitialiseFenceTileConnections(int index, int fenceRows)
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
	
	public int[] InitialiseConnections(int index, int size)
	{
		int[] connections = new int[4];
		connections[0] = index >= size ? index - size : -1; // North Tile
		connections[1] = (index + 1) % size != 0 ? index + 1 : -1; // East Tile
		connections[2] = index < size * (size - 1) ? index + size : -1; // South Tile
		connections[3] = index % size != 0 ? index - 1 : -1; // West Tile
		return connections;
	}

	public void GenerateTiles(int boardSize)
	{
		int totalTiles = boardSize * boardSize;
		
		// Generate empty tiles in the form of a 2D array
		Tiles = Enumerable.Range(0, totalTiles)
			.Select(_ => new int[4])
			.ToArray();

		// Loop through tiles, and apply SetTileConnections function to each tile
		for (int i = 0; i < totalTiles; i++)
		{
			Tiles[i] = InitialiseConnections(i, boardSize);
		}
	}

	public void GenerateFences(int fenceRows)
	{
		int totalFences = fenceRows * fenceRows;

		// Generate empty fences in the form of a 3D array
		Fences = new int[totalFences][][][];
		PlacedFences = new bool[totalFences];
		DFSDisabledFences = new bool[totalFences][];
		DirDisabledFences = new bool[totalFences][];
		
		for (int i = 0; i < totalFences; i++)
		{
			Fences[i] = new int[2][][];
			InitialiseFenceTileConnections(i, fenceRows);
			DFSDisabledFences[i] = new bool[] { false, false };
			DirDisabledFences[i] = new bool[] { false, false };
		}
	}

	#endregion

	#region Setters
	public void SetDFSDisabled(int fence, int direction, bool val)
	{
		DFSDisabledFences[fence][direction] = val;
	}

	public void SetDirDisabled(int fence, int direction, bool val)
	{
		DirDisabledFences[fence][direction] = val;
	}

	public void SetFencePlaced(int fence)
	{
		PlacedFences[fence] = true;
	}

	#endregion

	#region Getters
	public bool GetDFSDisabled(int fence, int direction)
	{
		return DFSDisabledFences[fence][direction];
	}

	public bool GetDirDisabled(int fence, int direction)
	{
		return DirDisabledFences[fence][direction];
	}

	public int[] GetAdjacentTiles(int tile)
	{
		return Tiles[tile];
	}

	public bool GetFencePlaced(int fence)
	{
		return PlacedFences[fence];
	}

	public int GetFenceAmount()
	{
		return Fences.Length;
	}

	public bool GetFenceEnabled(int fence, int direction)
	{
		return GetFencePlaced(fence) || GetDFSDisabled(fence, direction) || GetDirDisabled(fence, direction);
	}

	public string GetMoveHistory()
	{
		return MoveHistory.ToString();
	}

	public bool GetWinner(int playerIndex)
	{
		return WinPositions[playerIndex].Contains(PawnPositions[playerIndex]);
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

	public bool IsFenceAvailable(int playerIndex)
	{
		return FenceCounts[playerIndex] > 0;
	}

	public void PlaceFence(int fenceIndex, int currentPlayer, bool isIFS = false)
	{
		// Get the fence index
		int direction = fenceIndex < 0 ? 1 : 0;
		fenceIndex = Math.Abs(fenceIndex);

		// Get the adjacent tiles and remove the connections
		foreach (var connection in Fences[fenceIndex][direction])
		{
			RemoveTileConnection(connection, 0);
			RemoveTileConnection(connection, 1);
		}

		// Continue if User or AI is placing the fence
		if (isIFS) return;

		// Flip the index (for NESW adjustment)
		int flipped_index = 1 - direction;

		// Get the adjacent directionals
		int[] disabled_indexes = new int[2] { flipped_index, flipped_index + 2 };

		// Disable the adjacents buttons, for that direction
		foreach (int indexes in disabled_indexes)
		{
			int[] adj_fences = InitialiseConnections(fenceIndex, boardSize - 1);
			int index = adj_fences[indexes];
			if (index > -1)
			{
				SetDirDisabled(index, direction, true);
			}
		}

		FenceCounts[currentPlayer]--;
		SetFencePlaced(fenceIndex);

		boardReady = true;
		EmitSignal(SignalName.BoardUpdated);
	}

	public void RemoveTileConnection(int[] connection, int index)
	{
		int tileIndex = connection[index];
		int removingIndex = connection[1 - index];
		Tiles[tileIndex][Array.IndexOf(Tiles[tileIndex], removingIndex)] = -1;
	}

	public void MovePawn(int tileIndex, int currentPlayer)
	{
		PawnPositions[currentPlayer] = tileIndex;
		boardReady = true;
		EmitSignal(SignalName.BoardUpdated);
	}

	public Dictionary<int, int[]> GetPossibleMoves()
	{
		Dictionary<int, int[]> possibleMoves = new();
		int[] bits = new int[2] {0, 1};

		// Loop through both directions
		foreach	(int direction in bits)
		{
			var moves = Enumerable.Range(0, GetFenceAmount())
				.Where(fence => !GetFenceEnabled(fence, direction))
				.ToArray();
			possibleMoves[direction] = moves;
		}
		
		// Add the tile connections as index 2
		possibleMoves.Add(2, GetAdjacentTiles(PawnPositions[CurrentPlayer]));

		return possibleMoves;
	}

	public void AddMove(string code)
	{
		boardReady = false;

		MoveHistory.Append(code + ";");

		int currentPlayer = int.Parse(code[0].ToString());
		char moveType = code[1];
		int value = int.Parse(code[2..]);

		if (moveType == 'm')
		{
			MovePawn(value, currentPlayer);
		}
		else
		{
			PlaceFence(value, currentPlayer);
		}
	}

	// Converts fence direction which is [0, 1] to [-1, 1] for notation
	public static int GetMappedFenceIndex(int fenceIndex, int direction)
	{
		return direction == 0 ? fenceIndex : -fenceIndex;
	}
}
