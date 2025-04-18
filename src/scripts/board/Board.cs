using Godot;
using System;
using System.Collections.Generic;
using System.Linq;



// TODO EvaluateBoard
// TODO Cleanup Helper
// TODO Remove Tile.cs
// TODO Override BoardState.cs
// TODO Override Fence.cs


[GlobalClass]
public partial class Board: Control
{
	private FenceBB[] Fences;
    private Pawn[] Pawns = new Pawn[2];

    #region Initialization ---
    
    public Board Clone() => new()
    {
        Fences = [Fences[0].Clone(), Fences[1].Clone()],
        Pawns = [Pawns[0].Clone(), Pawns[1].Clone()]
    };

    public void Initialise()
    {
        Pawns[0] = new Pawn((byte)(Helper.BoardSize >> 1));
        Pawns[1] = new Pawn((byte)(Helper.BoardSize * (Helper.BoardSize - 1) + (Helper.BoardSize >> 1)));
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
        if (GetFencePlaced(dir, index) || Fences[dir].IsIllegal(index)) return false;

        return Helper.Bits.All(bit =>
        {
            int adjFence = Helper.AdjacentFunctions[2 * bit + (1 - dir)](index, Helper.BoardSize - 1);
            return adjFence == -1 || !Fences[dir].IsPlaced(adjFence);
        });
    }

    #endregion

    # region Get Moves ---

    /// Returns all the fences that are surrounding the given index
    public ulong[] GetAllSurroundingFences(int index)
    {
        // Store surrounding fences [Horizontal, Vertical]
        ulong[] surrFences = [0, 0];
        int[] adjFences = Helper.InitialiseConnections(index, Helper.BoardSize - 1);
        
        int dir = GetPlacedDirection(index);
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
			if (!IsFenceEnabled(leapedAdjFence, dir)) continue;

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
            if (!IsFenceEnabled(cornerFence, oppDir)) continue;
            surrFences[dir] |= 1UL << cornerFence;
        }

        return surrFences;
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

    // TODO GetFenceMovesWeighted

    public ulong[] GetFencesSmart(int player)
    {
        ulong[] fences = [0, 0];

        if (!HasFences(player)) return fences;

        // Get all placed fences
        
        

        return fences;
    }

	// public Dictionary<string, float> GetFenceMovesWeighted(int currentPlayer)
	// {
	// 	// Stores all possible moves with their weights
	// 	Dictionary<string, float> allMoves = [];

	// 	if (GetFenceCount(currentPlayer) >= Helper.MaxFences) return allMoves;

	// 	// Get all surrounding fences as a string mappedIndexes
	// 	List<string> surroundingFences = [.. GetPlacedFences()
	// 		.SelectMany(GetAllSurroundingFences)
	// 		.Where(fence => fence != "")];

	// 	// Add horizontal fences which are behind the player
	// 	foreach (int direction in Helper.Bits)
	// 	{
	// 		surroundingFences.AddRange(
	// 			Enumerable.Range(0, (Helper.BoardSize - 1) * (Helper.BoardSize - 1))
	// 				.Where(i => IsFenceEnabled(i, direction))
	// 				.Select(i => Helper.GetMappedIndex(i, direction))
	// 				.Where(mappedIndex => !surroundingFences.Contains(mappedIndex))
	// 		);
	// 	}

	// 	// Add fences that surround the enemy
	// 	surroundingFences.AddRange(GetTileAdjacentFences(1 - currentPlayer));
	// 	surroundingFences = [.. surroundingFences.Where(fenceIndex => IsFenceEnabled(int.Parse(fenceIndex[1..]), fenceIndex[0].ToString() == "+" ? 0 : 1))];


	// 	foreach (string fenceIndex in surroundingFences)
	// 	{
	// 		int direction = fenceIndex[0].ToString() == "+" ? 0 : 1;
	// 		int index = int.Parse(fenceIndex[1..]);

	// 		if (!IsFenceEnabled(index, direction)) continue;

	// 		float relativePlayerIndex = PawnPositions[currentPlayer] / Helper.BoardSize + 0.5f;
	// 		float relativeFenceIndex = index / (Helper.BoardSize - 1) + 1;

	// 		if (currentPlayer == 0 && relativeFenceIndex < relativePlayerIndex) continue;
	// 		if (currentPlayer == 1 && relativeFenceIndex > relativePlayerIndex) continue;

	// 		string moveCode = Helper.GetMoveCodeAsString(currentPlayer, "f", direction, index);

	// 		// If the key already exists, we don't add it again, preventing duplicate keys
	// 		if (allMoves.ContainsKey(moveCode)) continue;

	// 		// Add the fence to the dictionary
	// 		allMoves[moveCode] = 5;
	// 	}

	// 	// Loop through all fence moves and make sure the fence is placeable
	// 	allMoves = allMoves
	// 		.Where(pair =>
	// 		{
	// 			string moveCode = pair.Key;
	// 			int index = int.Parse(moveCode[3..]);
	// 			int direction = moveCode[2] == '-' ? 1 : 0;
	// 			return IsFenceEnabled(index, direction);
	// 		})
	// 		.ToDictionary(pair => pair.Key, pair => pair.Value);

	// 	return allMoves;
	// }

    // TODO GetAllMovesWeighted
    // TODO GetAllFences
    // TODO GetAllMoves
    // TODO GetAllMovesSmart


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

    #endregion

}