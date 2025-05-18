using System;
using System.Collections.Generic;
using System.Linq;

public class BoardState
{
	public ParsedMove LastMove { get; set; } = null;
	public FenceData[] Fences;
	public FenceData[] IllegalFences;
	public Pawn[] Pawns = new Pawn[2];

	#region Initialization ---

	public BoardState Clone() => new()
	{
		Fences = [Fences[0], Fences[1]],
		Pawns = [Pawns[0].Clone(), Pawns[1].Clone()],
		IllegalFences = [IllegalFences[0].Clone(), IllegalFences[1].Clone()],
		LastMove = LastMove?.Clone()
	};

	public void Initialise()
	{
		Pawns[0] = new Pawn(Helper.BoardSize * Helper.BitBoardSize + (Helper.BoardSize >> 1));
		Pawns[1] = new Pawn(Helper.BoardSize >> 1);
		Fences = [new FenceData(), new FenceData()];
		IllegalFences = [new FenceData(), new FenceData()];
	}

	#endregion

	#region Setters ---

	public FenceData[] GetIllegalFences() => IllegalFences;
	public void SetFence(int index, int direction) => Fences[direction].SetPlaced(index);
	public void SetLastMove(string code)
	{
		if (code == string.Empty) return;
		ParsedMove move = ParsedMove.Create(code);
		var (player, moveType, dir, index, previousIndex) = move.GetMoveAsTuple();
		LastMove = new((sbyte)player, moveType[0], dir == 0, (sbyte)index, (sbyte)previousIndex);
	}

	#endregion

	#region Getters ---

	public bool GetFencePlaced(int direction, int index) => Fences[direction].IsPlaced(index);
	public int GetPawnTile(int player) => Pawns[player].Index;
	public ulong GetIllegalFences(int dir) => IllegalFences[dir].Fences;
	public string GetLastMove() => LastMove?.ToString() ?? string.Empty;
	public int GetFencesRemaining(int player) => Pawns[player].FencesRemaining;
	public ulong[] GetFences() => [.. Helper.Bits.Select(dir => Fences[dir].Fences)];

	/// <summary>
	/// Returns all the enabled fences for both players.
	/// The fences are represented as a bitboard, where each bit represents a tile on the board.
	/// </summary>
	/// <param name="checkIllegal"></param>
	/// <returns></returns>
	public ulong[] GetEnabledFences(bool checkIllegal = true) => [.. Helper.Bits.Select(dir => Enumerable
		.Range(0, Helper.BitBoardSize * Helper.BitBoardSize)
		.Where(i => IsFenceEnabled(dir, i, checkIllegal))
		.Aggregate(0UL, (acc, i) => acc | (1UL << i))
	)];

	#endregion

	#region Moves ---

	public void PlaceFence(int player, int direction, int index)
	{
		Pawns[player].PlaceFence();
		SetFence(index, direction);
	}

	public void UndoSetFence(int player, int direction, int index)
	{
		Pawns[player].UndoPlaceFence();
		Fences[direction].UndoSetPlaced(index);
	}

	public void ShiftPawn(int player, int index) => Pawns[player].Move((byte)index);

	public void AddMove(string code)
	{
		ParsedMove move = ParsedMove.Create(code);
		var (player, moveType, dir, index, _) = move.GetMoveAsTuple();
		if (moveType == "m") ShiftPawn(player, index);
		if (moveType == "f") PlaceFence(player, dir, index);
		LastMove = new((sbyte)player, moveType[0], dir == 0, (sbyte)index);
	}

	public void AddMove(ParsedMove move)
	{
		if (move.MoveType == 'm') ShiftPawn(move.Player, move.Index);
		if (move.MoveType == 'f') PlaceFence(move.Player, move.IsHorizontal ? 0 : 1, move.Index);
		LastMove = move.Clone();
	}

	public void UndoMove(string code)
	{
		ParsedMove move = ParsedMove.Create(code);
		var (currentPlayer, moveType, direction, index, previousIndex) = move.GetMoveAsTuple();
		if (moveType == "m") ShiftPawn(currentPlayer, previousIndex);
		if (moveType == "f") UndoSetFence(currentPlayer, direction, index);
	}

