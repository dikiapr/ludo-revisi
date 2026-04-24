using Ludo.Backend.Interfaces;

namespace Ludo.Backend.Models;

public class Dice : IDice
{
    private readonly Random _random;

    public Dice()
    {
        _random = new Random();
    }

    public Dice(int seed)
    {
        _random = new Random(seed);
    }

    public int Roll()
    {
        return _random.Next(1, 7);
    }
}
