using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

[GlobalClass]
public partial class MiniMax : Node
{
	public void CreateGameTree(Godot.Collections.Dictionary<int, int[]> enabledFences)
	{
		GD.Print(enabledFences);
	}
}
