using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly Random Random = new();
	public const int PATH_WEIGHT = 20;
	public const int FENCE_WEIGHT = 20;
	public const int MaxFences = 10;
	public const int BoardSize = 9;

	public static readonly int[] Bits = [0, 1];

	public static readonly int[][][] DefaultTileGridConnections =
	[
		[[0, 2], [1, 3]],
		[[0, 1], [2, 3]]
	];

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
}
