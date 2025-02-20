using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

[GlobalClass]
public partial class BoardState : Control
{
	[Export] private int[] PlacedFences { get; set; } // Array of placed fences, index = fence, value = direction => [-1 = Empty, 0 = Horizontally Placed, 1 = Vertically Placed]
	[Export] private int[] IllegalFences { get; set;} // Array of illegal fences, index = fence, value = direction => [-1 = Empty, 0 = Horizontally Illegal, 1 = Vertically Illegal]
	[Export] private int[] PawnPositions { get; set; }
	[Export] private int[] FenceCounts { get; set; }

	private int[][] Tiles { get; set; }
	private List<int>[] VisitedTiles { get; set; } = new List<int>[2] { new(), new() };

	public string LastMove { get; set; }
	public int BoardSize { get; set; }
	public StringBuilder MoveHistory { get; set; } = new();

	#region Initialisation ---
	#endregion

	public BoardState Clone() => new()
	{
		FenceCounts = FenceCounts.Clone() as int[],
		PawnPositions = PawnPositions.Clone() as int[],
		Tiles = Tiles.Select(tile => tile.Clone() as int[]).ToArray(),
		IllegalFences = IllegalFences.Clone() as int[],
		MoveHistory = new StringBuilder(MoveHistory.ToString()),
		PlacedFences = PlacedFences.Clone() as int[],
		BoardSize = BoardSize
	};

	public void InitialiseBoard(int boardSize, int fencesPerPlayer)
	{
		BoardSize = boardSize;
		FenceCounts = Enumerable.Repeat(fencesPerPlayer, Helper.PlayerCount).ToArray();

		InitialisePawnPositions();
		InitialiseTiles();
		InitialiseFences();
		InitialiseIllegalFences();
	}

	private void InitialiseFences() => PlacedFences = Enumerable.Repeat(-1, (BoardSize - 1) * (BoardSize - 1)).ToArray();

	public void InitialisePawnPositions()
	{
		PawnPositions = new int[Helper.PlayerCount];
		PawnPositions[0] = (int)(BoardSize * (BoardSize - 0.5));
		PawnPositions[1] = BoardSize / 2;
	}

	public void InitialiseTiles()
	{
		Tiles = Enumerable.Range(0, BoardSize * BoardSize)
			.Select(index => Helper.InitialiseConnections(index, BoardSize))
			.ToArray();
	}

	public void InitialiseIllegalFences() => IllegalFences = Enumerable.Repeat(-1, (BoardSize - 1) * (BoardSize - 1)).ToArray();

	#region Setters ---
	#endregion

	public void SetIllegalFence(int fence, int direction) => IllegalFences[fence] = direction;

	#region Getters ---
	#endregion

	public int[] GetPlacedFences() => PlacedFences;

	public int GetPlacedFence(int fence) => PlacedFences[fence];

	public int[] GetTileConnections(int tile) => Tiles[tile];

	public int[] GetPathConnections(int tile, int player)
	{
		// Return NEWS connections for the bottom player
		if (player == 0) return new int[] { Tiles[tile][0], Tiles[tile][1], Tiles[tile][3], Tiles[tile][2] };
		// Return SNEW connections for the top player
		else return new int[] { Tiles[tile][2], Tiles[tile][3], Tiles[tile][1], Tiles[tile][0] };
	}

	public string GetMoveHistory() => MoveHistory.ToString();

	public int[] GetGoalTiles(int player)
	{
		int startRow = player * (BoardSize - 1) * BoardSize;
		return Enumerable.Range(startRow, BoardSize).ToArray();
	}

	public bool GetWinner(int player) => GetGoalTiles(player).Contains(GetPawnPosition(player));

	public int GetPawnPosition(int index) => PawnPositions[index];

	public int GetBoardSize() => BoardSize;

