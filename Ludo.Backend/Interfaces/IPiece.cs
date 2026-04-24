using System;
using Ludo.Backend.Enums;
using Ludo.Backend.Models;

namespace Ludo.Backend.Interfaces;

public interface IPiece
{
    PlayerColor Color { get; }
    Position CurrentPosition { get; set; }
    int CurrentStep { get; set; }
    PieceState State { get; set; }
}