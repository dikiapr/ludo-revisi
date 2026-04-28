using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

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
    public void CurrentPlayerIndexShouldIncrementAfterNextTurn()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 1;

        // Act
        _gameController.NextTurn();

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }

    [Test]
    public void CurrentPlayerIndexShouldWrapAroundToFirstPlayerAfterLastPlayer()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 0;

        // Act
        _gameController.NextTurn(); // 0 -> 1
        _gameController.NextTurn(); // 1 -> 2
        _gameController.NextTurn(); // 2 -> 3
        _gameController.NextTurn(); // 3 -> 0

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }

    [Test]
    public void CurrentPlayerIndexShouldNotChangeWhenBonusTurnAfterRollingSix()
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

    [Test]
    public void CurrentPlayerIndexShouldIncrementAfterBonusTurnConsumed()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 1;
        _fakeDice.ValueToReturn = 6;
        _gameController.RollDice();

        // Act
        _gameController.NextTurn(); // bonus turn consumed, index stays 0
        _gameController.NextTurn(); // normal advance, index goes to 1

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }

    [Test]
    public void CurrentPlayerIndexShouldIncrementNormallyWhenDiceIsNotSix()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 1;
        _fakeDice.ValueToReturn = 3;
        _gameController.RollDice();

        // Act
        _gameController.NextTurn();

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }
}
