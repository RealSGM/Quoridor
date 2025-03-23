using System;
using System.Collections.Generic;
using System.Linq;

public class MCTSNode(MCTSNode parent, BoardState state, int player, string move = "")
{
	public MCTSNode Parent = parent;
	public BoardState State = state;
	public List<MCTSNode> Children = [];
	public int Wins = 0;
	public int Visits = 0;
	public int CurrentPlayer = player;
	public string LastMove = move;

	public bool IsLeaf() => Children.Count == 0;

	public MCTSNode GetBestChild(double explorationConstant)
	{
		return Children.OrderByDescending(c =>
			(double)c.Wins / (c.Visits + 1) +
			explorationConstant * Math.Sqrt(Math.Log(Visits + 1) / (c.Visits + 1))
		).First();
	}
}
