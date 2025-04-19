using System;
using Godot;

[GlobalClass]
public partial class ParsedMove(byte player, char moveType, char direction, byte index, sbyte previousIndex = -1) : Node
{
    public byte Player { get; init; } = player;
    public char MoveType { get; init; } = moveType;
    public char Direction { get; init; } = direction;
    public byte Index { get; init; } = index;
    public sbyte PreviousIndex { get; init; } = previousIndex;

    public ParsedMove Clone() => new(Player, MoveType, Direction, Index, PreviousIndex);

    public string GetMoveCodeAsString()
    {
        string moveCode = $"{Player}{MoveType}{(Direction == '-' ? "-" : "+")}{Math.Abs(Index)}";

        if (PreviousIndex != -1)
            moveCode += $"_{PreviousIndex}";

        return moveCode;
    }
}
