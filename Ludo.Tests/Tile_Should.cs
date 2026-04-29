using Ludo.Backend.Enums;
using Ludo.Backend.Interfaces;
using Ludo.Backend.Models;

namespace Ludo.Tests;

[TestFixture]
public class Tile_Should
{
    [Test]
    public void Position_WhenPositionProvided_ShouldReturnCorrectPosition()
    {
        // Arrange
        Position expectedPosition = new Position(3, 5);
        Tile tile = new(expectedPosition, TileTypes.Normal);

        // Act
        Position position = tile.Position;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(position.X, Is.EqualTo(expectedPosition.X));
            Assert.That(position.Y, Is.EqualTo(expectedPosition.Y));
        });
    }

    [Test]
    public void Type_WhenCreatedWithNormalType_ShouldReturnNormal()
    {
        // Arrange
        Tile tile = new(new Position(0, 0), TileTypes.Normal);

        // Act
        TileTypes type = tile.Type;

        // Assert
        Assert.That(type, Is.EqualTo(TileTypes.Normal));
    }

    [Test]
    public void Type_WhenCreatedWithBaseType_ShouldReturnBase()
    {
        // Arrange
        Tile tile = new(new Position(2, 2), TileTypes.Base);

        // Act
        TileTypes type = tile.Type;

        // Assert
        Assert.That(type, Is.EqualTo(TileTypes.Base));
    }

    [Test]
    public void Type_WhenCreatedWithFinishType_ShouldReturnFinish()
    {
        // Arrange
        Tile tile = new(new Position(7, 7), TileTypes.Finish);

        // Act
        TileTypes type = tile.Type;

        // Assert
        Assert.That(type, Is.EqualTo(TileTypes.Finish));
    }

    [Test]
    public void Pieces_WhenTileCreated_ShouldBeEmpty()
    {
        // Arrange
        Tile tile = new(new Position(0, 0), TileTypes.Normal);

        // Act
        IList<IPiece> pieces = tile.Pieces;

        // Assert
        Assert.That(pieces, Is.Empty);
    }
}
