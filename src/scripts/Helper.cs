using Godot;

[GlobalClass]
public partial class Helper : Node
{
	public static readonly int PlayerCount = 2;

	public static readonly int[] Bits = new int[2] {0, 1};

	public static readonly int[][][] DefaultTileGridConnections = new int[][][]
	{
		new int[][] { new int[] { 0, 2 }, new int[] { 1, 3 } },
		new int[][] { new int[] { 0, 1 }, new int[] { 2, 3 } }
	};

	// Converts fence direction which is [0, 1] to [-1, 1] for notation
	public static int GetMappedFenceIndex(int fenceIndex, int direction) => direction == 0 ? fenceIndex : -fenceIndex;

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
