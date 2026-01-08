struct Point
{
    public int X { get; init; }
    public int Y { get; init; }
}

record Person(string Name, int Age);
record struct ValuePoint(int X, int Y);

public readonly record struct Money(decimal Amount, string Currency);