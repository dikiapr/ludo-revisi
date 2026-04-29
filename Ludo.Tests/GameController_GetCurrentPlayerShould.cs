using System;
using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

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
