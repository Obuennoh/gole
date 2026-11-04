using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gole.Core;
using Gole.Desktop;

namespace Gole.Desktop.Tests;

[Collection("Rendering")]
public sealed class AccessoryRenderingTests
{
    [Fact]
    public void Every_accessory_fits_the_viewport_on_every_model_in_both_poses() => Sta(() =>
    {
        for (int model = 0; model < PetCatalog.Models.Count; model++)
            for (int item = 1; item < PetCatalog.Accessories.Count; item++)
                foreach (double level in new[] { 1.0, 0.0 })
                {
                    var pixels = Render(new Bottle { Vessel = model, Accessory = item, Level = level, Phase = 1.5 });
                    Assert.Contains(pixels, p => p != 0);
                    for (int i = 0; i < 120; i++)
                    {
                        Assert.Equal(0, pixels[i * 4 + 3]);
                        Assert.Equal(0, pixels[((139 * 120) + i) * 4 + 3]);
                    }
                    for (int i = 0; i < 140; i++)
                    {
                        Assert.Equal(0, pixels[(i * 120) * 4 + 3]);
                        Assert.Equal(0, pixels[(i * 120 + 119) * 4 + 3]);
                    }
                }
    });

    [Fact]
    public void Clothing_surfaces_follow_the_body_and_never_cover_the_eyes() => Sta(() =>
    {
        for (int model = 0; model < PetCatalog.Models.Count; model++)
        {
            var fit = Bottle.Fit(model);
            var bottle = new Bottle();
            var garment = bottle.Garment(fit, "M 10,160 L 210,160 L 210,232 L 10,232 Z");
            Assert.True(garment.GetArea() > 1500);
            // Boolean geometry flattens curves; allow a quarter of a logical pixel at their edge.
            var contour = fit.Body.GetWidenedPathGeometry(new Pen(Brushes.Black, .5));
            var allowed = Geometry.Combine(fit.Body, contour, GeometryCombineMode.Union, null);
            var outside = Geometry.Combine(garment, allowed, GeometryCombineMode.Exclude, null);
            Assert.True(outside.GetArea() < .1, $"Model {model}: outside area {outside.GetArea()} of {garment.GetArea()}");
            Assert.False(garment.FillContains(new Point(87, 126)));
            Assert.False(garment.FillContains(new Point(133, 126)));
        }
    });

    [Fact]
    public void Layering_does_not_depend_on_selection_order_and_each_piece_keeps_its_style() => Sta(() =>
    {
        var styles = new[] { new AccessoryStyle { AccessoryId = 27, Color = "#D875B3", Pattern = 1 }, new AccessoryStyle { AccessoryId = 44, Color = "#3C82D5", Pattern = 2 } };
        var a = Render(new Bottle { Vessel = 19, EquippedAccessories = new[] { 27, 44, 11 }, AccessoryStyles = styles });
        var b = Render(new Bottle { Vessel = 19, EquippedAccessories = new[] { 11, 44, 27 }, AccessoryStyles = styles });
        Assert.Equal(a, b);
        Assert.NotEqual(a, Render(new Bottle { Vessel = 19, EquippedAccessories = new[] { 27, 44, 11 } }));
    });

    [Fact]
    public void All_patterns_are_visible_and_neutral_and_dark_accessories_can_be_recolored() => Sta(() =>
    {
        for (int model = 0; model < PetCatalog.Models.Count; model++)
        {
            var previous = new List<byte[]>();
            for (int pattern = 0; pattern < AccessoryStyle.Patterns.Count; pattern++)
            {
                var pixels = Render(new Bottle
                {
                    Vessel = model,
                    Accessory = 29,
                    AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = 29, Color = "#E44958", Pattern = pattern } }
                });
                Assert.All(previous, earlier => Assert.False(earlier.SequenceEqual(pixels)));
                previous.Add(pixels);
            }
        }
        foreach (int item in new[] { 2, 17, 21, 24, 35, 44, 51, 52, 66 })
        {
            var original = Render(new Bottle { Accessory = item });
            var colored = Render(new Bottle { Accessory = item, AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = item, Color = "#E44958" } } });
            Assert.False(original.SequenceEqual(colored), "Accessory " + item + " did not change color.");
        }
    });

    static byte[] Render(Bottle art)
    {
        art.Width = 120;
        art.Height = 140;
        art.Measure(new Size(120, 140));
        art.Arrange(new Rect(0, 0, 120, 140));
        var target = new RenderTargetBitmap(120, 140, 96, 96, PixelFormats.Pbgra32);
        target.Render(art);
        var pixels = new byte[120 * 140 * 4];
        target.CopyPixels(pixels, 120 * 4, 0);
        return pixels;
    }

    static void Sta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() => { try { action(); } catch (Exception ex) { error = ex; } });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(120)), "Rendering exceeded its deadline.");
        if (error != null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }
}

[CollectionDefinition("Rendering", DisableParallelization = true)]
public sealed class RenderingCollection
{
}

