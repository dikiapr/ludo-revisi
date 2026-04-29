using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

public class FakeDice : IDice
{
    public int ValueToReturn { get; set; }

    public int Roll()
    {
        return ValueToReturn;
    }
}

[TestFixture]
public class GameController_StartGameShould
{
    private GameController _gameController;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        IDice dice = new Dice();
        _players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        };
        Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

        _gameController = new GameController(board, dice, _players, pieces);
    }

    [Test]
    public void StartGame_WhenExecuted_ShouldSetCurrentPlayerIndexToZero()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 0;

        // Act
        _gameController.StartGame(_players);

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }

    [Test]
    public void StartGame_WhenExecuted_ShouldSetIsGameOverToFalse()
    {
        // Arrange
        bool expectedIsGameOver = false;

        // Act
        _gameController.StartGame(_players);

        // Assert
        Assert.That(_gameController.IsGameOver, Is.EqualTo(expectedIsGameOver));
    }

    [Test]
    public void StartGame_WhenExecuted_ShouldCreateFourPiecesPerPlayer()
    {
        // Arrange
        int expectedPiecesPerPlayer = 4;

        // Act
        _gameController.StartGame(_players);

        // Assert
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();
        Assert.Multiple(() =>
        {
            Assert.That(allPieces[PlayerColor.Red], Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Blue], Has.Count.EqualTo(expectedPiecesPerPlayer));
        });
    }

    [Test]
    public void StartGame_WhenTwoPlayersProvided_ShouldInitialize()
    {
        // Arrange
        int expectedPlayerCount = 2;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPlayerCount));
    }

    [Test]
    public void StartGame_WhenThreePlayersProvided_ShouldInitialize()
    {
        // Arrange
        int expectedPlayerCount = 3;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPlayerCount));
    }

    [Test]
    public void StartGame_WhenFourPlayersProvided_ShouldInitialize()
    {
        // Arrange
        int expectedPlayerCount = 4;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPlayerCount));
    }

    [Test]
    public void StartGame_WhenLessThanTwoPlayersProvided_ShouldNotInitialize()
    {
        // Arrange
        int expectedPieceCount = 0;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPieceCount));
    }

    [Test]
    public void StartGame_WhenMoreThanFourPlayersProvided_ShouldNotInitialize()
    {
        // Arrange
        int expectedPieceCount = 0;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
            new Player("Extra", PlayerColor.Red),
            new Player("Extra1", PlayerColor.Red),
            new Player("Extra2", PlayerColor.Red),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPieceCount));
    }
}

[TestFixture]
public class GameController_GetPlayersShould
{
    private GameController _gameController;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        IDice dice = new Dice();
        _players =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
        ];
        Dictionary<PlayerColor, List<IPiece>> pieces = [];

        _gameController = new GameController(board, dice, _players, pieces);
    }

    [Test]
    public void GetPlayers_WhenFourPlayersProvided_ShouldReturnCorrectPlayerCount()
    {
        // Arrange
        int expectedCount = 4;

        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.That(players, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void GetPlayers_WhenCalled_ShouldReturnPlayersInCorrectOrder()
    {
        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(players[0], Is.EqualTo(_players[0]));
            Assert.That(players[1], Is.EqualTo(_players[1]));
            Assert.That(players[2], Is.EqualTo(_players[2]));
            Assert.That(players[3], Is.EqualTo(_players[3]));
        });
    }

    [Test]
    public void GetPlayers_WhenCalled_ShouldReturnPlayersWithCorrectColors()
    {
        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(players[0].Color, Is.EqualTo(PlayerColor.Red));
            Assert.That(players[1].Color, Is.EqualTo(PlayerColor.Blue));
            Assert.That(players[2].Color, Is.EqualTo(PlayerColor.Yellow));
            Assert.That(players[3].Color, Is.EqualTo(PlayerColor.Green));
        });
    }

    [Test]
    public void GetPlayers_WhenCalled_ShouldReturnPlayersWithCorrectNames()
    {
        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(players[0].Name, Is.EqualTo("Red"));
            Assert.That(players[1].Name, Is.EqualTo("Blue"));
            Assert.That(players[2].Name, Is.EqualTo("Yellow"));
            Assert.That(players[3].Name, Is.EqualTo("Green"));
        });
    }

    [Test]
    public void GetPlayers_WhenCalled_ShouldReturnReadOnlyList()
    {
        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.That(players.IsReadOnly, Is.True);
    }

    [Test]
    public void GetPlayers_WhenOnlyTwoPlayersProvided_ShouldReturnTwoPlayers()
    {
        // Arrange
        int expectedCount = 2;
        List<IPlayer> twoPlayers =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        ];
        Dictionary<PlayerColor, List<IPiece>> pieces = [];
        GameController controller = new(new Board(), new Dice(), twoPlayers, pieces);

        // Act
        IList<IPlayer> players = controller.GetPlayers();

        // Assert
        Assert.That(players, Has.Count.EqualTo(expectedCount));
    }
}

