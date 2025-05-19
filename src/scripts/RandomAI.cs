using Godot;

[GlobalClass]
public partial class RandomAI : Node
{
	Window Console;
	Node SignalManager;

	public override void _Ready()
	{
		Console = GetNode<Window>("/root/Console");
		SignalManager = GetNode<Node>("/root/SignalManager");
		Console.Call("add_entry", "MiniMaxAlgorithm ready", 0);
	}

	public void GetMove(BoardWrapper wrapper, int currentPlayer)
	{
		BoardState board = wrapper.State.Clone();
		string[] possibleMoves = board.GetAllMoves(currentPlayer);
		int randomIndex = Helper.Random.Next(possibleMoves.Length);

		string bestMove = possibleMoves[randomIndex];
		int pawnMoves = bestMove.Contains('m') ? 1 : 0;

		SignalManager.EmitSignal("move_selected", bestMove);
		SignalManager.EmitSignal("data_collected", this, "moves_made_cumulative", 1);
		SignalManager.EmitSignal("data_collected", this, "current_turn", 1);
		SignalManager.EmitSignal("data_collected", this, "nodes_searched_cumulative", 0);
		SignalManager.EmitSignal("data_collected", this, "move_speeds_cumulative", 0);
		SignalManager.EmitSignal("data_collected", this, "pawn_moves_cumulative", pawnMoves);
	}
}
