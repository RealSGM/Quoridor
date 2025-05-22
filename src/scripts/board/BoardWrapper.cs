using Godot;
using System.Collections.Generic;

/// <summary>
/// Wrapper class for the BoardState to expose its methods and properties to Godot.
/// </summary>
[GlobalClass]
public partial class BoardWrapper : Node
{
	public BoardState State { get; set; }

	public void Initialise()
	{
		State = new BoardState();
		State.Initialise();
	}

	public void AddMove(string code) => State.AddMove(code);

	public int GetPawnTile(int player) => State.GetPawnTile(player);

	public int GetFencesRemaining(int player) => State.GetFencesRemaining(player);

	public int[] GetReachableTiles(int player) => State.GetReachableTiles(player);

	public bool IsWinner(int player) => State.IsWinner(player);

	public bool GetFencePlaced(int direction, int index) => State.GetFencePlaced(direction, index);

	public void UndoMove(string code) => State.UndoMove(code);

	public bool IsFenceEnabled(int direction, int index, bool checkIllegal) => State.IsFenceEnabled(direction, index, checkIllegal);

	public bool HasFences(int player) => State.HasFences(player);

	public void SetLastMove(string code) => State.SetLastMove(code);

	#region Godot Helper Methods ---

	public int[] GetFencesAsArray(int dir) => Helper.BitboardToArray(State.Fences[dir].Fences);

	public int[] GetEnabledFencesAsArray(int dir)
	{
		List<int> bits = [];

		for (int i = 0; i < Helper.BitBoardSize * Helper.BitBoardSize; i++)
		{
			if (State.IsFenceEnabled(dir, i)) bits.Add(1);
			else bits.Add(0);
		}

		return [.. bits];
	}

	public int EvaluateBoard(int player = 0) => State.EvaluateBoard(player);

	#endregion
}