[TestFixture]
public class GameController_GetCurrentPlayerShould
{
    private GameController _gameController;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        IDice dice = new Dice();
        _players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Green", PlayerColor.Green),
            new Player("Yellow", PlayerColor.Yellow),
        };
        Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

        _gameController = new GameController(board, dice, _players, pieces);
    }

    [Test]
    public void GetCurrentPlayer_WhenGameJustStarted_ShouldReturnFirstPlayer()
    {
        // Arrange
        IPlayer expectedPlayer = _players[0];

        // Act
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer, Is.EqualTo(expectedPlayer));
    }

    [Test]
    public void GetCurrentPlayer_WhenOneNextTurnCalled_ShouldReturnSecondPlayer()
    {
        // Arrange
        IPlayer expectedPlayer = _players[1];

        // Act
        _gameController.NextTurn();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer, Is.EqualTo(expectedPlayer));
    }

    [Test]
    public void GetCurrentPlayer_WhenTwoNextTurnsCalled_ShouldReturnThirdPlayer()
    {
        // Arrange
        IPlayer expectedPlayer = _players[2];

        // Act
        _gameController.NextTurn();
        _gameController.NextTurn();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer, Is.EqualTo(expectedPlayer));
    }

    [Test]
    public void GetCurrentPlayer_WhenThreeNextTurnsCalled_ShouldReturnFourthPlayer()
    {
        // Arrange
        IPlayer expectedPlayer = _players[3];

        // Act
        _gameController.NextTurn();
        _gameController.NextTurn();
        _gameController.NextTurn();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer, Is.EqualTo(expectedPlayer));
    }

    [Test]
    public void GetCurrentPlayer_WhenFirstPlayerIsCurrent_ShouldReturnRedColor()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Red;

        // Act
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer.Color, Is.EqualTo(expectedColor));
    }

    [Test]
    public void GetCurrentPlayer_WhenSecondPlayerIsCurrent_ShouldReturnBlueColor()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Blue;

        // Act
        _gameController.NextTurn();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer.Color, Is.EqualTo(expectedColor));
    }

    [Test]
    public void GetCurrentPlayer_WhenThirdPlayerIsCurrent_ShouldReturnGreenColor()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;

        // Act
        _gameController.NextTurn();
        _gameController.NextTurn();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer.Color, Is.EqualTo(expectedColor));
    }

    [Test]
    public void GetCurrentPlayer_WhenFourthPlayerIsCurrent_ShouldReturnYellowColor()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Yellow;

        // Act
        _gameController.NextTurn();
        _gameController.NextTurn();
        _gameController.NextTurn();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer.Color, Is.EqualTo(expectedColor));
    }
}

