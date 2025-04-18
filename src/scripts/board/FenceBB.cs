public class FenceBB(ulong fences = 0)
{
    public ulong Fences { get; private set; } = fences;

    public FenceBB Clone() => new(Fences);
    public bool IsPlaced(int index) => (Fences & (1UL << index)) != 0;
    public void SetPlaced(int index) => Fences |= 1UL << index;
    public void UndoSetPlaced(int index) => Fences &= ~(1UL << index);
}
