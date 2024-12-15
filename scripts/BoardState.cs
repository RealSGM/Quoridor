using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[GlobalClass]
public partial class BoardState : Node
{
	private static readonly int[][][] DefaultTileGridConnections = new int[][][]
	{
		new int[][] { new int[] { 0, 2 }, new int[] { 1, 3 } },
		new int[][] { new int[] { 0, 1 }, new int[] { 2, 3 } }
	};

	public bool BoardReady { get; set; } = false;
	public int BoardSize { get; set; }
	public int CurrentPlayer { get; set; } = 0;
	public StringBuilder MoveHistory { get; set; } = new();

	private int[][] Tiles { get; set; }
	private bool[][] Fences { get; set; }
	/// Stores if the fence should be disabled for each direction based off latest DFS
	private bool[][] DFSDisabledFences { get; set;}
	private int[] PawnPositions { get; set; }
	private int[] AdjacentOffsets { get; set; }
	private int[] FenceCounts { get; set; }
	private int[][] WinPositions { get; set; }
	
	[Signal] public delegate void BoardUpdatedEventHandler();

	// Converts fence direction which is [0, 1] to [-1, 1] for notation
	public static int GetMappedFenceIndex(int fenceIndex, int direction)
	{
		return direction == 0 ? fenceIndex : -fenceIndex;
	}

	// Initalise the NESW connections for given index
	public static int[] InitialiseConnections(int index, int size)
	{
		int[] connections = new int[4];
		connections[0] = index >= size ? index - size : -1; // North Tile
		connections[1] = (index + 1) % size != 0 ? index + 1 : -1; // East Tile
		connections[2] = index < size * (size - 1) ? index + size : -1; // South Tile
		connections[3] = index % size != 0 ? index - 1 : -1; // West Tile
		return connections;
	}

	#region Initialization
	
	public BoardState Clone()
	{
		BoardState boardState = new()
		{
			FenceCounts = (int[])FenceCounts.Clone(),
			PawnPositions = (int[])PawnPositions.Clone(),
			Tiles = Tiles.Select(tile => (int[])tile.Clone()).ToArray(),
			AdjacentOffsets = (int[])AdjacentOffsets.Clone(),
			WinPositions = WinPositions.Select(winPosition => (int[])winPosition.Clone()).ToArray(),
			CurrentPlayer = CurrentPlayer,
			DFSDisabledFences = DFSDisabledFences.Select(dfs => (bool[])dfs.Clone()).ToArray(),
			MoveHistory = new StringBuilder(MoveHistory.ToString()),
			Fences = Fences.Select(fence => (bool[])fence.Clone()).ToArray(),
		};
		return boardState;
	}

	public void InitialiseBoard(int boardSize, int fencesPerPlayer, int playerCount)
	{
		BoardSize = boardSize;
		FenceCounts = Enumerable.Repeat(fencesPerPlayer, playerCount).ToArray();
		AdjacentOffsets = new int[4] { -boardSize, 1, boardSize, -1 };

		InitialisePawnPositions(playerCount, boardSize);
		InitialiseWinPositions(boardSize, playerCount);
		InitialiseTiles(boardSize);
		InitialiseFences(boardSize - 1);
	}

    private void InitialiseFences(int fenceSize)
    {
		Fences = new bool[fenceSize * fenceSize][];
		DFSDisabledFences = new bool[fenceSize * fenceSize][];
		for (int i = 0; i < fenceSize * fenceSize; i++)
		{
			Fences[i] = new bool[2];
			DFSDisabledFences[i] = new bool[2];
		}
    }

    public void InitialisePawnPositions(int playerCount, int boardSize)
	{
		PawnPositions = new int[playerCount];
		PawnPositions[0] = (int)(boardSize * (boardSize - 0.5));
		PawnPositions[1] = boardSize / 2;
	}

	public void InitialiseWinPositions(int boardSize, int playerCount)
	{
		int totalTiles = boardSize * boardSize;
		WinPositions = new int[playerCount][];
		WinPositions[0] = Enumerable.Range(0, boardSize).ToArray(); // Player 1's winning positions
		WinPositions[1] = Enumerable.Range(totalTiles - boardSize, boardSize).ToArray(); // Player 2's winning positions
	}

	public void InitialiseTiles(int boardSize)
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

	#endregion

	#region Setters

	public void SetDFSDisabled(int fence, int direction, bool val)
	{
		DFSDisabledFences[fence][direction] = val;
	}

	public void SetFencePlaced(int fence, int direction)
	{
		Fences[fence][direction] = true;
	}

	#endregion

	#region Getters
	
	public bool GetDFSDisabled(int fence, int direction)
	{
		return DFSDisabledFences[fence][direction];
	}

	public int[] GetTileConnections(int tile)
	{
		return Tiles[tile];
	}

	public bool GetFencePlaced(int fence, int direction)
	{
		return Fences[fence][direction];
	}

	public int GetFenceAmount()
	{
		return Fences.Length;
	}

	public int GetFenceCount(int playerIndex)
	{
		return FenceCounts[playerIndex];
	}

