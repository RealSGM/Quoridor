using Godot;
using System;

public partial class MiniMaxAlgorithm : Node
{
	int MAX_DEPTH = 1;
	int START_DEPTH = 0;

	ulong startTime;
	bool debugging = false;
	int nodesVisited = 0;

	Window Console;

	public string GetMove(BoardState board, int currentPlayer, bool isMaximising = true, bool isDebugging = false)
	{
		// Setup console
		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "Creating Game Tree...", 0);
		ulong startTime = Time.GetTicksMsec();
		debugging = isDebugging;
		nodesVisited = 1;

		// Set first move depth to 1, as first move is super time consuming
		string moveHistory = board.GetMoveHistory();
		string[] moves = moveHistory.ToString().Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		MAX_DEPTH = moves.Length <= 1 ? 1 : 3;

		ValueTuple<int, string> bestMoveTuple = MiniMax(board, START_DEPTH, isMaximising, currentPlayer, int.MinValue, int.MaxValue);
		string bestMove = bestMoveTuple.Item2;
		int bestValue = bestMoveTuple.Item1;

		Console.Call("add_entry", "Found Best Move in " + (Time.GetTicksMsec() - startTime) + " ms", 0);
		Console.Call("add_entry", $"Best Value: {bestValue}, Best Move: {bestMove}, Nodes visited: {nodesVisited}", 0);

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

			if (debugging) Console.Call("add_entry", $"Move: {move}, Value: {value}, Depth: {depth}, Player: {currentPlayer}", 0);

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
