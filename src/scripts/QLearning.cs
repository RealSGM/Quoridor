using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

[GlobalClass]
public partial class QLearning : Node
{
    private string defaultSavePath = "data/q_table.json";
	private ConcurrentDictionary<(string state, string action), float> qTable = [];
    private IllegalFenceCheck illegalFenceChecker = new();

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
        GD.Print("Training Q-learning agent...");

        LoadQTable(defaultSavePath);

        epsilon = 1.0f; // Reset epsilon for training


		for (int episode = 0; episode < episodes; episode++)
		{
            GD.Print($"Episode {episode + 1}/{episodes}");
            epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
            TrainSingleEpisode();
		}

        GD.Print("Training completed.");
        SaveQTable(defaultSavePath);
	}

    private void TrainSingleEpisode()
    {
        BoardState board = new();
        board.InitialiseBoard();
        int currentPlayer = 0;
        pruneCounter++;

        if (pruneCounter >= pruneInterval)
        {
            PruneQTable(pruneThreshold);
            pruneCounter = 0;
        }

        while (!board.IsGameOver())
        {
            string stateKey = board.GetStateKey();
            string action = ChooseAction(stateKey, currentPlayer, board);

            BoardState newBoard = board.Clone();
            newBoard.AddMove(action);
            string nextStateKey = newBoard.GetStateKey();

            float reward = GetReward(currentPlayer, newBoard, board);
            float maxFutureQ = GetMaxQValue(nextStateKey, board, 1 - currentPlayer);
            float oldQ = GetQValue(stateKey, action);

            float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);

            // Safely update Q-value
            qTable.AddOrUpdate((stateKey, action), newQ, (key, existing) => newQ);

            board = newBoard;
            currentPlayer = 1 - currentPlayer;
            
            illegalFenceChecker.GetIllegalFences(board, currentPlayer);
        }
    }

    #region Training ---

    public string ChooseAction(string stateKey, int currentPlayer, BoardState board)
    {
        string[] allMoves = board.GetAllMovesSmart(currentPlayer);

        if (allMoves.Length == 0) return "";

        // Exploration
        if (Helper.Random.NextDouble() < epsilon) return allMoves[Helper.Random.Next(allMoves.Length)];

        float maxQ = allMoves.Max(action => GetQValue(stateKey, action));
        var bestMoves = allMoves.Where(action => GetQValue(stateKey, action) == maxQ).ToArray();
        return bestMoves[Helper.Random.Next(bestMoves.Length)];
    }

    public float GetQValue(string stateKey, string action) => qTable.TryGetValue((stateKey, action), out float qValue) ? qValue : 0;

    public float GetMaxQValue(string stateKey, BoardState board, int currentPlayer)
    {
        string[] allMoves = board.GetAllMoves(currentPlayer);

        if (allMoves.Length == 0) return 0f;

        return allMoves.Max(action => GetQValue(stateKey, action));
    }

    public float GetReward(int currentPlayer, BoardState board, BoardState prevBoard)
    {
        if (board.IsGameOver())
            return board.IsWinner(currentPlayer) ? 100f : -100f;

        float reward = 0f;

        int[] oldOpponentPath = Algorithms.GetShortestPath(1 - currentPlayer, prevBoard);
        int[] newOpponentPath = Algorithms.GetShortestPath(1 - currentPlayer, board);
        reward += (newOpponentPath.Length - oldOpponentPath.Length) * 0.5f;

        return reward;
    }


    #endregion Training ---

    #region QTable Management ---

    public void SaveQTable(string filePath)
    {
        var saveData = new Dictionary<string, float>();
        foreach (var kvp in qTable)
        {
            string key = $"{kvp.Key.state};{kvp.Key.action}";
            saveData[key] = kvp.Value;
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

        if (string.IsNullOrWhiteSpace(json)) return;

        GD.Print($"Loading Q-table from {filePath}...");

        var loadedData = JsonSerializer.Deserialize<Dictionary<string, float>>(json);

        qTable = new ConcurrentDictionary<(string state, string action), float>(
            loadedData.ToDictionary(
                kvp => (kvp.Key.Split(';')[0], kvp.Key.Split(';')[1]),
                kvp => kvp.Value
            )
        );
    }

    public void PruneQTable(float threshold)
    {
        GD.Print($"Pruning Q-table with threshold: {threshold}");
        
        if (qTable.IsEmpty) return;
        
        var keysToRemove = qTable
            .Where(kvp => kvp.Value <= threshold)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            qTable.TryRemove(key, out _);
        }
    }

    #endregion QTable Management ---
}

