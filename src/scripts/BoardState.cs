using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[GlobalClass]
public partial class BoardState : Control
{
	[Export] private int[] PlacedFences { get; set; } // Array of placed fences, index = fence, value = direction => [-1 = Empty, 0 = Horizontally Placed, 1 = Vertically Placed]
	[Export] private int[] PawnPositions { get; set; }
	[Export] private int[] FenceCounts { get; set; }

	private int[][] Tiles { get; set; }
	private bool[][] IllegalFences { get; set; }
	private int[] EvaluationScores { get; set; }

	public int BoardSize { get; set; }
	public StringBuilder MoveHistory { get; set; } = new();


    #region Initialisation ---
    #endregion

    public BoardState Clone() => new()
	{
		PlacedFences = PlacedFences.Clone() as int[],
		PawnPositions = PawnPositions.Clone() as int[],
		FenceCounts = FenceCounts.Clone() as int[],
		Tiles = Tiles.Select(tile => tile.Clone() as int[]).ToArray(),
		IllegalFences = IllegalFences.Select(fence => fence.Clone() as bool[]).ToArray(),
		EvaluationScores = EvaluationScores.Clone() as int[],
		BoardSize = BoardSize,
		MoveHistory = new StringBuilder(MoveHistory.ToString()),
	};

	public void InitialiseBoard(int boardSize, int fencesPerPlayer)
	{
		BoardSize = boardSize;
		FenceCounts = Enumerable.Repeat(fencesPerPlayer, Helper.PlayerCount).ToArray();
		EvaluationScores = new int[Helper.PlayerCount];
		Array.Fill(EvaluationScores, 0);
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

	public void InitialiseIllegalFences()
	{
		IllegalFences = Enumerable.Range(0, (BoardSize - 1) * (BoardSize - 1))
			.Select(_ => Helper.emptyFences)
			.ToArray();
	}

	#region Setters ---
	#endregion

	public void SetIllegalFence(int fence, int direction, bool value) => IllegalFences[fence][direction] = value;

	#region Getters ---
	#endregion

	public int[] GetPlacedFences() => PlacedFences;

	public int GetPlacedFence(int fence) => PlacedFences[fence];

	public int[] GetTileConnections(int tile) => Tiles[tile];

	public int[] GetPathConnections(int tile, int player)
	{
		// Return NEWS connections for the bottom player
		if (player == 0) return [Tiles[tile][0], Tiles[tile][1], Tiles[tile][3], Tiles[tile][2]];
		// Return SNEW connections for the top player
		else return [Tiles[tile][2], Tiles[tile][3], Tiles[tile][1], Tiles[tile][0]];
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

	public string GetIllegalFencesString()
	{
		StringBuilder illegalFences = new();

		for (int i = 0; i < IllegalFences.Length; i++)
		{
			for (int j = 0; j < IllegalFences[i].Length; j++)
			{
				illegalFences.Append($"Index: {i}, Direction: {j} = {IllegalFences[i][j]}\n");
			}
		}

		return illegalFences.ToString();
	}

	public string GetLastMove()
	{
		if (MoveHistory.Length == 0) return "";
		return MoveHistory.ToString().Split([';'], StringSplitOptions.RemoveEmptyEntries).Last();
	}

	#region Movement ---
	#endregion

	public int[] GetReachableTiles(int player)
	{
		int playerPawnPosition = PawnPositions[player];
		int[] playerPawnTile = Tiles[playerPawnPosition];
		List<int> reachableTiles = [];

		// Loop through all tiles
		foreach (int connectedTile in playerPawnTile)
		{
			// Check if the tile is not empty
			if (connectedTile == -1) continue;

			// CheckFor Enemy and merge list
			reachableTiles.AddRange(CheckForEnemy(connectedTile, player));
		}
		return [.. reachableTiles];
	}

	public List<int> CheckForEnemy(int connectedTile, int player)
	{
		int enemyPawnPosition = PawnPositions[1 - player];

		// Check if the enemy pawn is not on the tile
		if (enemyPawnPosition != connectedTile) return [connectedTile];

		// Get the direction of the enemy pawn
		int directionIndex = Array.IndexOf(Tiles[PawnPositions[player]], enemyPawnPosition);

		// Check if the leaped tile is within boundaries
		int[] AdjacentOffsets = [-BoardSize, 1, BoardSize, -1];
		int leapedTileIndex = connectedTile + AdjacentOffsets[directionIndex];

		// Return an empty list, as the leaped tile is out of boundaries
		if (leapedTileIndex < 0 || leapedTileIndex >= Tiles.Length) return [];

		return [.. GetLeapedTiles(PawnPositions[player], leapedTileIndex, connectedTile, directionIndex)];
	}

	public int[] GetLeapedTiles(int playerIndex, int leapedTileIndex, int tileIndex, int dirIndex)
	{
		int[] leapedTile = Tiles[leapedTileIndex];

		// Check no fence blocking it
		if (Tiles[tileIndex][dirIndex] == leapedTileIndex) return [leapedTileIndex];

		// Fence blocking it, check adjacent tiles over the enemy pawn
		var leapedTiles = Tiles[tileIndex]
			.Where(tile => tile != -1 && tile != playerIndex)
			.ToArray();

		return leapedTiles;
	}

	public void MovePawn(int tileIndex, int currentPlayer) => PawnPositions[currentPlayer] = tileIndex;

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
		if (IllegalFences[fence][direction]) return false;

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

	public void PlaceFence(int direction, int fenceIndex, int currentPlayer)
	{
		// Set the fence as placed
		PlacedFences[fenceIndex] = direction;

		// Convert the index to a 2D grid index
		int convertedIndex = fenceIndex + (fenceIndex / (BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = Helper.GetTileGrid(convertedIndex, BoardSize);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			RemoveTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			RemoveTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}

		FenceCounts[currentPlayer]--;
	}

	public List<int> GetAllSurroundingFences(int fenceIndex)
	{
		List<int> surroundingFences = [];

		int[] adjFences = Helper.InitialiseConnections(fenceIndex, BoardSize - 1)
			.Where(fence => fence >= 0)
			.ToArray();

		int[] cornerFences = Helper.InitialiseCornerConnections(fenceIndex, BoardSize - 1)
			.Where(fence => fence >= 0)
			.ToArray();

		// Add every adjacent fence and corner fence in both directions
		foreach (int fenceDirection in Helper.Bits)
		{
			int mappedFenceDirection = fenceDirection == 0 ? 1 : -1;

			foreach (int fence in adjFences)
			{
				surroundingFences.Add(mappedFenceDirection * fence);
			}

			foreach (int fence in cornerFences)
			{
				surroundingFences.Add(mappedFenceDirection * fence);
			}
		}

		return surroundingFences.Distinct().ToList();
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
		string LastMove = GetLastMove();
		if (LastMove == "") return "";

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
		return returnLastMove;
	}

	private void UndoPawnMove()
	{
		string LastMove = GetLastMove();
		int player = int.Parse(LastMove[0].ToString());
		int position = int.Parse(LastMove.Split('_')[1]);
		MovePawn(position, player);
	}

	private void UndoFenceMove()
	{
		string LastMove = GetLastMove();
		// Separate the fence index and direction
		int[] moveCode = Helper.GetMoveCode(LastMove[2..]);

		int direction = moveCode[1];
		int fence = moveCode[0];

		PlacedFences[fence] = -1;

		// Convert the index to a 2D grid index
		int convertedIndex = fence + (fence / (BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = Helper.GetTileGrid(convertedIndex, BoardSize);

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
				MovePawn(value, currentPlayer);
				break;
			case 'f':
				PlaceFence(direction, value, currentPlayer);
				break;
		}

		MoveHistory.Append(code + ";");
		EvaluateBoard();
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

		GD.Print(allMoves.ToString());

		return allMoves.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
	}

	#region  Evaluation ---
	#endregion

	public bool IsGameOver() => GetWinner(0) || GetWinner(1);

	public int[] GetShortestPath(int player)
	{
		Queue<int> queue = new();
		Dictionary<int, int> previous = [];
		HashSet<int> visited = [];
		HashSet<int> goalTiles = new(GetGoalTiles(player));

		queue.Enqueue(GetPawnPosition(player));

		while (queue.Count > 0)
		{
			int current = queue.Dequeue();

			if (goalTiles.Contains(current))
			{
				List<int> path = [current];

				while (previous.ContainsKey(current))
				{
					current = previous[current];
					path.Add(current);
				}

				path.Reverse();
				return [.. path];
			}

			if (visited.Contains(current)) continue;

			visited.Add(current);

			foreach (int connectedTile in GetPathConnections(current, player))
			{
				if (visited.Contains(connectedTile) || connectedTile == -1) continue;
				queue.Enqueue(connectedTile);
				previous[connectedTile] = current;
			}
		}

		return [];
	}

	public int EvaluateBoard(bool isMaximising = false)
	{
		string lastMove = GetLastMove();
		int currentPlayer = int.Parse(lastMove[0].ToString());

		int playerPath = GetShortestPath(currentPlayer).Length;
		int opponentPath = GetShortestPath(1 - currentPlayer).Length;
		int evaluation = (opponentPath - playerPath) * Helper.PATH_WEIGHT;

		// Add the different in number of fences remaining
		evaluation += (FenceCounts[currentPlayer] - FenceCounts[1 - currentPlayer]) * Helper.FENCE_WEIGHT;

		// If this move decreases the player's path length, decrease the evaluation score
		if (playerPath > EvaluationScores[currentPlayer])
		{
			evaluation -= 100;
		}

		// If this move increases the opponent's path length, increase the evaluation score
		if (opponentPath > EvaluationScores[1 - currentPlayer])
		{
			evaluation += 100;
		}

		EvaluationScores[currentPlayer] = playerPath;
		EvaluationScores[1 - currentPlayer] = opponentPath;

		return isMaximising ? evaluation : -evaluation;
	}
}
