using Gole.Core;

namespace Gole.Tests;

public sealed class PersonalityTests
{
    [Fact]
    public void Every_style_has_distinct_content_for_all_moods_and_after_drinking()
    {
        var all = new HashSet<string>();
        foreach (var option in PersonalityCatalog.Options.Where(p => p.Id != 0))
        {
            var reminders = new ReminderPhrases(new Random(42));
            foreach (var mood in Enum.GetValues<HydrationMood>())
            {
                var pool = PersonalityCatalog.Reminders(option.Id, mood);
                Assert.True(pool.Count >= 8);
                string? previous = null;
                for (int i = 0; i < 100; i++)
                {
                    string message = reminders.Next(mood, option.Id);
                    Assert.Contains(message, pool);
                    Assert.NotEqual(previous, message);
                    previous = message;
                }
                foreach (string message in pool)
                {
                    Assert.True(all.Add(message), "Repeated personality content: " + message);
                    Assert.InRange(message.Length, 1, 160);
                }
            }
            var reactions = new DrinkReactions(new Random(42));
            string? last = null;
            for (int i = 0; i < 100; i++)
            {
                string message = reactions.Next(option.Id);
                Assert.Contains(message, PersonalityCatalog.Reactions(option.Id));
                Assert.NotEqual(last, message);
                Assert.DoesNotContain(message, char.IsDigit);
                Assert.DoesNotContain("ml", message, StringComparison.OrdinalIgnoreCase);
                last = message;
            }
            foreach (string message in PersonalityCatalog.Reactions(option.Id))
                Assert.True(all.Add(message));
        }
        Assert.True(all.Count >= 560);
    }

    [Fact]
    public void Random_mode_uses_all_styles_without_consecutive_repetition()
    {
        var randomMode = new ReminderPhrases(new Random(13));
        var seen = new HashSet<int>();
        string? previous = null;
        for (int i = 0; i < 300; i++)
        {
            string message = randomMode.Next(HydrationMood.Sad);
            Assert.NotEqual(previous, message);
            var owner = Assert.Single(PersonalityCatalog.Options, p => p.Id != 0 && PersonalityCatalog.Reminders(p.Id, HydrationMood.Sad).Contains(message));
            seen.Add(owner.Id);
            previous = message;
        }
        Assert.Equal(PersonalityCatalog.Options.Count - 1, seen.Count);
    }

    [Fact]
    public void Unknown_personality_falls_back_to_random_and_valid_choices_are_preserved()
    {
        var state = HydrationState.Fresh(DateTime.UtcNow);
        state.PersonalityId = 999;
        state.Validate(DateTime.UtcNow);
        Assert.Equal(0, state.PersonalityId);
        state.PersonalityId = 9;
        state.Validate(DateTime.UtcNow);
        Assert.Equal(9, state.PersonalityId);
        state.PersonalityId = 10;
        state.Validate(DateTime.UtcNow);
        Assert.Equal(10, state.PersonalityId);
        Assert.Equal("Brava", PersonalityCatalog.Get(state.PersonalityId).Name);
        Assert.Contains(ReminderPhrases.Next(new Random(1), HydrationMood.Dead, -1), PersonalityCatalog.Reminders(0, HydrationMood.Dead));
    }
}
