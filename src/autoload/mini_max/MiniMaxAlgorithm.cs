using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class MiniMaxAlgorithm : Node
{
	public int START_DEPTH = 1;
	private int nodesVisited = 0;
	private ulong startTime;
	private bool debugging = false;

	Window Console;

	public string GetMove(BoardState board, int currentPlayer, bool isMaximising = true, bool isDebugging = false)
	{
		// Setup console
		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		ulong startTime = Time.GetTicksMsec();
		debugging = isDebugging;
		nodesVisited = 1;

		ValueTuple<int, string> bestMoveTuple = MiniMax(board, START_DEPTH, isMaximising, currentPlayer, int.MinValue, int.MaxValue);
		string bestMove = bestMoveTuple.Item2;
		int bestValue = bestMoveTuple.Item1;

		Console.Call("add_entry", "Found Best Move in " + (Time.GetTicksMsec() - startTime) + " ms", 0);
		Console.Call("add_entry", $"Best Value: {bestValue}, Best Move: {bestMove}, Nodes visited: {nodesVisited}", 0);

		return bestMove;
	}

	public void SetMaxDepth(BoardState board)
	{
		// Set first move depth to 1, as first move is super time consuming
		string moveHistory = board.GetMoveHistory();
		string[] moves = moveHistory.ToString().Split([';'], StringSplitOptions.RemoveEmptyEntries);
		START_DEPTH = moves.Length <= 2 ? 1 : 3;
	}


	private (int v, string m) MiniMax(BoardState board, int depth, bool isMaximising, int currentPlayer, int alpha, int beta)
	{
		nodesVisited++;

		// Check for winner
		if (board.IsWinner(1 - currentPlayer)) return (int.MaxValue, board.GetLastMove());
		if (board.IsWinner(currentPlayer)) return (int.MinValue, board.GetLastMove());
		if (depth == 0) return (board.EvaluateBoard(!isMaximising, 1 - currentPlayer), board.GetLastMove());

		string[] moves = [.. board.GetAllMovesWeighted(currentPlayer).Keys];
		string bestMove = moves[0];
		int bestValue = isMaximising ? int.MinValue : int.MaxValue;

		Dictionary<string, int> moveScores = [];

		// Recursively call MiniMax for each move for the current player
		foreach (string move in moves)
		{
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);

			// Value return: Negative number better for player 1, positive number better for player 0, 0 is a neutral standing
			int value = MiniMax(newBoard, depth - 1, !isMaximising, 1 - currentPlayer, alpha, beta).v;

			moveScores[move] = value;

			if (debugging && depth == START_DEPTH) Console.Call("add_entry", $"Move: {move}, Value: {value}, Depth: {depth}, Player: {currentPlayer}, Last Move: {board.GetLastMove()}", 0);

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

		if (depth == START_DEPTH)
		{
			// Sort moves by score
			moveScores = moveScores.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

			// Print sorted moves
			foreach (var kvp in moveScores)
			{
				Console.Call("add_entry", $"Move: {kvp.Key}, Value: {kvp.Value}", 0);
			}
		}
		

		return (bestValue, bestMove);
	}

}
