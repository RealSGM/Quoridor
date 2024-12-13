using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class TreeNode : Node
{
    public TreeNode Parent { get; set; }
    public List<TreeNode> Children { get; set; }
    public string MoveHistory { get; set; }

    // Constructor for root node
    public TreeNode(string moveHistory)
    {
        Parent = null;
        Children = new();
        MoveHistory = moveHistory;
    }

    // Constructor for child nodes
    public TreeNode(TreeNode parent, string moveHistory)
    {
        Parent = parent;
        Children = new();
        MoveHistory = moveHistory;
    }

    public void AddChild(TreeNode child)
    {
        Children.Add(child);
        child.Parent = this;
    }

    public void RemoveChild(TreeNode child)
    {
        Children.Remove(child);
    }
}
