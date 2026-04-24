using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;

namespace Ludo.Backend.Models;

public class Tile : ITile
{
    public Position Position { get; }
    public TileTypes Type { get; }
    public IList<IPiece> Pieces { get; } = new List<IPiece>();

    public Tile(Position position, TileTypes type)
    {
        Position = position;
        Type = type;
    }
}
