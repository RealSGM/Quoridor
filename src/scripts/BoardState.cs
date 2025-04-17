using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[GlobalClass]
public partial class BoardState : Control
{
	[Export] private byte[] PawnPositions { get; set; }
	private Tile[] Tiles { get; set; }
	public Fence[] Fences { get; set; }
	public StringBuilder MoveHistory { get; set; } = new();

	#region Initialisation ---

	public BoardState Clone() => new()
	{
		Fences = Fences.Clone() as Fence[],
		PawnPositions = PawnPositions.Clone() as byte[],
		Tiles = [.. Tiles.Select(tile => tile.Clone())],
		MoveHistory = new StringBuilder(MoveHistory.ToString()),
	};

	public void InitialiseBoard()
	{
		Fences = [.. Enumerable.Range(0, (Helper.BoardSize - 1) * (Helper.BoardSize - 1)).Select(_ => new Fence())];
		Tiles = [.. Enumerable.Range(0, Helper.BoardSize * Helper.BoardSize).Select(index => new Tile(index))];
		PawnPositions = [(byte)(Helper.BoardSize * (Helper.BoardSize - 0.5)), (byte)(Helper.BoardSize / 2)];
	}

	#endregion

	#region Godot Functions ---

	public bool IsFencePlaced(int fenceIndex) => Fences[fenceIndex].IsPlaced();

	public int[] GetTileConnections(int tileIndex) => Tiles[tileIndex].GetConnections();

	public void PrintAllMoves(int currentPlayer)
	{
		// GetTileAdjacentFences(currentPlayer);
		// GD.Print(string.Join(",", PawnPositions));
		// Store the rows each pawn position is in
		// foreach (int pawnPosition in PawnPositions)
		// {
		// 	int row = pawnPosition / Helper.BoardSize;
		// 	GD.Print($"Pawn {pawnPosition} is in row {row}");
		// }
		// Sum up the number of fences in each row
		// Sum up the number of fences in each row
		GD.Print($"State Key: {GetStateKey()}");
	}

	#endregion

	#region Getters ---

	public Fence[] GetFences() => Fences;

	public Tile[] GetTiles() => Tiles;

	public string GetMoveHistory() => MoveHistory.ToString();

	public Tile GetTile(int tileIndex) => Tiles[tileIndex];

	public int GetPawnPosition(int index) => PawnPositions[index];

