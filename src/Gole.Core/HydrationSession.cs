namespace Gole.Core;

public readonly record struct TickResult(bool Notify, bool Changed, bool Chatter = false);

public sealed class HydrationSession(HydrationState state)
{
    private sealed record UndoRecord(Sip Sip, long LastDrink, long Anchor, long Deadline, long PhraseDeadline, long PauseStart);
    private UndoRecord? undo;
    public HydrationState State { get; } = state;

    public bool CanUndo => State.Sips.Count > 0;

    public void Drink(DateTime utcNow)
    {
        if (State.PauseEnd > 0 && !State.Paused(utcNow))
            State.Resume(utcNow);
        var timing = (State.LastDrink, State.VisualAnchor, State.NextReminder, State.NextPhrase, State.PauseStart);
        var sip = State.Drink(utcNow);
        undo = new UndoRecord(sip, timing.LastDrink, timing.VisualAnchor, timing.NextReminder, timing.NextPhrase, timing.PauseStart);
    }

    public bool Undo(DateTime utcNow)
    {
        if (!CanUndo)
            return false;
        var latest = State.Sips.Max(s => s.Utc);
        return RemoveDrink(State.Sips.Last(s => s.Utc == latest), utcNow);
    }

    public bool RemoveDrink(Sip sip, DateTime utcNow)
    {
        if (!State.Sips.Contains(sip))
            return false;
        var latest = State.Sips.Max(s => s.Utc);
        State.Sips.Remove(sip);
        if (undo is not null && ReferenceEquals(undo.Sip, sip))
        {
            State.LastDrink = undo.LastDrink;
            State.VisualAnchor = undo.Anchor;
            State.NextReminder = undo.Deadline;
            State.NextPhrase = undo.PhraseDeadline;
            State.PauseStart = undo.PauseStart;
        }
        else if (sip.Utc == latest && !State.Sips.Any(s => s.Utc == latest))
            RebuildTiming(utcNow);
        // Removing any entry invalidates the snapshot so it cannot restore deleted history later.
        undo = null;
        return true;
    }

    public bool ClearDrinks(DateTime utcNow)
    {
        if (State.Sips.Count == 0)
            return false;
        State.Sips.Clear();
        undo = null;
        RebuildTiming(utcNow);
        return true;
    }

    private void RebuildTiming(DateTime utcNow)
    {
        long latest = State.Sips.Count == 0 ? 0 : State.Sips.Max(s => s.Utc);
        long interval = TimeSpan.FromMinutes(State.Interval).Ticks;
        long clock = State.Paused(utcNow) ? State.PauseStart : utcNow.Ticks;
        State.LastDrink = latest;
        State.VisualAnchor = latest == 0 ? clock - interval : latest;
        if (State.Paused(utcNow))
            State.NextReminder = State.PauseEnd + (latest == 0 ? interval : Math.Max(0, latest + interval - clock));
        else
        {
            State.PauseStart = State.PauseEnd = 0;
            State.NextReminder = latest == 0 ? utcNow.AddMinutes(State.Interval).Ticks : Math.Max(utcNow.Ticks, latest + interval);
        }
        State.NextPhrase = utcNow.AddMinutes(HydrationState.PhraseInterval).Ticks;
    }

    public void Pause(DateTime utcNow, int minutes)
    {
        undo = null;
        State.Pause(utcNow, minutes);
    }
    public void Resume(DateTime utcNow)
    {
        undo = null;
        State.Resume(utcNow);
    }
    public void SetInterval(DateTime utcNow, int minutes)
    {
        undo = null;
        State.SetInterval(utcNow, minutes);
    }

    public TickResult Advance(DateTime utcNow)
    {
        var changed = false;
        if (State.PauseEnd > 0 && !State.Paused(utcNow))
        {
            Resume(utcNow);
            changed = true;
        }
        if (State.Paused(utcNow))
            return new(false, changed);
        if (utcNow.Ticks >= State.NextReminder)
        {
            // A reminder replaces simultaneous chatter; missed intervals are never replayed.
            State.NextReminder = utcNow.AddMinutes(State.Interval).Ticks;
            State.NextPhrase = utcNow.AddMinutes(HydrationState.PhraseInterval).Ticks;
            return new(true, true);
        }
        if (utcNow.Ticks >= State.NextPhrase)
        {
            State.NextPhrase = utcNow.AddMinutes(HydrationState.PhraseInterval).Ticks;
            return new(false, true, true);
        }
        return new(false, changed);
    }
}
