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
        Dictionary<PlayerColor, List<IPiece>> pieces = new Dictionary<PlayerColor, List<IPiece>>();

       _gameController = new GameController(board, dice, players, pieces);
    }

    [Test]
    public void RollDice_WhenCall_ShouldReturnOneToSix()
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
    public void RollDice_WhenCall_ShouldNotReturnOutOfRange()
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
