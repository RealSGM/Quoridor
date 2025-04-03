using Godot;

public struct FenceData
{   
    sbyte PlacedBy;
    sbyte Direction;
    bool[] isIllegal;

    public FenceData()
    {
        PlacedBy = -1;
        Direction = -1;
        isIllegal = [false, false];
    }

    public readonly sbyte GetPlacedBy() => PlacedBy;
    public readonly sbyte GetDirection() => Direction;
    public readonly bool GetIllegal(int index) => isIllegal[index];
    public readonly bool IsPlaced() => PlacedBy != -1;
    public readonly bool IsFencePlaceable(int index) => !isIllegal[index] && PlacedBy == -1;

    /// Sets the player who placed the fence
    /// -1 = None, 0 = Player 0, 1 = Player 1
    public void SetPlaced(sbyte placedBy)
    {
        PlacedBy = placedBy;
    }

    /// Sets the direction of the fence
    /// -1 = None, 0 = Horizontal, 1 = Vertical
    public void SetDirection(sbyte direction)
    {
        Direction = direction;
    }

    /// Sets the illegal state of the fence
    /// true = Illegal, false = Legal
    public readonly void SetIllegal(int index, bool illegal)
    {
        isIllegal[index] = illegal;
    }

    
}