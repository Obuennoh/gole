namespace Gole.Core;

public sealed class ReminderPhrases(Random random)
{
    private readonly Random random = random ?? throw new ArgumentNullException(nameof(random));
    private string? previous;

    public string Next(HydrationMood mood, int personalityId = 0)
    {
        previous = PhraseSelection.Next(random, PersonalityCatalog.Reminders(personalityId, mood), previous);
        return previous;
    }

    public static string Next(Random random, HydrationMood mood, int personalityId = 0)
    {
        ArgumentNullException.ThrowIfNull(random);
        return PhraseSelection.Next(random, PersonalityCatalog.Reminders(personalityId, mood), null);
    }
}
