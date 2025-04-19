using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// TODO Cleanup Helper
// TODO Remove Tile.cs
// TODO Override BoardState.cs
// TODO Override Fence.cs

[GlobalClass]
public partial class Board: Control
{
	private FenceBB[] Fences;
    private FenceBB[] IllegalFences;
    private Pawn[] Pawns = new Pawn[2];

    #region Initialization ---
    
    public Board Clone() => new()
    {
        Fences = [Fences[0].Clone(), Fences[1].Clone()],
        Pawns = [Pawns[0].Clone(), Pawns[1].Clone()],
        IllegalFences = [IllegalFences[0].Clone(), IllegalFences[1].Clone()]
    };

    public void Initialise()
    {
        Pawns[0] = new Pawn((byte)(Helper.BoardSize >> 1));
        Pawns[1] = new Pawn((byte)(Helper.BoardSize * (Helper.BoardSize - 1) + (Helper.BoardSize >> 1)));
        Fences = [new FenceBB(), new FenceBB()];
        IllegalFences = [new FenceBB(), new FenceBB()];
    }

    #endregion

    #region Setters ---

    public void SetFence(int index, int direction) => Fences[direction].SetPlaced(index);

    #endregion

	#region Getters ---

	public FenceBB[] GetFences() => Fences;
	public FenceBB GetFencesInDirection(int direction) => Fences[direction];
    public bool GetFencePlaced(int direction, int index) => Fences[direction].IsPlaced(index);
    public int GetPlacedDirection(int index) => Array.FindIndex(Fences, fence => fence.IsPlaced(index));
    public int GetPawnPosition(int player) => Pawns[player].Index;

	#endregion

    #region Moves ---

    public void PlaceFence(int direction, int index, int player)
    {
        Pawns[player].PlaceFence();
        SetFence(index, direction);
    }

    public void UndoSetFence(int index, int direction) => Fences[direction].UndoSetPlaced(index);
    public void ShiftPawn(int player, int index) => Pawns[player].Move((byte)index);

    public void AddMove(string code)
    {
        var (player, moveType, dir, index, _) = Helper.GetMoveCodeAsTuple(code);
        if (moveType == "m") ShiftPawn(index, player);
        if (moveType == "f") PlaceFence(dir, index, player);
    }

    public void UndoMove(string code)
    {
        var (currentPlayer, moveType, direction, index, previousIndex) = Helper.GetMoveCodeAsTuple(code);
        if (moveType == "m") ShiftPawn(currentPlayer, previousIndex);
        if (moveType == "f") UndoSetFence(index, direction);
    }

    #endregion

    #region Tiles ---

