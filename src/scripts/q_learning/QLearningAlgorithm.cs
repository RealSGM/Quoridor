using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

[GlobalClass]
public partial class QLearningAlgorithm : Node
{
	// private string defaultSavePath = "data/q_table.json";

	// private float epsilon = 1.0f;
	// private float minEpsilon = 0.05f;
	// private float epsilonDecay = 0.999f; // or something like 0.99

	// private float learningRate = 0.5f;   // 10% weight on new information
	// private float discountFactor = 0.9f; // Discount factor for future rewards

	// private float pruneThreshold = 0; // Threshold for pruning Q-table entries
	// private int pruneInterval = 100; // Interval for pruning Q-table entries
	// private int pruneCounter = 0; // Counter for pruning Q-table entries

	public Dictionary<StateKey, Dictionary<ParsedMove, float>> QTable = [];

	// public void TrainQAgent(int episodes)
	// {
	//     // Load the Q-table from the default path if qTable is empty
	//     if (qTable.IsEmpty)
	//     {
	//         GD.Print($"Loading Q-table from {defaultSavePath}...");
	//         LoadQTable(defaultSavePath);
	//     } 

	//     GD.Print($"Starting training with {episodes} episodes...");

	//     epsilon = 1.0f; // Reset epsilon for training

	// 	for (int episode = 0; episode < episodes; episode++)
	// 	{
	//         epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
	//         TrainSingleEpisode();
	// 	}

	//     GD.Print("Training completed.");
	//     SaveQTable(defaultSavePath);
	// }

	// private void TrainSingleEpisode(BoardState boardState, int player, float alpha, float gamma, float epsilon)
	// {
	//     BoardState board = boardState.Clone();
	//     int currentPlayer = player;

	//     while (!board.IsGameOver())
	//     {
	//         // Get the current state
	//         StateKey stateKey = board.GetStateKey();

	//         // Choose an action using epsilon-greedy strategy
	//         ParsedMove action = SelectAction(stateKey, epsilon);

	//         // Perform the action and get the reward
	//         float reward = board.PerformAction(action, currentPlayer);

	//         // Get the next state
	//         StateKey nextStateKey = board.GetStateKey();

	//         // Update Q-value using the Bellman equation
	//         UpdateQValue(stateKey, action, reward, nextStateKey, alpha, gamma);

	//         // Switch players
	//         currentPlayer = 1 - currentPlayer;
	//     }   
	// }

	// private ParsedMove SelectAction(StateKey key, float epsilon)
	// {
	//     var legalMoves = QTable[key].Keys.ToList();
	//     if (Helper.Random.NextDouble() < epsilon) return legalMoves[Helper.Random.Next(legalMoves.Count)];
	//     return legalMoves.OrderByDescending(m => QTable[key][m]).First();
	// }
}
