using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[GlobalClass]
public partial class MiniMaxAlgorithm : Node
{
	public int START_DEPTH = 1;
	private int nodesVisited = 0;
	private ulong startTime;
	private bool debugging = false;

	Window Console;

	public void SetMaxDepth(int turns_played) => START_DEPTH = turns_played <= 2 ? 1 : 3;

	public string GetMove(BoardState board, int currentPlayer)
	{
		// Debugging ---
		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		nodesVisited = 1;
		Stopwatch stopwatch = new();
		stopwatch.Start();
		// Debugging ---
		
		ValueTuple<int, string> bestMoveTuple = MiniMax(board, START_DEPTH, currentPlayer, currentPlayer, int.MinValue, int.MaxValue);
		string bestMove = bestMoveTuple.Item2;
		
		// Debugging ---
		stopwatch.Stop();
		long milliseconds = (long)(stopwatch.ElapsedTicks * (1_000.0 / Stopwatch.Frequency));
		Console.Call("add_entry", $"Best Move: {bestMove}, Value: {bestMoveTuple.Item1}", 0);
		Console.Call("add_entry", $"Nodes Visited: {nodesVisited}, Time: {milliseconds}ms", 0);
		// Debugging ---

		return bestMove;
	}

	private (int v, string m) MiniMax(BoardState board, int depth, int currentPlayer, int maximisingPlayer, int alpha, int beta)
	{
		nodesVisited++;

		// Check for winner before evaluating, and before expanding further
		if (board.IsWinner(1 - currentPlayer)) return (int.MaxValue, board.GetLastMove());
		if (board.IsWinner(currentPlayer)) return (int.MinValue, board.GetLastMove());
		if (depth == 0) return (board.EvaluateBoard(maximisingPlayer), board.GetLastMove());

		IllegalFenceCheck.GetIllegalFences(board, currentPlayer);
		string[] moves = [.. board.GetAllMovesSmart(currentPlayer)];

		bool isMaximising = currentPlayer == maximisingPlayer;
		int bestValue = isMaximising ? int.MinValue : int.MaxValue;

		Dictionary<string, int> moveScores = [];

		// Recursively call MiniMax for each move for the current player
		foreach (string move in moves)
		{
			BoardState newBoard = board.Clone();
			newBoard.AddMove(move);

			int value = MiniMax(newBoard, depth - 1, 1 - currentPlayer, maximisingPlayer, alpha, beta).v;
			newBoard.Free();

			moveScores[move] = value;

			// Update best value
			bestValue = isMaximising ? Math.Max(bestValue, value) : Math.Min(bestValue, value);

			// Alpha-beta pruning
			if (isMaximising) alpha = Math.Max(alpha, value);
			else beta = Math.Min(beta, value);

			if (beta <= alpha) break;
		}

		// Filter dictionary to only include moves with the best value
		moveScores = moveScores.Where(x => x.Value == bestValue).ToDictionary(x => x.Key, x => x.Value);

		return (bestValue, moveScores.Keys.ElementAt(Helper.Random.Next(0, moveScores.Count)));
	}
}
