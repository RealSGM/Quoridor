using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly Random Random = new();
	public static readonly int PATH_WEIGHT = 25;
	public static readonly int FENCE_WEIGHT = 2;

	public static readonly int PlayerCount = 2;
	public static readonly int MaxFences = 10;
	public static readonly int BoardSize = 9;
	public static readonly int FenceSize = 8;

	public static readonly int[] Bits = [0, 1];
	public static readonly int[] CardinalDirections = [0, 1, 2, 3];

	public static readonly int[][][] DefaultTileGridConnections =
    [
        [[0, 2], [1, 3]],
		[[0, 1], [2, 3]]
	];

	public static readonly int[] AdjacentOffsets = [-BoardSize, 1, BoardSize, -1];

	public static readonly List<Func<int, int, int>> AdjacentFunctions =
    [
        GetNorthAdjacent,
		GetEastAdjacent,
		GetSouthAdjacent,
		GetWestAdjacent
	];

	public static string GetMoveString(int index, int direction = 0) => $"{(direction == 1 ? "-" : "+")}{index}";

	public static int[] GetMoveCode(string moveCode)
	{
		int direction = moveCode[0] == '-' ? 1 : 0;
		int index = int.Parse(moveCode[1..]);

		return [index, direction];
	}

	public static int GetNorthAdjacent(int index, int size) => index >= size ? index - size : -1;

	public static int GetEastAdjacent(int index, int size) => (index + 1) % size != 0 ? index + 1 : -1;

	public static int GetSouthAdjacent(int index, int size) => index < size * (size - 1) ? index + size : -1;

	public static int GetWestAdjacent(int index, int size) => index % size != 0 ? index - 1 : -1;

	public static int GetNorthEastAdjacent(int index, int size) => GetNorthAdjacent(GetEastAdjacent(index, size), size);

	public static int GetSouthEastAdjacent(int index, int size) => GetSouthAdjacent(GetEastAdjacent(index, size), size);

	public static int GetSouthWestAdjacent(int index, int size) => GetSouthAdjacent(GetWestAdjacent(index, size), size);

	public static int GetNorthWestAdjacent(int index, int size) => GetNorthAdjacent(GetWestAdjacent(index, size), size);

	public static int[] InitialiseCornerConnections(int index, int size) =>
	[
		GetNorthEastAdjacent(index, size),
		GetSouthEastAdjacent(index, size),
		GetSouthWestAdjacent(index, size),
		GetNorthWestAdjacent(index, size)
	];

	// Initalise the NESW connections for given index
	public static int[] InitialiseConnections(int index, int size) =>
    [
        GetNorthAdjacent(index, size),
		GetEastAdjacent(index, size),
		GetSouthAdjacent(index, size),
		GetWestAdjacent(index, size)
	];

	public static int[] GetTileGrid(int index, int boardSize)
	{
		int topLeft = index;
		int topRight = GetEastAdjacent(index, boardSize);
		int bottomLeft = GetSouthAdjacent(index, boardSize);
		int bottomRight = GetSouthAdjacent(topRight, boardSize);
		return [topLeft, topRight, bottomLeft, bottomRight];
	}

	public static int[] GetGoalTiles(int player)
	{
		int startRow = player * (BoardSize - 1) * BoardSize;
		return [.. Enumerable.Range(startRow, BoardSize)];
	}

	public static List<int> GetAllSurroundingFences(int fenceIndex)
	{
		List<int> surroundingFences = [];

		int[] adjFences = [.. InitialiseConnections(fenceIndex, BoardSize - 1).Where(fence => fence >= 0)];
		int[] cornerFences = [.. InitialiseCornerConnections(fenceIndex, BoardSize - 1).Where(fence => fence >= 0)];

		// Add every adjacent fence and corner fence in both directions
		foreach (int fenceDirection in Bits)
		{
			int mappedFenceDirection = fenceDirection == 0 ? 1 : -1;

			surroundingFences.AddRange(adjFences.Select(fence => mappedFenceDirection * fence));
			surroundingFences.AddRange(cornerFences.Select(fence => mappedFenceDirection * fence));
		}

		return [.. surroundingFences.Distinct()];
	}
}
