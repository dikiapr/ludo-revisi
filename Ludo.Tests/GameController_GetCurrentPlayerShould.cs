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
    public void ReturnFirstPlayerAtGameStart()
    {
        // Arrange
        IPlayer expectedPlayer = _players[0];

        // Act
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer, Is.EqualTo(expectedPlayer));
    }

        [Test]
    public void ReturnSecondPlayerAfterOneNextTurn()
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
    public void ReturnThirdPlayerAfterTwoNextTurns()
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
    public void ReturnFourthPlayerAfterThreeNextTurns()
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
    public void ReturnFirstPlayerColorIsRed()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Red;

        // Act
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();

        // Assert
        Assert.That(currentPlayer.Color, Is.EqualTo(expectedColor));
    }

    [Test]
    public void ReturnSecondPlayerColorIsBlue()
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
    public void ReturnThirdPlayerColorIsGreen()
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
    public void ReturnFourthPlayerColorIsYellow()
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
