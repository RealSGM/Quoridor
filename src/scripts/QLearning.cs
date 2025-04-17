using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class QLearning : Node
{
	readonly Dictionary<(string state, string action), float> qTable = [];

    private float epsilon = 0.1f; // Exploration rate


    // State Key representation
    // p0p1f0f-4f9f-2
    // p = pawn position with index of pawn
    // f = fence placement, - = vertical, + = horizontal, with fence index

	public void TrainQAgent(int episodes)
	{
		for (int episode = 0; episode < episodes; episode++)
		{
			BoardState board = new();
			int currentPlayer = 0;

            // Run the game until it's over
			while (!board.IsGameOver())
			{
				string stateKey = board.GetStateKey();
				string action = ChooseAction(stateKey, currentPlayer, board);

				BoardState newBoard = board.Clone();
				newBoard.AddMove(action);
				string nextStateKey = newBoard.GetStateKey();

				float reward = GetReward(newBoard, currentPlayer);
				float maxFutureQ = GetMaxQValue(nextStateKey);
				float oldQ = GetQValue(stateKey, action);

				float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);
				qTable[(stateKey, action)] = newQ;

				board = newBoard;
				currentPlayer = 1 - currentPlayer;
			}
		}
	}

    public string ChooseAction(string stateKey, int currentPlayer, BoardState board)
    {
        string[] allMoves = board.GetAllMoves(currentPlayer);

        if (allMoves.Length == 0) return "";

        // Exploration
        if (Helper.Random.NextDouble() < epsilon) return allMoves[Helper.Random.Next(allMoves.Length)];

        // Exploitation
        // TODO Code for random best move, if multiple best moves
        // float maxQ = allMoves.Max(action => GetQValue(stateKey, action));
        // var bestMoves = allMoves.Where(action => GetQValue(stateKey, action) == maxQ).ToArray();
        // return bestMoves[Helper.Random.Next(bestMoves.Length)];

        return allMoves.OrderByDescending(action => GetQValue(stateKey, action)).First();
    }

    public float GetQValue(string stateKey, string action) => qTable.TryGetValue((stateKey, action), out float qValue) ? qValue : 0;

    public float GetMaxQValue(string stateKey, BoardState board, int currentPlayer)
    {
        string[] allMoves = board.GetAllMoves(currentPlayer);

        if (allMoves.Length == 0) return 0f;

        return allMoves.Max(action => GetQValue(stateKey, action));
    }

    public static float GetReward(BoardState board, int currentPlayer)
    {
        if (board.IsGameOver()) return board.IsWinner(currentPlayer) ? 100f : -100f;

        return 0f; // Neutral reward for non-terminal states
    }
}
