public struct FenceData(ulong fences)
{
    public ulong Fences { readonly get => fences; private set => fences = value; }

    public readonly FenceData Clone() => new(Fences);
    public readonly bool IsPlaced(int index) => (Fences & (1UL << index)) != 0;
    public void SetPlaced(int index) => Fences |= 1UL << index;
    public void UndoSetPlaced(int index) => Fences &= ~(1UL << index);
}