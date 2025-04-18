public struct Pawn 
{
    public int FencesRemaining { get; set; }
    public byte Index { get; private set; }

    public Pawn(byte index, int fencesRemaining = Helper.MaxFences)
    {
        Index = index;
        FencesRemaining = fencesRemaining;
    }

    public Pawn Clone() => new(Index, FencesRemaining);
    public void PlaceFence() => FencesRemaining--;
    public void UndoPlaceFence() => FencesRemaining++;
    public void Move(byte index) => Index = index;
}
