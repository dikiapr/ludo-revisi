using System;
using Ludo.Backend.Models;

namespace Ludo.Tests;

[TestFixture]
public class DiceTests
{
    [Test]
    public void ReturnRollRangeOneToSix()
    {
        Dice dice = new Dice();
        for(int i = 0; i < 1000; i++)
        {
            int result = dice.Roll();
            Assert.That(result, Is.InRange(1, 6));
        }
    }
}
