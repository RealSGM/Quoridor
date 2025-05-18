using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly Random Random = new();
	public static readonly int[] Bits = [0, 1]; // General purpose bits

	public const int PATH_WEIGHT = 20;
	public const int FENCE_WEIGHT = 10;
	public const int CENTRALITY_WEIGHT = 10;

	public const int MaxFences = 10;
	public const int BoardSize = 9;
	public const int BitBoardSize = 8;
	public const int centerRow = BoardSize / 2;

	#region Index Mapping ---

	/// List of all functions to get adjacent tiles in respective directions
	public static readonly List<Func<int, int, int>> AdjacentFunctions =
	[
		GetNorthAdjacent,
		GetEastAdjacent,
		GetSouthAdjacent,
		GetWestAdjacent
	];

	public static int GetNorthAdjacent(int index, int size) => index >= size ? index - size : -1; // Get the index of the tile above
	public static int GetEastAdjacent(int index, int size) => (index + 1) % size != 0 ? index + 1 : -1; // Get the index of the tile to the right
	public static int GetSouthAdjacent(int index, int size) => index < size * (size - 1) ? index + size : -1; // Get the index of the tile below
	public static int GetWestAdjacent(int index, int size) => index % size != 0 ? index - 1 : -1; // Get the index of the tile to the left

	/// Initalise the corner connections for given index
	public static int[] InitialiseCornerConnections(int index, int size) =>
	[
		GetNorthAdjacent(GetEastAdjacent(index, size), size),
		GetSouthAdjacent(GetEastAdjacent(index, size), size),
		GetSouthAdjacent(GetWestAdjacent(index, size), size),
		GetNorthAdjacent(GetWestAdjacent(index, size), size),
	];

	/// Initalise the NESW connections for given index
	public static int[] InitialiseConnections(int index, int size) =>
	[
		GetNorthAdjacent(index, size),
		GetEastAdjacent(index, size),
		GetSouthAdjacent(index, size),
		GetWestAdjacent(index, size)
	];

	#endregion

	/// <summary>
	/// Returns the move code as a string
	/// </summary>
	/// <param name="player">Player number</param>
	/// <param name="moveType">Move type (e.g. "f" for fence)</param>
	/// <param name="direction">Direction (0 for horizontal, 1 for vertical)</param>
	/// <param name="index">Index</param>
	/// <param name="previousIndex">Previous index (optional)</param>
	/// <returns>Move code as a string</returns>
	public static string GetMoveCodeAsString(int player, string moveType, int direction, int index, int previousIndex = -1)
	{
		string moveCode = $"{player}{moveType}{(direction == 0 ? "+" : "-")}{Math.Abs(index)}";
		if (previousIndex != -1)
			moveCode += $"_{previousIndex}";

		return moveCode;
	}

	#region Tile Mapping ---
	/// <summary>
	/// Returns the tile index for a given fence tile
	/// </summary>
	/// <param name="tile"></param>
	/// <param name="offset"></param>
	/// <param name="cornerIndex"></param>
	/// <returns>>Tile index for the given fence tile</returns>
	public static int GetFenceCorner(int tile, int offset, int cornerIndex)
	{
		if (tile == -1) return -1;
		int row = tile / BoardSize;
		int col = tile % BoardSize;
		return (row + offset) * BitBoardSize + col + (cornerIndex % 2 == 0 ? -1 : 0);
	}

	/// <summary>
	/// Gets the fence index for a given tile index and offsets
	/// </summary>
	/// <param name="tile">Tile index</param>
	/// <param name="verticalOffset">Vertical offset</param>
	/// <param name="horizontalOffset">Horizontal offset</param>
	/// <returns>Fence index for the given tile index</returns>
	/// <remarks>Returns -1 if the tile is out of bounds</remarks>
	public static int TileToFence(int tile, int verticalOffset, int horizontalOffset)
	{
		int row = tile / BoardSize + verticalOffset;
		int col = tile % BoardSize + horizontalOffset;
		return (row is < 0 or >= BitBoardSize || col is < 0 or >= BitBoardSize) ? -1 : row * BitBoardSize + col;
	}

	/// Returns the fence buttons that surround a tile
	public static int[] GetFenceCorners(int tile) =>
		[
			TileToFence(tile, -1, -1),  // TopLeft
			TileToFence(tile, -1, 0),  // TopRight
			TileToFence(tile, 0, 0),   // BottomLeft
			TileToFence(tile, 0, -1)  // BottomRight
		];

	/// Order the connections based on the player
	public static int[] OrderConnections(int[] tiles, int player)
	{
		int[] order = player == 0 ? [0, 1, 3, 2] : [2, 1, 3, 0];
		return [.. order.Select(i => tiles[i])];
	}

	#endregion

	#region BitBoard Functions ---

	/// Returns the index of all the ones in a bitboard in constant time
	public static IEnumerable<int> GetOnesInBitBoard(ulong bitboard)
	{
		while (bitboard != 0)
		{
			int index = BitOperations.TrailingZeroCount(bitboard);
			yield return index;
			bitboard &= bitboard - 1;
		}
	}

	/// Gets the fences surrounding a tile
	public static ulong[] GetFencesSurroundingTile(int index)
	{
		ulong mask = GetFenceCorners(index)
			.Where(corner => corner >= 0)
			.Aggregate(0UL, (acc, corner) => acc | (1UL << corner));
		return [mask, mask];
	}

	public static ulong[] GetSurroundingFences(int index, int dir)
	{
		ulong[] surrFences = [0, 0]; // Store surrounding fences [Horizontal, Vertical]
		int[] adjFences = InitialiseConnections(index, BitBoardSize);
		int oppDir = 1 - dir;

		foreach (int bit in Bits)
		{
			int adjIndex = (2 * bit) + oppDir;
			int adjFence = adjFences[adjIndex]; // Get the adjacent fences
			if (adjFence == -1) continue;

			int leapedAdjFence = AdjacentFunctions[adjIndex](adjFence, BitBoardSize); // Get the fence leaped over the adjacent fence
			if (leapedAdjFence == -1) continue;

			surrFences[dir] |= 1UL << leapedAdjFence; // Set the bit for the leaped adjacent fence
		}

		surrFences[oppDir] |= adjFences // Add perpendicular adjacent fences
			.Where(adjFence => adjFence != -1)
			.Aggregate(0UL, (acc, adjFence) => acc | (1UL << adjFence));

		surrFences[oppDir] |= InitialiseCornerConnections(index, BitBoardSize) // Add corner connections
			.Where(connection => connection != -1)
			.Aggregate(0UL, (acc, connection) => acc | (1UL << connection));

		return surrFences;
	}

	public static int[] BitboardToArray(ulong bitBoard) => [.. Enumerable.Range(0, BitBoardSize * BitBoardSize).Select(i => (int)((bitBoard >> i) & 1))];

	public static void PrintBitBoard(ulong bitboard)
	{
		int[] arr = BitboardToArray(bitboard);
		for (int i = 0; i < BitBoardSize; i++)
		{
			string row = string.Join("  ", arr.Skip(i * BitBoardSize).Take(BitBoardSize));
			GD.Print(row);
		}
		GD.Print("----------");
	}

	#endregion

	public static void Shuffle<T>(IList<T> list, Random rng)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = rng.Next(i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}
	
	/// <summary>
	/// Returns an array of indices for the goal tiles for a given player
	/// </summary>
	/// <param name="player">Player number</param>
	/// <returns>Array of goal tile indices</returns>
	public static int[] GetGoalTiles(int player) => [.. Enumerable.Range(player * BitBoardSize * BoardSize, BoardSize)];
}
