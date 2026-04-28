using System;
using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

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
        Dictionary<PlayerColor, List<IPiece>> pieces = new();

        _gameController = new GameController(board, dice, players, pieces);
    }

    [Test]
    public void RollDiceShouldReturnOneToSix()
    {
        // Arrange
        List<int> results = new List<int>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            results.Add(_gameController.RollDice());
        }

        // Assert
        Assert.That(results, Has.All.InRange(1, 6));
    }

    [Test]
    public void RollDiceShouldNotReturnOutOfRange()
    {
        // Arrange
        List<int> results = new List<int>();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            results.Add(_gameController.RollDice());
        }

        // Assert
        Assert.That(results, Has.None.LessThan(1));
        Assert.That(results, Has.None.GreaterThan(6));
    }
}
