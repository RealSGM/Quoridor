using Godot;
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

	private float learningRate = 0.5f;   // 10% weight on new information
	private float discountFactor = 0.9f; // Discount factor for future rewards

	public float epsilon = 0.5f; // Exploration rate
	public float simulationDelay = 0.1f;
	public bool isRunning = false;
	public bool showMoves = true;

	public override void _Ready() => SignalManager = GetNode("/root/SignalManager");

	public void GetMove(BoardWrapper wrapper, int currentPlayer)
	{
		BoardState board = wrapper.State;
		StateKey stateKey = board.GetStateKey();
		ExploreState(board, currentPlayer, stateKey);

		string bestMove = "";

		if (!QTable.TryGetValue(stateKey, out Dictionary<string, float> value))
		{
			GD.Print($"State {stateKey} not found in QTable");
			QTable[stateKey] = [];
			// State not trained for, get random move
			string[] allMoves = board.GetAllMoves(currentPlayer);
			bestMove = allMoves[Helper.Random.Next(allMoves.Length)];
		}
		else
		{
			// Get the best move based off Q Values
			string[] allMoves = [.. value.Keys];
			float maxQ = allMoves.Max(action => GetQValue(stateKey, action));
			string[] bestMoves = allMoves.Where(action => GetQValue(stateKey, action) == maxQ).ToArray();
			bestMove = bestMoves[Helper.Random.Next(bestMoves.Length)];
		}

		stateKey = null;
		int pawnMoves = bestMove.Contains('m') ? 1 : 0;

		SignalManager.EmitSignal("move_selected", bestMove);
		SignalManager.EmitSignal("data_collected", this, "moves_made_cumulative", 1);
		SignalManager.EmitSignal("data_collected", this, "current_turn", 1);
		SignalManager.EmitSignal("data_collected", this, "pawn_moves_cumulative", pawnMoves);
	}

	#region Training ---

	public async void TrainSingleEpisode()
	{
		BoardState board = new();
		board.Initialise();
		int currentPlayer = 0;

		while (!board.IsGameOver())
		{
			StateKey stateKey = board.GetStateKey();
			ExploreState(board, currentPlayer, stateKey);

			string action = ChooseAction(stateKey, currentPlayer, board);

			if (action == "") break;
			if (showMoves) SignalManager.EmitSignal("move_selected", action);
			if (simulationDelay > 0) await ToSignal(GetTree().CreateTimer(simulationDelay), "timeout");

			BoardState newBoard = board.Clone();
			newBoard.AddMove(action);
			StateKey nextStateKey = newBoard.GetStateKey();

			float reward = GetReward(currentPlayer, newBoard, board);
			float maxFutureQ = GetMaxQValue(nextStateKey, board, 1 - currentPlayer);
			float oldQ = GetQValue(stateKey, action);

			float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);
			QTable[stateKey][action] = newQ;

			board = newBoard;
			currentPlayer = 1 - currentPlayer;

			IllegalFenceCheck.GetIllegalFences(board);
		}

		SaveQTable(defaultSavePath);
		int winner = board.IsWinner(0) ? 0 : board.IsWinner(1) ? 1 : 2;
		SignalManager.EmitSignal("training_finished", winner);

		isRunning = false;
	}

	public void ExploreState(BoardState board, int currentPlayer, StateKey key)
	{
		// Check if the state is already in the QTable
		if (!QTable.TryGetValue(key, out Dictionary<string, float> value))
		{
			value = [];
			QTable[key] = value;
		}

		string[] allMoves = board.GetAllMoves(currentPlayer); // Get all possible moves for the current player

		foreach (string action in allMoves) // Loop through all possible moves
		{
			if (value.ContainsKey(action)) continue; // Skip if already trained

			BoardState newBoard = board.Clone(); // Clone the board and apply the action
			newBoard.AddMove(action);

			// Get the new state key after the action
			StateKey nextStateKey = newBoard.GetStateKey();
			float reward = GetReward(currentPlayer, newBoard, board);
			float maxFutureQ = GetMaxQValue(nextStateKey, newBoard, 1 - currentPlayer);
			float oldQ = GetQValue(key, action);
			float newQ = oldQ + learningRate * (reward + discountFactor * maxFutureQ - oldQ);
			value[action] = newQ;
		}
	}

	public string ChooseAction(StateKey stateKey, int currentPlayer, BoardState board)
	{
		ulong[] fences = board.GetAllFences(currentPlayer);

		string[] allMoves = board.GetAllMoves(currentPlayer); // Get all possible moves for the current player
		string[] pawnMoves = [.. board
			.GetReachableTilesSmart(currentPlayer)
			.Select(tile => Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tile))]; // Get all pawn moves
		string[] fenceMoves = [.. Helper.Bits
			.SelectMany(dir => Helper.GetOnesInBitBoard(fences[dir])
			.Select(i => Helper.GetMoveCodeAsString(currentPlayer, "f", dir, i)))]; // Get all fence moves

		// Exploitation - Pick the move wiith the highest Q value
		if (Helper.Random.NextDouble() > epsilon)
		{
			float maxQ = allMoves.Max(action => GetQValue(stateKey, action));
			string[] bestMoves = [.. allMoves.Where(action => GetQValue(stateKey, action) == maxQ)];
			return bestMoves[Helper.Random.Next(bestMoves.Length)];
		}

		// Exploration - Pick a random move, 50% chance to pick a fence move
		if (fenceMoves.Length > 0 && Helper.Random.NextDouble() > 0.5f)
			return fenceMoves[Helper.Random.Next(fenceMoves.Length)];

		return pawnMoves[Helper.Random.Next(pawnMoves.Length)];
	}

	public static float GetReward(int currentPlayer, BoardState board, BoardState prevBoard)
	{
		if (board.IsGameOver()) return board.IsWinner(currentPlayer) ? 100f : -100f; // Game over, reward based on winner

		int[] oldOpponentPath = Algorithms.GetPathToGoal(prevBoard, 1 - currentPlayer);
		int[] newOpponentPath = Algorithms.GetPathToGoal(board, 1 - currentPlayer);

		int[] oldPlayerPath = Algorithms.GetPathToGoal(prevBoard, currentPlayer);
		int[] newPlayerPath = Algorithms.GetPathToGoal(board, currentPlayer);

		float reward = (oldPlayerPath.Length - newPlayerPath.Length) * 0.25f;
		reward += (newOpponentPath.Length - oldOpponentPath.Length) * 0.5f;

		return reward;
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
		foreach (var state in QTable.Keys.ToList())
		{
			QTable[state] = QTable[state]
				.Where(pair => pair.Value > threshold)
				.ToDictionary(pair => pair.Key, pair => pair.Value);

			if (QTable[state].Count == 0)
				QTable.Remove(state);
		}
	}

	public void FreeQTable()
	{
		QTable.Clear();
		QTable = [];
	}

	#endregion QTable Management ---
}
