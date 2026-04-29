using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

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
