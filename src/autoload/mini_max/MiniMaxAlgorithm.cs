using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MiniMaxAlgorithm : Node
{
	int MAX_DEPTH = 1;
	int START_DEPTH = 0;
	ulong startTime;

	[Export] int nodesVisited = 0;

	Window Console;

	public string GetBestMove(BoardState board, int currentPlayer, bool isDebugging = false)
	{
		// Setup console
		Console = GetNode<Window>("/root/Console");

		// Set first move depth to 1, as first move is super time consuming
		string moveHistory = board.GetMoveHistory();
		string[] moves = moveHistory.ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		MAX_DEPTH = moves.Length <= 1 ? 1 : 2;

		ValueTuple<int, string> bestMoveTuple = MiniMax(board, START_DEPTH, true, currentPlayer, int.MinValue, int.MaxValue);
		string bestMove = bestMoveTuple.Item2;

		return bestMove;
	}

	private (int v, string m) MiniMax(BoardState board, int depth, bool isMaximising, int currentPlayer, int alpha, int beta)
	{
		nodesVisited++;

		// Do 1 - currentPlayer to get the evaluation of the previous move
		if (depth == MAX_DEPTH || board.IsGameOver()) return (board.EvaluateBoard(currentPlayer), board.LastMove);

		int bestValue = isMaximising ? int.MinValue : int.MaxValue;
		string[] moves = board.GetAllMoves(currentPlayer);
		string bestMove = moves[0];

		// Recursively call MiniMax for each move for the current player
		foreach (string move in moves)
		{
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);
			int value = MiniMax(newBoard, depth + 1, !isMaximising, 1 - currentPlayer, alpha, beta).v;

			newBoard.Free();

			// Update best move and best value
			if ((isMaximising && value > bestValue) || (!isMaximising && value < bestValue))
			{
				bestValue = value;
				bestMove = move;
			}

			// Alpha-beta pruning
			if (isMaximising) alpha = Math.Max(alpha, value);
			else beta = Math.Min(beta, value);

			if (beta <= alpha) break;
		}

		return (bestValue, bestMove);
	}
}
