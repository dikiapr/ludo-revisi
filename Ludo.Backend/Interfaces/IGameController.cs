using System;
using Ludo.Backend.Enums;

namespace Ludo.Backend.Interfaces;

public interface IGameController
{
    bool IsGameOver { get; }
    int CurrentPlayerIndex { get; }

    event Action<IPiece, IPiece>? OnPieceCaptured;
    event Action? onGameFinished;

    void StartGame(List<IPlayer> players);
    int RollDice();

    IPlayer GetCurrentPlayer();
    IList<IPiece> GetMovablePieces();

    IPiece ChoosePiece(IList<IPiece> movablePieces);
    void MovePiece(IPlayer player, IPiece piece, int steps);

    void NextTurn();
    IList<IPlayer> GetPlayers();
    IDictionary<PlayerColor, IList<IPiece>> GetAllPieces();
    void EndGame();
}
