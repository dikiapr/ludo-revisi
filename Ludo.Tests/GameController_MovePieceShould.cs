using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

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
