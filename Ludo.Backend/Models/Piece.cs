using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;

namespace Ludo.Backend.Models;

public class Piece : IPiece
{
    public PlayerColor Color { get; }
    public Position CurrentPosition { get; set; }
    public int CurrentStep { get; set; }
    public PieceState State { get; set; }

    public Piece(PlayerColor color, Position basePosition)
    {
        Color = color;
        CurrentPosition = basePosition;
        CurrentStep = -1;
        State = PieceState.Base;
    }
}
