using System;

public class ParsedMove(sbyte player, char moveType, bool isHorizontal, sbyte index, sbyte previousIndex = -1)
{
	public sbyte Player { get; set; } = player;
	public char MoveType { get; set; } = moveType;
	public bool IsHorizontal { get; set; } = isHorizontal;
	public sbyte Index { get; set; } = index;
	public sbyte PreviousIndex { get; set; } = previousIndex;

	public ParsedMove Clone() => new(Player, MoveType, IsHorizontal, Index, PreviousIndex);
	public override string ToString()
	{
		string moveCode = $"{Player}{MoveType}{(IsHorizontal ? "+" : "-")}{Math.Abs(Index)}";

		if (PreviousIndex != -1)
			moveCode += $"_{PreviousIndex}";

		return moveCode;
	}

	public static ParsedMove Create(string code)
	{
		// Split the code into parts using '_' as the delimiter
		string[] parts = code.Split('_');
		code = parts[0];

		int previousIndex = parts.Length > 1 ? int.Parse(parts[1]) : -1; // Get the previous index, used for undoing moves
		sbyte player = sbyte.Parse(code[0].ToString()); // Parse the player number
		char[] moveType = code[1].ToString().ToCharArray(); // Parse the move type
		char dir = code[2]; // Parse the direction
		sbyte index = sbyte.Parse(code[3..]); // Parse the index
		
		// Return a new ParsedMove object with the parsed values
		return new ParsedMove(
			player,
			moveType[0],
			dir == '+',
			index,
			(sbyte)(previousIndex == -1 ? -1 : previousIndex));
	}

	public (int player, string moveType, int dir, int index, int previousIndex) GetMoveAsTuple() => (Player, MoveType.ToString(), IsHorizontal ? 0 : 1, Index, PreviousIndex);
}