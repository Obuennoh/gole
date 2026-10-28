using Gole.Core;
using Gole.Infrastructure;

namespace Gole.Tests;

public sealed class HistoryTests
{
    private static readonly DateTime Start = new(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Removing_an_older_drink_preserves_current_level_and_deadlines()
    {
        var state = HydrationState.Fresh(Start);
        var older = state.Drink(Start);
        state.ServingMl = 500;
        state.Drink(Start.AddMinutes(10));
        var before = (state.LastDrink, state.VisualAnchor, state.NextReminder, state.NextPhrase);
        var session = new HydrationSession(state);
        Assert.True(session.RemoveDrink(older, Start.AddMinutes(20)));
        Assert.Equal(500, state.Today(Start));
        Assert.Equal(before, (state.LastDrink, state.VisualAnchor, state.NextReminder, state.NextPhrase));
    }

    [Fact]
    public void Removing_latest_after_reopening_uses_the_last_remaining_drink()
    {
        var state = HydrationState.Fresh(Start);
        state.Drink(Start);
        var latest = state.Drink(Start.AddMinutes(20));
        var session = new HydrationSession(state);
        Assert.True(session.RemoveDrink(latest, Start.AddMinutes(25)));
        Assert.Equal(Start.Ticks, state.LastDrink);
        Assert.Equal(Start.AddMinutes(30).Ticks, state.NextReminder);
        Assert.Equal(1.0 / 6, state.Level(Start.AddMinutes(25)), 8);
    }

    [Fact]
    public void Older_record_removal_cannot_be_resurrected_by_a_later_undo()
    {
        var session = new HydrationSession(HydrationState.Fresh(Start));
        session.Drink(Start);
        var first = session.State.Sips[0];
        session.Drink(Start.AddMinutes(10));
        Assert.True(session.RemoveDrink(first, Start.AddMinutes(11)));
        Assert.True(session.Undo(Start.AddMinutes(12)));
        Assert.Empty(session.State.Sips);
        Assert.Equal(0, session.State.LastDrink);
        Assert.Equal(0, session.State.Level(Start.AddMinutes(12)));
    }

    [Fact]
    public void Identical_timestamp_and_volume_records_are_removed_individually()
    {
        var state = HydrationState.Fresh(Start);
        var first = state.Drink(Start);
        var second = state.Drink(Start);
        var session = new HydrationSession(state);
        Assert.True(session.RemoveDrink(first, Start.AddMinutes(5)));
        Assert.Same(second, Assert.Single(state.Sips));
        Assert.Equal(200, state.Today(Start));
        Assert.False(session.RemoveDrink(first, Start.AddMinutes(5)));
        Assert.True(session.Undo(Start.AddMinutes(5)));
        Assert.Empty(state.Sips);
    }

    [Fact]
    public void Clearing_all_days_resets_counts_and_timing_but_keeps_preferences()
    {
        var state = HydrationState.Fresh(Start);
        state.Name = "Lulu";
        state.Color = "#D875B3";
        state.Accessories = [26, 27, 28];
        state.ServingMl = 333;
        state.Goal = 2500;
        state.AlertVolume = 35;
        state.Drink(Start);
        var now = Start.AddDays(1);
        state.Drink(now);
        var session = new HydrationSession(state);
        Assert.True(session.ClearDrinks(now));
        Assert.Empty(state.Sips);
        Assert.Equal(0, state.LastDrink);
        Assert.Equal(0, state.Level(now));
        Assert.Equal(now.AddMinutes(state.Interval).Ticks, state.NextReminder);
        Assert.Equal(now.AddMinutes(5).Ticks, state.NextPhrase);
        Assert.Equal("Lulu", state.Name);
        Assert.Equal("#D875B3", state.Color);
        Assert.Equal(new[] { 26, 27, 28 }, state.Accessories);
        Assert.Equal(333, state.ServingMl);
        Assert.Equal(2500, state.Goal);
        Assert.Equal(35, state.AlertVolume);
        Assert.False(session.CanUndo);
        Assert.False(session.ClearDrinks(now));
        Assert.False(session.Advance(now).Notify);
    }

    [Fact]
    public void Deleted_history_stays_deleted_after_saving_and_reopening()
    {
        string root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Gole.History.Tests")) + Path.DirectorySeparatorChar;
        string folder = Path.Combine(root, Guid.NewGuid().ToString("N"));
        try
        {
            var store = new JsonStateStore(folder);
            var state = HydrationState.Fresh(Start);
            state.Drink(Start.AddDays(-1));
            state.Drink(Start);
            Assert.True(store.Save(state));
            var loaded = store.Load(Start).State;
            var session = new HydrationSession(loaded);
            Assert.True(session.RemoveDrink(loaded.Sips[0], Start));
            Assert.True(store.Save(loaded));
            Assert.Single(store.Load(Start).State.Sips);
            Assert.True(session.ClearDrinks(Start));
            Assert.True(store.Save(loaded));
            var reopened = store.Load(Start).State;
            Assert.Empty(reopened.Sips);
            Assert.Equal(0, reopened.LastDrink);
        }
        finally
        {
            if (Path.GetFullPath(folder).StartsWith(root, StringComparison.OrdinalIgnoreCase) && Directory.Exists(folder))
                Directory.Delete(folder, recursive: true);
        }
    }
}
