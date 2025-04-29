using System;
using System.Collections.Generic;
using System.Linq;

public class MCTSNode(MCTSNode parent, BoardState state, int currentplayer)
{
    public MCTSNode Parent = parent;
    public BoardState State = state;
    public List<MCTSNode> Children = [];
    public int Wins = 0;
    public int Visits = 0;
    public int CurrentPlayer = currentplayer;

    public bool IsLeaf() => Children.Count == 0;

    // Returns a child node with the highest UCT value
    public MCTSNode SelectChild(double explorationConstant = 1.41)
    {
        if (IsLeaf() || State.IsGameOver()) return this;

        return Children.OrderByDescending(c =>
            (double)c.Wins / (c.Visits + 1) +
            explorationConstant * Math.Sqrt(Math.Log(Visits + 1) / (c.Visits + 1))
        ).First();
    }

    // Expands the node by adding all moves that have not been explored yet, considering weighted moves
    public MCTSNode Expand()
    {
        HashSet<BoardState> exploredStates = [.. Children.Select(c => c.State)];

        int[] pawnMoves = [.. State.GetReachableTilesSmart(CurrentPlayer)];
        ulong[] fencesMoves = State.GetFenceMovesSmart(CurrentPlayer);

        List<string> mappedPawnMoves = [.. pawnMoves.Select(tile => Helper.GetMoveCodeAsString(CurrentPlayer, "m", 0, tile))];
        List<string> mappedFenceMoves = [.. fencesMoves
            .SelectMany((fence, index) => Helper.GetOnesInBitBoard(fence)
            .Select(i => Helper.GetMoveCodeAsString(CurrentPlayer, "f", index, i)))];

        List<string> biasedMoves = Helper.Random.NextDouble() < 0.5
            ? [.. mappedPawnMoves, .. mappedFenceMoves]
            : [.. mappedFenceMoves, .. mappedPawnMoves];

        // Try adding the first unvisited state
        foreach (string move in biasedMoves)
        {
            BoardState newState = State.Clone();
            newState.AddMove(move);

            if (exploredStates.Contains(newState)) continue;

            MCTSNode childNode = new(this, newState, 1 - CurrentPlayer);
            Children.Add(childNode);

            newState.Free();

            return childNode;
        }

        return this; // All states already explored
    }


    // Simulates a game from the current state until it reaches a terminal state

    public int Simulate(int simulatingPlayer, int maxPlayoutDepth = 50)
    {
        BoardState tempState = State.Clone();
        int depth = 0;
        Random random = new();

        // While the game is not over and the simulation depth is not reached
        while (!tempState.IsGameOver() && depth < maxPlayoutDepth)
        {
            string selectedMove = null;

            List<string> moves = random.NextDouble() <= 0.5
                ? [.. State.GetReachableTilesSmart(CurrentPlayer)
                    .Select(tile => Helper.GetMoveCodeAsString(CurrentPlayer, "m", 0, tile))]
                : [.. State.GetFenceMovesSmart(CurrentPlayer)
                    .SelectMany((fence, index) => Helper.GetOnesInBitBoard(fence)
                    .Select(i => Helper.GetMoveCodeAsString(CurrentPlayer, "f", index, i)))];


            if (moves.Count == 0) break;

            selectedMove = moves[random.Next(moves.Count)];

            tempState.AddMove(selectedMove);
            CurrentPlayer = 1 - CurrentPlayer;
            IllegalFenceCheck.GetIllegalFences(tempState, CurrentPlayer);
            depth++;
        }

        int result = tempState.GetGameResult(simulatingPlayer);
        tempState.Free();

        return result;
    }


    public void Backpropagate(int result)
    {
        MCTSNode node = this;

        // Propagate the result back to the root node
        while (node != null)
        {
            node.Visits += 1;
            // If the result is a win, increment the wins for this node
            if (result == int.MaxValue) node.Wins += 1;
            // Move up to the parent node
            node = node.Parent;
        }
    }
}
