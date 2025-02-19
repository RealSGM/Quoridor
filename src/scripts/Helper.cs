using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly int PATH_WEIGHT = 25;
	public static readonly int WALL_WEIGHT = 1;
	public static readonly int REPETITION_WEIGHT = 10;

	public static readonly int LargeValue = 1000;
	public static readonly int PlayerCount = 2;

	public static readonly int[] Bits = new int[2] {0, 1};
	public static readonly int[] CardinalDirections = new int[4] {0, 1, 2, 3};

	public static readonly int[][][] DefaultTileGridConnections = new int[][][]
	{
		new int[][] { new int[] { 0, 2 }, new int[] { 1, 3 } },
		new int[][] { new int[] { 0, 1 }, new int[] { 2, 3 } }
	};

	public static readonly List<Func<int, int, int>> AdjacentFunctions = new()
	{
		GetNorthAdjacent,
		GetEastAdjacent,
		GetSouthAdjacent,
		GetWestAdjacent
	};

	public static string GetMoveString(int index, int direction = 0) => $"{(direction == 1 ? "-" : "+")}{index}";

	public static int[] GetMoveCode(string moveCode)
	{
		int direction = moveCode[0] == '-' ? 1 : 0;
		int index = int.Parse(moveCode[1..]);

		return new int[] { index, direction };
	}

	public static int GetNorthAdjacent(int index, int size) => index >= size ? index - size : -1;

	public static int GetEastAdjacent(int index, int size) => (index + 1) % size != 0 ? index + 1 : -1;

	public static int GetSouthAdjacent(int index, int size) => index < size * (size - 1) ? index + size : -1;

	public static int GetWestAdjacent(int index, int size) => index % size != 0 ? index - 1 : -1;

	// Initalise the NESW connections for given index
	public static int[] InitialiseConnections(int index, int size) => new int[]
	{
		GetNorthAdjacent(index, size),
		GetEastAdjacent(index, size),
		GetSouthAdjacent(index, size),
		GetWestAdjacent(index, size)
	};

}
