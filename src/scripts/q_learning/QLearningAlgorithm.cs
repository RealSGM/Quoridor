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
    private string defaultSavePath = "data/q_table.json";
    private Dictionary<StateKey, Dictionary<string, float>> QTable = new();

    private float epsilon = 1.0f;
    private float minEpsilon = 0.05f;
    private float epsilonDecay = 0.999f; // or something like 0.99

    private float learningRate = 0.5f;   // 10% weight on new information
    private float discountFactor = 0.9f; // Discount factor for future rewards

    private float pruneThreshold = 0; // Threshold for pruning Q-table entries
    private int pruneInterval = 100; // Interval for pruning Q-table entries
    private int pruneCounter = 0; // Counter for pruning Q-table entries

	public void TrainQAgent(int episodes)
	{
        // Load the Q-table from the default path if QTable is empty
        if (QTable.Count == 0) LoadQTable(defaultSavePath);

        GD.Print($"Starting training with {episodes} episodes...");
        epsilon = 1.0f; // Reset epsilon for training

		for (int episode = 0; episode < episodes; episode++)
		{
            epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
            TrainSingleEpisode();
		}

        SaveQTable(defaultSavePath);
	}

    private void TrainSingleEpisode()
    {
        BoardState board = new();
        board.Initialise();
        int currentPlayer = 0;
        pruneCounter++;

        if (pruneCounter >= pruneInterval)
        {
            PruneQTable(pruneThreshold);
            pruneCounter = 0;
        }

        while (!board.IsGameOver())
        {
            StateKey stateKey = board.GetStateKey();
            string action = ChooseAction(stateKey, currentPlayer, board);

            if (action == "")
            {
                GD.Print("No valid moves available.");
                GD.Print($"StateKey: {stateKey}");
                GD.Print($"Current Player: {currentPlayer}");
                GD.Print($"Player 0 Position: {board.GetPawnTile(0)}");
                GD.Print($"Player 1 Position: {board.GetPawnTile(1)}");
                Helper.PrintBitBoard(board.GetFences()[0]);
                Helper.PrintBitBoard(board.GetFences()[1]);

                int[] adjacentTiles = board.GetAdjacentTiles(board.GetPawnTile(currentPlayer));
                GD.Print($"Adjacent Tiles: {string.Join(", ", adjacentTiles)}");
                break;
            }

            BoardState newBoard = board.Clone();
            newBoard.AddMove(action);
            StateKey nextStateKey = newBoard.GetStateKey();

            float reward = GetReward(currentPlayer, newBoard, board);
            float maxFutureQ = GetMaxQValue(nextStateKey, board, 1 - currentPlayer);
            float oldQ = GetQValue(stateKey, action);

            float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);

            // Safely update Q-value
            if (!QTable.ContainsKey(stateKey))
                QTable[stateKey] = [];
            QTable[stateKey][action] = newQ;

            board = newBoard;
            currentPlayer = 1 - currentPlayer;
            
            IllegalFenceCheck.GetIllegalFences(board, currentPlayer);
        }
    }

    #region Training ---

    public string ChooseAction(StateKey stateKey, int currentPlayer, BoardState board)
    {
        string[] allMoves = board.GetAllMoves(currentPlayer);

        if (allMoves.Length == 0) return "";

        // Exploration
        if (Helper.Random.NextDouble() < epsilon) return allMoves[Helper.Random.Next(allMoves.Length)];

        float maxQ = allMoves.Max(action => GetQValue(stateKey, action));
        var bestMoves = allMoves.Where(action => GetQValue(stateKey, action) == maxQ).ToArray();
        return bestMoves[Helper.Random.Next(bestMoves.Length)];
    }

    public float GetQValue(StateKey stateKey, string action) => 
        QTable.TryGetValue(stateKey, out var actionDict) && 
        actionDict.TryGetValue(action, out var qVal) 
            ? qVal 
            : 0f;

    public float GetMaxQValue(StateKey stateKey, BoardState board, int currentPlayer)
    {
        string[] allMoves = board.GetAllMoves(currentPlayer);
        if (allMoves.Length == 0) return 0f;

        return allMoves.Max(action => GetQValue(stateKey, action));
    }

    public static float GetReward(int currentPlayer, BoardState board, BoardState prevBoard)
    {
        if (board.IsGameOver())
            return board.IsWinner(currentPlayer) ? 100f : -100f;

        float reward = 0f;

        int[] oldOpponentPath = Algorithms.GetPathToGoal(prevBoard, 1 - currentPlayer);
        int[] newOpponentPath = Algorithms.GetPathToGoal(board, 1 - currentPlayer);
        reward += (newOpponentPath.Length - oldOpponentPath.Length) * 0.5f;

        return reward;
    }


    #endregion Training ---

    #region QTable Management ---

    public void SaveQTable(string filePath)
    {
        var saveData = new Dictionary<string, Dictionary<string, float>>();

        foreach (var (state, actionDict) in QTable)
        {
            string stateKey = state.ToString();
            saveData[stateKey] = new Dictionary<string, float>(actionDict);
        }

        var saveOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        string json = JsonSerializer.Serialize(saveData, saveOptions);
        File.WriteAllText(filePath, json);
    }


    public void LoadQTable(string filePath)
    {
        if (!File.Exists(filePath)) return;

        string json = File.ReadAllText(filePath);

        if (string.IsNullOrEmpty(json)) return;

        var loadedData = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, float>>>(File.ReadAllText(filePath));

        QTable = loadedData.ToDictionary(
            entry => StateKey.ParseFromFile(entry.Key),
            entry => entry.Value
        );
    }

    public void PruneQTable(float threshold)
    {
        GD.Print($"Pruning Q-table with threshold: {threshold}");

        foreach (var state in QTable.Keys.ToList())
        {
            QTable[state] = QTable[state]
                .Where(pair => pair.Value > threshold)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            if (QTable[state].Count == 0)
                QTable.Remove(state);
        }
    }

    #endregion QTable Management ---
}

