using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[GlobalClass]
public partial class BoardState : Control
{
	[Export] private byte[] PawnPositions { get; set; }

	public FenceData[] PlacedFences { get; set; }
	private int[][] Tiles { get; set; }
	private bool[][] IllegalFences { get; set; }

	public int BoardSize { get; set; }
	public StringBuilder MoveHistory { get; set; } = new();

	#region Initialisation ---
	#endregion

	public BoardState Clone() => new()
	{
		PlacedFences = PlacedFences.Clone() as FenceData[],
		PawnPositions = PawnPositions.Clone() as byte[],
		Tiles = [.. Tiles.Select(tile => tile.Clone() as int[])],
		IllegalFences = [.. IllegalFences.Select(fence => fence.Clone() as bool[])],
		BoardSize = BoardSize,
		MoveHistory = new StringBuilder(MoveHistory.ToString()),
	};

	public void InitialiseBoard(int boardSize, int fencesPerPlayer)
	{
		BoardSize = boardSize;
		InitialisePawnPositions();
		InitialiseTiles();
		InitialiseFences();
		InitialiseIllegalFences();
	}

	private void InitialiseFences() 
	{
		PlacedFences = [.. Enumerable.Range(0, (BoardSize - 1) * (BoardSize - 1)).Select(_ => new FenceData())];
	}

	public void InitialisePawnPositions()
	{
		PawnPositions = new byte[Helper.PlayerCount];
		PawnPositions[0] = (byte)(BoardSize * (BoardSize - 0.5));
		PawnPositions[1] = (byte)(BoardSize / 2);
	}

	public void InitialiseTiles()
	{
		Tiles = [.. Enumerable.Range(0, BoardSize * BoardSize).Select(index => Helper.InitialiseConnections(index, BoardSize))];
	}

	public void InitialiseIllegalFences()
	{
		IllegalFences = [.. Enumerable.Range(0, (BoardSize - 1) * (BoardSize - 1)).Select(_ => new bool[2] { false, false })];
	}

	#region Setters ---
	#endregion

	public void SetIllegalFence(int fence, int direction, bool value) => IllegalFences[fence][direction] = value;

