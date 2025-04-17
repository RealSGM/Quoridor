public struct Tile
{
    // Stored as [N, E, S, W]
    private int[] Connections;

    public Tile(int index)
    {
        int boardSize = Helper.BoardSize;
        Connections = Helper.InitialiseConnections(index, boardSize);
    }

    public readonly Tile Clone() => new() { Connections = (int[])Connections.Clone() };

    public readonly int[] GetConnections() => Connections;

    public readonly int[] GetOrderedConnections(int player)
    {
        return player == 0
            ? [Connections[0], Connections[1], Connections[3], Connections[2]]
            : [Connections[2], Connections[3], Connections[1], Connections[0]];
    }
}