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
		// Template: 0m+67_76
		string[] parts = code.Split('_');
		int previousIndex = parts.Length > 1 ? int.Parse(parts[1]) : -1;

		code = parts[0];	
		sbyte player = sbyte.Parse(code[0].ToString());
		char[] moveType = code[1].ToString().ToCharArray();
		char dir = code[2];
		sbyte index = sbyte.Parse(code[3..]);
		
		return new ParsedMove(
			player,
			moveType[0],
			dir == '+',
            index,
			(sbyte)(previousIndex == -1 ? -1 : previousIndex));
	}

	public (int player, string moveType, int dir, int index, int previousIndex) GetMoveAsTuple() => (Player, MoveType.ToString(), IsHorizontal ? 0 : 1, Index, PreviousIndex);
}