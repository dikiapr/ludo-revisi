using Ludo.Backend.Interfaces;

namespace Ludo.Backend.Models;

public class Dice : IDice
{
    private readonly Random _random = new();

    public int Value { get; set; }

    public int Roll()
    {
        Value = _random.Next(1, 7);
        return Value;
    }
}
