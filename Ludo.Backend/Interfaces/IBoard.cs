using System;

namespace Ludo.Backend.Interfaces;

public interface IBoard
{
    ITile[][] Grid { get; }
    IList<IPiece> FinishedPieces { get; }
}
