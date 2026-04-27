using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Backend.Controllers;

public class GameController : IGameController
{
    private readonly IBoard _board;
    private readonly IDice _dice;
    private readonly List<IPlayer> _players;
    private Dictionary<PlayerColor, List<IPiece>> _pieces;
    private int _lastDiceRoll;
    private bool _bonusTurn;

    public bool IsGameOver { get; private set; }
    public int CurrentPlayerIndex { get; private set; }

    public event Action<IPiece, IPiece>? OnPieceCaptured;
    public event Action? onGameFinished;

    private static readonly Position[] MainTrack = BuildMainTrack();

    private static readonly Dictionary<PlayerColor, Position[]> HomeColumns = BuildHomeColumns();

    private static readonly Dictionary<PlayerColor, Position> StartOffsets = new()
    {
        { PlayerColor.Red,    new Position(1,  6)  },
        { PlayerColor.Blue,   new Position(8,  1)  },
        { PlayerColor.Yellow, new Position(13, 8)  },
        { PlayerColor.Green,  new Position(6,  13) },
    };

    private static readonly List<Position> SafeSquares = new()
    {
        new Position(1,  6),
        new Position(8,  1),
        new Position(13, 8),
        new Position(6,  13),
        new Position(6,  2),
        new Position(12, 6),
        new Position(8,  12),
        new Position(2,  8),
    };

    private static readonly Dictionary<PlayerColor, Position[]> BasePositions = new()
    {
        { PlayerColor.Red,    new[] { new Position(2,2),   new Position(3,2),   new Position(2,3),   new Position(3,3)   } },
        { PlayerColor.Blue,   new[] { new Position(11,2),  new Position(12,2),  new Position(11,3),  new Position(12,3)  } },
        { PlayerColor.Yellow, new[] { new Position(11,11), new Position(12,11), new Position(11,12), new Position(12,12) } },
        { PlayerColor.Green,  new[] { new Position(2,11),  new Position(3,11),  new Position(2,12),  new Position(3,12)  } },
    };

    public GameController(IBoard board, IDice dice)
    {
        _board = board;
        _dice = dice;
        _players = new List<IPlayer>();
        _pieces = new Dictionary<PlayerColor, List<IPiece>>();
    }

    public void StartGame(List<IPlayer> players)
    {
        if (players == null || players.Count < 2 || players.Count > 4)
        {
            return;
        }
        _players.Clear();
        _players.AddRange(players);

        _pieces.Clear();
        foreach (IPlayer player in _players)
        {
            Position[] bases = BasePositions[player.Color];
            List<IPiece> list = new List<IPiece>();
            for (int i = 0; i < 4; i++)
            {
                list.Add(new Piece(player.Color, new Position(bases[i].X, bases[i].Y)));
            }
            _pieces[player.Color] = list;
        }

        foreach (PlayerColor color in _pieces.Keys)
        {
            foreach (IPiece piece in _pieces[color])
            {
                AddPieceToTile(piece.CurrentPosition, piece);
            }
        }

        CurrentPlayerIndex = 0;
        IsGameOver = false;
        _bonusTurn = false;
        _lastDiceRoll = 0;
    }

    public int RollDice()
    {
        _lastDiceRoll = _dice.Roll();
        if (_lastDiceRoll == 6)
        {
            _bonusTurn = true;
        }
        return _lastDiceRoll;
    }

    public IPlayer GetCurrentPlayer()
    {
        IPlayer currentPlayer = _players[CurrentPlayerIndex];
        return currentPlayer;
    }

    public IList<IPiece> GetMovablePieces()
    {
        PlayerColor color = GetCurrentPlayer().Color;
        List<IPiece> result = new List<IPiece>();

        foreach (IPiece piece in _pieces[color])
        {
            if (piece.State == PieceState.Finished)
            {
                continue;
            }

            if (piece.State == PieceState.Base)
            {
                if (_lastDiceRoll == 6)
                {
                    result.Add(piece);
                }
            }
            else // Active
            {
                if (piece.CurrentStep + _lastDiceRoll <= 56)
                {
                    result.Add(piece);
                }
            }
        }

        return result;
    }

    public void MovePiece(IPlayer player, IPiece piece, int steps)
    {
        Position oldPos = new Position(piece.CurrentPosition.X, piece.CurrentPosition.Y);

        if (piece.State == PieceState.Base)
        {
            piece.CurrentStep = 0;
            piece.State = PieceState.Active;
            piece.CurrentPosition = GetBoardPosition(piece.Color, 0);
        }
        else
        {
            int newStep = piece.CurrentStep + steps;
            piece.CurrentStep = newStep;
            piece.CurrentPosition = GetBoardPosition(piece.Color, newStep);

            if (newStep == 56)
            {
                piece.State = PieceState.Finished;
                _board.FinishedPieces.Add(piece);
                RemovePieceFromTile(oldPos, piece);

                if (HasPlayerWon(player))
                {
                    IsGameOver = true;
                    onGameFinished?.Invoke();
                }
                return;
            }
        }

        RemovePieceFromTile(oldPos, piece);
        AddPieceToTile(piece.CurrentPosition, piece);

        if (piece.CurrentStep < 51)
        {
            CheckCapture(player, piece);
        }
    }

    public void NextTurn()
    {
        if (_bonusTurn)
        {
            _bonusTurn = false;
            return;
        }
        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % _players.Count;
    }

    public IList<IPlayer> GetPlayers()
    {
        IList<IPlayer> players = _players.AsReadOnly();
        return players;
    }

