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
}
