using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly int[] Bits = [0, 1]; // General purpose bits
	public static readonly Random Random = new();

	/// <summary>
	/// Weights for the different types of evaluations
	/// </summary>
	public const int PATH_WEIGHT = 20;
	public const int FENCE_WEIGHT = 10;
	public const int CENTRALITY_WEIGHT = 10;

	public const int MaxFences = 10;
	public const int BoardSize = 9;
	public const int BitBoardSize = 8;
	public const int centerRow = BoardSize / 2;

	/// <summary>
	/// List of functions to get adjacent tiles in the order of North, East, South, West
	/// </summary>
	public static readonly List<Func<int, int, int>> AdjacentFunctions =
	[
		GetNorthAdjacent,
		GetEastAdjacent,
		GetSouthAdjacent,
		GetWestAdjacent
	];

	/// <summary>
	/// Returns the move code as a tuple
	/// Template: 0m+12_13, 1m+1_2, 1f+5, 1f-5, 0f+24, 0f-24
	/// </summary>
	/// <param name="moveCode"></param>
	/// <returns>Tuple of (player, moveType, direction, index, previousIndex)</returns>
	public static (int player, string moveType, int direction, int index, int previousIndex) GetMoveCodeAsTuple(string moveCode)
	{
		// Separate previousIndex from moveCode
		string[] parts = moveCode.Split('_');
		int previousIndex = parts.Length > 1 ? int.Parse(parts[1]) : -1;
		moveCode = parts[0];

		int player = int.Parse(moveCode[0].ToString());
		string moveType = moveCode[1].ToString();
		int direction = moveCode[2] == '-' ? 1 : 0;
		int index = int.Parse(moveCode[3..]);

		return (player, moveType, direction, index, previousIndex);
	}

	/// <summary>
	/// Returns the move code as a string
	/// Template: 0m+12_13, 1m+1_2, 1f+5, 1f-5, 0f+24, 0f-24
	/// </summary>
	/// <param name="player"></param>
	/// <param name="moveType"></param>
	/// <param name="direction"></param>
	/// <param name="index"></param>
	/// <param name="previousIndex"></param>
	/// <returns>Move code as a string</returns>
	public static string GetMoveCodeAsString(int player, string moveType, int direction, int index, int previousIndex = -1)
	{
		string moveCode = $"{player}{moveType}{GetMappedIndex(index, direction)}";
		if (previousIndex != -1)
			moveCode += $"_{previousIndex}";

		return moveCode;
	}

	/// <summary>
	/// Returns the fence index as a string with + and - for direction
	/// </summary>
	/// <param name="index"></param>
	/// <param name="direction"></param>
	/// <returns>Fence index as a string</returns>
	public static string GetMappedIndex(int index, int direction) => $"{(direction == 0 ? "+" : "-")}{Math.Abs(index)}";

	public static int[] GetGoalTiles(int player) => [.. Enumerable.Range(player * BitBoardSize * BoardSize, BoardSize)];

	#region Index Mapping ---

	public static int GetNorthAdjacent(int index, int size) => index >= size ? index - size : -1;
	public static int GetEastAdjacent(int index, int size) => (index + 1) % size != 0 ? index + 1 : -1;
	public static int GetSouthAdjacent(int index, int size) => index < size * (size - 1) ? index + size : -1;
	public static int GetWestAdjacent(int index, int size) => index % size != 0 ? index - 1 : -1;

	public static int[] InitialiseCornerConnections(int index, int size) =>
	[
		GetNorthAdjacent(GetEastAdjacent(index, size), size),
		GetSouthAdjacent(GetEastAdjacent(index, size), size),
		GetSouthAdjacent(GetWestAdjacent(index, size), size),
		GetNorthAdjacent(GetWestAdjacent(index, size), size),
	];

	// Initalise the NESW connections for given index
	public static int[] InitialiseConnections(int index, int size) =>
	[
		GetNorthAdjacent(index, size),
		GetEastAdjacent(index, size),
		GetSouthAdjacent(index, size),
		GetWestAdjacent(index, size)
	];

	#endregion

	#region Tile Mapping ---

	public static int GetFenceCorner(int tile, int offset, int cornerIndex)
	{
		if (tile == -1) return -1;
		int row = tile / BoardSize;
		int col = tile % BoardSize;
		return (row + offset) * BitBoardSize + col + (cornerIndex % 2 == 0 ? -1 : 0);
	}

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

	public static int[] OrderConnections(int[] tiles, int player)
	{
		int[] order = player == 0 ? [0, 1, 3, 2] : [2, 1, 3, 0];
		return [.. order.Select(i => tiles[i])];
	}

	#endregion

	#region BitBoard Functions ---

	public static IEnumerable<int> GetOnesInBitBoard(ulong bitboard)
	{
		while (bitboard != 0)
		{
			int index = BitOperations.TrailingZeroCount(bitboard);
			yield return index;
			bitboard &= bitboard - 1;
		}
	}

	public static ulong[] GetFencesSurroundingTile(int index)
	{
		ulong mask = GetFenceCorners(index)
			.Where(corner => corner >= 0)
			.Aggregate(0UL, (acc, corner) => acc | (1UL << corner));
		return [mask, mask];
	}

	public static ulong[] GetSurroundingFences(int index, int dir)
	{
		// Store surrounding fences [Horizontal, Vertical]
		ulong[] surrFences = [0, 0];
		int[] adjFences = InitialiseConnections(index, BitBoardSize);
		int oppDir = 1 - dir;

		foreach (int bit in Bits)
		{
			// Get the adjacent fence
			int adjIndex = (2 * bit) + oppDir;
			int adjFence = adjFences[adjIndex];
			if (adjFence == -1) continue;

			// Get the leaped adjacent fence
			int leapedAdjFence = AdjacentFunctions[adjIndex](adjFence, BitBoardSize);
			if (leapedAdjFence == -1) continue;

			surrFences[dir] |= 1UL << leapedAdjFence;
		}

		// Add perpendicular adjacent fences
		surrFences[oppDir] |= adjFences
			.Where(adjFence => adjFence != -1)
			.Aggregate(0UL, (acc, adjFence) => acc | (1UL << adjFence));

		surrFences[oppDir] |= InitialiseCornerConnections(index, BitBoardSize)
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
}
