public readonly struct FenceBB(ulong fences)
{
    public ulong Fences { get; init; } = fences;

    public FenceBB Clone() => new(Fences);
    public bool IsPlaced(int index) => (Fences & (1UL << index)) != 0;
    public FenceBB SetPlaced(int index) => new(Fences | (1UL << index));
    public FenceBB UndoSetPlaced(int index) => new(Fences & ~(1UL << index));
}