using Godot;
using System.Linq;

[GlobalClass]
public partial class MCTSAlgorithm : Node
{
	private const int SIMULATIONS = 1000;  // Increased simulations for better decision-making
	private const double EXPLORATION_CONSTANT = 1.41; // sqrt(2), UCB1 exploration factor

	Window Console;
	Node SignalManager;

	public override void _Ready()
	{
		Console = GetNode<Window>("/root/Console");
		Console.Call("add_entry", "MCTSAlgorithm ready", 0);
	}

	public void GetMove(BoardState board, int currentPlayer)
	{
		Console.Call("add_entry", "Creating Game Tree...", 0);
		ulong startTime = Time.GetTicksMsec();

		MCTSNode root = new(null, board, currentPlayer);
		for (int i = 0; i < SIMULATIONS; i++)
		{
			MCTSNode selectedNode = root.SelectChild(EXPLORATION_CONSTANT);
			MCTSNode expandedNode = selectedNode.Expand();
			int simulationResult = expandedNode.Simulate(currentPlayer);
			expandedNode.Backpropagate(simulationResult);
		}
		MCTSNode bestChild = root.Children.OrderByDescending(c => c.Visits).First();

		Console.Call("add_entry", "Found Best Move in " + (Time.GetTicksMsec() - startTime) + " ms", 0);
		Console.Call("add_entry", $"Best Move: {bestChild.State.GetLastMove()}, Wins: {bestChild.Wins}, Visits: {bestChild.Visits}", 0);

		SignalManager.EmitSignal("move_selected", bestChild.State.GetLastMove().Split('_')[0]);
	}
}
