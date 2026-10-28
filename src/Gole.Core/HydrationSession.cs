namespace Gole.Core;

public readonly record struct TickResult(bool Notify, bool Changed, bool Chatter = false);

public sealed class HydrationSession(HydrationState state)
{
    private sealed record UndoRecord(Sip Sip, long LastDrink, long Anchor, long Deadline, long PhraseDeadline, long PauseStart, int AmountMl, DateTime Until);
    private UndoRecord? undo;
    public HydrationState State { get; } = state;

    public bool CanUndo(DateTime utcNow) => undo is not null && utcNow < undo.Until && State.Sips.Contains(undo.Sip);

    public void Drink(DateTime utcNow, int ml = HydrationState.RegistrationMl)
    {
        if (ml is < HydrationState.MinimumDrinkMl or > HydrationState.MaximumDrinkMl)
            throw new ArgumentOutOfRangeException(nameof(ml));
        if (State.PauseEnd > 0 && !State.Paused(utcNow))
            State.Resume(utcNow);
        var timing = (State.LastDrink, State.VisualAnchor, State.NextReminder, State.NextPhrase, State.PauseStart, State.LastAmountMl);
        var sip = State.Drink(utcNow, ml);
        undo = new UndoRecord(sip, timing.LastDrink, timing.VisualAnchor, timing.NextReminder, timing.NextPhrase, timing.PauseStart, timing.LastAmountMl, utcNow.AddSeconds(20));
    }

    public bool Undo(DateTime utcNow)
    {
        if (!CanUndo(utcNow))
            return false;
        var record = undo!;
        State.Sips.Remove(record.Sip);
        State.LastDrink = record.LastDrink;
        State.VisualAnchor = record.Anchor;
        State.NextReminder = record.Deadline;
        State.NextPhrase = record.PhraseDeadline;
        State.PauseStart = record.PauseStart;
        State.LastAmountMl = record.AmountMl;
        undo = null;
        return true;
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