	#region Getters ---
	#endregion

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
		return [.. Enumerable.Range(startRow, BoardSize)];
	}

	public bool GetWinner(int player) => GetGoalTiles(player).Contains(GetPawnPosition(player));

	public int GetPawnPosition(int index) => PawnPositions[index];

	public int GetBoardSize() => BoardSize;

	public bool[][] GetIllegalFences() => IllegalFences;

	public string GetLastMove()
	{
		if (MoveHistory.Length == 0) return "";
		return MoveHistory.ToString().Split([';'], StringSplitOptions.RemoveEmptyEntries).Last();
	}

	public bool IsFencePlaced(int fenceIndex) => PlacedFences[fenceIndex].IsPlaced();

	// Returns all fences that have been placed
	public FenceData[] GetPlacedFences() => [.. PlacedFences.Where(fence => fence.IsPlaced())];

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

	public void MovePawn(int tileIndex, int currentPlayer) => PawnPositions[currentPlayer] = (byte)tileIndex;

	#region Fences ---
	#endregion

	public int GetFenceAmount() => PlacedFences.Length;

	public int GetFenceCount(int player) => PlacedFences.Count(fence => fence.GetPlacedBy() == player);

	// Check if the fence at the respective index and direction can be placed
	// Check if the adjacent fences have been placed in the same direction
	public bool GetFenceEnabled(int fence, int direction)
	{
		// Return false if the fence is already placed
		if (!PlacedFences[fence].IsFencePlaceable(direction)) return false;

		// Return false if the fence is illegal
		if (IllegalFences[fence][direction]) return false;

		foreach (int bit in Helper.Bits)
		{
			// bit = 0, 1
			// polarDirection = 0, 2 if direction = 1
			// polarDirection = 1, 3 if direction = 0
			int polarDirection = 2 * bit + (1 - direction);

			int adjfence = Helper.AdjacentFunctions[polarDirection](fence, BoardSize - 1);
			if (adjfence != -1 && PlacedFences[adjfence].GetDirection() == direction) return false;
		}
		return true;
	}

	public void PlaceFence(int direction, int fenceIndex, int currentPlayer)
	{
		// Set the fence as placed
		PlacedFences[fenceIndex].SetPlaced((sbyte)currentPlayer);
		PlacedFences[fenceIndex].SetDirection((sbyte)direction);

		// Convert the index to a 2D grid index
		int convertedIndex = fenceIndex + (fenceIndex / (BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = Helper.GetTileGrid(convertedIndex, BoardSize);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			RemoveTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			RemoveTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}
	}

	public List<int> GetAllSurroundingFences(int fenceIndex)
	{
		List<int> surroundingFences = [];

		int[] adjFences = [.. Helper.InitialiseConnections(fenceIndex, BoardSize - 1).Where(fence => fence >= 0)];

		int[] cornerFences = [.. Helper.InitialiseCornerConnections(fenceIndex, BoardSize - 1).Where(fence => fence >= 0)];

		// Add every adjacent fence and corner fence in both directions
		foreach (int fenceDirection in Helper.Bits)
		{
			int mappedFenceDirection = fenceDirection == 0 ? 1 : -1;

			surroundingFences.AddRange(adjFences.Select(fence => mappedFenceDirection * fence));
			surroundingFences.AddRange(cornerFences.Select(fence => mappedFenceDirection * fence));
		}

		return [.. surroundingFences.Distinct()];
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

		PlacedFences[fence] = new FenceData();

		// Convert the index to a 2D grid index
		int convertedIndex = fence + (fence / (BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = Helper.GetTileGrid(convertedIndex, BoardSize);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			AddTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			AddTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}
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
			if (GetFenceCount(currentPlayer) <= 0) continue;

			for (int i = 0; i < GetFenceAmount(); i++)
			{
				if (!GetFenceEnabled(i, direction)) continue;

				allMoves.Append($"{currentPlayer}f{Helper.GetMoveString(i, direction)};");
			}
		}

		// Add all possible pawn moves
		GetReachableTiles(currentPlayer).ToList()
			.ForEach(index => allMoves.Append($"{currentPlayer}m{Helper.GetMoveString(index, 0)};"));

		return [.. allMoves.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries).Distinct()];
	}

	#region  Evaluation ---
	#endregion

	public bool IsGameOver() => GetWinner(0) || GetWinner(1);

	public int[] GetShortestPath(int player)
	{
		Queue<int> queue = new();
		Dictionary<int, int> previous = [];
		HashSet<int> visited = [];
		HashSet<int> goalTiles = [.. GetGoalTiles(player)];

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

	public int EvaluateBoard(bool isMaximising = true)
	{
		string lastMove = GetLastMove();
		int currentPlayer = int.Parse(lastMove[0].ToString());

		// Calculate the score, relative to the current player, where negative is not in favour of the player
		int playerPath = GetShortestPath(currentPlayer).Length;
		int opponentPath = GetShortestPath(1 - currentPlayer).Length;
		int evaluation = (opponentPath - playerPath) * Helper.PATH_WEIGHT;

		// Add the different in number of fences remaining
		evaluation += (GetFenceCount(currentPlayer) - GetFenceCount(1 - currentPlayer)) * Helper.FENCE_WEIGHT;

		// Add +- 1 random value to the evaluation score to avoid ties
		evaluation += Helper.Random.Next(-1, 2);

		return isMaximising ? evaluation : -evaluation;
	}

	public int GetGameResult(int simulatingPlayer)
	{
		if (GetWinner(simulatingPlayer)) return int.MaxValue;
		if (GetWinner(1 - simulatingPlayer)) return int.MinValue;
		return 0;
	}
}
