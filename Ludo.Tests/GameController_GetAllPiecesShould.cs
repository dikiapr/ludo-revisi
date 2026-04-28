using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

[TestFixture]
public class GameController_GetAllPiecesShould
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
    public void ReturnEmptyDictionaryBeforeStartGame()
    {
        // Arrange
        int expectedCount = 0;

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.That(allPieces, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void ReturnFourEntriesAfterStartGameWithFourPlayers()
    {
        // Arrange
        int expectedCount = 4;
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.That(allPieces, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void ReturnFourPiecesPerPlayerAfterStartGame()
    {
        // Arrange
        int expectedPiecesPerPlayer = 4;
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(allPieces[PlayerColor.Red],    Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Blue],   Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Yellow], Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Green],  Has.Count.EqualTo(expectedPiecesPerPlayer));
        });
    }

    [Test]
    public void AllPiecesStartInBaseState()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        foreach (IList<IPiece> pieces in allPieces.Values)
        {
            Assert.That(pieces, Has.All.Property(nameof(IPiece.State)).EqualTo(PieceState.Base));
        }
    }

    [Test]
    public void AllPiecesStartWithStepMinusOne()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        foreach (IList<IPiece> pieces in allPieces.Values)
        {
            Assert.That(pieces, Has.All.Property(nameof(IPiece.CurrentStep)).EqualTo(-1));
        }
    }

    [Test]
    public void EachPieceColorMatchesItsPlayerColor()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        foreach (KeyValuePair<PlayerColor, IList<IPiece>> entry in allPieces)
        {
            Assert.That(entry.Value, Has.All.Property(nameof(IPiece.Color)).EqualTo(entry.Key));
        }
    }

    [Test]
    public void ReturnTwoEntriesAfterStartGameWithTwoPlayers()
    {
        // Arrange
        int expectedCount = 2;
        List<IPlayer> twoPlayers =
        [
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
        ];
        _gameController.StartGame(twoPlayers);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.That(allPieces, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void ContainsAllPlayerColorKeysAfterStartGame()
    {
        // Arrange
        _gameController.StartGame(_players);

        // Act
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(allPieces.ContainsKey(PlayerColor.Red),    Is.True);
            Assert.That(allPieces.ContainsKey(PlayerColor.Blue),   Is.True);
            Assert.That(allPieces.ContainsKey(PlayerColor.Yellow), Is.True);
            Assert.That(allPieces.ContainsKey(PlayerColor.Green),  Is.True);
        });
    }
}
