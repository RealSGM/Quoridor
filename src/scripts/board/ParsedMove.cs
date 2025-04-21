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

    #region Godot Methods

    // Static method for Godot to create an instance
    public static ParsedMove Create(int player, string moveType, string direction, int index, int previousIndex = -1) => new((byte)player, moveType[0], direction[0], (sbyte)index, (sbyte)previousIndex);
    public void SetPlayer(int player) => Player = (byte)player;
    public void SetMoveType(string moveType) => MoveType = moveType[0];
    public void SetDirection(string direction) => Direction = direction[0];
    public void SetIndex(int index) => Index = (sbyte)index;
    public void SetPreviousIndex(int previousIndex) => PreviousIndex = (sbyte)previousIndex;
    
    #endregion
}