[TestFixture]
public class GameController_GetAllPiecesShould
{
    private GameController _gameController;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        IDice dice = new Dice();
        _players =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
        ];
        Dictionary<PlayerColor, List<IPiece>> pieces = [];

        _gameController = new GameController(board, dice, _players, pieces);
    }

    [Test]
    public void GetAllPieces_WhenGameNotStarted_ShouldReturnEmptyDictionary()
    {
        // Arrange
        int expectedCount = 0;

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.That(allPieces, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void GetAllPieces_WhenStartedWithFourPlayers_ShouldReturnFourEntries()
    {
        // Arrange
        int expectedCount = 4;
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.That(allPieces, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void GetAllPieces_WhenGameStarted_ShouldReturnFourPiecesPerPlayer()
    {
        // Arrange
        int expectedPiecesPerPlayer = 4;
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(allPieces[PlayerColor.Red],    Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Blue],   Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Yellow], Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Green],  Has.Count.EqualTo(expectedPiecesPerPlayer));
        });
    }

    [Test]
    public void GetAllPieces_WhenGameStarted_ShouldReturnAllPiecesInBaseState()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        foreach (IList<IPiece> pieces in allPieces.Values)
        {
            Assert.That(pieces, Has.All.Property(nameof(IPiece.State)).EqualTo(PieceState.Base));
        }
    }

    [Test]
    public void GetAllPieces_WhenGameStarted_ShouldReturnAllPiecesWithStepMinusOne()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        foreach (IList<IPiece> pieces in allPieces.Values)
        {
            Assert.That(pieces, Has.All.Property(nameof(IPiece.CurrentStep)).EqualTo(-1));
        }
    }

    [Test]
    public void GetAllPieces_WhenGameStarted_ShouldReturnPiecesMatchingPlayerColor()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        foreach (KeyValuePair<PlayerColor, IList<IPiece>> entry in allPieces)
        {
            Assert.That(entry.Value, Has.All.Property(nameof(IPiece.Color)).EqualTo(entry.Key));
        }
    }

    [Test]
    public void GetAllPieces_WhenStartedWithTwoPlayers_ShouldReturnTwoEntries()
    {
        // Arrange
        int expectedCount = 2;
        List<IPlayer> twoPlayers =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        ];
        _gameController.StartGame(twoPlayers);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.That(allPieces, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void GetAllPieces_WhenGameStarted_ShouldContainAllPlayerColorKeys()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(allPieces.ContainsKey(PlayerColor.Red),    Is.True);
            Assert.That(allPieces.ContainsKey(PlayerColor.Blue),   Is.True);
            Assert.That(allPieces.ContainsKey(PlayerColor.Yellow), Is.True);
            Assert.That(allPieces.ContainsKey(PlayerColor.Green),  Is.True);
        });
    }
}

[TestFixture]
public class GameController_RollDiceShould
{
    private GameController _gameController;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        IDice dice = new Dice();
        List<IPlayer> players = new List<IPlayer>()
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        };
        Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

        _gameController = new GameController(board, dice, players, pieces);
    }

    [Test]
    public void RollDice_WhenCalled_ShouldReturnValueBetweenOneAndSix()
    {
        // Arrange
        int expectedMin = 1;
        int expectedMax = 6;
        List<int> results = new List<int>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            results.Add(_gameController.RollDice());
        }

        // Assert
        Assert.That(results, Has.All.InRange(expectedMin, expectedMax));
    }

    [Test]
    public void RollDice_WhenCalledMultipleTimes_ShouldReturnValueWithinRange()
    {
        // Arrange
        int expectedMin = 1;
        int expectedMax = 6;
        List<int> results = new List<int>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            results.Add(_gameController.RollDice());
        }

        // Assert
        Assert.That(results, Has.None.LessThan(expectedMin));
        Assert.That(results, Has.None.GreaterThan(expectedMax));
    }
}

[TestFixture]
public class GameController_NextTurnShould
{
    private GameController _gameController;
    private FakeDice _fakeDice;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        _fakeDice = new FakeDice();
        _players =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
        ];
        Dictionary<PlayerColor, List<IPiece>> pieces = [];

        _gameController = new GameController(board, _fakeDice, _players, pieces);
    }

    [Test]
    public void NextTurn_WhenCalled_ShouldIncrementCurrentPlayerIndex()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 1;

        // Act
        _gameController.NextTurn();

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }

    [Test]
    public void NextTurn_WhenBonusTurnEarnedByRollingSix_ShouldKeepCurrentPlayerIndex()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 0;
        _fakeDice.ValueToReturn = 6;
        _gameController.RollDice();

        // Act
        _gameController.NextTurn();

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }
}

