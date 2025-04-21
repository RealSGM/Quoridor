using System;
using Godot;

[GlobalClass]
[Serializable]
public partial class ParsedMove(byte player, char moveType, char direction, sbyte index, sbyte previousIndex = -1) : Node
{
    public byte Player { get; set; } = player;
    public char MoveType { get; set; } = moveType;
    public char Direction { get; set; } = direction;
    public sbyte Index { get; set; } = index;
    public sbyte PreviousIndex { get; set; } = previousIndex;

    public ParsedMove Clone() => new(Player, MoveType, Direction, Index, PreviousIndex);
    public string GetMoveCodeAsString()
    {
        string moveCode = $"{Player}{MoveType}{(Direction == '-' ? "-" : "+")}{Math.Abs(Index)}";

        if (PreviousIndex != -1)
            moveCode += $"_{PreviousIndex}";

        return moveCode;
    }
}