	#endregion

	#region Tiles ---

	/// <summary>
	/// Returns the adjacent tiles of the given tile index.
	/// If a tile is blocked by a fence, it will be marked as -1.
	/// </summary>
	/// <param name="index">The index of the tile.</param>
	/// <returns>An array of integers representing the adjacent tiles.</returns>
	public int[] GetAdjacentTiles(int index)
	{
		int[] cons = Helper.InitialiseConnections(index, Helper.BoardSize); // Get the tile connections
		int[] corners = Helper.GetFenceCorners(index); // Get the indexes of the fences around the tile
		int dir = 1;

		for (int i = 0; i < cons.Length; i++) // Loop through each connection
		{
			dir = 1 - dir; // Flip the direction
			if (cons[i] == -1) continue; // Ignore tiles that are blocked by fences
			if (corners[i] > -1 && GetFencePlaced(dir, corners[i])) cons[i] = -1; // Check if the tile is blocked by a fence
			if (corners[(i + 1) % cons.Length] > -1 && GetFencePlaced(dir, corners[(i + 1) % cons.Length])) cons[i] = -1; // Check if the tile is blocked by a fence
		}
		return cons;
	}

	/// <summary>
	/// Gets the leaped tiles for the given enemy tile and cardinal direction.
	/// If the leaped tile is blocked by a fence, it will be marked as -1.
	/// </summary>
	/// <param name="enemyTile">The index of the enemy tile.</param>
	/// <param name="cardinalDirection">The direction to leap in.</param>
	/// <param name="filterSet">A set of tiles to filter out.</param>
	/// <returns>An array of integers representing the leaped tiles.</returns>
	public int[] GetLeapedTiles(int enemyTile, int cardinalDirection, HashSet<int> filterSet)
	{
		int[] enemyCons = GetAdjacentTiles(enemyTile);
		int leapedTile = enemyCons[cardinalDirection];

		return leapedTile != -1 ? [leapedTile] : [.. enemyCons.Where(tile => !filterSet.Contains(tile))];
	}

	/// <summary>
	/// Gets the adjacent tiles for the given player tile.
	/// If a tile is blocked by a fence, it will be marked as -1.
	/// If the enemy pawn is on the tile, it will be marked as -1, but will attempt to leap over it.
	/// If the leaped tile is blocked by a fence, it will be marked as -1 and get the adjacent tiles instead.
	/// </summary>
	public int[] GetReachableTiles(int player)
	{
		int playerTile = GetPawnTile(player);
		int enemyTile = Pawns[1 - player].Index;
		HashSet<int> filterSet = [-1, playerTile, enemyTile]; // Tiles which are deemed unreachable
		List<int> reachables = [];

		foreach (var (tile, direction) in GetAdjacentTiles(playerTile).Select((tile, dir) => (tile, dir)))
		{
			if (tile == -1) continue; // Ignore tiles that are blocked by fences
			reachables.AddRange(tile == enemyTile ? GetLeapedTiles(enemyTile, direction, filterSet): [tile]);
		}

		return [.. reachables];
	}

	#endregion

	#region Fences ---

	public bool HasFences(int player) => Pawns[player].FencesRemaining > 0;

	/// <summary>
	/// Checks if a fence is placeable in either direction.
	/// Checks if the fence is illegal and if the adjacent fences are placed.
	/// </summary>
	/// <param name="dir"></param>
	/// <param name="index"></param>
	/// <param name="checkIllegal"></param>
	/// <returns></returns>
	public bool IsFenceEnabled(int dir, int index, bool checkIllegal = true)
	{
		if (GetFencePlaced(dir, index)) return false; // Fence in the same direction is already placed
		if (GetFencePlaced(1 - dir, index)) return false; // Fence in the opposite direction is already placed
		if (checkIllegal && IllegalFences[dir].IsPlaced(index)) return false; // Fence is illegal

		// Check if the adjacent fences are placed
		return Helper.Bits.All(bit =>
		{
			int adjFence = Helper.AdjacentFunctions[2 * bit + (1 - dir)](index, Helper.BitBoardSize);
			return adjFence == -1 || !Fences[dir].IsPlaced(adjFence);
		});
	}