[TestFixture]
public class GameController_GetMovablePiecesShould
{
    private GameController _gameController;
    private FakeDice _fakeDice;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        _fakeDice = new FakeDice();
        _players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        };
        Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

        _gameController = new GameController(board, _fakeDice, _players, pieces);
        _gameController.StartGame(_players);
    }

    [Test]
    public void GetMovablePieces_WhenDiceRollIsSix_ShouldReturnAllBasePiecesAsMovable()
    {
        // Arrange
        int expectedMovablePieceCount = 4;
        _fakeDice.ValueToReturn = 6;

        // Act
        _gameController.RollDice();
        IList<IPiece> movablePieces = _gameController.GetMovablePieces();

        // Assert
        Assert.That(movablePieces, Has.Count.EqualTo(expectedMovablePieceCount));
    }

    [Test]
    public void GetMovablePieces_WhenDiceRollIsNotSix_ShouldReturnNoMovablePieces()
    {
        // Arrange
        int expectedMovablePieceCount = 0;
        _fakeDice.ValueToReturn = 3;

        // Act
        _gameController.RollDice();
        IList<IPiece> movablePieces = _gameController.GetMovablePieces();

        // Assert
        Assert.That(movablePieces, Has.Count.EqualTo(expectedMovablePieceCount));
    }

    [Test]
    public void GetMovablePieces_WhenStepsDoNotExceedFiftySix_ShouldIncludeActivePiece()
    {
        // Arrange
        _fakeDice.ValueToReturn = 6;
        _gameController.RollDice();
        IList<IPiece> redPieces = _gameController.GetAllPieces()[PlayerColor.Red];
        IPiece expectedPiece = redPieces[0];
        expectedPiece.State = PieceState.Active;
        expectedPiece.CurrentStep = 50;

        // Act
        IList<IPiece> movablePieces = _gameController.GetMovablePieces();

        // Assert
        Assert.That(movablePieces, Contains.Item(expectedPiece));
    }

    [Test]
    public void GetMovablePieces_WhenStepsExceedFiftySix_ShouldExcludeActivePiece()
    {
        // Arrange
        _fakeDice.ValueToReturn = 6;
        _gameController.RollDice();
        IList<IPiece> redPieces = _gameController.GetAllPieces()[PlayerColor.Red];
        IPiece expectedPiece = redPieces[0];
        expectedPiece.State = PieceState.Active;
        expectedPiece.CurrentStep = 51;

        // Act
        IList<IPiece> movablePieces = _gameController.GetMovablePieces();

        // Assert
        Assert.That(movablePieces, Does.Not.Contain(expectedPiece));
    }

    [Test]
    public void GetMovablePieces_WhenPieceIsFinished_ShouldExcludePiece()
    {
        // Arrange
        _fakeDice.ValueToReturn = 6;
        _gameController.RollDice();
        IList<IPiece> redPieces = _gameController.GetAllPieces()[PlayerColor.Red];
        IPiece expectedPiece = redPieces[0];
        expectedPiece.State = PieceState.Finished;

        // Act
        IList<IPiece> movablePieces = _gameController.GetMovablePieces();

        // Assert
        Assert.That(movablePieces, Does.Not.Contain(expectedPiece));
    }
}

