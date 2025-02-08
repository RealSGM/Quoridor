using Godot;
using System;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	const int MAX_DEPTH = 1;

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

			int value = MiniMax(newBoard, MAX_DEPTH, !isMaximising, int.MinValue, int.MaxValue, board.CurrentPlayer, move);

			Console.Call("add_entry", "Move: " + move + " Value: " + value, 0);

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

	private int MiniMax(BoardState board, int depth, bool isMaximising, int alpha, int beta, int currentPlayer, string lastMove)
	{
		nodesVisited++;

		// Return max or min value if game is over
		if (board.IsGameOver()) return isMaximising ? int.MaxValue : int.MinValue;

		// Return evaluation of board if max depth is reached
		if (depth == 0) return board.EvaluateBoard(currentPlayer, lastMove);

		string[] possibleMoves = board.GetAllMoves();

		if (isMaximising)
		{
			int bestValue = int.MinValue;
			
			foreach (string move in possibleMoves)
			{
				BoardState newBoard = board.Clone();
				newBoard.AddMove(move);

				int value = MiniMax(newBoard, depth - 1, !isMaximising, alpha, beta, 1 - currentPlayer, move);
				bestValue = Math.Max(bestValue, value);
				alpha = Math.Max(alpha, value);

				newBoard.Free();

				if (beta <= alpha)
				{
					break;
				}
			}

			return bestValue;
		}
		else
		{
			int bestValue = int.MaxValue;

			foreach (string move in possibleMoves)
			{
				BoardState newBoard = board.Clone();
				newBoard.AddMove(move);

				int value = MiniMax(newBoard, depth - 1, !isMaximising, alpha, beta, 1 - currentPlayer, move);
				bestValue = Math.Min(bestValue, value);
				beta = Math.Min(beta, value);

				newBoard.Free();

				if (beta <= alpha)
				{
					break;
				}
			}

			return bestValue;
		}
	}
}
