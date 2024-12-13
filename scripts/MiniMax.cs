using Godot;
using System.Linq;
using System.Collections.Generic;

[GlobalClass]
public partial class MiniMax : Node
{
	public void CreateGameTree(BoardState originalBoard)
	{
		Dictionary<int, int[]> possibleMoves = originalBoard.GetPossibleMoves();

		// Create a Tree based off moves
		Tree tree = new();


		// Loop through possibleMoves
		// Generate a BoardState for each move
		// Add to Tree
		
	}
}