[TestFixture]
public class GameController_MovePieceShould
{
    private GameController _gameController;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        IBoard board = new Board();
        IDice dice = new Dice();
        _players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        };
        Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

        _gameController = new GameController(board, dice, _players, pieces);
        _gameController.StartGame(_players);
    }

    [Test]
    public void MovePiece_WhenBasePieceMovedWithSix_ShouldChangePieceToActive()
    {
        // Arrange
        PieceState expectedState = PieceState.Active;
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        IPiece piece = _gameController.GetAllPieces()[PlayerColor.Red][0];

        // Act
        _gameController.MovePiece(currentPlayer, piece, 6);

        // Assert
        Assert.That(piece.State, Is.EqualTo(expectedState));
    }

    [Test]
    public void MovePiece_WhenBasePieceMovedWithSix_ShouldPlacePieceAtStartPosition()
    {
        // Arrange
        Position expectedPosition = new Position(1, 6);
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        IPiece piece = _gameController.GetAllPieces()[PlayerColor.Red][0];

        // Act
        _gameController.MovePiece(currentPlayer, piece, 6);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(piece.CurrentPosition.X, Is.EqualTo(expectedPosition.X));
            Assert.That(piece.CurrentPosition.Y, Is.EqualTo(expectedPosition.Y));
        });
    }

    [Test]
    public void MovePiece_WhenActivePieceMoved_ShouldIncreaseCurrentStep()
    {
        // Arrange
        int steps = 3;
        int expectedStep = 3;
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        IPiece piece = _gameController.GetAllPieces()[PlayerColor.Red][0];
        piece.State = PieceState.Active;
        piece.CurrentStep = 0;

        // Act
        _gameController.MovePiece(currentPlayer, piece, steps);

        // Assert
        Assert.That(piece.CurrentStep, Is.EqualTo(expectedStep));
    }

    [Test]
    public void MovePiece_WhenActivePieceReachesStepFiftySix_ShouldChangePieceToFinished()
    {
        // Arrange
        PieceState expectedState = PieceState.Finished;
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        IPiece piece = _gameController.GetAllPieces()[PlayerColor.Red][0];
        piece.State = PieceState.Active;
        piece.CurrentStep = 50;

        // Act
        _gameController.MovePiece(currentPlayer, piece, 6);

        // Assert
        Assert.That(piece.State, Is.EqualTo(expectedState));
    }

    [Test]
    public void MovePiece_WhenAllPiecesFinished_ShouldSetIsGameOverToTrue()
    {
        // Arrange
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        IList<IPiece> redPieces = _gameController.GetAllPieces()[PlayerColor.Red];
        for (int i = 0; i < 3; i++)
        {
            redPieces[i].State = PieceState.Finished;
        }
        redPieces[3].State = PieceState.Active;
        redPieces[3].CurrentStep = 50;

        // Act
        _gameController.MovePiece(currentPlayer, redPieces[3], 6);

        // Assert
        Assert.That(_gameController.IsGameOver, Is.True);
    }

    [Test]
    public void MovePiece_WhenEnemyPieceIsCaptured_ShouldSendEnemyPieceToBase()
    {
        // Arrange
        PieceState expectedEnemyState = PieceState.Base;
        IPlayer redPlayer = _players[0];
        IPiece redPiece = _gameController.GetAllPieces()[PlayerColor.Red][0];
        IPiece bluePiece = _gameController.GetAllPieces()[PlayerColor.Blue][0];

        redPiece.State = PieceState.Active;
        redPiece.CurrentStep = 13;
        redPiece.CurrentPosition = new Position(8, 1);

        bluePiece.State = PieceState.Active;
        bluePiece.CurrentStep = 1;
        bluePiece.CurrentPosition = new Position(8, 2);

        // Act
        _gameController.MovePiece(redPlayer, redPiece, 1);

        // Assert
        Assert.That(bluePiece.State, Is.EqualTo(expectedEnemyState));
    }
}

[TestFixture]
public class Tile_Should
{
    [Test]
    public void Position_WhenPositionProvided_ShouldReturnCorrectPosition()
    {
        // Arrange
        Position expectedPosition = new Position(3, 5);
        Tile tile = new(expectedPosition, TileTypes.Normal);

        // Act
        Position position = tile.Position;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(position.X, Is.EqualTo(expectedPosition.X));
            Assert.That(position.Y, Is.EqualTo(expectedPosition.Y));
        });
    }

    [Test]
    public void Type_WhenCreatedWithNormalType_ShouldReturnNormal()
    {
        // Arrange
        Tile tile = new(new Position(0, 0), TileTypes.Normal);

        // Act
        TileTypes type = tile.Type;

        // Assert
        Assert.That(type, Is.EqualTo(TileTypes.Normal));
    }

    [Test]
    public void Type_WhenCreatedWithBaseType_ShouldReturnBase()
    {
        // Arrange
        Tile tile = new(new Position(2, 2), TileTypes.Base);

        // Act
        TileTypes type = tile.Type;

        // Assert
        Assert.That(type, Is.EqualTo(TileTypes.Base));
    }

    [Test]
    public void Type_WhenCreatedWithFinishType_ShouldReturnFinish()
    {
        // Arrange
        Tile tile = new(new Position(7, 7), TileTypes.Finish);

        // Act
        TileTypes type = tile.Type;

        // Assert
        Assert.That(type, Is.EqualTo(TileTypes.Finish));
    }

    [Test]
    public void Pieces_WhenTileCreated_ShouldBeEmpty()
    {
        // Arrange
        Tile tile = new(new Position(0, 0), TileTypes.Normal);

        // Act
        IList<IPiece> pieces = tile.Pieces;

        // Assert
        Assert.That(pieces, Is.Empty);
    }
}
