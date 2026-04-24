using System;
using Ludo.Backend.Enums;

namespace Ludo.Backend.Interfaces;

public interface IPlayer
{
    string Name { get; }
    PlayerColor Color { get; }
}
