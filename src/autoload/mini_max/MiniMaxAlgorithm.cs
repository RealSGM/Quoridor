using Godot;
using System;
using System.Diagnostics;

public partial class MiniMaxAlgorithm : Node
{
	int MAX_DEPTH = 2;

	[Export] int nodesVisited = 0;

	Window Console;

	public string GetBestMove(BoardState board, int currentPlayer)
	{
		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		ulong startTime = Time.GetTicksMsec();
		nodesVisited = 1;

		// Set first move depth to 1, as first move is super time consuming
		string moveHistory = board.GetMoveHistory();
		string[] moves = moveHistory.ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		MAX_DEPTH = moves.Length <= 1 ? 1 : 2;

		// Set MAX_DEPTH to higher value depending on the number of moves available / length into the game	

		ValueTuple<int, string> bestMoveTuple = MiniMax(board, 0, true, int.MinValue, int.MaxValue, 1 - currentPlayer, board.LastMove);
		int bestValue = bestMoveTuple.Item1;
		string bestMove = bestMoveTuple.Item2;


		Console.Call("add_entry", "Found Best Move in " + (Time.GetTicksMsec() - startTime) + " ms", 0);
		Console.Call("add_entry", $"Best Value: {bestValue}\nBest Move: {bestMove}\nNodes visited: {nodesVisited}", 0);

		return bestMove;
	}

	private (int value, string move) MiniMax(BoardState board, int depth, bool isMaximising, int alpha, int beta, int currentPlayer, string lastMove)
	{
		nodesVisited++;

		if (board.IsGameOver()) return board.GetWinner(currentPlayer) ? (int.MaxValue, lastMove) : (int.MinValue, lastMove);

		if (depth == MAX_DEPTH) return (board.EvaluateBoard(isMaximising, currentPlayer, lastMove), lastMove);

		string bestMove = "";
		int bestValue = isMaximising ? int.MinValue : int.MaxValue;
		string[] moves = board.GetAllMoves(1 - currentPlayer); // Future me, do not change this to currentPlayer

		foreach (string move in moves)
		{
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);
			var (value, _) = MiniMax(newBoard, depth + 1, !isMaximising, alpha, beta, 1 - currentPlayer, move);

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
