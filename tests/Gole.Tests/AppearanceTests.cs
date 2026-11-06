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
    public void Accessory_selection_caps_at_five_and_allows_replacing_an_item()
    {
        var items = new List<int>();
        Assert.True(PetCatalog.Toggle(items, 3));
        Assert.True(PetCatalog.Toggle(items, 15));
        Assert.True(PetCatalog.Toggle(items, 21));
        Assert.True(PetCatalog.Toggle(items, 69));
        Assert.True(PetCatalog.Toggle(items, 80));
        Assert.False(PetCatalog.Toggle(items, 20));
        Assert.Equal(new[] { 3, 15, 21, 69, 80 }, items);
        Assert.True(PetCatalog.Toggle(items, 15));
        Assert.True(PetCatalog.Toggle(items, 20));
        Assert.Equal(new[] { 3, 21, 69, 80, 20 }, items);
        Assert.True(PetCatalog.Toggle(items, 0));
        Assert.Empty(items);
    }

    [Theory]
    [InlineData(27, 55)]
    [InlineData(29, 53)]
    [InlineData(44, 52)]
    [InlineData(21, 37)]
    [InlineData(5, 33)]
    [InlineData(42, 43)]
    [InlineData(35, 50)]
    [InlineData(5, 67)]
    [InlineData(61, 68)]
    [InlineData(1, 70)]
    [InlineData(40, 71)]
    [InlineData(71, 72)]
    [InlineData(59, 73)]
    [InlineData(73, 74)]
    [InlineData(35, 75)]
    [InlineData(23, 76)]
    [InlineData(19, 77)]
    [InlineData(48, 78)]
    [InlineData(42, 79)]
    [InlineData(79, 80)]
    [InlineData(64, 81)]
    [InlineData(47, 82)]
    public void Clothing_and_accessories_in_the_same_slot_replace_each_other_even_when_full(int oldItem, int newItem)
    {
        var items = new List<int> { oldItem, 3, 7, 20, 32 };
        Assert.True(PetCatalog.CanEquip(items, newItem));
        Assert.True(PetCatalog.Toggle(items, newItem));
        Assert.Equal(5, items.Count);
        Assert.DoesNotContain(oldItem, items);
        Assert.Contains(newItem, items);
        Assert.Equal(new[] { 3, 7, 20, 32, newItem }, PetCatalog.Normalize(new[] { oldItem, 3, 7, 20, 32, newItem }));
    }

    [Fact]
    public void Invalid_or_duplicate_items_cannot_bypass_the_limit()
    {
        Assert.Equal(new[] { 3, 14, 21, 69, 80 }, PetCatalog.Normalize([0, -1, 999, 3, 3, 14, 21, 69, 80, 15]));
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
