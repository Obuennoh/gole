using Gole.Core;
using Gole.Infrastructure;

namespace Gole.Tests;

public sealed class StateStoreTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "Gole.Tests", Guid.NewGuid().ToString("N"));
    private static readonly DateTime Now = new(2026, 10, 27, 12, 0, 0, DateTimeKind.Utc);
    private JsonStateStore Store() => new(directory);

    [Theory]
    [InlineData(25)]
    [InlineData(125)]
    [InlineData(250)]
    [InlineData(500)]
    public void Existing_volumes_survive_updates_and_new_records_use_the_default_serving(int historicalMl)
    {
        var state = HydrationState.Fresh(Now);
        state.Version = 6;
        state.Sips.Add(new Sip { Utc = Now.Ticks, Ml = historicalMl });
        Assert.True(Store().Save(state));
        var loaded = Store().Load(Now.AddMinutes(3)).State;
        Assert.Equal(historicalMl, loaded.Today(Now));
        Assert.Equal(state.NextReminder, loaded.NextReminder);
        Assert.Equal(.9, loaded.Level(Now.AddMinutes(3)), 8);
        loaded.Drink(Now.AddMinutes(3));
        Assert.Equal(historicalMl + 200, loaded.Today(Now));
        Assert.Equal(1, loaded.Level(Now.AddMinutes(3)));
        Assert.True(Store().Save(loaded));
        Assert.Equal(historicalMl + 200, Store().Load(Now.AddMinutes(3)).State.Today(Now));
    }

    [Fact]
    public void Version_three_migration_keeps_timing_records_and_appearance()
    {
        var state = HydrationState.Fresh(Now);
        state.Version = 3;
        state.Accessories = [26, 27];
        state.Sips.Add(new Sip { Utc = Now.Ticks, Ml = 500 });
        Assert.True(Store().Save(state));
        var loaded = Store().Load(Now.AddMinutes(15)).State;
        Assert.Equal(HydrationState.CurrentVersion, loaded.Version);
        Assert.Equal(.5, loaded.Level(Now.AddMinutes(15)), 8);
        Assert.Equal(500, loaded.Today(Now));
        Assert.Equal(new[] { 26, 27 }, loaded.Accessories);
    }

    [Fact]
    public void First_load_returns_defaults_without_writing_files()
    {
        var store = Store();
        var result = store.Load(Now);
        Assert.False(result.Recovered);
        Assert.False(result.Reset);
        Assert.False(File.Exists(store.FilePath));
        Assert.Equal(30, result.State.Interval);
        Assert.Equal(3, result.State.AlertSoundId);
    }

    [Fact]
    public void Save_and_load_preserve_records_appearance_and_deadline()
    {
        var state = HydrationState.Fresh(Now);
        state.ServingMl = 375;
        state.Drink(Now);
        state.Name = "Água da Lu";
        state.Vessel = 5;
        state.Accessories = [3, 21, 11];
        state.Color = "#D875B3";
        state.SetInterval(Now, 45);
        state.Goal = 2500;
        state.Size = 160;
        state.Motion = false;
        state.Sound = true;
        state.SpeechSound = false;
        state.AlertVolume = 35;
        state.X = 100;
        state.Y = 200;
        var store = Store();
        Assert.True(store.Save(state));
        var loaded = Store().Load(Now).State;
        Assert.Equal(state.Name, loaded.Name);
        Assert.Equal(state.Color, loaded.Color);
        Assert.Equal(5, loaded.Vessel);
        Assert.Equal(new[] { 3, 21, 11 }, loaded.Accessories);
        Assert.Equal(375, loaded.Today(Now));
        Assert.Equal(375, loaded.ServingMl);
        Assert.Equal(state.NextReminder, loaded.NextReminder);
        Assert.Equal(state.NextPhrase, loaded.NextPhrase);
        Assert.Equal(45, loaded.Interval);
        Assert.Equal(2500, loaded.Goal);
        Assert.Equal(160, loaded.Size);
        Assert.False(loaded.Motion);
        Assert.True(loaded.Sound);
        Assert.False(loaded.SpeechSound);
        Assert.Equal(35, loaded.AlertVolume);
        Assert.Equal(100, loaded.X);
        Assert.Equal(200, loaded.Y);
    }

    [Fact]
    public void Damaged_primary_recovers_backup_and_preserves_it_on_first_save()
    {
        var state = HydrationState.Fresh(Now);
        var store = Store();
        store.Save(state);
        state.Drink(Now);
        store.Save(state);
        var backup = File.ReadAllText(store.BackupPath);
        File.WriteAllText(store.FilePath, "broken json");
        var recovery = Store();
        var result = recovery.Load(Now);
        Assert.True(result.Recovered);
        Assert.Empty(result.State.Sips);
        Assert.True(recovery.Save(result.State));
        Assert.Equal(backup, File.ReadAllText(store.BackupPath));
        Assert.False(File.Exists(store.FilePath + ".tmp"));
    }

    [Fact]
    public void Legacy_data_migrates_without_discarding_drinks_or_accessories()
    {
        Directory.CreateDirectory(directory);
        var json = "{\"Name\":\"Lulu\",\"Color\":\"#E694A7\",\"Interval\":45,\"Tone\":2,\"Cup\":500,\"Accessory\":3,\"Sips\":[{\"Utc\":" + Now.Ticks + ",\"Ml\":500}]}";
        File.WriteAllText(Store().FilePath, json);
        var state = Store().Load(Now).State;
        Assert.Equal(HydrationState.CurrentVersion, state.Version);
        Assert.Equal(30, state.Interval);
        Assert.Equal(500, state.Today(Now));
        Assert.Equal(new[] { 3 }, state.Accessories);
        Assert.Equal(2000, state.Goal);
        Assert.Equal("Lulu", state.Name);
        Assert.Equal(200, state.ServingMl);
        Assert.True(state.SpeechSound);
        Assert.Equal(3, state.AlertSoundId);
        Assert.Equal(70, state.AlertVolume);
    }

    [Fact]
    public void Invalid_values_are_normalized_and_impossible_records_removed()
    {
        var state = HydrationState.Fresh(Now);
        state.Name = "";
        state.Color = "Transparent";
        state.Interval = 0;
        state.Size = double.NaN;
        state.LastDrink = long.MaxValue;
        state.Sips.Add(new Sip { Utc = long.MaxValue, Ml = 250 });
        state.Validate(Now);
        Assert.Equal("Gole", state.Name);
        Assert.Equal("#26B9C4", state.Color);
        Assert.Equal(5, state.Interval);
        Assert.Equal(180, state.Size);
        Assert.Equal(0, state.LastDrink);
        Assert.Empty(state.Sips);
    }

    [Fact]
    public void Newer_schema_is_rejected_instead_of_silently_overwritten()
    {
        Directory.CreateDirectory(directory);
        File.WriteAllText(Store().FilePath, "{\"Version\":999}");
        Assert.Throws<NotSupportedException>(() => Store().Load(Now));
        Assert.Contains("999", File.ReadAllText(Store().FilePath));
    }

    [Fact]
    public void Persistence_failure_is_reported_without_changing_the_existing_file()
    {
        var store = Store();
        var state = HydrationState.Fresh(Now);
        Assert.True(store.Save(state));
        var original = File.ReadAllText(store.FilePath);
        using (File.Open(store.FilePath + ".tmp", FileMode.Create, FileAccess.ReadWrite, FileShare.None))
        {
            state.Drink(Now);
            Assert.False(store.Save(state));
            Assert.True(store.SaveFailed);
            Assert.Equal(original, File.ReadAllText(store.FilePath));
        }
    }

    public void Dispose()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Gole.Tests")) + Path.DirectorySeparatorChar;
        if (!Path.GetFullPath(directory).StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Unexpected test directory.");
        if (Directory.Exists(directory))
            Directory.Delete(directory, recursive: true);
    }
}
