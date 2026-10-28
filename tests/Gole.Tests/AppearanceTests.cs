using Gole.Core;
using Gole.Infrastructure;

namespace Gole.Tests;

public sealed class AppearanceTests
{
    private sealed class FirstRandom : Random
    {
        public override int Next(int maxValue) => 0;
    }

    [Theory]
    [InlineData(HydrationMood.Happy, "Tô feliz")]
    [InlineData(HydrationMood.Content, "Tô de boa")]
    [InlineData(HydrationMood.Neutral, "Já fiquei sério")]
    [InlineData(HydrationMood.Sad, "Meu sorriso foi")]
    [InlineData(HydrationMood.Crying, "Tô chorando")]
    [InlineData(HydrationMood.Dead, "Morri de sede")]
    public void Random_messages_match_the_current_emotion(HydrationMood mood, string beginning)
    {
        Assert.StartsWith(beginning, ReminderPhrases.Next(new FirstRandom(), mood));
    }
    [Theory]
    [InlineData(1, HydrationMood.Happy)]
    [InlineData(.81, HydrationMood.Happy)]
    [InlineData(.8, HydrationMood.Content)]
    [InlineData(.6, HydrationMood.Neutral)]
    [InlineData(.4, HydrationMood.Sad)]
    [InlineData(.2, HydrationMood.Crying)]
    [InlineData(.01, HydrationMood.Crying)]
    [InlineData(0, HydrationMood.Dead)]
    public void Expressions_change_throughout_the_cycle(double level, HydrationMood expected)
    {
        Assert.Equal(expected, HydrationExpression.AtLevel(level));
    }

    [Fact]
    public void Accessory_selection_caps_at_three_and_allows_replacing_an_item()
    {
        var items = new List<int>();
        Assert.True(PetCatalog.Toggle(items, 3));
        Assert.True(PetCatalog.Toggle(items, 15));
        Assert.True(PetCatalog.Toggle(items, 21));
        Assert.False(PetCatalog.Toggle(items, 20));
        Assert.Equal(new[] { 3, 15, 21 }, items);
        Assert.True(PetCatalog.Toggle(items, 15));
        Assert.True(PetCatalog.Toggle(items, 20));
        Assert.Equal(new[] { 3, 21, 20 }, items);
        Assert.True(PetCatalog.Toggle(items, 0));
        Assert.Empty(items);
    }

    [Fact]
    public void Invalid_or_duplicate_items_cannot_bypass_the_limit()
    {
        Assert.Equal(new[] { 3, 14, 21 }, PetCatalog.Normalize([0, -1, 999, 3, 3, 14, 21, 15]));
    }

    [Fact]
    public void Version_two_migration_preserves_interval_and_current_water_level()
    {
        var now = DateTime.UtcNow;
        var state = HydrationState.Fresh(now);
        state.Version = 2;
        state.Accessory = 13;
        state.SetInterval(now, 45);
        var deadline = state.NextReminder;
        state.Validate(now.AddMinutes(10));
        Assert.Equal(45, state.Interval);
        Assert.Equal(now.Ticks, state.VisualAnchor);
        Assert.Equal(deadline, state.NextReminder);
        Assert.Equal(new[] { 13 }, state.Accessories);
        Assert.Equal(0, state.Accessory);
        Assert.Equal(HydrationState.CurrentVersion, state.Version);
    }

    [Fact]
    public void Clearing_a_migrated_item_does_not_restore_it_on_next_validation()
    {
        var now = DateTime.UtcNow;
        var state = HydrationState.Fresh(now);
        state.Version = 2;
        state.Accessory = 3;
        state.Validate(now);
        PetCatalog.Toggle(state.Accessories, 0);
        state.Validate(now);
        Assert.Empty(state.Accessories);
    }
}
