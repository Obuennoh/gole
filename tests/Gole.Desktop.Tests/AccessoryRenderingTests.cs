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
    public void Skirts_have_a_fitted_waist_and_a_flared_hem_in_every_model() => Sta(() =>
    {
        for (int model = 0; model < PetCatalog.Models.Count; model++)
            foreach (int item in new[] { 27, 29, 55, 56 })
            {
                var fit = Bottle.Fit(model);
                var cut = Bottle.CutSkirt(fit, item);
                Assert.True(cut.HemRight - cut.HemLeft >= cut.Right - cut.Left + 30);
                Assert.InRange(Math.Abs(cut.Left - fit.Left(cut.WaistY)), 0, 2);
                Assert.InRange(Math.Abs(cut.Right - fit.Right(cut.WaistY)), 0, 2);
                var outline = cut.Outline();
                var outside = Geometry.Combine(outline, fit.Body, GeometryCombineMode.Exclude, null);
                Assert.True(outside.GetArea() > 400, "Skirt silhouette was reduced to a body fill in model " + model);
                Assert.False(outline.FillContains(new Point(87, 126)));
                Assert.False(outline.FillContains(new Point(133, 126)));
            }
    });

    [Fact]
    public void Every_clothing_option_has_a_distinct_drawing_in_every_model() => Sta(() =>
    {
        int[] clothes = { 27, 29, 44, 51, 52, 53, 54, 55, 56, 57, 58 };
        for (int model = 0; model < PetCatalog.Models.Count; model++)
        {
            var drawings = clothes.Select(item => Render(new Bottle { Vessel = model, Accessory = item, MotionEnabled = false })).ToList();
            for (int i = 0; i < drawings.Count; i++)
                for (int j = i + 1; j < drawings.Count; j++)
                    Assert.False(drawings[i].SequenceEqual(drawings[j]), $"Clothes {clothes[i]} and {clothes[j]} look identical on model {model}");
        }
    });

    [Fact]
    public void New_models_are_distinct_and_new_accessories_are_visible_on_every_body() => Sta(() =>
    {
        var models = Enumerable.Range(0, PetCatalog.Models.Count)
            .Select(model => Render(new Bottle { Vessel = model, MotionEnabled = false })).ToArray();
        for (int model = 21; model < PetCatalog.Models.Count; model++)
            for (int other = 0; other < model; other++)
                Assert.False(models[model].SequenceEqual(models[other]), $"Models {model} and {other} look identical");
        for (int model = 0; model < PetCatalog.Models.Count; model++)
            for (int item = 67; item < PetCatalog.Accessories.Count; item++)
                Assert.False(models[model].SequenceEqual(Render(new Bottle { Vessel = model, Accessory = item, MotionEnabled = false })),
                    $"Accessory {item} is invisible on model {model}");
    });

    [Fact]
    public void Compatible_outfit_layers_do_not_depend_on_selection_order() => Sta(() =>
    {
        var a = Render(new Bottle { Vessel = 25, EquippedAccessories = new[] { 27, 44, 71, 67, 79 } });
        var b = Render(new Bottle { Vessel = 25, EquippedAccessories = new[] { 79, 67, 71, 44, 27 } });
        Assert.Equal(a, b);
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