	public string GetLastMove() => MoveHistory.Length == 0 ? "" : MoveHistory.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries)[^1];

	/// Returns all placed fences
	public int[] GetPlacedFences() => [.. Fences.Select((fence, index) => fence.IsPlaced() ? index : -1).Where(index => index != -1)];

	public string GetStateKey()
	{
		StringBuilder stateKey = new();

		// Store the rows each pawn position is in
		stateKey.Append(string.Join("", PawnPositions.Select(pawnPosition => $"p{pawnPosition / Helper.BoardSize}")));

		// Sum up the number of fences in each row, ensuring 0s are maintained
		var rowCounts = Enumerable.Range(0, Helper.BoardSize - 1)
			.Select(row => Fences
				.Select((fence, index) => fence.IsPlaced() ? index / (Helper.BoardSize - 1) : -1)
				.Count(fenceRow => fenceRow == row))
			.ToList();

		stateKey.Append($"f{string.Join("", rowCounts)}");

		return stateKey.ToString();
	}

	#endregion

	#region Player Moves ---

	public void ShiftPawn(int tileIndex, int currentPlayer) => PawnPositions[currentPlayer] = (byte)tileIndex;

	public void RemoveTileConnection(int tileIndex, int tileToRemove)
	{
		int[] connections = Tiles[tileIndex].GetConnections();
		int index = Array.IndexOf(connections, tileToRemove);
		if (index != -1) connections[index] = -1;
	}

	public void PlaceFence(int direction, int fenceIndex, int currentPlayer)
	{
		// Set the fence as placed
		Fences[fenceIndex].SetPlaced((sbyte)currentPlayer);
		Fences[fenceIndex].SetDirection((sbyte)direction);

		// Convert the index to a 2D grid index
		int convertedIndex = fenceIndex + (fenceIndex / (Helper.BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = Helper.GetTileGrid(convertedIndex, Helper.BoardSize);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			RemoveTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			RemoveTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}
	}

	public void AddMove(string code)
	{
		var (currentPlayer, moveType, direction, index, _) = Helper.GetMoveCodeAsTuple(code);

		switch (moveType)
		{
			case "m":
				ShiftPawn(index, currentPlayer);
				break;
			case "f":
				PlaceFence(direction, index, currentPlayer);
				break;
		}
		MoveHistory.Append(code + ";");
	}

	#endregion

	#region Undo Move ---

	private string UndoMove()
	{
		string LastMove = GetLastMove();
		var (_, moveType, _, _, _) = Helper.GetMoveCodeAsTuple(LastMove);

		if (LastMove == "") return "";

		switch (moveType)
		{
			case "m":
				UndoShiftPawn();
				break;
			case "f":
				UndoFenceMove();
				break;
		}

		string returnLastMove = LastMove;

		MoveHistory.Remove(MoveHistory.Length - LastMove.Length - 1, LastMove.Length + 1);
		return returnLastMove;
	}

	private void UndoShiftPawn()
	{
		string LastMove = GetLastMove();
		var (player, _, _, _, newPosition) = Helper.GetMoveCodeAsTuple(LastMove);
		ShiftPawn(newPosition, player);
	}

	private void AddTileConnection(int tileIndex, int tileToAdd)
	{
		Tile tile = Tiles[tileIndex];
		int[] connections = tile.GetConnections();
		int index = Array.IndexOf(connections, -1);

		if (index != -1) connections[index] = tileToAdd;
	}

	private void UndoFenceMove()
	{
		string LastMove = GetLastMove();
		var (_, _, direction, index, _) = Helper.GetMoveCodeAsTuple(LastMove);

		Fences[index] = new Fence();

		// Convert the index to a 2D grid index
		int convertedIndex = index + (index / (Helper.BoardSize - 1));
		// Get possible tile indexes in 2x2 grid
		int[] tileGrid = Helper.GetTileGrid(convertedIndex, Helper.BoardSize);

		foreach (int[] pair in Helper.DefaultTileGridConnections[direction])
		{
			AddTileConnection(tileGrid[pair[0]], tileGrid[pair[1]]);
			AddTileConnection(tileGrid[pair[1]], tileGrid[pair[0]]);
		}
	}

	#endregion

	#region Tiles ---

	/// Enemy is on an adjacent tile to the Player
	/// Check if the leaped tile is not blocked by a fence
	public int[] GetLeapedTiles(int playerPawnPosition, int enemyPawnPosition, int leapedTileIndex)
	{
		Tile enemyTile = Tiles[enemyPawnPosition];
		int[] enemyTileConnections = enemyTile.GetConnections();

 		// Return all valid surrounding tiles of the enemy pawn, ignoring the player pawn
		if (!enemyTileConnections.Contains(leapedTileIndex)) return [.. enemyTileConnections.Where(tile => tile != -1 && tile != playerPawnPosition)];

		// Leaped tile is not blocked / is valid
		return [leapedTileIndex];
	}

	/// Check if the enemy pawn is on the tile
	/// If the enemy is not the pawn, return the tile
	/// If the enemy is not on the tile check for leaped tiles
	public int[] CheckForEnemy(int connectedTile, int player)
	{
		int enemyPawnPosition = PawnPositions[1 - player];
		int playerPawnPosition = PawnPositions[player];

		// Enemy pawn IS NOT on the tile
		if (enemyPawnPosition != connectedTile) return [connectedTile];

		// Enemy pawn IS on the tile
		Tile playerPawnTile = Tiles[playerPawnPosition];
		int[] playerPawnConnections = playerPawnTile.GetConnections();

		// Get the direction of the enemy pawn
		int directionIndex = Array.IndexOf(playerPawnConnections, enemyPawnPosition);

		// Calculate the leaped tile index of the enemy pawn
		int leapedTileIndex = Helper.AdjacentFunctions[directionIndex](enemyPawnPosition, Helper.BoardSize);

		return GetLeapedTiles(playerPawnPosition, enemyPawnPosition, leapedTileIndex);
	}

	/// Loop through connections
	/// Check if the tile is not blocked by a fence
	/// Check if the tile is not occupied by the enemy pawn
	public int[] GetReachableTiles(int player)
	{
		int playerPawnPosition = PawnPositions[player];
		Tile playerPawnTile = Tiles[playerPawnPosition];

		return [.. playerPawnTile.GetConnections()
			.Where(connectedTile => connectedTile != -1)
			.SelectMany(connectedTile => CheckForEnemy(connectedTile, player))];
	}

	#endregion

	#region Fences ---

	public int GetFenceCount(int player) => Fences.Count(fence => fence.GetPlacedBy() == player);

	/// Check if the fence at the respective index and direction can be placed
	/// Check if the adjacent fences have been placed in the same direction
	public bool IsFenceEnabled(int fence, int direction)
	{
		if (fence >= Fences.Length) return false;

		if (!Fences[fence].IsFencePlaceable(direction)) return false;

		// Return false if the fence is illegal
		if (Fences[fence].GetIllegal(direction)) return false;

		foreach (int bit in Helper.Bits)
		{
			int polarDirection = 2 * bit + (1 - direction);

			int adjfence = Helper.AdjacentFunctions[polarDirection](fence, Helper.BoardSize - 1);
			if (adjfence != -1 && Fences[adjfence].GetDirection() == direction) return false;
		}
		return true;
	}

	#endregion

	#region Get Moves ---

	public List<string> GetAllSurroundingFences(int fenceIndex)
	{
		List<string> surroundingFences = [];

		// Get the fences that extend in the same direction
		// Get the direction of the fence, and add the adjcent fences that align
		int direction = Fences[fenceIndex].GetDirection();
		int oppositeDirection = 1 - direction;
		int[] adjFences = [.. Helper.InitialiseConnections(fenceIndex, Helper.BoardSize - 1)];

		// Leaped alligned fences
		foreach (int bit in Helper.Bits)
		{
			// Get the adjacent fence
			int adjIndex = (2 * bit) + oppositeDirection;
			int adjFence = adjFences[adjIndex];

			// Ignore if out of bounds
			if (adjFence == -1) continue;

			// Get the leaped adjacent fence
			int leapedAdjFence = Helper.AdjacentFunctions[adjIndex](adjFence, Helper.BoardSize - 1);

			// Ignore if out of bounds
			if (leapedAdjFence == -1) continue;

			// Ignore not enabled fences
			if (!IsFenceEnabled(leapedAdjFence, direction)) continue;

			// Convert fence index to mapped index using direction
			surroundingFences.Add(Helper.GetMappedIndex(leapedAdjFence, direction));
		}

		// Perpundicular adjacent fences
		foreach (int adjFence in adjFences)
		{
			// Ignore if out of bounds
			if (adjFence == -1) continue;

			// Ignore not enabled fences
			if (!IsFenceEnabled(adjFence, oppositeDirection)) continue;

			// Convert fence index to mapped index using direction
			surroundingFences.Add(Helper.GetMappedIndex(adjFence, oppositeDirection));
		}

		// Perpendicular corner fences
		int[] cornerFences = [.. Helper.InitialiseCornerConnections(fenceIndex, Helper.BoardSize - 1)
			.Where(fence => fence >= 0)];

		surroundingFences.AddRange(cornerFences
			.Where(cornerFence => IsFenceEnabled(cornerFence, oppositeDirection))
			.Select(cornerFence => Helper.GetMappedIndex(cornerFence, oppositeDirection)));

		return [.. surroundingFences.Distinct()];
	}

	public Dictionary<string, float> GetReachableTilesWeighted(int currentPlayer)
	{
		// Stores all possible moves with their weights
		Dictionary<string, float> allMoves = [];

		int[] playerShortestPath = Algorithms.GetShortestPath(currentPlayer, this);
		int[] playerReachableTiles = GetReachableTiles(currentPlayer);
		int playerPawnPosition = PawnPositions[currentPlayer];

		// Add best moves which are reachable
		foreach (int tileIndex in playerShortestPath.Intersect(playerReachableTiles))
		{
			allMoves[Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tileIndex, playerPawnPosition)] = 10;
		}

		// Add reachable tiles to the dictionary, if no best move was found
		if (allMoves.Count == 0)
		{
			// Add all reachable tiles to the dictionary
			foreach (int tileIndex in playerReachableTiles)
			{
				allMoves[Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tileIndex, playerPawnPosition)] = 1;
			}
		}

		return allMoves;
	}

	public List<string> GetTileAdjacentFences(int player)
	{
		int playerPawnPosition = PawnPositions[player];
		int[] connections = Tiles[playerPawnPosition].GetConnections();

		int[] fenceCorners = [
			Helper.GetFenceCorner(connections[0], 0, 0),
			Helper.GetFenceCorner(connections[0], 0, 1),
			Helper.GetFenceCorner(connections[2], -1, 2),
			Helper.GetFenceCorner(connections[2], -1, 3),
		];

		List<string> allMoves = [.. fenceCorners
			.Where(index => index != -1)
			.SelectMany(index => Helper.Bits
			.Select(direction => Helper.GetMappedIndex(index, direction)))];

		return allMoves;
	}

	public Dictionary<string, float> GetFenceMovesWeighted(int currentPlayer)
	{
		// Stores all possible moves with their weights
		Dictionary<string, float> allMoves = [];

		if (GetFenceCount(currentPlayer) >= Helper.MaxFences) return allMoves;

		// Get all surrounding fences as a string mappedIndexes
		List<string> surroundingFences = [.. GetPlacedFences()
			.SelectMany(GetAllSurroundingFences)
			.Where(fence => fence != "")];

		// Add horizontal fences which are behind the player
		foreach (int direction in Helper.Bits)
		{
			surroundingFences.AddRange(
				Enumerable.Range(0, (Helper.BoardSize - 1) * (Helper.BoardSize - 1))
					.Where(i => IsFenceEnabled(i, direction))
					.Select(i => Helper.GetMappedIndex(i, direction))
					.Where(mappedIndex => !surroundingFences.Contains(mappedIndex))
			);
		}

		// Add fences that surround the enemy
		surroundingFences.AddRange(GetTileAdjacentFences(1 - currentPlayer));
		surroundingFences = [.. surroundingFences.Where(fenceIndex => IsFenceEnabled(int.Parse(fenceIndex[1..]), fenceIndex[0].ToString() == "+" ? 0 : 1))];


		foreach (string fenceIndex in surroundingFences)
		{
			int direction = fenceIndex[0].ToString() == "+" ? 0 : 1;
			int index = int.Parse(fenceIndex[1..]);

			if (!IsFenceEnabled(index, direction)) continue;

			float relativePlayerIndex = PawnPositions[currentPlayer] / Helper.BoardSize + 0.5f;
			float relativeFenceIndex = index / (Helper.BoardSize - 1) + 1;

			if (currentPlayer == 0 && relativeFenceIndex < relativePlayerIndex) continue;
			if (currentPlayer == 1 && relativeFenceIndex > relativePlayerIndex) continue;

			string moveCode = Helper.GetMoveCodeAsString(currentPlayer, "f", direction, index);

			// If the key already exists, we don't add it again, preventing duplicate keys
			if (allMoves.ContainsKey(moveCode)) continue;

			// Add the fence to the dictionary
			allMoves[moveCode] = 5;
		}

		// Loop through all fence moves and make sure the fence is placeable
		allMoves = allMoves
			.Where(pair =>
			{
				string moveCode = pair.Key;
				int index = int.Parse(moveCode[3..]);
				int direction = moveCode[2] == '-' ? 1 : 0;
				return IsFenceEnabled(index, direction);
			})
			.ToDictionary(pair => pair.Key, pair => pair.Value);

		return allMoves;
	}

	public Dictionary<string, float> GetAllMovesWeighted(int currentPlayer)
	{
		Dictionary<string, float> allMoves = [];

		allMoves = GetReachableTilesWeighted(currentPlayer)
			.ToDictionary(x => x.Key, x => x.Value);

		allMoves = allMoves
			.Concat(GetFenceMovesWeighted(currentPlayer))
			.ToDictionary(x => x.Key, x => x.Value);

		return allMoves;
	}

	public string[] GetAllFences(int currentPlayer)
	{
		List<string> allFences = [];

		if (GetFenceCount(currentPlayer) >= Helper.MaxFences) return [.. allFences];

		foreach (int direction in Helper.Bits)
		{
			for (int i = 0; i < Fences.Length; i++)
			{
				if (!IsFenceEnabled(i, direction)) continue;
				allFences.Add(Helper.GetMoveCodeAsString(currentPlayer, "f", direction, i));
			}
		}

		return [.. allFences.Distinct()];
	}

	public string[] GetAllMoves(int currentPlayer)
	{
		List<string> allMoves = [];

		allMoves.AddRange(GetReachableTiles(currentPlayer)
			.Select(tileIndex => Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tileIndex)));

		allMoves.AddRange(GetAllFences(currentPlayer));

		return [.. allMoves.Distinct()];
	}

	public string[] GetAllMovesSmart(int currentPlayer)
	{
		List<string> allMoves = [.. GetReachableTilesWeighted(currentPlayer).Keys];

		allMoves.AddRange(GetAllFences(currentPlayer));

		return [.. allMoves.Distinct()];
	}

	#endregion

	#region  Evaluation ---

	public bool IsWinner(int player) => Helper.GetGoalTiles(player).Contains(GetPawnPosition(player));

	public bool IsGameOver() => IsWinner(0) || IsWinner(1);

	public int GetGameResult(int simulatingPlayer)
	{
		if (IsWinner(simulatingPlayer)) return int.MaxValue;
		if (IsWinner(1 - simulatingPlayer)) return int.MinValue;
		return 0;
	}

	/// Evaluate the board state
	public int EvaluateBoard(int maximisingPlayer)
	{
		int minimisingPlayer = 1 - maximisingPlayer;

		int maximisingPlayerPath = Algorithms.GetShortestPath(maximisingPlayer, this).Length;
		int minimisingPlayerPath = Algorithms.GetShortestPath(minimisingPlayer, this).Length;
		int pathDifference = minimisingPlayerPath - maximisingPlayerPath;
		int fenceScore = GetFenceCount(minimisingPlayer) - GetFenceCount(maximisingPlayer);

		int evaluation = 0;

		evaluation += pathDifference * Helper.PATH_WEIGHT;
		evaluation += fenceScore * Helper.FENCE_WEIGHT;

		return evaluation;
	}

	#endregion
}
