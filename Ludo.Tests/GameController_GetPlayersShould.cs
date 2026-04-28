using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

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
    public void ReturnCorrectPlayerCount()
    {
        // Arrange
        int expectedCount = 4;

        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.That(players, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void ReturnPlayersInCorrectOrder()
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
    public void ReturnPlayersWithCorrectColors()
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
    public void ReturnPlayersWithCorrectNames()
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
    public void ReturnReadOnlyList()
    {
        // Act
        IList<IPlayer> players = _gameController.GetPlayers();

        // Assert
        Assert.That(players.IsReadOnly, Is.True);
    }

    [Test]
    public void ReturnTwoPlayersWhenOnlyTwoPlayersAdded()
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
