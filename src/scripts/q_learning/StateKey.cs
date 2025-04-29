using System;

[Serializable]
public class StateKey : IEquatable<StateKey>
{
    public byte Player0 { get; set; }
    public byte Player1 { get; set; }
    public ulong HorizontalFences { get; set; }
    public ulong VerticalFences { get; set; }

    public override string ToString() => $"{Player0}|{Player1}|{HorizontalFences}|{VerticalFences}";
    public override int GetHashCode() => HashCode.Combine(Player0, Player1, HorizontalFences, VerticalFences);
    
    public static bool operator ==(StateKey left, StateKey right) => Equals(left, right);
    public static bool operator !=(StateKey left, StateKey right) => !Equals(left, right);
    public override bool Equals(object obj) => obj is StateKey other && Equals(other);

    public bool Equals(StateKey other) =>
        other != null &&
        Player0 == other.Player0 &&
        Player1 == other.Player1 &&
        HorizontalFences == other.HorizontalFences &&
        VerticalFences == other.VerticalFences;
    
    public static StateKey ParseFromFile(string text)
    {
        var parts = text.Split('|');
        return new StateKey
        {
            Player0 = byte.Parse(parts[0]),
            Player1 = byte.Parse(parts[1]),
            HorizontalFences = ulong.Parse(parts[2]),
            VerticalFences = ulong.Parse(parts[3])
        };
    }


}