    public IDictionary<PlayerColor, IList<IPiece>> GetAllPieces()
    {
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _pieces.ToDictionary(kvp => kvp.Key, kvp => (IList<IPiece>)kvp.Value);
        return allPieces;
    }

    private static Position GetBoardPosition(PlayerColor color, int step)
    {
        if (step >= 51)
        {
            Position[] homeCol = HomeColumns[color];
            Position homePosition = homeCol[step - 51];
            return homePosition;
        }

        Position startPos = StartOffsets[color];
        int startIndex = 0;
        for (int i = 0; i < MainTrack.Length; i++)
        {
            if (MainTrack[i].X == startPos.X && MainTrack[i].Y == startPos.Y)
            {
                startIndex = i;
                break;
            }
        }
        int trackIndex = (startIndex + step) % 52;
        Position raw = MainTrack[trackIndex];
        Position trackPosition = new Position(raw.X, raw.Y);
        return trackPosition;
    }

    private void CheckCapture(IPlayer currentPlayer, IPiece movingPiece)
    {
        Position position = movingPiece.CurrentPosition;

        bool isSafe = false;
        foreach (Position safePos in SafeSquares)
        {
            if (safePos.X == position.X && safePos.Y == position.Y)
            {
                isSafe = true;
                break;
            }
        }
        if (isSafe)
        {
            return;
        }

        foreach (IPlayer player in _players)
        {
            if (player.Color == currentPlayer.Color)
            {
                continue;
            }
            if (!_pieces.ContainsKey(player.Color))
            {
                continue;
            }

            foreach (IPiece piece in _pieces[player.Color])
            {
                if (piece.State != PieceState.Active)
                {
                    continue;
                }
                if (piece.CurrentStep >= 51)
                {
                    continue; // Home column = aman
                }

                if (piece.CurrentPosition.X == position.X && piece.CurrentPosition.Y == position.Y)
                {
                    RemovePieceFromTile(piece.CurrentPosition, piece);

                    int pieceIndex = _pieces[player.Color].IndexOf(piece);
                    Position basePos = BasePositions[player.Color][pieceIndex];
                    piece.CurrentPosition = new Position(basePos.X, basePos.Y);
                    piece.CurrentStep = -1;
                    piece.State = PieceState.Base;

                    AddPieceToTile(piece.CurrentPosition, piece);
                    OnPieceCaptured?.Invoke(movingPiece, piece);
                }
            }
        }
    }

    private bool HasPlayerWon(IPlayer player)
    {
        bool hasWon = true;

        foreach (IPiece piece in _pieces[player.Color])
        {
            if (piece.State != PieceState.Finished)
            {
                hasWon = false;
                break;
            }
        }

        return hasWon;
    }

    private void AddPieceToTile(Position position, IPiece piece)
    {
        if (IsValidTilePos(position))
        {
            _board.Grid[position.Y, position.X].Pieces.Add(piece);
        }
    }

    private void RemovePieceFromTile(Position position, IPiece piece)
    {
        if (IsValidTilePos(position))
        {
            _board.Grid[position.Y, position.X].Pieces.Remove(piece);
        }
    }

    private static bool IsValidTilePos(Position position)
    {
        bool isValid = position.X >= 0 && position.X < Board.Size && position.Y >= 0 && position.Y < Board.Size;
        return isValid;
    }

    private static Position[] BuildMainTrack()
    {
        List<Position> track = new List<Position>();

        for (int col = 1; col <= 5; col++) track.Add(new Position(col, 6));

        for (int row = 5; row >= 0; row--) track.Add(new Position(6, row));

        track.Add(new Position(7, 0));
        track.Add(new Position(8, 0));

        for (int row = 1; row <= 5; row++) track.Add(new Position(8, row));

        for (int col = 9; col <= 14; col++) track.Add(new Position(col, 6));

        track.Add(new Position(14, 7));
        track.Add(new Position(14, 8));

        for (int col = 13; col >= 9; col--) track.Add(new Position(col, 8));

        for (int row = 9; row <= 14; row++) track.Add(new Position(8, row));

        track.Add(new Position(7, 14));
        track.Add(new Position(6, 14));

        for (int row = 13; row >= 9; row--) track.Add(new Position(6, row));

        for (int col = 5; col >= 0; col--) track.Add(new Position(col, 8));

        track.Add(new Position(0, 7));
        track.Add(new Position(0, 6));

        Position[] result = track.ToArray();
        return result;
    }

    private static Dictionary<PlayerColor, Position[]> BuildHomeColumns()
    {
        Dictionary<PlayerColor, Position[]> homeColumns = new Dictionary<PlayerColor, Position[]>
        {
            {
                PlayerColor.Red, new[]
                {
                    new Position(1,7), new Position(2,7), new Position(3,7),
                    new Position(4,7), new Position(5,7), new Position(6,7),
                }
            },
            {
                PlayerColor.Blue, new[]
                {
                    new Position(7,1), new Position(7,2), new Position(7,3),
                    new Position(7,4), new Position(7,5), new Position(7,6),
                }
            },
            {
                PlayerColor.Yellow, new[]
                {
                    new Position(13,7), new Position(12,7), new Position(11,7),
                    new Position(10,7), new Position(9,7),  new Position(8,7),
                }
            },
            {
                PlayerColor.Green, new[]
                {
                    new Position(7,13), new Position(7,12), new Position(7,11),
                    new Position(7,10), new Position(7,9),  new Position(7,8),
                }
            },
        };
        return homeColumns;
    }
}
