using Gole.Core;

namespace Gole.Tests;

public sealed class HydrationSessionTests
{
    private static readonly DateTime Start = new(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);
    private static HydrationSession NewSession() => new(HydrationState.Fresh(Start));

    [Theory]
    [InlineData(50, .2)]
    [InlineData(100, .4)]
    [InlineData(125, .5)]
    [InlineData(250, 1)]
    [InlineData(1000, 1)]
    public void Selected_amount_refills_proportionally_and_records_the_full_volume(int ml, double expected)
    {
        var session = NewSession();
        var now = Start.AddHours(2);
        session.Drink(now, ml);
        Assert.Equal(expected, session.State.Level(now), 8);
        Assert.Equal(ml, session.State.Today(now));
        Assert.Equal(ml, session.State.LastAmountMl);
        Assert.Equal(now.AddMinutes(30).Ticks, session.State.NextReminder);
        Assert.Equal(Math.Max(0, expected - .1), session.State.Level(now.AddMinutes(3)), 8);
    }

    [Fact]
    public void Small_drinks_add_to_the_remaining_level_and_undo_restores_it()
    {
        var session = NewSession();
        var now = Start.AddMinutes(30);
        session.Drink(now, 100);
        session.Drink(now, 100);
        Assert.Equal(.8, session.State.Level(now), 8);
        Assert.True(session.Undo(now.AddSeconds(1)));
        Assert.Equal(.4, session.State.Level(now), 8);
        Assert.Equal(100, session.State.Today(now));
        Assert.Equal(100, session.State.LastAmountMl);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(49)]
    [InlineData(1001)]
    public void Invalid_amount_does_not_change_the_state(int ml)
    {
        var session = NewSession();
        Assert.Throws<ArgumentOutOfRangeException>(() => session.Drink(Start, ml));
        Assert.Empty(session.State.Sips);
        Assert.Equal(0, session.State.LastDrink);
        Assert.Equal(Start.Ticks, session.State.VisualAnchor);
    }

    [Fact]
    public void Chatter_occurs_every_five_minutes_without_a_system_notification()
    {
        var session = NewSession();
        Assert.False(session.Advance(Start.AddMinutes(5).AddTicks(-1)).Chatter);
        Assert.Equal(new TickResult(false, true, true), session.Advance(Start.AddMinutes(5)));
        Assert.False(session.Advance(Start.AddMinutes(5)).Chatter);
        Assert.Equal(Start.AddMinutes(10).Ticks, session.State.NextPhrase);
        Assert.True(session.Advance(Start.AddMinutes(10)).Chatter);
    }

    [Fact]
    public void A_drink_reminder_replaces_simultaneous_chatter()
    {
        var session = NewSession();
        var tick = session.Advance(Start.AddMinutes(30));
        Assert.True(tick.Notify);
        Assert.False(tick.Chatter);
        Assert.Equal(Start.AddMinutes(35).Ticks, session.State.NextPhrase);
    }

    [Fact]
    public void Drinking_restarts_chatter_and_undo_restores_its_old_deadline()
    {
        var session = NewSession();
        session.Drink(Start.AddMinutes(3));
        Assert.Equal(Start.AddMinutes(8).Ticks, session.State.NextPhrase);
        Assert.True(session.Undo(Start.AddMinutes(3).AddSeconds(5)));
        Assert.Equal(Start.AddMinutes(5).Ticks, session.State.NextPhrase);
    }

    [Fact]
    public void Reopening_preserves_the_chatter_deadline()
    {
        var state = NewSession().State;
        state.Validate(Start.AddMinutes(3));
        var reopened = new HydrationSession(state);
        Assert.False(reopened.Advance(Start.AddMinutes(4)).Chatter);
        Assert.True(reopened.Advance(Start.AddMinutes(5)).Chatter);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(15, .5)]
    [InlineData(30, 0)]
    [InlineData(90, 0)]
    public void Level_matches_elapsed_interval(int minutes, double expected)
    {
        var session = NewSession();
        Assert.Equal(expected, session.State.Level(Start.AddMinutes(minutes)));
    }

    [Fact]
    public void Fresh_state_does_not_invent_a_drink()
    {
        var state = NewSession().State;
        Assert.Empty(state.Sips);
        Assert.Equal(0, state.LastDrink);
        Assert.Equal(30, state.Interval);
        Assert.Equal(Start.AddMinutes(30).Ticks, state.NextReminder);
    }

    [Fact]
    public void Drinking_refills_the_pet_and_restarts_the_deadline()
    {
        var session = NewSession();
        var now = Start.AddMinutes(70);
        session.Drink(now);
        Assert.Equal(250, session.State.Today(now));
        Assert.Equal(1, session.State.Level(now));
        Assert.Equal(now.AddMinutes(30).Ticks, session.State.NextReminder);
    }

    [Fact]
    public void Overdue_ticks_emit_only_one_notification_and_leave_pet_empty()
    {
        var session = NewSession();
        var now = Start.AddHours(6);
        Assert.Equal(new TickResult(true, true), session.Advance(now));
        Assert.False(session.Advance(now.AddSeconds(1)).Notify);
        Assert.Equal(0, session.State.Level(now));
        Assert.Equal(now.AddMinutes(30).Ticks, session.State.NextReminder);
    }

