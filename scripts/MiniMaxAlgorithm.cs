using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	const int MAX_DEPTH = 2;

	public void CreateGameTree(BoardState originalBoard)
	{
		var Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		long startTime = DateTime.Now.Ticks;

		// Create a Tree based off moves
		TreeNode root = new(originalBoard.MoveHistory.ToString());
		// Recursively create subtrees
		CreateSubTree(originalBoard, root, 0, MAX_DEPTH);

		Console.Call("add_entry", "Game Tree created in " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + " ms", 0);
	}


	public void CreateSubTree(BoardState board, TreeNode parent, int currentDepth, int maxDepth)
	{
		if (currentDepth >= maxDepth) return;

		// Get all possible moves
		string allMoves = board.GetAllMoves();

		// Split the moves into individual moves and remove the last empty string
		string[] moves = allMoves.Split(';', StringSplitOptions.RemoveEmptyEntries);

		foreach (string move in moves)
		{
			string moveHistory = board.GetMoveHistory() + move;
			TreeNode child = new(parent, moveHistory);
			parent.AddChild(child);

			// // Create a new board state
			// BoardState newBoard = board.Clone();
			// newBoard.AddMove(move);
			// float score = newBoard.EvaluateBoard();

			// Recursively create subtrees
			CreateSubTree(board, child, currentDepth + 1, maxDepth);
		}
	}
}
