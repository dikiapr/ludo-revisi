using System;
using Ludo.Backend.Enums;
using Ludo.Backend.Models;

namespace Ludo.Backend.Interfaces;

public interface ITile
{
    Position Position { get; }
    TileTypes Type { get; }
    IList<IPiece> Pieces { get; }
}