	#region Movement ---
	#endregion

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
			reachableTiles.AddRange(CheckForEnemy(connectedTile, player));
		}
		return reachableTiles.ToArray();
	}

	public List<int> CheckForEnemy(int connectedTile, int player)
	{
		int enemyPawnPosition = PawnPositions[1 - player];
		// Check if the enemy pawn is not on the tile
		if (enemyPawnPosition != connectedTile)
		{
			return new List<int> { connectedTile };
		}

		// Get the direction of the enemy pawn
		int directionIndex = Array.IndexOf(Tiles[PawnPositions[player]], enemyPawnPosition);

		// Check if the leaped tile is within boundaries
		int[] AdjacentOffsets = new int[4] { -BoardSize, 1, BoardSize, -1 };
		int leapedTileIndex = connectedTile + AdjacentOffsets[directionIndex];

		if (leapedTileIndex < 0 || leapedTileIndex >= Tiles.Length)
		{
			// Return an empty list, as the leaped tile is out of boundaries
			return new List<int>();
		}

		return GetLeapedTiles(PawnPositions[player], leapedTileIndex, connectedTile, directionIndex).ToList();
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

	public void MovePawn(int tileIndex, int currentPlayer, bool isUndo) 
	{ 
		PawnPositions[currentPlayer] = tileIndex;

		if (isUndo) return;
		VisitedTiles[currentPlayer].Add(tileIndex);
	}

	#region Fences ---
	#endregion

	public bool IsFencePlaced(int fence) => PlacedFences[fence] != -1;

	public int GetFenceAmount() => PlacedFences.Length;

	public int GetFenceCount(int index) => FenceCounts[index];

	// Check if the fence at the respective index and direction can be placed
	public bool GetFenceEnabled(int fence, int direction)
	{
		// Return false if the fence is already placed
		if (IsFencePlaced(fence)) return false;

		// Return false if the fence is illegal
		if (IllegalFences[fence] == direction) return false;

		// Horizontal Fence -> Check if the WEST or EAST adjacent fences are placed
		if (direction == 0)
		{
			int westFence = Helper.GetWestAdjacent(fence, BoardSize - 1);
			int eastFence = Helper.GetEastAdjacent(fence, BoardSize - 1);

			if (eastFence != -1 && GetPlacedFence(eastFence) == 0) return false;
			if (westFence != -1 && GetPlacedFence(westFence) == 0) return false;
		}

		// Horizontal Fence -> Check if the NORTH or SOUTH adjacent fences are placed
		if (direction == 1)
		{
			int northFence = Helper.GetNorthAdjacent(fence, BoardSize - 1);
			int southFence = Helper.GetSouthAdjacent(fence, BoardSize - 1);

			if (northFence != -1 && GetPlacedFence(northFence) == 1) return false;
			if (southFence != -1 && GetPlacedFence(southFence) == 1) return false;
		}

		return true;
	}

	public int[] GetTileGrid(int index)
	{
		int topLeft = index;
		int topRight = Helper.GetEastAdjacent(index, BoardSize);
		int bottomLeft = Helper.GetSouthAdjacent(index, BoardSize);
		int bottomRight = Helper.GetSouthAdjacent(topRight, BoardSize);
		return new int[] { topLeft, topRight, bottomLeft, bottomRight };
	}

	public void PlaceFence(int direction, int fenceIndex, int currentPlayer, bool isIFS = false)
	{
		// Set the fence as placed
		PlacedFences[fenceIndex] = direction;

		// Convert the index to a 2D grid index
		int convertedIndex = fenceIndex + (fenceIndex / (BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = GetTileGrid(convertedIndex);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			RemoveTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			RemoveTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}

		// Continue if User or AI is placing the fence
		if (isIFS) return;

		FenceCounts[currentPlayer]--;
	}

	public List<int[]> GetParallelAdjacentFences(int fenceDirection, int[] adjFences, int direction)
	{
		List<int[]> fences = new();

		foreach (int cardinalBit in Helper.Bits)
		{
			int cardinalDirection = (cardinalBit * 2) + fenceDirection;

			// Ignore if the adjacent fence is out of bounds
			if (adjFences[cardinalDirection] == -1) continue;

			// Add adjacent fences of the same direction
			fences.Add(new int[] { adjFences[cardinalDirection], fenceDirection });

			if (fenceDirection != direction) continue;

			// Add adjacent fences of the perpendicular direction
			fences.Add(new int[] { adjFences[cardinalDirection], 1 - direction });
		}

		return fences;
	}

	public List<int[]> GetEnclosingCornerFences(int direction, int[] adjFences, int cardinalOpposites)
	{
		List<int[]> fences = new();

		foreach (int cornerBit in Helper.Bits)
		{
			int cornerDirection = (cornerBit * 2) + (1 - direction);
			int cornerFence = Helper.AdjacentFunctions[cornerDirection](adjFences[cardinalOpposites], BoardSize - 1);

			if (cornerFence == -1) continue;
			fences.Add(new int[] { cornerFence, 1 - direction });
		}

		return fences;
	}

	public List<int[]> GetSurroundingFences(int fenceIndex)
	{
		// Early exit if the fence is not placed
		if (PlacedFences[fenceIndex] == -1) return new();

		List<int[]> surroundingFences = new();
		int direction = PlacedFences[fenceIndex];
		int[] adjFences = Helper.InitialiseConnections(fenceIndex, BoardSize - 1);

		// Add to the surrounding fences the fences which can be both horizontal and vertical
		foreach (int fenceDirection in Helper.Bits)
		{
			surroundingFences.AddRange(GetParallelAdjacentFences(fenceDirection, adjFences, direction));

			int cardinalOpposites = (fenceDirection * 2) + direction;

			// Ignore if the adjacent fence is out of bounds
			if (adjFences[cardinalOpposites] == -1) continue;

			// Get the adjacent fence's adjacent fence
			int leapedFence = Helper.AdjacentFunctions[cardinalOpposites](adjFences[cardinalOpposites], BoardSize - 1);

			// Check if the leaped fence is at a boundary or is a placed fence of the same direction
			if (!(leapedFence == -1 || PlacedFences[leapedFence] == direction)) continue;

			surroundingFences.AddRange(GetEnclosingCornerFences(direction, adjFences, cardinalOpposites));

			// TODO Check leaped fences cardinal Opposites, for wall or placed, if so, add the leaped fence

		}

		return surroundingFences;
	}

	public void RemoveTileConnection(int tile, int tileToRemove)
	{
		int[] connections = Tiles[tile];
		int index = Array.IndexOf(connections, tileToRemove);

		if (index == -1) return;
		connections[index] = -1;
	}

	#region Undo Move ---
	#endregion

	public string UndoMove()
	{
		if (LastMove == null) return "";

		char moveType = LastMove[1];

		switch (moveType)
		{
			case 'm':
				UndoPawnMove();
				break;
			case 'f':
				UndoFenceMove();
				break;
		}

		string returnLastMove = LastMove;

		MoveHistory.Remove(MoveHistory.Length - LastMove.Length - 1, LastMove.Length + 1);
		LastMove = MoveHistory.Length == 0 ? "" : MoveHistory.ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Last();
		return returnLastMove;
	}

	private void UndoPawnMove()
	{
		int player = int.Parse(LastMove[0].ToString());
		int position = int.Parse(LastMove.Split('_')[1]);
		VisitedTiles[player].RemoveAt(VisitedTiles[player].Count - 1);
		MovePawn(position, player, true);
	}

	private void UndoFenceMove()
	{
		// Separate the fence index and direction
		int[] moveCode = Helper.GetMoveCode(LastMove[2..]);

		int direction = moveCode[1];
		int fence = moveCode[0];

		PlacedFences[fence] = -1;

		// Convert the index to a 2D grid index
		int convertedIndex = fence + (fence / (BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = GetTileGrid(convertedIndex);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			AddTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			AddTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}

		int currentPlayer = int.Parse(LastMove[0].ToString());
		FenceCounts[currentPlayer]++;
	}

	private void AddTileConnection(int tile, int tileToAdd)
	{
		int[] connections = Tiles[tile];
		int index = Array.IndexOf(connections, -1);

		if (index == -1) return;
		connections[index] = tileToAdd;
	}

	#region Turn ---
	#endregion

	public void AddMove(string code)
	{
		int currentPlayer = int.Parse(code[0].ToString());
		char moveType = code[1];
		int direction = code[2] == '-' ? 1 : 0;
		int value = int.Parse(code[3..]);

		switch (moveType)
		{
			case 'm':
				code += "_" + GetPawnPosition(currentPlayer);
				MovePawn(value, currentPlayer, false);
				break;
			case 'f':
				PlaceFence(direction, value, currentPlayer);
				break;
		}
		LastMove = code;
		MoveHistory.Append(code + ";");
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

	public string[] GetAllMoves(int currentPlayer)
	{
		StringBuilder allMoves = new();

		// Loop through both directions and add all possible fence placements
		foreach (var direction in Helper.Bits)
		{
			if (FenceCounts[currentPlayer] <= 0) continue;

			for (int i = 0; i < GetFenceAmount(); i++)
			{
				if (!GetFenceEnabled(i, direction)) continue;

				allMoves.Append($"{currentPlayer}f{Helper.GetMoveString(i, direction)};");
			}
		}

		// Add all possible pawn moves
		GetReachableTiles(currentPlayer).ToList()
			.ForEach(index => allMoves.Append($"{currentPlayer}m{Helper.GetMoveString(index, 0)};"));

		return allMoves.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
	}

	public bool IsGameOver() => GetWinner(0) || GetWinner(1);

	// Evaluate the board state, assuming there is no winner
	public int EvaluateBoard(bool isMaximising, int currentPlayer, string lastMove)
	{
		int pathScore, wallScore, progressScore, repetitionPenalty;
		
		int opponent = 1 - currentPlayer;
		
		int playerPos = GetPawnPosition(currentPlayer);
		int opponentPos = GetPawnPosition(opponent);


		var playerShortestPath = Algorithms.GetShortestPath(this, playerPos, new HashSet<int>(GetGoalTiles(currentPlayer)), currentPlayer);
		var opponentShortestPath = Algorithms.GetShortestPath(this, opponentPos, new HashSet<int>(GetGoalTiles(1 - currentPlayer)), 1 - currentPlayer);

		// TODO Only apply repetition penalty if the moveString is MovePawn
		// TODO Only apply wallScore if the moveString is PlaceFence?

		if (isMaximising)
		{
			pathScore = (playerShortestPath.Count - opponentShortestPath.Count) * Helper.PATH_WEIGHT;
			// wallScore = (FenceCounts[currentPlayer] - FenceCounts[opponent]) * Helper.WALL_WEIGHT;
			// repetitionPenalty = VisitedTiles[currentPlayer].ToHashSet().Contains(playerPos) ? Helper.REPETITION_WEIGHT : 0;
		}
		else
		{
			pathScore = (opponentShortestPath.Count - playerShortestPath.Count) * Helper.PATH_WEIGHT;
			// wallScore = (FenceCounts[opponent] - FenceCounts[currentPlayer]) * Helper.WALL_WEIGHT;
			// repetitionPenalty = VisitedTiles[opponent].ToHashSet().Contains(opponentPos) ? Helper.REPETITION_WEIGHT : 0;
		}

		return pathScore; // + wallScore - repetitionPenalty;
	}

}
