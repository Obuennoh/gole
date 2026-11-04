using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using Gole.Core;
using Gole.Desktop;
using Gole.Desktop.Audio;
using Gole.Infrastructure;

namespace Gole.Desktop.Tests;

[Collection("Rendering")]
public sealed class DesktopPlacementTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1880, 0)]
    [InlineData(0, 1020)]
    [InlineData(1880, 1020)]
    [InlineData(-30, -40)]
    [InlineData(1910, 1070)]
    public void Corners_edges_partial_placement_and_taskbar_area_are_not_clamped(double x, double y)
    {
        var offset = DesktopPlacement.RecoveryOffset(new Rect(x, y, 40, 60), new[] { new Rect(0, 0, 1920, 1080) });
        Assert.Equal(new Vector(), offset);
    }

    [Fact]
    public void Negative_coordinates_and_monitor_seams_remain_valid()
    {
        Rect[] screens = { new(-1920, 0, 1920, 1080), new(0, 0, 1920, 1080), new(0, -1080, 1920, 1080) };
        foreach (var point in new[] { new Point(-1920, 0), new Point(-20, 500), new Point(300, -1080), new Point(1900, -20) })
            Assert.Equal(new Vector(), DesktopPlacement.RecoveryOffset(new Rect(point, new Size(40, 60)), screens));
    }

    [Fact]
    public void Only_unreachable_positions_are_recovered_to_the_nearest_connected_screen()
    {
        var lost = new Rect(-1800, 400, 40, 60);
        var offset = DesktopPlacement.RecoveryOffset(lost, new[] { new Rect(0, 0, 1920, 1080) });
        Assert.Equal(new Vector(1800, 0), offset);
        var gap = new Rect(2000, 200, 40, 60);
        offset = DesktopPlacement.RecoveryOffset(gap, new[] { new Rect(0, 0, 1920, 1080), new Rect(2300, 0, 1920, 1080) });
        Assert.Equal(new Vector(-120, 0), offset);
        Assert.Equal(new Vector(), DesktopPlacement.RecoveryOffset(gap, Array.Empty<Rect>()));
    }

    [Fact]
    public void Recovery_uses_device_pixels_and_a_scaled_grab_area()
    {
        var screen = new Rect(0, 0, 3840, 2160);
        Assert.Equal(new Vector(), DesktopPlacement.RecoveryOffset(new Rect(3820, 2140, 80, 120), new[] { screen }, 16));
        Assert.Equal(new Vector(-110, -170), DesktopPlacement.RecoveryOffset(new Rect(3870, 2210, 80, 120), new[] { screen }, 16));
    }

    [Fact]
    public void Speech_is_separate_from_the_avatar_and_cannot_shift_its_saved_position() => Sta(() =>
    {
        string folder = Path.Combine(Path.GetTempPath(), "Gole.Desktop.Tests", Guid.NewGuid().ToString("N"));
        var state = HydrationState.Fresh(DateTime.UtcNow);
        state.X = -25;
        state.Y = -35;
        state.SpeechSound = false;
        var store = new JsonStateStore(folder);
        var pet = new CompanionWindow(new HydrationSession(state), store, null, new SilentSound()) { Left = state.X, Top = state.Y };
        try
        {
            Assert.Same(pet.Art, pet.Content);
            Assert.Equal(pet.Art.Width, pet.Width);
            Assert.Equal(pet.Art.Height, pet.Height);
            pet.Speak("Oi!", 10);
            pet.Speak(string.Join(" ", Enumerable.Repeat("Uma fala bem longa para conferir os cantos.", 8)), 10);
            Assert.False(pet.IsSpeechOpen);
            Assert.False(pet.IsVisible);
            Assert.Equal(-25, pet.Left);
            Assert.Equal(-35, pet.Top);
            state.Size = 280;
            pet.Apply();
            Assert.Equal(pet.Art.Width, pet.Width);
            Assert.Equal(280, pet.Height);
            Assert.Equal(-25, store.Load(DateTime.UtcNow).State.X);
            Assert.Equal(-35, store.Load(DateTime.UtcNow).State.Y);
            foreach (int model in Enumerable.Range(0, PetCatalog.Models.Count))
            {
                pet.Art.Vessel = model;
                var grip = pet.Art.DragBounds;
                Assert.True(grip.Width > 8 && grip.Height > 8);
                Assert.True(new Rect(0, 0, pet.Art.Width, pet.Art.Height).Contains(grip));
            }
            var positions = CompanionWindow.SpeechPlacements(new Size(220, 70), new Size(150, 180), new Point());
            Assert.Equal(4, positions.Length);
            Assert.True(positions[0].Point.Y < 0);
            Assert.True(positions[1].Point.Y >= 180);
            Assert.True(positions[2].Point.X < 0);
            Assert.True(positions[3].Point.X >= 150);
        }
        finally
        {
            pet.Close();
            string expected = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Gole.Desktop.Tests")) + Path.DirectorySeparatorChar;
            if (Path.GetFullPath(folder).StartsWith(expected, StringComparison.OrdinalIgnoreCase) && Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    });

    sealed class SilentSound : IWaterSound
    {
        public void Play(int soundId = SoundCatalog.DefaultId, int volume = SoundCatalog.DefaultVolume, bool preview = false)
        {
        }
        public void Dispose()
        {
        }
    }

    static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { error = ex; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if (error != null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }
}
