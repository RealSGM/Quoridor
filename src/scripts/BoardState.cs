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

	public void PrintAllMoves()
	{
		// foreach (var move in GetAllMovesWeighted(1))
		// {
		// 	GD.Print($"{move.Key} : {move.Value}");
		// }
		// var foo = GetPlacedFences();

		// for (int i = 0; i < foo.Length; i++)
		// {
		// 	int fenceIndex = foo[i];
		// 	GD.Print($"Fence {fenceIndex}: Placer: {Fences[fenceIndex].GetPlacedBy()}");
		// 	var bar = GetAllSurroundingFences(fenceIndex);
		// 	GD.Print($"Surrounding fences: {string.Join(", ", bar)}");
		// }
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
	public bool GetFenceEnabled(int fence, int direction)
	{
		// Return false if the fence is already placed
		if (!Fences[fence].IsFencePlaceable(direction)) return false;

		// Return false if the fence is illegal
		if (Fences[fence].GetIllegal(direction)) return false;

		foreach (int bit in Helper.Bits)
		{
			// bit = 0, 1
			// polarDirection = 0, 2 if direction = 1
			// polarDirection = 1, 3 if direction = 0
			int polarDirection = 2 * bit + (1 - direction);

			int adjfence = Helper.AdjacentFunctions[polarDirection](fence, Helper.BoardSize - 1);
			if (adjfence != -1 && Fences[adjfence].GetDirection() == direction) return false;
		}
		return true;
	}

	#endregion
	
	#region Get Moves ---
	
	public Dictionary<string, float> GetReachableTilesWeighted(int currentPlayer)
	{
		// Stores all possible moves with their weights
		Dictionary<string, float> allMoves = [];

		int[] playerShortestPath = Algorithms.GetShortestPath(currentPlayer, this);
		int[] playerReachableTiles = GetReachableTiles(currentPlayer);
		int playerPawnPosition = PawnPositions[currentPlayer];

		// Scale weight based on how far the player is from the goal
		int weight = playerShortestPath.Length - 1;
		

		// Add best moves to the dictionary
		for (int i = 0; i < playerShortestPath.Length; i++)
		{
			int tileIndex = playerShortestPath[i];

			if (!playerReachableTiles.Contains(tileIndex)) continue;

			allMoves[Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tileIndex, playerPawnPosition)] = weight;
		}

		return allMoves;
	}

	public Dictionary<string, float> GetFenceMovesWeighted(int currentPlayer)
	{
		// Stores all possible moves with their weights
		Dictionary<string, float> allMoves = [];

		if (GetFenceCount(currentPlayer) >= Helper.MaxFences) return allMoves;

		foreach (var direction in Helper.Bits)
		{
			for (int i = 0; i < (Helper.BoardSize - 1) * (Helper.BoardSize - 1); i++)
			{
				if (!GetFenceEnabled(i, direction)) continue;

				// Convert pawn positions and fence as int to relative fence row as a float
				float relativePlayerIndex = PawnPositions[currentPlayer] / Helper.BoardSize + 0.5f;
				float relativeEnemyIndex = PawnPositions[1 - currentPlayer] / Helper.BoardSize  + 0.5f;
				float relativeFenceIndex = i / (Helper.BoardSize - 1);
				float playerFactor = currentPlayer == 0 ? 1 : -1;
				float enemyFactor = -playerFactor;
				float weight = 0;

				weight += playerFactor * (relativeFenceIndex > relativePlayerIndex ? 4 : -8);
                // weight += enemyFactor * (relativeFenceIndex > relativeEnemyIndex ? 1 : -2);
				allMoves[Helper.GetMoveCodeAsString(currentPlayer, "f", direction, i)] = weight;
			}
		}

		return allMoves;
	}

	public Dictionary<string, float> GetAllMovesWeighted(int currentPlayer)
	{
		// Stores all possible moves with their weights
		Dictionary<string, float> allMoves = GetFenceMovesWeighted(currentPlayer).ToDictionary(x => x.Key, x => x.Value);

		Dictionary<string, float> weightedTiles = GetReachableTilesWeighted(currentPlayer).ToDictionary(x => x.Key, x => x.Value);

		float maxFenceWeight = allMoves.Count > 0 ? allMoves.Values.Max() : 0;
		float maxTileWeight = weightedTiles.Count > 0 ? weightedTiles.Values.Max() : 0;

		foreach (var tile in weightedTiles.Where(tile => tile.Value == maxTileWeight))
		{
			allMoves[tile.Key] = maxFenceWeight; // Overwrite or add
		}

		foreach (var kvp in weightedTiles)
		{
			allMoves[kvp.Key] = kvp.Value; // Overwrite or add
		}

		return allMoves;
	}

	public List<int> GetAllSurroundingFences(int fenceIndex)
	{
		List<int> surroundingFences = [];

		// Get the fences that extend in the same direction
		// Get the direction of the fence, and add the adjcent fences that align
		int direction = Fences[fenceIndex].GetDirection(); 
		int oppositeDirection = 1 - direction;
		int[] adjFences = [.. Helper.InitialiseConnections(fenceIndex, Helper.BoardSize - 1).Where(fence => fence >= 0)];

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

			// Ignore if the fence is already placed
			if (!Fences[leapedAdjFence].IsFencePlaceable(direction)) continue;

			// Convert fence index to mapped index using direction
			surroundingFences.Add(Helper.GetMappedIndex(leapedAdjFence, direction));
		}

		// Perpundicular adjacent fences
		foreach (int adjFence in adjFences)
		{
			// Ignore if out of bounds
			if (adjFence == -1) continue;

			// Ignore if the fence is already placed
			if (!Fences[adjFence].IsFencePlaceable(oppositeDirection)) continue;

			// Convert fence index to mapped index using direction
			surroundingFences.Add(Helper.GetMappedIndex(adjFence, oppositeDirection));
		}
		
		// Perpendicular corner fences
		int[] cornerFences = [.. Helper.InitialiseCornerConnections(fenceIndex, Helper.BoardSize - 1)
			.Where(fence => fence >= 0)];

		surroundingFences.AddRange(cornerFences
			.Where(cornerFence => Fences[cornerFence].IsFencePlaceable(oppositeDirection))
			.Select(cornerFence => Helper.GetMappedIndex(cornerFence, oppositeDirection)));

		return [.. surroundingFences.Distinct()];
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

	public int EvaluateBoard(bool isMaximising = true)
	{
		string lastMove = GetLastMove();
		int currentPlayer = int.Parse(lastMove[0].ToString());

		// Calculate the score, relative to the current player, where negative is not in favour of the player
		int playerPath = Algorithms.GetShortestPath(currentPlayer, this).Length;
		int opponentPath = Algorithms.GetShortestPath(1 - currentPlayer, this).Length;
		int evaluation = (opponentPath - playerPath) * Helper.PATH_WEIGHT;

		// Add the different in number of fences remaining
		evaluation += (GetFenceCount(currentPlayer) - GetFenceCount(1 - currentPlayer)) * Helper.FENCE_WEIGHT;

		// Add +- 1 random value to the evaluation score to avoid ties
		evaluation += Helper.Random.Next(-1, 2);

		return isMaximising ? evaluation : -evaluation;
	}

	#endregion
}
