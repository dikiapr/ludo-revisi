using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;

namespace Ludo.Backend.Models;

public class Player : IPlayer
{
    public string Name { get; }
    public PlayerColor Color { get; }

    public Player(string name, PlayerColor color)
    {
        Name = name;
        Color = color;
    }
}
