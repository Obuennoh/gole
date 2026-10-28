using Gole.Core;

namespace Gole.Tests;

public sealed class DrinkReactionTests
{
    [Fact]
    public void Feedback_varies_without_quantities_or_consecutive_repetitions()
    {
        var reactions = new DrinkReactions(new Random(42));
        var seen = new HashSet<string>();
        string? previous = null;
        for (int i = 0; i < 200; i++)
        {
            var message = reactions.Next();
            Assert.False(string.IsNullOrWhiteSpace(message));
            Assert.NotEqual(previous, message);
            Assert.DoesNotContain(message, char.IsDigit);
            Assert.DoesNotContain("ml", message, StringComparison.OrdinalIgnoreCase);
            seen.Add(message);
            previous = message;
        }
        Assert.True(seen.Count >= 5);
    }
}
