using System;

/// <summary>
/// Represents a parsed move in the game.
/// The move consists of a player number, move type, direction, index, and an optional previous index.
/// The move type can be 'm' for move, 'f' for place fence
/// Previous index is used for the move type 'm' to indicate the previous tile of the player.
/// The direction is either '+' or '-' indicating the direction of the move.
/// </summary>
/// <param name="player"></param>
/// <param name="moveType"></param>
/// <param name="direction"></param>
/// <param name="index"></param>
/// <param name="previousIndex"></param>
public class ParsedMove(sbyte player, char moveType, char direction, sbyte index, sbyte previousIndex = -1)
{
	public sbyte Player { get; set; } = player;
	public char MoveType { get; set; } = moveType;
	public char Direction { get; set; } = direction;
	public sbyte Index { get; set; } = index;
	public sbyte PreviousIndex { get; set; } = previousIndex;

	public ParsedMove Clone() => new(Player, MoveType, Direction, Index, PreviousIndex);
	public override string ToString()
	{
		string moveCode = $"{Player}{MoveType}{(Direction == '-' ? "-" : "+")}{Math.Abs(Index)}";

		if (PreviousIndex != -1)
			moveCode += $"_{PreviousIndex}";

		return moveCode;
	}

	public static ParsedMove Create(string code)
	{
		var (player, moveType, direction, index, previousIndex) = Helper.GetMoveCodeAsTuple(code);
		return new ParsedMove((sbyte)player,
			moveType[0],
			direction == 0 ? '+' : '-',
			(sbyte)index,
			(sbyte)(previousIndex == -1 ? -1 : previousIndex));
	}
}
