using System.Linq;
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

		ulong[] fences = board.GetAllFences(currentPlayer);

		string[] pawnMoves = [.. board
			.GetReachableTilesSmart(currentPlayer)
			.Select(tile => Helper.GetMoveCodeAsString(currentPlayer, "m", 0, tile))]; // Get all pawn moves
		string[] fenceMoves = [.. Helper.Bits
			.SelectMany(dir => Helper.GetOnesInBitBoard(fences[dir])
			.Select(i => Helper.GetMoveCodeAsString(currentPlayer, "f", dir, i)))]; // Get all fence moves

		bool isFence = Helper.Random.NextDouble() > 0.5f && board.HasFences(currentPlayer);
		string[] chosenMoveList = isFence ? fenceMoves : pawnMoves;
		int randomIndex = Helper.Random.Next(chosenMoveList.Length);
		string bestMove = chosenMoveList[randomIndex];
		int isPawn = bestMove.Contains('m') ? 1 : 0;

		SignalManager.EmitSignal("move_selected", bestMove);
		SignalManager.EmitSignal("data_collected", this, "moves_made_cumulative", 1);
		SignalManager.EmitSignal("data_collected", this, "current_turn", 1);
		SignalManager.EmitSignal("data_collected", this, "nodes_searched_cumulative", 0);
		SignalManager.EmitSignal("data_collected", this, "move_speeds_cumulative", 0);
		SignalManager.EmitSignal("data_collected", this, "pawn_moves_cumulative", isPawn);
	}
}
