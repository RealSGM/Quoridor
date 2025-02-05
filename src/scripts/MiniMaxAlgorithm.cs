using Godot;
using System.Linq;
using System.Collections.Generic;
using System;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	const int MAX_DEPTH = 2;

	[Export] int nodesVisited = 0;

	public string GetBestMove(BoardState board, bool isMaximising)
	{
		var Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		long startTime = DateTime.Now.Ticks;
		nodesVisited = 1;


		string[] possibleMoves = board.GetAllMoves();
		string bestMove = "";
		int bestValue = isMaximising ? int.MinValue : int.MaxValue;

		foreach (string move in possibleMoves)
		{
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);

			int value = MiniMax(newBoard, MAX_DEPTH, !isMaximising, int.MinValue, int.MaxValue);

			if ((isMaximising && value > bestValue) || (!isMaximising && value < bestValue))
			{
				bestValue = value;
				bestMove = move;
			}
		}

		Console.Call("add_entry", "Found Best Move in " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + " ms", 0);
		Console.Call("add_entry", "Best Move: " + bestMove, 0);
		Console.Call("add_entry", "Nodes visited: " + nodesVisited, 0);

		return bestMove;
	}

	public int MiniMax(BoardState board, int depth, bool isMaximising, int alpha, int beta)
	{
		nodesVisited++;

		if (depth == 0 || board.GetWinner(board.CurrentPlayer))
		{
			return EvaluateBoard(board);
		}

		// Get all possible moves
		IllegalFenceCheck illegalFenceCheck = GetParent().GetNode<IllegalFenceCheck>("IllegalFenceCheck");
		illegalFenceCheck.GetIllegalFences(board);
		
		string[] moves = board.GetAllMoves();

		return EvaluateMoves(moves, board, depth, isMaximising, isMaximising ? int.MinValue : int.MaxValue, alpha, beta, isMaximising ? Math.Max : Math.Min);
	}

	public int EvaluateMoves(string[] moves, BoardState board, int current_depth, bool isMaximising, int pruningValue, int alpha, int beta, Func<int, int, int> pruningFunction)
	{
		int evaluation = isMaximising ? int.MinValue : int.MaxValue;

		foreach (string move in moves)
		{
			// Clone the board and add the move
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);

			// Recusively Evaluate
			int eval = MiniMax(newBoard, current_depth - 1, !isMaximising, alpha, beta);

			// Update evaluation
			evaluation = pruningFunction(evaluation, eval);
			pruningValue = pruningFunction(pruningValue, eval);

			if (beta <= alpha) break;
		}

		return evaluation;
	}

    private int EvaluateBoard(BoardState board)
    {
		return 0;
    }

}
