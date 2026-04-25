using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Backend.Controllers;

/// <summary>
/// GameController - pusat semua logika permainan Ludo.
/// Mengelola giliran pemain, pergerakan bidak, penangkapan, dan kondisi menang.
///
/// KONSEP PATH:
///   - Main track  : 52 kotak melingkar (step 0-51), searah jarum jam mulai dari (1,6)
///   - Home column : 6 kotak khusus per warna (step 52-57) menuju finish
///   - Step 57     : bidak selesai (Finished)
///
/// STARTING OFFSETS (jarak dari index 0 main track):
///   Red=0, Blue=13, Yellow=26, Green=39
/// </summary>
public class GameController : IGameController
{
    // ── Dependencies ────────────────────────────────────────────────────────
    private readonly IBoard _board;
    private readonly IDice _dice;
    private readonly List<IPlayer> _players;

    // ── State ────────────────────────────────────────────────────────────────
    private Dictionary<PlayerColor, List<IPiece>> _pieces;
    private int _lastDiceRoll;
    private bool _bonusTurn; // rolling 6 → giliran ekstra

    public bool IsGameOver { get; private set; }
    public int CurrentPlayerIndex { get; private set; }

    // ── Events ───────────────────────────────────────────────────────────────
    public event Action<IPiece, IPiece>? OnPieceCaptured;
    public event Action? onGameFinished;

    // ── Static Path Data ─────────────────────────────────────────────────────

    /// <summary>52 kotak main track (indeks 0-51), searah jarum jam dari Red start.</summary>
    private static readonly Position[] MainTrack = BuildMainTrack();

    /// <summary>6 kotak home column per warna (indeks 0-5 → step 52-57).</summary>
    private static readonly Dictionary<PlayerColor, Position[]> HomeColumns = BuildHomeColumns();

    /// <summary>Offset index main track untuk masing-masing warna.</summary>
    private static readonly Dictionary<PlayerColor, int> StartOffsets = new()
    {
        { PlayerColor.Red,    0  },
        { PlayerColor.Blue,   13 },
        { PlayerColor.Yellow, 26 },
        { PlayerColor.Green,  39 },
    };

    /// <summary>Kotak aman di main track (tidak bisa ditangkap).</summary>
    private static readonly HashSet<(int X, int Y)> SafeSquares = new()
    {
        (1,  6),   // Red start
        (8,  1),   // Blue start
        (13, 8),   // Yellow start
        (6,  13),  // Green start
    };

    /// <summary>Posisi base untuk 4 bidak tiap warna (X=col, Y=row).</summary>
    private static readonly Dictionary<PlayerColor, Position[]> BasePositions = new()
    {
        { PlayerColor.Red,    new[] { new Position(2,2),  new Position(3,2),  new Position(2,3),  new Position(3,3)  } },
        { PlayerColor.Blue,   new[] { new Position(10,2), new Position(11,2), new Position(10,3), new Position(11,3) } },
        { PlayerColor.Yellow, new[] { new Position(10,10),new Position(11,10),new Position(10,11),new Position(11,11)} },
        { PlayerColor.Green,  new[] { new Position(2,10), new Position(3,10), new Position(2,11), new Position(3,11) } },
    };

    // ── Constructor ──────────────────────────────────────────────────────────

    public GameController(IBoard board, IDice dice)
    {
        _board = board ?? throw new ArgumentNullException(nameof(board));
        _dice = dice ?? throw new ArgumentNullException(nameof(dice));
        _players = new List<IPlayer>();
        _pieces = new Dictionary<PlayerColor, List<IPiece>>();
    }

    // ── IGameController Implementation ───────────────────────────────────────

