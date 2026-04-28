using System;
using Ludo.Backend.Controllers;
using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

[TestFixture]
public class GameController_StartGameShould
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
    }

    [Test]
    public void CurrentPlayerIndexMustZeroWhenStartGameExecute()
    {
        // Arrange
        int expectedCurrentPlayerIndex = 0;

        // Act
        _gameController.StartGame(_players);

        // Assert
        Assert.That(_gameController.CurrentPlayerIndex, Is.EqualTo(expectedCurrentPlayerIndex));
    }

    [Test]
    public void IsGameOverMustFalseWhenStartGameExecute()
    {
        // Arrange
        bool expectedIsGameOver = false;

        // Act
        _gameController.StartGame(_players);

        // Assert
        Assert.That(_gameController.IsGameOver, Is.EqualTo(expectedIsGameOver));
    }

    [Test]
    public void StartGameShouldCreateFourPiecesPerPlayer()
    {
        // Arrange
        int expectedPiecesPerPlayer = 4;

        // Act
        _gameController.StartGame(_players);

        // Assert
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();
        Assert.Multiple(() =>
        {
            Assert.That(allPieces[PlayerColor.Red], Has.Count.EqualTo(expectedPiecesPerPlayer));
            Assert.That(allPieces[PlayerColor.Blue], Has.Count.EqualTo(expectedPiecesPerPlayer));
        });
    }

    [Test]
    public void StartGameShouldSetAllPiecesToBaseState()
    {
        // Arrange
        PieceState expectedPieceState = PieceState.Base;

        // Act
        _gameController.StartGame(_players);

        // Assert
        IDictionary<PlayerColor, IList<IPiece>> allPieces = _gameController.GetAllPieces();
        foreach (IList<IPiece> pieces in allPieces.Values)
        {
            Assert.That(pieces, Has.All.Property("State").EqualTo(expectedPieceState));
        }
    }

    [Test]
    public void StartGameShouldAcceptFourPlayersAsMaximum()
    {
        // Arrange
        int expectedPlayerCount = 4;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPlayerCount));
    }

    [Test]
    public void StartGameShouldNotInitializeWhenPlayersIsNull()
    {
        // Arrange
        int expectedPieceCount = 0;

        // Act
        _gameController.StartGame(null!);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPieceCount));
    }

    [Test]
    public void StartGameShouldNotInitializeWhenLessThanTwoPlayers()
    {
        // Arrange
        int expectedPieceCount = 0;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPieceCount));
    }

    [Test]
    public void StartGameShouldNotInitializeWhenMoreThanFourPlayers()
    {
        // Arrange
        int expectedPieceCount = 0;
        List<IPlayer> players = new List<IPlayer>
        {
            new Player("Red", PlayerColor.Red),
            new Player("Blue", PlayerColor.Blue),
            new Player("Yellow", PlayerColor.Yellow),
            new Player("Green", PlayerColor.Green),
            new Player("Extra", PlayerColor.Red),
        };

        // Act
        _gameController.StartGame(players);

        // Assert
        Assert.That(_gameController.GetAllPieces(), Has.Count.EqualTo(expectedPieceCount));
    }
}
