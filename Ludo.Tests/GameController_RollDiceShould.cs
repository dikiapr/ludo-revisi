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
    public void RollDiceShouldReturnOneToSix()
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
    public void RollDiceShouldNotReturnOutOfRange()
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

    [Test]
    public void RollDiceWithSeededDiceShouldReturnValueInRange()
    {
        // Arrange
        int expectedMin = 1;
        int expectedMax = 6;
        IDice seededDice = new Dice(42);
        GameController controller = new(new Board(), seededDice, [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        ], []);

        // Act
        int result = controller.RollDice();

        // Assert
        Assert.That(result, Is.InRange(expectedMin, expectedMax));
    }

    [Test]
    public void RollDiceWithSameSeedShouldProduceSameSequence()
    {
        // Arrange
        int seed = 42;
        List<int> firstSequence = [];
        List<int> secondSequence = [];
        List<IPlayer> players =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        ];

        GameController firstController = new(new Board(), new Dice(seed), players, []);
        GameController secondController = new(new Board(), new Dice(seed), players, []);

        // Act
        for (int i = 0; i < 10; i++)
        {
            firstSequence.Add(firstController.RollDice());
            secondSequence.Add(secondController.RollDice());
        }

        // Assert
        Assert.That(firstSequence, Is.EqualTo(secondSequence));
    }
}