	public bool GetFenceEnabled(int fence, int direction)
	{
		return GetFencePlaced(fence, direction) || GetDFSDisabled(fence, direction);
	}

	public string GetMoveHistory()
	{
		return MoveHistory.ToString();
	}

	public bool GetWinner(int playerIndex)
	{
		return WinPositions[playerIndex].Contains(PawnPositions[playerIndex]);
	}
	
	public int GetPawnPosition(int playerIndex)
	{
        return PawnPositions[playerIndex];
	}

	public int[] GetWinPositions(int playerIndex) => WinPositions[playerIndex];

	#endregion

	#region Selectable Tiles
	public int[] GetReachableTiles(int playerIndex)
	{
		int playerPawnPosition = PawnPositions[playerIndex];
		int[] playerPawnTile = Tiles[playerPawnPosition];
		int[] reachableTiles = new int[0];

		// Loop through all tiles
		foreach (int connectedTile in playerPawnTile)
		{
			// Check if the tile is not empty
			if (connectedTile == -1) continue;

			reachableTiles = reachableTiles.Concat(CheckForEnemy(connectedTile)).ToArray();
		}
		return reachableTiles;
	}

	public int[] CheckForEnemy(int connectedTile)
	{
		int enemyPawnPosition = PawnPositions[1 - CurrentPlayer];
		// Check if the enemy pawn is not on the tile
		if (enemyPawnPosition != connectedTile)
		{
			return new int[] { connectedTile };
		}

		// Get the direction of the enemy pawn
		int directionIndex = Array.IndexOf(Tiles[PawnPositions[CurrentPlayer]], enemyPawnPosition);
		
		// Check if the leaped tile is within boundaries
		int leapedTileIndex = connectedTile + AdjacentOffsets[directionIndex];
		if (leapedTileIndex < 0 || leapedTileIndex >= Tiles.Length)
		{
			// Return an empty array, as the leaped tile is out of boundaries
			return Array.Empty<int>();
		}

		return GetLeapedTiles(PawnPositions[CurrentPlayer], leapedTileIndex, connectedTile, directionIndex);
	}

	public int[] GetLeapedTiles(int playerIndex, int leapedTileIndex, int tileIndex, int dirIndex)
	{
		int[] leapedTile = Tiles[leapedTileIndex];

		// Check no fence blocking it
		if (Tiles[tileIndex][dirIndex] == leapedTileIndex) return new int[] { leapedTileIndex };

		// Fence blocking it, check adjacent tiles over the enemy pawn
		var leapedTiles = Tiles[tileIndex]
			.Where(tile => tile != -1 && tile != playerIndex)
			.ToArray();

		return leapedTiles;
	}
	
	#endregion

	#region Fence Placement
	
	public int[] GetTileGrid(int index){
		int topLeft = index;
		int topRight = index + 1;
		int bottomLeft = index + BoardSize;
		int bottomRight = index + 1 + BoardSize;
		return new int[] {topLeft, topRight, bottomLeft, bottomRight};
	}

	public void PlaceFence(int fenceIndex, int currentPlayer, bool isIFS = false)
	{
		// Separate the fence index and direction
		int direction = fenceIndex < 0 ? 1 : 0;
		fenceIndex = Math.Abs(fenceIndex);

		// Match the fence button index to the index of the Top-Left Tile in 2x2 Grid
		int convertedIndex = fenceIndex + fenceIndex / (BoardSize - 1);

		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = GetTileGrid(convertedIndex);

		foreach (int[] pair in DefaultTileGridConnections[direction])
		{
			RemoveTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			RemoveTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}

		// Continue if User or AI is placing the fence
		if (isIFS) return;

		FenceCounts[currentPlayer]--;
		SetFencePlaced(fenceIndex, direction);
		SetFencePlaced(fenceIndex, 1 - direction);

		// Disable adjacent fences
		DisableAdjacentFences(fenceIndex, direction);

		BoardReady = true;
		EmitSignal(SignalName.BoardUpdated);
	}

	public void DisableAdjacentFences(int fenceIndex, int direction)
	{
		int flippedIndex = 1 - direction; // Flip the index (for NESW adjustment)
		int[] disabledIndexes = new int[2] { flippedIndex, flippedIndex + 2 };
		int[] adjacentFences = InitialiseConnections(fenceIndex, BoardSize - 1);

		foreach (int index in disabledIndexes)
		{
			int adjFenceIndex = adjacentFences[index];
			if (adjFenceIndex == -1) continue;

			SetFencePlaced(adjFenceIndex, direction);
		}
	}

	public void RemoveTileConnection(int tile, int tileToRemove)
	{
		int[] connections = Tiles[tile];
		int index = Array.IndexOf(connections, tileToRemove);
		if (index == -1) return;
		connections[index] = -1;
	}



	#endregion

	public void MovePawn(int tileIndex, int currentPlayer)
	{
		PawnPositions[currentPlayer] = tileIndex;
		BoardReady = true;
		EmitSignal(SignalName.BoardUpdated);
	}

	public void AddMove(string code)
	{
		BoardReady = false;

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
		possibleMoves.Add(2, GetTileConnections(PawnPositions[CurrentPlayer]));

		return possibleMoves;
	}
}
