using Godot;
using System;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	const int MAX_DEPTH = 3;

	[Export] int nodesVisited = 0;

	public string GetBestMove(BoardState board, bool isMaximising)
	{
		var Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		long startTime = DateTime.Now.Ticks;
		nodesVisited = 1;

		ValueTuple<int, string> bestMoveTuple = MiniMax(board, MAX_DEPTH, isMaximising, int.MinValue, int.MaxValue, 0, board.LastMove);
		int bestValue = bestMoveTuple.Item1;
		string bestMove = bestMoveTuple.Item2;

		Console.Call("add_entry", "Found Best Move in " + (DateTime.Now.Ticks - startTime) / TimeSpan.TicksPerMillisecond + " ms", 0);
		Console.Call("add_entry", $"Best Value: {bestValue}\nBest Move: {bestMove}\nNodes visited: {nodesVisited}", 0);

		return bestMove;
	}

	private (int value, string move) MiniMax(BoardState board, int depth, bool isMaximising, int alpha, int beta, int currentPlayer, string lastMove)
	{
		nodesVisited++;

		// Base cases: game over or max depth reached
		if (board.IsGameOver()) return (isMaximising ? int.MaxValue : int.MinValue, lastMove);
		if (depth == 0) return (board.EvaluateBoard(currentPlayer, lastMove), lastMove);

		string bestMove = "";
		int bestValue = isMaximising ? int.MinValue : int.MaxValue;

		foreach (string move in board.GetAllMoves())
		{
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);
			var (value, _) = MiniMax(newBoard, depth - 1, !isMaximising, alpha, beta, 1 - currentPlayer, move);

			newBoard.Free();

			// Update best move and best value
			if ((isMaximising && value > bestValue) || (!isMaximising && value < bestValue))
			{
				bestValue = value;
				bestMove = move;
			}

			// Alpha-beta pruning
			if (isMaximising) alpha = Math.Max(alpha, bestValue);
			else beta = Math.Min(beta, bestValue);

			if (beta <= alpha) break; // Prune search
		}

		return (bestValue, bestMove);
	}
}