    /// <summary>Inisialisasi ulang semua bidak dan mulai permainan.</summary>
    public void StartGame(List<IPlayer> players)
    {
        if (players == null || players.Count < 2 || players.Count > 4)
        {
            throw new ArgumentException("Ludo memerlukan 2-4 pemain.", nameof(players));
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

        // Tempatkan bidak pada tile base di grid board
        foreach ((PlayerColor color, List<IPiece> pieces) in _pieces)
        {
            foreach (IPiece piece in pieces)
            {
                AddPieceToTile(piece.CurrentPosition, piece);
            }
        }

        CurrentPlayerIndex = 0;
        IsGameOver = false;
        _bonusTurn = false;
        _lastDiceRoll = 0;
    }

    /// <summary>Lempar dadu. Rolling 6 memberi giliran bonus.</summary>
    public int RollDice()
    {
        _lastDiceRoll = _dice.Roll();
        if (_lastDiceRoll == 6)
        {
            _bonusTurn = true;
        }
        return _lastDiceRoll;
    }

    /// <summary>Dapatkan pemain yang sedang giliran.</summary>
    public IPlayer GetCurrentPlayer()
    {
        IPlayer currentPlayer = _players[CurrentPlayerIndex];
        return currentPlayer;
    }

    /// <summary>
    /// Dapatkan daftar bidak yang bisa digerakkan berdasarkan hasil dadu terakhir.
    ///   - Dadu 6 : bidak di Base BISA keluar + bidak Active bisa gerak
    ///   - Selain  : hanya bidak Active yang tidak overshoot step 57
    /// </summary>
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
                if (piece.CurrentStep + _lastDiceRoll <= 57)
                {
                    result.Add(piece);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Pilih bidak dari daftar yang bisa bergerak.
    /// Default: bidak pertama. UI bisa override dengan membiarkan pemain memilih.
    /// </summary>
    public IPiece ChoosePiece(IList<IPiece> movablePieces)
    {
        if (movablePieces == null || movablePieces.Count == 0)
        {
            throw new InvalidOperationException("Tidak ada bidak yang bisa digerakkan.");
        }

        IPiece chosen = movablePieces[0];
        return chosen;
    }

    /// <summary>
    /// Gerakkan bidak sejauh <paramref name="steps"/> langkah.
    ///   - Jika bidak di Base dan steps == 6: keluarkan bidak ke starting position (step 0)
    ///   - Jika Active: pindah steps langkah, cek penangkapan, cek selesai
    /// </summary>
    public void MovePiece(IPlayer player, IPiece piece, int steps)
    {
        Position oldPos = new Position(piece.CurrentPosition.X, piece.CurrentPosition.Y);

        if (piece.State == PieceState.Base)
        {
            // Keluarkan bidak ke posisi start (step 0)
            piece.CurrentStep = 0;
            piece.State = PieceState.Active;
            piece.CurrentPosition = GetBoardPosition(piece.Color, 0);
        }
        else
        {
            int newStep = piece.CurrentStep + steps;
            piece.CurrentStep = newStep;
            piece.CurrentPosition = GetBoardPosition(piece.Color, newStep);

            if (newStep == 57)
            {
                // Bidak selesai!
                piece.State = PieceState.Finished;
                _board.FinishedPieces.Add(piece);
                RemovePieceFromTile(oldPos, piece);
                // Tidak perlu tambah ke tile finish (sudah dicatat di FinishedPieces)

                if (HasPlayerWon(player))
                {
                    IsGameOver = true;
                    onGameFinished?.Invoke();
                }
                return;
            }
        }

        // Update posisi tile di board grid
        RemovePieceFromTile(oldPos, piece);
        AddPieceToTile(piece.CurrentPosition, piece);

        // Cek penangkapan (hanya di main track, bukan home column)
        if (piece.CurrentStep < 52)
        {
            CheckCapture(player, piece);
        }
    }

    /// <summary>
    /// Pindah ke pemain berikutnya.
    /// Jika dadu sebelumnya = 6 (_bonusTurn), pemain yang sama giliran lagi.
    /// </summary>
    public void NextTurn()
    {
        if (_bonusTurn)
        {
            _bonusTurn = false;
            return; // Giliran yang sama tetap berjalan
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

    /// <summary>Paksa akhiri permainan.</summary>
    public void EndGame()
    {
        IsGameOver = true;
        onGameFinished?.Invoke();
    }

    // ── Private Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Hitung posisi papan (koordinat tile) berdasarkan warna dan step bidak.
    ///   step -1     → posisi base (tidak dipakai di sini)
    ///   step 0-51   → main track (dengan offset per warna)
    ///   step 52-57  → home column spesifik warna
    /// </summary>
    private static Position GetBoardPosition(PlayerColor color, int step)
    {
        if (step >= 52)
        {
            Position[] homeCol = HomeColumns[color];
            Position homePosition = homeCol[step - 52];
            return homePosition;
        }

        int trackIndex = (StartOffsets[color] + step) % 52;
        Position raw = MainTrack[trackIndex];
        Position trackPosition = new Position(raw.X, raw.Y);
        return trackPosition;
    }

    /// <summary>Cek apakah ada bidak lawan di posisi yang sama → tangkap.</summary>
    private void CheckCapture(IPlayer currentPlayer, IPiece movingPiece)
    {
        Position pos = movingPiece.CurrentPosition;
        (int X, int Y) posKey = (pos.X, pos.Y);

        // Kotak aman: tidak bisa menangkap
        if (SafeSquares.Contains(posKey))
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
                if (piece.CurrentStep >= 52)
                {
                    continue; // Home column = aman
                }

                if (piece.CurrentPosition.X == pos.X && piece.CurrentPosition.Y == pos.Y)
                {
                    // Tangkap! Kembalikan bidak ke base
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

    /// <summary>Cek apakah semua 4 bidak pemain sudah Finished.</summary>
    private bool HasPlayerWon(IPlayer player)
    {
        bool hasWon = _pieces[player.Color].All(p => p.State == PieceState.Finished);
        return hasWon;
    }

    /// <summary>Tambahkan bidak ke tile pada koordinat (X=col, Y=row) di grid board.</summary>
    private void AddPieceToTile(Position pos, IPiece piece)
    {
        if (IsValidTilePos(pos))
        {
            _board.Grid[pos.Y, pos.X].Pieces.Add(piece);
        }
    }

    /// <summary>Hapus bidak dari tile pada koordinat (X=col, Y=row) di grid board.</summary>
    private void RemovePieceFromTile(Position pos, IPiece piece)
    {
        if (IsValidTilePos(pos))
        {
            _board.Grid[pos.Y, pos.X].Pieces.Remove(piece);
        }
    }

    private static bool IsValidTilePos(Position pos)
    {
        bool isValid = pos.X >= 0 && pos.X < Board.Size && pos.Y >= 0 && pos.Y < Board.Size;
        return isValid;
    }

    // ── Static Path Builders ─────────────────────────────────────────────────

    /// <summary>
    /// Bangun 52 kotak main track searah jarum jam mulai dari Red start (1,6).
    ///
    /// Layout papan 15x15 (X=col, Y=row, 0-indexed):
    ///
    ///   Seg A: row 6, col 1→5        (5 kotak)  – kiri   →  kanan
    ///   Seg B: col 6, row 5→0        (6 kotak)  – atas   ↑  (kiri top arm)
    ///   Seg C: row 0, col 7→8        (2 kotak)  – puncak →
    ///   Seg D: col 8, row 1→5        (5 kotak)  – turun  ↓  (kanan top arm)
    ///   Seg E: row 6, col 9→14       (6 kotak)  – kanan  →  (atas right arm)
    ///   Seg F: col 14, row 7→8       (2 kotak)  – sisi kanan ↓
    ///   Seg G: row 8, col 13→9       (5 kotak)  – kanan  ←  (bawah right arm)
    ///   Seg H: col 8, row 9→14       (6 kotak)  – turun  ↓  (kanan bottom arm)
    ///   Seg I: row 14, col 7→6       (2 kotak)  – bawah  ←
    ///   Seg J: col 6, row 13→9       (5 kotak)  – naik   ↑  (kiri bottom arm)
    ///   Seg K: row 8, col 5→0        (6 kotak)  – kiri   ←  (bawah left arm)
    ///   Seg L: col 0, row 7→6        (2 kotak)  – sisi kiri ↑
    ///   Total = 5+6+2+5+6+2+5+6+2+5+6+2 = 52 ✓
    ///
    /// Starting offsets (index pada array ini):
    ///   Red=0 (1,6), Blue=13 (8,1), Yellow=26 (13,8), Green=39 (6,13)
    /// </summary>
    private static Position[] BuildMainTrack()
    {
        List<Position> track = new List<Position>();

        // Seg A: Left arm top edge → right
        for (int col = 1; col <= 5; col++) track.Add(new Position(col, 6));

        // Seg B: Top arm left side ↑
        for (int row = 5; row >= 0; row--) track.Add(new Position(6, row));

        // Seg C: Top of board → right
        track.Add(new Position(7, 0));
        track.Add(new Position(8, 0));

        // Seg D: Top arm right side ↓
        for (int row = 1; row <= 5; row++) track.Add(new Position(8, row));

        // Seg E: Right arm top edge → right
        for (int col = 9; col <= 14; col++) track.Add(new Position(col, 6));

        // Seg F: Right side ↓
        track.Add(new Position(14, 7));
        track.Add(new Position(14, 8));

        // Seg G: Right arm bottom edge ← left
        for (int col = 13; col >= 9; col--) track.Add(new Position(col, 8));

        // Seg H: Bottom arm right side ↓
        for (int row = 9; row <= 14; row++) track.Add(new Position(8, row));

        // Seg I: Bottom of board ← left
        track.Add(new Position(7, 14));
        track.Add(new Position(6, 14));

        // Seg J: Bottom arm left side ↑
        for (int row = 13; row >= 9; row--) track.Add(new Position(6, row));

        // Seg K: Left arm bottom edge ← left
        for (int col = 5; col >= 0; col--) track.Add(new Position(col, 8));

        // Seg L: Left side ↑
        track.Add(new Position(0, 7));
        track.Add(new Position(0, 6));

        if (track.Count != 52)
        {
            throw new InvalidOperationException($"Main track harus 52 kotak, tapi {track.Count}.");
        }

        Position[] result = track.ToArray();
        return result;
    }

    /// <summary>
    /// Home column per warna: 6 kotak (step 52-57) menuju finish.
    ///   Red    : row 7, col 1→6  (masuk dari kiri)
    ///   Blue   : col 7, row 1→6  (masuk dari atas)
    ///   Yellow : row 7, col 13→8 (masuk dari kanan)
    ///   Green  : col 7, row 13→8 (masuk dari bawah)
    /// Indeks 5 (step 57) = kotak finish di tepi area tengah 3×3.
    /// </summary>
    private static Dictionary<PlayerColor, Position[]> BuildHomeColumns()
    {
        Dictionary<PlayerColor, Position[]> homeColumns = new Dictionary<PlayerColor, Position[]>
        {
            {
                PlayerColor.Red, new[]
                {
                    new Position(1,7), new Position(2,7), new Position(3,7),
                    new Position(4,7), new Position(5,7), new Position(6,7),  // finish
                }
            },
            {
                PlayerColor.Blue, new[]
                {
                    new Position(7,1), new Position(7,2), new Position(7,3),
                    new Position(7,4), new Position(7,5), new Position(7,6),  // finish
                }
            },
            {
                PlayerColor.Yellow, new[]
                {
                    new Position(13,7), new Position(12,7), new Position(11,7),
                    new Position(10,7), new Position(9,7),  new Position(8,7), // finish
                }
            },
            {
                PlayerColor.Green, new[]
                {
                    new Position(7,13), new Position(7,12), new Position(7,11),
                    new Position(7,10), new Position(7,9),  new Position(7,8), // finish
                }
            },
        };
        return homeColumns;
    }
}
