using System;

public class FenceBB(ulong normal = 0, ulong illegal = 0)
{
    public ulong Normal { get; set; } = normal;
    public ulong Illegal { get; set; } = illegal;

    public FenceBB Clone() => new(Normal, Illegal);
    public bool IsPlaced(int index) => (Normal & (1UL << index)) != 0;
    public void SetPlaced(int index) => Normal |= 1UL << index;
    public void UndoSetPlaced(int index) => Normal &= ~(1UL << index);

    public bool IsIllegal(int index) => (Illegal & (1UL << index)) != 0;
    public void SetIllegal(int index) => Illegal |= 1UL << index;
    public void UndoSetIllegal(int index) => Illegal &= ~(1UL << index);
}
