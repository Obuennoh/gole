using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gole.Core;
using Gole.Desktop;

namespace Gole.Desktop.Tests;

[Collection("Rendering")]
public sealed class AnimationTests
{
    [Fact]
    public void Reduced_motion_and_pause_stop_all_motion_including_accessories_and_celebrations() => Sta(() =>
    {
        foreach (var items in new[] { new[] { 17, 35, 47 }, new[] { 44, 32, 48 }, new[] { 29, 46, 50 }, new[] { 1, 27, 15 } })
            foreach (bool paused in new[] { false, true })
            {
                var art = new Bottle
                {
                    EquippedAccessories = items,
                    MotionEnabled = paused,
                    Paused = paused,
                    Celebrate = true,
                    CelebrationSeconds = .7,
                    Level = .7,
                };
                var before = Render(art);
                art.Phase = 9.9;
                art.CelebrationSeconds = 1.5;
                Assert.Equal(before, Render(art));
            }
    });

    [Fact]
    public void Accessory_motion_is_independent_of_body_bobbing() => Sta(() =>
    {
        foreach (int item in new[] { 1, 2, 9, 15, 17, 18, 20, 23, 24, 26, 32, 35, 36, 37, 38, 39, 41, 45, 46, 47, 48, 50, 61, 64 })
        {
            var art = new Bottle { Level = 1 };
            var before = RenderAccessory(art, item);
            art.Phase = 1.1;
            Assert.False(before.SequenceEqual(RenderAccessory(art, item)), "No independent animation for item " + item);
            art.MotionEnabled = false;
            var still = RenderAccessory(art, item);
            art.Phase = 7;
            Assert.Equal(still, RenderAccessory(art, item));
        }
    });

    [Fact]
    public void Animated_outfits_keep_a_stable_frame_and_stay_inside_the_viewport_in_all_models() => Sta(() =>
    {
        int[][] outfits = { new[] { 17, 35, 29 }, new[] { 18, 50, 47 }, new[] { 44, 32, 48 },
            new[] { 51, 41, 26 }, new[] { 52, 15, 20 }, new[] { 54, 39, 38 }, new[] { 45, 46, 9 }, new[] { 27, 1, 48 }, new[] { 55, 57, 21 },
            new[] { 56, 41, 20 }, new[] { 53, 35, 26 }, new[] { 58, 40, 66 } };
        for (int model = 0; model < PetCatalog.Models.Count; model++)
            foreach (var outfit in outfits)
                foreach (double level in new[] { 1.0, 0.0 })
                {
                    var art = new Bottle
                    {
                        Vessel = model,
                        Level = level,
                        EquippedAccessories = outfit,
                        Celebrate = level > 0,
                    };
                    Rect? reference = null;
                    foreach (double seconds in new[] { 0, .6, 1.1, 2.8, 4.6 })
                    {
                        art.Phase = seconds;
                        art.CelebrationSeconds = seconds;
                        var pixels = Render(art);
                        reference ??= art.ViewFrame;
                        Assert.Equal(reference.Value, art.ViewFrame);
                        for (int x = 0; x < 120; x++)
                        {
                            Assert.Equal(0, pixels[x * 4 + 3]);
                            Assert.Equal(0, pixels[(139 * 120 + x) * 4 + 3]);
                        }
                        for (int y = 0; y < 140; y++)
                        {
                            Assert.Equal(0, pixels[y * 120 * 4 + 3]);
                            Assert.Equal(0, pixels[(y * 120 + 119) * 4 + 3]);
                        }
                    }
                }
    });

    [Fact]
    public void Poses_are_bounded_deterministic_and_celebrations_return_to_idle()
    {
        foreach (double level in new[] { 1, .7, .5, .3, .1, 0 })
            for (int i = 0; i < 500; i++)
            {
                double seconds = i / 30.0;
                var pose = CharacterMotion.Sample(seconds, level, true, false, true, seconds);
                Assert.Equal(pose, CharacterMotion.Sample(seconds, level, true, false, true, seconds));
                Assert.InRange(pose.ScaleX, .95, 1.05);
                Assert.InRange(pose.ScaleY, .95, 1.05);
                Assert.InRange(pose.Lift, -15, 0);
                Assert.InRange(pose.Lean, -9, 5);
                if (seconds >= CharacterMotion.CelebrationDuration)
                    Assert.Equal(CharacterMotion.Sample(seconds, level, true, false, false, 0), pose);
            }
        Assert.True(CharacterMotion.Sample(4.6, .7, true, false, false, 0).Blink);
        Assert.False(CharacterMotion.Sample(4.8, .7, true, false, false, 0).Blink);
        Assert.Equal(CharacterMotion.Sample(0, 1, true, false, false, 0), CharacterMotion.Sample(double.NaN, 1, true, false, false, 0));
    }

    static byte[] Render(Bottle art)
    {
        art.Width = 120;
        art.Height = 140;
        art.InvalidateVisual();
        art.Measure(new Size(120, 140));
        art.Arrange(new Rect(0, 0, 120, 140));
        art.UpdateLayout();
        return Pixels(art, 120, 140);
    }

    static byte[] RenderAccessory(Bottle art, int item)
    {
        var visual = new DrawingVisual();
        using (var d = visual.RenderOpen())
        {
            d.PushTransform(new TranslateTransform(40, 40));
            art.DrawFittedAccessory(d, 0, item);
            d.Pop();
        }
        return Pixels(visual, 300, 300);
    }

    static byte[] Pixels(Visual visual, int width, int height)
    {
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        var pixels = new byte[width * height * 4];
        bitmap.CopyPixels(pixels, width * 4, 0);
        return pixels;
    }

    static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { error = ex; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(120)), "Animation rendering exceeded its deadline.");
        if (error != null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }
}