    [Fact]
    public void Reminder_occurs_exactly_at_the_deadline()
    {
        var session = NewSession();
        Assert.False(session.Advance(Start.AddMinutes(30).AddTicks(-1)).Notify);
        Assert.True(session.Advance(Start.AddMinutes(30)).Notify);
        Assert.False(session.Advance(Start.AddMinutes(30)).Notify);
    }

    [Fact]
    public void Pause_freezes_both_level_and_remaining_countdown()
    {
        var session = NewSession();
        session.Pause(Start.AddMinutes(10), 60);
        Assert.Equal(2.0 / 3, session.State.Level(Start.AddMinutes(50)), 8);
        Assert.False(session.Advance(Start.AddMinutes(50)).Notify);
        Assert.False(session.Advance(Start.AddMinutes(70)).Notify);
        Assert.Equal(Start.AddMinutes(90).Ticks, session.State.NextReminder);
        Assert.True(session.Advance(Start.AddMinutes(90)).Notify);
        Assert.Equal(0, session.State.Level(Start.AddMinutes(90)));
    }

    [Fact]
    public void Early_resume_counts_only_the_time_actually_paused()
    {
        var session = NewSession();
        session.Pause(Start.AddMinutes(10), 60);
        session.Resume(Start.AddMinutes(25));
        Assert.Equal(Start.AddMinutes(45).Ticks, session.State.NextReminder);
        Assert.Equal(0, session.State.Level(Start.AddMinutes(45)));
    }

    [Fact]
    public void Sleeping_past_pause_end_emits_one_due_notification_on_wake()
    {
        var session = NewSession();
        session.Pause(Start.AddMinutes(10), 60);
        Assert.True(session.Advance(Start.AddHours(3)).Notify);
        Assert.False(session.Advance(Start.AddHours(3)).Notify);
        Assert.Equal(0, session.State.Level(Start.AddHours(3)));
    }

    [Fact]
    public void Drinking_during_pause_keeps_full_pet_until_resume()
    {
        var session = NewSession();
        session.Pause(Start.AddMinutes(10), 60);
        session.Drink(Start.AddMinutes(20));
        session.Advance(Start.AddMinutes(70));
        Assert.Equal(1, session.State.Level(Start.AddMinutes(70)));
        Assert.Equal(Start.AddMinutes(100).Ticks, session.State.NextReminder);
    }

    [Fact]
    public void Interval_change_keeps_the_last_drink_as_the_anchor()
    {
        var session = NewSession();
        session.SetInterval(Start.AddMinutes(10), 60);
        Assert.Equal(Start.AddMinutes(60).Ticks, session.State.NextReminder);
        Assert.Equal(.5, session.State.Level(Start.AddMinutes(30)));
    }

    [Fact]
    public void Interval_change_while_paused_excludes_paused_time()
    {
        var session = NewSession();
        session.Pause(Start.AddMinutes(10), 60);
        session.SetInterval(Start.AddMinutes(20), 45);
        Assert.Equal(Start.AddMinutes(105).Ticks, session.State.NextReminder);
        session.Advance(Start.AddMinutes(70));
        Assert.Equal(Start.AddMinutes(105).Ticks, session.State.NextReminder);
    }

    [Fact]
    public void Undo_restores_water_and_deadline_without_deleting_earlier_records()
    {
        var session = NewSession();
        session.Drink(Start.AddMinutes(5));
        session.Drink(Start.AddMinutes(6));
        Assert.True(session.Undo(Start.AddMinutes(6).AddSeconds(5)));
        Assert.Equal(250, session.State.Today(Start));
        Assert.Equal(Start.AddMinutes(5).Ticks, session.State.LastDrink);
        Assert.Equal(Start.AddMinutes(35).Ticks, session.State.NextReminder);
        Assert.False(session.Undo(Start.AddMinutes(6).AddSeconds(6)));
    }

    [Fact]
    public void Undo_expires_after_twenty_seconds()
    {
        var session = NewSession();
        session.Drink(Start);
        Assert.False(session.Undo(Start.AddSeconds(20)));
        Assert.Single(session.State.Sips);
    }

    [Fact]
    public void Pause_invalidates_undo_so_it_cannot_restore_stale_timing()
    {
        var session = NewSession();
        session.Drink(Start);
        session.Pause(Start.AddSeconds(3), 60);
        Assert.False(session.CanUndo(Start.AddSeconds(4)));
    }

    [Fact]
    public void Undo_during_pause_restores_the_original_pause_anchor()
    {
        var session = NewSession();
        session.Pause(Start.AddMinutes(10), 60);
        session.Drink(Start.AddMinutes(20));
        Assert.True(session.Undo(Start.AddMinutes(20).AddSeconds(1)));
        session.Advance(Start.AddMinutes(70));
        Assert.Equal(Start.AddMinutes(90).Ticks, session.State.NextReminder);
        Assert.Empty(session.State.Sips);
    }

    [Fact]
    public void Daily_total_uses_local_midnight_instead_of_utc_midnight()
    {
        var zone = TimeZoneInfo.CreateCustomTimeZone("Test/Brazil", TimeSpan.FromHours(-3), "Test", "Test");
        var midnight = new DateTime(2026, 10, 27, 3, 0, 0, DateTimeKind.Utc);
        var state = HydrationState.Fresh(midnight);
        state.Drink(midnight.AddSeconds(-1));
        state.Drink(midnight.AddSeconds(1));
        Assert.Equal(250, state.Today(midnight.AddMinutes(5), zone));
    }
}
