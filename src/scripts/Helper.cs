using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Numerics;
using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly int[] Bits = [0, 1];
	public static readonly Random Random = new();
	public const int PATH_WEIGHT = 20;
	public const int FENCE_WEIGHT = 10;
	public const int MaxFences = 10;
	public const int BoardSize = 9;

	public static readonly List<Func<int, int, int>> AdjacentFunctions =
	[
		GetNorthAdjacent,
		GetEastAdjacent,
		GetSouthAdjacent,
		GetWestAdjacent
	];

	#region Move Codes ---

	/// Separates the move code into its components
	/// Template: 0m12_13, 1m1_2, 1f5, 1f-5, 0f24, 0f-24
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

	/// Returns the move code as a string
	/// Template: 0m+12_13, 1m+1_2, 1f+5, 1f-5, 0f+24, 0f-24
	public static string GetMoveCodeAsString(int player, string moveType, int direction, int index, int previousIndex = -1)
	{
		string moveCode = $"{player}{moveType}{(direction == 1 ? "-" : "+")}{Math.Abs(index)}";

		if (previousIndex != -1)
			moveCode += $"_{previousIndex}";

		return moveCode;
	}

	public static string GetMappedIndex(int index, int direction) => $"{(direction == 0 ? "+" : "-")}{Math.Abs(index)}";

	#endregion

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
		return (row + offset) * (BoardSize - 1) + col + (cornerIndex % 2 == 0 ? -1 : 0);
	}

	public static int TileToFence(int tile, int verticalOffset, int horizontalOffset)
	{
		int row = tile / BoardSize + verticalOffset;
		int col = tile % BoardSize + horizontalOffset;
		return (row >= BoardSize - 1 || col >= BoardSize - 1) ? -1 : row * (BoardSize - 1) + col;
	}

	/// Returns the fence buttons that surround a tile
	public static int[] GetFenceCorners(int tile) =>
		[
			TileToFence(tile, -1, -1),  // TopLeft
			TileToFence(tile, -1, 0),  // TopRight
			TileToFence(tile, 0, -1),  // BottomRight
			TileToFence(tile, 0, 0)   // BottomLeft
		];
	
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
        return [.. Bits.Select(dir =>
			GetFenceCorners(index)
                .Where(fence => fence != -1)
                .Aggregate(0UL, (acc, fenceIndex) => acc | (1UL << fenceIndex))
        )];
    }

	public static ulong[] GetSurroundingFences(int index, int dir)
    {
        // Store surrounding fences [Horizontal, Vertical]
        ulong[] surrFences = [0, 0];
        int[] adjFences = InitialiseConnections(index, BoardSize - 1);
        int oppDir = 1 - dir;

        foreach (int bit in Bits)
        {
            // Get the adjacent fence
			int adjIndex = (2 * bit) + oppDir;
            int adjFence = adjFences[adjIndex];
            if (adjFence == -1) continue;

            // Get the leaped adjacent fence
			int leapedAdjFence = AdjacentFunctions[adjIndex](adjFence, BoardSize - 1);
			if (leapedAdjFence == -1) continue;

            surrFences[dir] |= 1UL << leapedAdjFence;
        }

		// Add perpendicular adjacent fences
		surrFences[oppDir] |= adjFences
			.Where(adjFence => adjFence != -1)
			.Aggregate(0UL, (acc, adjFence) => acc | (1UL << adjFence));

		// Perpendicular corner fences
		surrFences[dir] |= InitialiseCornerConnections(index, BoardSize - 1)
			.Where(fence => fence >= 0)
			.Aggregate(0UL, (acc, cornerFence) => acc | (1UL << cornerFence));

        return surrFences;
    }

	#endregion

	public static int[] GetGoalTiles(int player)
	{
		int startRow = player * (BoardSize - 1) * BoardSize;
		return [.. Enumerable.Range(startRow, BoardSize)];
	}

}
