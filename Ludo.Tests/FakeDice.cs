using Ludo.Backend.Interfaces;

namespace Ludo.Tests;

public class FakeDice : IDice
{
    public int ValueToReturn { get; set; }

    public int Roll()
    {
        return ValueToReturn;
    }
}