	#endregion

	# region Get Moves ---

	/// <summary>
	/// Returns all fences that connected to existing fences.
	public ulong[] GetAllSurroundingFences()
	{
		ulong[] fences = [0, 0];

		foreach (int dir in Helper.Bits)
		{
			foreach (int i in Helper.GetOnesInBitBoard(Fences[dir].Fences))
			{
				ulong[] surrFences = Helper.GetSurroundingFences(i, dir);
				fences[0] |= surrFences[0];
				fences[1] |= surrFences[1];
			}
		}
		return fences;
	}

	/// <summary>
	/// Returns all the reachable tiles for the given player.
	/// Only returns reachable tiles within the shortest path to the goal.
	/// </summary>
	/// <param name="player"></param>
	/// <returns> An array of integers representing the reachable tiles.</returns>
	public int[] GetReachableTilesSmart(int player)
	{
		int[] path = Algorithms.GetPathToGoal(this, player);
		int[] tiles = GetReachableTiles(player);
		int playerTile = GetPawnTile(player);

		List<int> allMoves = [.. path.Intersect(tiles)]; // Add best moves which are reachable

		// Add reachable tiles to the dictionary, if no best move was found
		if (allMoves.Count == 0) allMoves.AddRange(tiles);

		return [.. allMoves
			.Where(tile => tile != playerTile)
			.Distinct()
			.ToArray()];
	}

	public ulong[] GetAllFences(int player)
	{
		if (!HasFences(player)) return [0, 0];
		return GetEnabledFences();
	}

	public string[] GetAllMoves(int player)
	{
		List<string> moves = [];

		// Add all tiles
		moves.AddRange(GetReachableTilesSmart(player)
			.Select(tile => Helper.GetMoveCodeAsString(player, "m", 0, tile)));

		ulong[] fences = GetAllFences(player);

		moves.AddRange(Helper.Bits.SelectMany(dir => Helper.GetOnesInBitBoard(fences[dir])
			.Select(i => Helper.GetMoveCodeAsString(player, "f", dir, i))
		));

		return [.. moves];
	}

	#endregion

	#region Evaluation ---

	public bool IsWinner(int player) => Helper.GetGoalTiles(player).Contains(GetPawnTile(player));
	public bool IsGameOver() => IsWinner(0) || IsWinner(1);

	public int GetGameResult(int simulatingPlayer)
	{
		if (IsWinner(simulatingPlayer)) return int.MaxValue;
		if (IsWinner(1 - simulatingPlayer)) return int.MinValue;
		return 0;
	}

	public int EvaluateBoard(int maximisingPlayer)
	{
		int minimisingPlayer = 1 - maximisingPlayer;

		int maximisingPlayerPath = Algorithms.GetPathToGoal(this, maximisingPlayer).Length;
		int minimisingPlayerPath = Algorithms.GetPathToGoal(this, minimisingPlayer).Length;
		int pathDifference = minimisingPlayerPath - maximisingPlayerPath;

		int fenceScore = Pawns[maximisingPlayer].FencesRemaining - Pawns[minimisingPlayer].FencesRemaining;

		// Flat bonus for being past the center row
		int playerRow = Pawns[maximisingPlayer].Index / Helper.BoardSize;
		int centralityScore = (maximisingPlayer == 0)
			? (playerRow <= Helper.centerRow ? 1 : -1)
			: (playerRow >= Helper.centerRow ? 1 : -1);

		int offset = Helper.Random.Next(0, 10); // Small offset to break ties

		return centralityScore
			+ pathDifference * Helper.PATH_WEIGHT
			+ fenceScore * Helper.FENCE_WEIGHT
			+ offset;
	}

	public StateKey GetStateKey() => new()
	{
		Player0 = Pawns[0].Index,
		Player1 = Pawns[1].Index,
		HorizontalFences = Fences[0].Fences,
		VerticalFences = Fences[1].Fences
	};

	#endregion
}
