using System;

[Serializable]
public struct StateKey : IEquatable<StateKey>
{
    public byte Player0 { get; set; }
    public byte Player1 { get; set; }
    public ulong HorizontalFences { get; set; }
    public ulong VerticalFences { get; set; }
    public override readonly int GetHashCode() => HashCode.Combine(Player0, Player1, HorizontalFences, VerticalFences);
    public static bool operator ==(StateKey left, StateKey right) => left.Equals(right);
    public static bool operator !=(StateKey left, StateKey right) => !(left == right);
    public override readonly bool Equals(object obj) => obj is StateKey other && Equals(other);
    public override readonly string ToString() => $"Player0: {Player0}, Player1: {Player1}, HorizontalFences: {HorizontalFences}, VerticalFences: {VerticalFences}";
    public readonly bool Equals(StateKey other) =>
        Player0 == other.Player0 &&
        Player1 == other.Player1 &&
        HorizontalFences == other.HorizontalFences &&
        VerticalFences == other.VerticalFences;
}