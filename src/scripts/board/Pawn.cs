public struct Pawn(byte index, int fencesRemaining = Helper.MaxFences)
{
	public sbyte FencesRemaining { get; set; } = (sbyte)fencesRemaining;
	public byte Index { get; private set; } = index;

	public readonly Pawn Clone() => new(Index, FencesRemaining);
	public void PlaceFence() => FencesRemaining--;
	public void UndoPlaceFence() => FencesRemaining++;
	public void Move(byte index) => Index = index;
}
