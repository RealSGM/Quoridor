using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class BoardState : Node
{
	public ParsedMove LastMove { get; private set; } = null;
	private FenceData[] Fences;
	private FenceData[] IllegalFences;
	private Pawn[] Pawns = new Pawn[2];

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

	public void Delete()
	{
		QueueFree();
	}

	#endregion

	#region Godot Helper Methods ---

	public int[] GetFencesAsArray(int dir) => Helper.BitboardToArray(Fences[dir].Fences);

	public int[] GetEnabledFencesAsArray(int dir)
	{
		List<int> bits = [];


		for (int i = 0; i < Helper.BitBoardSize * Helper.BitBoardSize; i++)
		{
			if (IsFenceEnabled(dir, i)) bits.Add(1);
			else bits.Add(0);
		}

		return [.. bits];
	}

	#endregion

	#region Setters ---

	public FenceData[] GetIllegalFences() => IllegalFences;
	public void SetFence(int index, int direction) => Fences[direction].SetPlaced(index);
	public void SetLastMove(string code)
	{
		if (code == string.Empty) return;
		var (player, moveType, dir, index, previousIndex) = Helper.GetMoveCodeAsTuple(code);
		LastMove = new((sbyte)player, moveType[0], (char)dir, (sbyte)index, (sbyte)previousIndex);
	}

	#endregion

	#region Getters ---


	public bool GetFencePlaced(int direction, int index) => Fences[direction].IsPlaced(index);
	public int GetPawnTile(int player) => Pawns[player].Index;
	public ulong GetIllegalFences(int dir) => IllegalFences[dir].Fences;
	public string GetLastMove() => LastMove?.ToString() ?? string.Empty;
	public int GetFencesRemaining(int player) => Pawns[player].FencesRemaining;
	public ulong[] GetFences() => [.. Helper.Bits.Select(dir => Fences[dir].Fences)];

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
		var (player, moveType, dir, index, _) = Helper.GetMoveCodeAsTuple(code);
		if (moveType == "m") ShiftPawn(player, index);
		if (moveType == "f") PlaceFence(player, dir, index);
		LastMove = new((sbyte)player, moveType[0], (char)dir, (sbyte)index);
	}

	public void AddMove(ParsedMove move)
	{
		if (move.MoveType == 'm') ShiftPawn(move.Player, move.Index);
		if (move.MoveType == 'f') PlaceFence(move.Player, move.Direction, move.Index);
		LastMove = move.Clone();
	}

	public void UndoMove(string code)
	{
		var (currentPlayer, moveType, direction, index, previousIndex) = Helper.GetMoveCodeAsTuple(code);
		if (moveType == "m") ShiftPawn(currentPlayer, previousIndex);
		if (moveType == "f") UndoSetFence(currentPlayer, direction, index);
	}

	#endregion

	#region Tiles ---

	/// Return NESW connections for given index, -1 if no connection
	public int[] GetAdjacentTiles(int index)
	{
		int[] cons = Helper.InitialiseConnections(index, Helper.BoardSize);
		int[] corners = Helper.GetFenceCorners(index);
		int dir = 1;

		// Loop through each connection
		for (int i = 0; i < cons.Length; i++)
		{
			dir = 1 - dir;
			if (cons[i] == -1) continue;
			if (corners[i] > -1 && GetFencePlaced(dir, corners[i])) cons[i] = -1;
			if (corners[(i + 1) % cons.Length] > -1 && GetFencePlaced(dir, corners[(i + 1) % cons.Length])) cons[i] = -1;
		}

		return cons;
	}

	/// Return the tiles leaped over by the enemy
	public int[] GetLeapedTiles(int enemyTile, int cardinalDirection, HashSet<int> filterSet)
	{
		int[] enemyCons = GetAdjacentTiles(enemyTile);
		int leapedTile = enemyCons[cardinalDirection];

		return leapedTile != -1

			? [leapedTile]

			: [.. enemyCons.Where(tile => !filterSet.Contains(tile))];
	}

	public int[] GetReachableTiles(int player)
	{
		int playerTile = GetPawnTile(player);
		int enemyTile = Pawns[1 - player].Index;
		HashSet<int> filterSet = [-1, playerTile, enemyTile];
		List<int> reachables = [];

		foreach (var (tile, direction) in GetAdjacentTiles(playerTile).Select((tile, dir) => (tile, dir)))
		{
			if (tile == -1) continue;
			reachables.AddRange(tile == enemyTile

				? GetLeapedTiles(enemyTile, direction, filterSet)

				: [tile]);
		}

		return [.. reachables];
	}

	#endregion

	#region Fences ---

	public bool HasFences(int player) => Pawns[player].FencesRemaining > 0;

	public bool IsFenceEnabled(int dir, int index, bool checkIllegal = true)
	{
		if (GetFencePlaced(dir, index)) return false;
		if (GetFencePlaced(1 - dir, index)) return false;
		if (checkIllegal && IllegalFences[dir].IsPlaced(index)) return false;

		return Helper.Bits.All(bit =>
		{
			int adjFence = Helper.AdjacentFunctions[2 * bit + (1 - dir)](index, Helper.BitBoardSize);
			return adjFence == -1 || !Fences[dir].IsPlaced(adjFence);
		});
	}

	#endregion

	# region Get Moves ---

	public ulong[] GetFencesBehindPlayer(int player)
	{
		int goalRow = Helper.GetGoalTiles(player)[0] / Helper.BoardSize;
		int playerRow = GetPawnTile(player) / Helper.BoardSize;
		int numRowsToFill = playerRow - goalRow;

		ulong fencesBehind = (1UL << (8 * numRowsToFill)) - 1;

		if (player == 0) fencesBehind = ~fencesBehind;

		return [fencesBehind, fencesBehind];
	}

	/// Returns all the fences that are surrounding the given index and direction
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

	public int[] GetReachableTilesSmart(int player)
	{
		int[] path = Algorithms.GetPathToGoal(this, player);
		int[] tiles = GetReachableTiles(player);
		int playerTile = GetPawnTile(player);

		// Add best moves which are reachable
		List<int> allMoves = [.. path.Intersect(tiles)];

		// Add reachable tiles to the dictionary, if no best move was found
		if (allMoves.Count == 0) allMoves.AddRange(tiles);

		return [.. allMoves
			.Where(tile => tile != playerTile)
			.Distinct()
			.ToArray()];
	}

	public ulong[] GetFenceMovesSmart(int player)
	{
		ulong[] fences = [0, 0];

		if (!HasFences(player)) return fences;

		ulong[] enabledFences = GetEnabledFences();
		ulong[] surroundingFences = GetAllSurroundingFences();
		ulong[] fencesBehind = GetFencesBehindPlayer(player);
		ulong[] enemySurrFences = Helper.GetFencesSurroundingTile(Pawns[1 - player].Index);

		// Loop through both directions
		foreach (int direction in Helper.Bits)
		{
			fences[direction] |= surroundingFences[direction];
			fences[direction] |= enemySurrFences[direction];
			fences[direction] |= fencesBehind[direction];
			fences[direction] &= enabledFences[direction];
		}

		return fences;
	}

	public string[] GetAllMovesSmart(int player)
	{
		List<string> moves = [];

		// Add all tiles
		moves.AddRange(GetReachableTilesSmart(player)
			.Select(tile => Helper.GetMoveCodeAsString(player, "m", 0, tile)));

		ulong[] fences = GetFenceMovesSmart(player);

		moves.AddRange(Helper.Bits.SelectMany(dir => Helper.GetOnesInBitBoard(fences[dir])
			.Select(i => Helper.GetMoveCodeAsString(player, "f", dir, i))
		));

		return [.. moves];
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

		// Value moves makes the player closer to the goal or opponent further away
		int pathDifference = minimisingPlayerPath - maximisingPlayerPath;

		// Value using less fences
		int fenceScore = Pawns[maximisingPlayer].FencesRemaining - Pawns[minimisingPlayer].FencesRemaining;

		// Value the player being past the cneter
		int playerRow = Pawns[maximisingPlayer].Index / Helper.BoardSize;
		int centralityScore = (maximisingPlayer == 0)

			? (playerRow <= Helper.centerRow ? 1 : -1)

			: (playerRow >= Helper.centerRow ? 1 : -1);

		return centralityScore

			+ pathDifference * Helper.PATH_WEIGHT

			+ fenceScore * Helper.FENCE_WEIGHT;
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
