using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

[GlobalClass]
public partial class QLearningAlgorithm : Node
{
    private Node SignalManager;
    private string defaultSavePath = "data/q_table.json";
    private Dictionary<StateKey, Dictionary<string, float>> QTable = [];

    private float epsilon = 1.0f;
    private float minEpsilon = 0.05f;
    private float epsilonDecay = 0.999f; // or something like 0.99

    private float learningRate = 0.5f;   // 10% weight on new information
    private float discountFactor = 0.9f; // Discount factor for future rewards

    public override void _Ready() => SignalManager = GetNode("/root/SignalManager");

	public void TrainQAgent(int episodes)
	{
        // Load the Q-table from the default path if QTable is empty
        if (QTable.Count == 0) LoadQTable(defaultSavePath);

		for (int episode = 0; episode < episodes; episode++)
		{
            epsilon = Math.Max(minEpsilon, epsilon * epsilonDecay);
            TrainSingleEpisode(false);
		}

        epsilon = 1.0f;
        GD.Print("Wagwan");
        SignalManager.EmitSignal("training_finished");
	}

    public async void TrainSingleEpisode(bool simulate)
    {
        BoardState board = new();
        board.Initialise();
        int currentPlayer = 0;

        while (!board.IsGameOver())
        {
            StateKey stateKey = board.GetStateKey();
            string action = ChooseAction(stateKey, currentPlayer, board);

            if (action == "")
            {
                break;
            }

            if (simulate) 
            {
                SignalManager.EmitSignal("move_selected", action);
                await ToSignal(GetTree().CreateTimer(0.1f), "timeout");
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
        if (string.IsNullOrEmpty(filePath)) filePath = defaultSavePath;
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
        if (QTable.Count > 0) return;

        if (string.IsNullOrEmpty(filePath)) filePath = defaultSavePath;
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

