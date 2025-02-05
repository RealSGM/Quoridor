using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[GlobalClass]
public partial class BoardState : Control
{
	private static readonly int[][][] DefaultTileGridConnections = new int[][][]
	{
		new int[][] { new int[] { 0, 2 }, new int[] { 1, 3 } },
		new int[][] { new int[] { 0, 1 }, new int[] { 2, 3 } }
	};
	public static readonly int[] Bits = new int[2] {0, 1};

	public int BoardSize { get; set; }
	public int CurrentPlayer { get; set; } = 0;
	public StringBuilder MoveHistory { get; set; } = new();

	public bool[][] Fences { get; set; }
	private int[][] Tiles { get; set; }
	/// Stores if the fence should be disabled for each direction based off latest DFS
	private bool[][] DFSDisabledFences { get; set;}
	private int[] PawnPositions { get; set; }
	private int[] FenceCounts { get; set; }

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
			CurrentPlayer = CurrentPlayer,
			DFSDisabledFences = DFSDisabledFences.Select(dfs => (bool[])dfs.Clone()).ToArray(),
			MoveHistory = new StringBuilder(MoveHistory.ToString()),
			Fences = Fences.Select(fence => (bool[])fence.Clone()).ToArray(),
			BoardSize = BoardSize
		};
		return boardState;
	}

	public void InitialiseBoard(int boardSize, int fencesPerPlayer, int playerCount)
	{
		BoardSize = boardSize;
		FenceCounts = Enumerable.Repeat(fencesPerPlayer, playerCount).ToArray();

		InitialisePawnPositions(playerCount, boardSize);
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

	public int[] GetFencesOnBoard() => Enumerable.Range(0, Fences.Length)
			.Where(i => Fences[i][0] && Fences[i][1])
			.ToArray();

	public bool GetFenceEnabled(int fence, int direction) => GetFencePlaced(fence, direction) || GetDFSDisabled(fence, direction);

	public bool GetDFSDisabled(int fence, int direction) => DFSDisabledFences[fence][direction];

	public int[] GetTileConnections(int tile) => Tiles[tile];

	public int[] GetPathConnections(int tile, int player)
	{
		// Return NEWS connections for the bottom player
		if (player == 0) return new int[] { Tiles[tile][0], Tiles[tile][1], Tiles[tile][3], Tiles[tile][2] };
		// Return SNEW connections for the top player
		else return new int[] { Tiles[tile][2], Tiles[tile][3], Tiles[tile][1], Tiles[tile][0] };
	}

	public bool GetFencePlaced(int fence, int direction) => Fences[fence][direction];

	public int GetFenceAmount() => Fences.Length;

	public int GetFenceCount(int index) => FenceCounts[index];

	public string GetMoveHistory() => MoveHistory.ToString();

	public int[] GetGoalTiles(int player)
	{
		int startRow = player * (BoardSize - 1) * BoardSize;
		int endRow = startRow + BoardSize;
		return Enumerable.Range(startRow, endRow).ToArray();
	}

	public bool GetWinner(int player) => GetGoalTiles(player).Contains(GetPawnPosition(player));

	public int GetPawnPosition(int index) => PawnPositions[index];

	public int GetBoardSize() => BoardSize;

	#endregion

	#region Reachable Tiles

	public int[] GetReachableTiles(int player)
	{
		int playerPawnPosition = PawnPositions[player];
		int[] playerPawnTile = Tiles[playerPawnPosition];
		List<int> reachableTiles = new();

		// Loop through all tiles
		foreach (int connectedTile in playerPawnTile)
		{
			// Check if the tile is not empty
			if (connectedTile == -1) continue;

			// CheckFor Enemy and merge list
			reachableTiles.AddRange(CheckForEnemy(connectedTile));
		}
		return reachableTiles.ToArray();
	}

	public List<int> CheckForEnemy(int connectedTile)
	{
		int enemyPawnPosition = PawnPositions[1 - CurrentPlayer];
		// Check if the enemy pawn is not on the tile
		if (enemyPawnPosition != connectedTile)
		{
			return new List<int> { connectedTile };
		}

		// Get the direction of the enemy pawn
		int directionIndex = Array.IndexOf(Tiles[PawnPositions[CurrentPlayer]], enemyPawnPosition);

		// Check if the leaped tile is within boundaries
		int[] AdjacentOffsets = new int[4] { -BoardSize, 1, BoardSize, -1 };
		int leapedTileIndex = connectedTile + AdjacentOffsets[directionIndex];

		if (leapedTileIndex < 0 || leapedTileIndex >= Tiles.Length)
		{
			// Return an empty list, as the leaped tile is out of boundaries
			return new List<int>();
		}

		return GetLeapedTiles(PawnPositions[CurrentPlayer], leapedTileIndex, connectedTile, directionIndex).ToList();
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

		int convertedIndex = fenceIndex + (fenceIndex / (BoardSize - 1));

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

	public List<int> GetSurroundingFences(int fenceIndex)
	{
		// Get horizontal and vertically adjacent fences
		List<int> adjFences = InitialiseConnections(fenceIndex, BoardSize - 1).ToList();

		adjFences.Add(adjFences[0] % (BoardSize - 1) != 0 ? adjFences[0] - 1 : -1); // Top Left
		adjFences.Add((adjFences[0] + 1) % (BoardSize - 1) != 0 ? adjFences[0] + 1 : -1); // Top Right
		adjFences.Add(adjFences[2] % (BoardSize - 1) != 0 ? adjFences[2] - 1 : -1); // Bottom Left
		adjFences.Add((adjFences[2] + 1) % (BoardSize - 1) != 0 ? adjFences[2] + 1 : -1); // Bottom Right

		return adjFences.Where(fence => fence > -1).ToList();
	}

	#endregion

	#region Minimax

	/// Evaluate the board state, calculate the number of rows the player is from the goal and return a score
	public float EvaluateBoard()
	{
		// If the player has won, return 100
		if (GetWinner(CurrentPlayer)) return 100;
		// If the player has lost, return -100
		if (GetWinner(1 - CurrentPlayer)) return -100;

		return GetManhattanDistance();
	}

	public float GetManhattanDistance()
	{
		int boardSize = GetBoardSize();
		int startTile = GetPawnPosition(CurrentPlayer);
		return -1;
	}

	#endregion

	public void MovePawn(int tileIndex, int currentPlayer)
	{
		PawnPositions[currentPlayer] = tileIndex;
	}

	public void AddMove(string code)
	{
		MoveHistory.Append(code + ";");

		int currentPlayer = int.Parse(code[0].ToString());
		char moveType = code[1];
		int value = int.Parse(code[2..]);

		switch (moveType)
		{
			case 'm':
				MovePawn(value, currentPlayer);
				break;
			case 'f':
				PlaceFence(value, currentPlayer);
				break;
		}
	}

	public void BuildFromMoveHistory(string moveHistory)
	{
		string[] moves = moveHistory.Split(';');
		foreach (string move in moves)
		{
			if (move == "") continue;
			AddMove(move);
		}
	}

	public string GetAllMoves()
	{
		StringBuilder allMoves = new();

		// Loop through both directions and add all possible fence placements
		foreach (var direction in Bits)
		{
			for (int i = 0; i < GetFenceAmount(); i++)
			{
				if (GetFenceEnabled(i, direction)) continue;

				int mappedFence = GetMappedFenceIndex(i, direction);
				allMoves.Append($"{CurrentPlayer}f{mappedFence};");
			}
		}

		// Add all possible pawn moves
		GetReachableTiles(CurrentPlayer).ToList()
			.ForEach(index => allMoves.Append($"{CurrentPlayer}m{index};"));

		return allMoves.ToString();
	}

	public int GetDistanceToGoal(int tileIndex, int player)
	{
		int[] goalTiles = GetGoalTiles(player);
		int distance = 0;
		foreach (int goalTile in goalTiles)
		{
			distance += Math.Abs(goalTile - tileIndex);
		}
		return distance;
	}
}