    /// Return NESW connections for given index, -1 if no connection
    public int[] GetAdjacentTiles(int index)
    {
        int[] cons = Helper.InitialiseConnections(index, Helper.BoardSize);
        int[] corners = Helper.GetFenceCorners(index);
        int dir = 0;

        // Loop through each connection
        for (int i = 0; i < cons.Length; i++)
        {
            if (cons[i] == -1) continue;
            if (GetFencePlaced(dir, corners[i])) cons[i] = -1;
            if (GetFencePlaced(dir, corners[(i + 1) % cons.Length])) cons[i] = -1;
            
            dir = 1 - dir;
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
        int playerTile = GetPawnPosition(player);
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

    public bool IsFenceEnabled(int dir, int index)
    {
        if (GetFencePlaced(dir, index) || IllegalFences[dir].IsPlaced(index)) return false;

        return Helper.Bits.All(bit =>
        {
            int adjFence = Helper.AdjacentFunctions[2 * bit + (1 - dir)](index, Helper.BoardSize - 1);
            return adjFence == -1 || !Fences[dir].IsPlaced(adjFence);
        });
    }

    #endregion

    # region Get Moves ---

    /// Returns all the fences that are surrounding the given index and direction
    public ulong[] GetSurroundingFences(int index, int dir)
    {
        // Store surrounding fences [Horizontal, Vertical]
        ulong[] surrFences = [0, 0];
        int[] adjFences = Helper.InitialiseConnections(index, Helper.BoardSize - 1);
        int oppDir = 1 - dir;

        foreach (int bit in Helper.Bits)
        {
            // Get the adjacent fence
			int adjIndex = (2 * bit) + oppDir;
            int adjFence = adjFences[adjIndex];
            if (adjFence == -1) continue;

            // Get the leaped adjacent fence
			int leapedAdjFence = Helper.AdjacentFunctions[adjIndex](adjFence, Helper.BoardSize - 1);
			if (leapedAdjFence == -1) continue;
			if (!IsFenceEnabled(dir, leapedAdjFence)) continue;

            surrFences[dir] |= 1UL << leapedAdjFence;
        }

        // Add perpendicular adjacent fences
		foreach (int adjFence in adjFences)
		{
			if (adjFence == -1) continue;
			if (!IsFenceEnabled(adjFence, oppDir)) continue;
            surrFences[oppDir] |= 1UL << adjFence;
		}

        // Perpendicular corner fences
		int[] cornerFences = [.. Helper.InitialiseCornerConnections(index, Helper.BoardSize - 1)
			.Where(fence => fence >= 0)];

        foreach (int cornerFence in cornerFences)
        {
            if (!IsFenceEnabled(oppDir, cornerFence)) continue;
            surrFences[dir] |= 1UL << cornerFence;
        }

        return surrFences;
    }

    public ulong[] GetAllSurroundingFences()
    {
        ulong[] fences = [0, 0];

        foreach (int bit in Helper.Bits)
        {
            foreach (int i in Helper.GetOnesInBitBoard(Fences[bit].Fences))
            {
                ulong[] surrFences = GetSurroundingFences(i, bit);
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
        int playerTile = GetPawnPosition(player);

        // Add best moves which are reachable
        List<int> allMoves = [.. path.Intersect(tiles)];

		// Add reachable tiles to the dictionary, if no best move was found
		if (allMoves.Count == 0) allMoves.AddRange(tiles);

        return [.. allMoves
            .Where(tile => tile != playerTile)
            .Distinct()
            .ToArray()];
    }

    public ulong[] GetFencesSmart(int player)
    {
        ulong[] fences = [0, 0];

        if (!HasFences(player)) return fences;

        // Get all surrounding fences
        ulong[] surroundingFences = GetAllSurroundingFences();
        fences[0] |= surroundingFences[0];
        fences[1] |= surroundingFences[1];

        // Get all fences behind the player

        int goalRow = Helper.GetGoalTiles(player)[0] / Helper.BoardSize;
        int playerRow = GetPawnPosition(player) / Helper.BoardSize;
        int numRowsToFill = playerRow - goalRow;

        // Retrieve all fences behind the player
        ulong fencesBehind = player == 0 
            ? (1UL << (8 * numRowsToFill)) - 1 
            : ~((1UL << (8 * numRowsToFill)) - 1);
        
        fences[0] |= fencesBehind;

        // Add fences that surround the enemy
        int enemyTile = Pawns[1 - player].Index;

        int[] enemySurrFences = [.. Helper.GetFenceCorners(enemyTile).Where(fence => fence != -1)];

        // Loop through all fences
        foreach (int direction in Helper.Bits)
        {
            fences[direction] |= enemySurrFences.Aggregate(0UL, (acc, fenceIndex) => acc | (1UL << fenceIndex));
        }

        // Filter out fenaces that are not enabled
        foreach (int direction in Helper.Bits)
        {
            for (int i = 0; i < (Helper.BoardSize - 1) * (Helper.BoardSize - 1); i++)
            {   
                if (!IsFenceEnabled(direction, i)) continue;
                fences[direction] &= ~(1UL << i);
            }
        }

        return fences;
    }

    public ulong[] GetAllFences(int player)
    {
        if (!HasFences(player)) return [0, 0];
        return [.. Helper.Bits.Select(dir =>
            Enumerable.Range(0, (Helper.BoardSize - 1) * (Helper.BoardSize - 1))
            .Where(i => IsFenceEnabled(dir, i))
            .Aggregate(0UL, (acc, i) => acc | (1UL << i))
        )];
    }

    public string[] GetAllMoves(int player)
    {
        List<string> moves = [];

        // Add all tiles
        moves.AddRange(GetReachableTiles(player)
            .Select(tile => Helper.GetMoveCodeAsString(player, "m", 0, tile)));

        ulong[] fences = GetAllFences(player);

        moves.AddRange(Helper.Bits.SelectMany(dir => Helper.GetOnesInBitBoard(fences[dir])
            .Select(i => Helper.GetMoveCodeAsString(player, "f", dir, i))
        ));

        return [.. moves];
    }

    public string[] GetAllMovesSmart(int player)
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

    public bool IsWinner(int player) => Helper.GetGoalTiles(player).Contains(GetPawnPosition(player));

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
		int fenceScore = Pawns[minimisingPlayer].FencesRemaining - Pawns[maximisingPlayer].FencesRemaining;

		int evaluation = 0;

		evaluation += pathDifference * Helper.PATH_WEIGHT;
		evaluation += fenceScore * Helper.FENCE_WEIGHT;

		return evaluation;
    }
    #endregion
}