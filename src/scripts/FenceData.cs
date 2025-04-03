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


    public void SetPlaced(sbyte placedBy)
    {
        PlacedBy = placedBy;
    }


    public void SetDirection(sbyte direction)
    {
        Direction = direction;
    }


    public readonly void SetIllegal(int index, bool illegal)
    {
        isIllegal[index] = illegal;
    }

    
}