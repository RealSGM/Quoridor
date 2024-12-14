using Godot;
using System.Linq;
using System.Collections.Generic;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	public void CreateGameTree(BoardState originalBoard)
	{
		// Get all possible moves
		Dictionary<int, int[]> possibleMoves = originalBoard.GetPossibleMoves();

		// Create a Tree based off moves
		TreeNode root = new(originalBoard.MoveHistory.ToString());
		int currentPlayer = originalBoard.CurrentPlayer;

		foreach (KeyValuePair<int, int[]> move in possibleMoves)
		{
			foreach (int index in move.Value)
			{
				// Get the move code
				string moveCode = GetMoveCode(move, currentPlayer, index);
				string moveHistory = originalBoard.MoveHistory.ToString() + moveCode;
				
				TreeNode child = new(root, moveHistory);
				root.AddChild(child);

				// Create a new BoardState and make the move
				// BoardState newBoard = originalBoard.Clone();
				// if (move.Key == 2) newBoard.MovePawn(index, currentPlayer);
				// else newBoard.PlaceFence(index, move.Key, currentPlayer);
			}
		}
	}

	public string GetMoveCode(KeyValuePair<int, int[]> move, int currentPlayer, int index)
	{
		// Code: currentPlayer, m, value;
		if (move.Key == 2) return $"{currentPlayer},m,{index}";
		// Code: currentPlayer, p, value
		else return $"{currentPlayer},p,{index}";
	}

	public void MiniMax(string moveCode)
	{
		// Generate GameState from moveCode
	
	}

	public void Evaluate()
	{
		
	}

}
