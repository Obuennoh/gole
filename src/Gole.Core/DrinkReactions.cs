namespace Gole.Core;

public sealed class DrinkReactions(Random random)
{
    private readonly Random random = random ?? throw new ArgumentNullException(nameof(random));
    private string? previous;

    public string Next(int personalityId = 0)
    {
        previous = PhraseSelection.Next(random, PersonalityCatalog.Reactions(personalityId), previous);
        return previous;
    }
}
