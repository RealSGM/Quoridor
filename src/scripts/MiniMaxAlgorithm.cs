using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	const int MAX_DEPTH = 2;

	public string CreateGameTree(BoardState originalBoard)
	{
		var Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		long startTime = DateTime.Now.Ticks;

		// Create a Tree based off moves
		TreeNode root = new(originalBoard.MoveHistory.ToString());
		// Recursively create subtrees
		string move =  CreateSubTree(originalBoard, root, 0, MAX_DEPTH);

		Console.Call("add_entry", "Game Tree created in " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + " ms", 0);

		return move;
	}


	public string CreateSubTree(BoardState board, TreeNode parent, int currentDepth, int maxDepth)
	{
		if (currentDepth >= maxDepth) return "";

		// Get all possible moves
		IllegalFenceCheck illegalFenceCheck = GetParent().GetNode<IllegalFenceCheck>("IllegalFenceCheck");
		illegalFenceCheck.GetIllegalFences(board);
		string allMoves = board.GetAllMoves();

		// Split the moves into individual moves and remove the last empty string
		string[] moves = allMoves.Split(';', StringSplitOptions.RemoveEmptyEntries);

		foreach (string move in moves)
		{
			string moveHistory = board.GetMoveHistory() + move;
			TreeNode child = new(parent, moveHistory);
			parent.AddChild(child);

			// // Create a new board state
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);

			// Recursively create subtrees
			CreateSubTree(board, child, currentDepth + 1, maxDepth);
		}

		// return random move in moves
		// Pick random nuber between 0 and moves.Length
		if (moves.Length > 0)
		{
			Random random = new();
			int randomIndex = random.Next(0, moves.Length);
			return moves[randomIndex];
		}
		return "";
	}
}
