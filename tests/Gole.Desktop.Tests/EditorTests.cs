using System.Runtime.ExceptionServices;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gole.Core;
using Gole.Desktop;
using Gole.Infrastructure;
using Gole.Desktop.Audio;

namespace Gole.Desktop.Tests;

public sealed class EditorTests
{
    private sealed class SilentSound : IWaterSound
    {
        public int Plays;
        public int LastId;
        public bool Preview;
        public int Volume;
        public void Play(int soundId = SoundCatalog.DefaultId, int volume = SoundCatalog.DefaultVolume, bool preview = false)
        {
            Plays++;
            Volume = volume;
            LastId = soundId;
            Preview = preview;
        }
        public void Dispose()
        {
        }
    }

    [Fact]
    public void Editor_preview_discard_save_and_registration_work_without_showing_windows()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            string folder = Path.Combine(Path.GetTempPath(), "Gole.Desktop.Tests", Guid.NewGuid().ToString("N"));
            CompanionWindow? pet = null;
            PanelWindow? panel = null;
            try
            {
                var state = HydrationState.Fresh(DateTime.UtcNow);
                var store = new JsonStateStore(folder);
                var sound = new SilentSound();
                pet = new CompanionWindow(new HydrationSession(state), store, null, sound) { Left = 100, Top = 100 };
                pet.Speak("Som ativado", 1);
                Assert.Equal(1, sound.Plays);
                state.SpeechSound = false;
                pet.Speak("Som desativado", 1);
                Assert.Equal(1, sound.Plays);
                state.SpeechSound = true;
                panel = new PanelWindow(pet) { Resources = Theme.CreateResources() };
                panel.SelectPage(1);
                Assert.False(panel.IsVisible);
                Assert.False(pet.IsVisible);
                AssertAccessoryBorderTracksSelection(panel);

                Click(panel, "Caneca");
                Assert.Equal(0, state.Vessel);
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Vessel == 5 && b.Height > 100);
                Click(panel, "Morto");
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Level == 0 && b.Height > 100);

                Click(panel, "Descartar alterações");
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Vessel == 0 && b.Height > 100);
                Click(panel, "Caneca");
                Click(panel, "Cor Rosa");
                Click(panel, "Salvar visual");
                Assert.Equal(5, state.Vessel);
                Assert.Equal("#D875B3", state.Color);
                Assert.Equal(5, store.Load(DateTime.UtcNow).State.Vessel);

                Assert.DoesNotContain(Descendants(panel).OfType<TextBox>(), t => AutomationProperties.GetName(t) == "Cor hexadecimal");
                Assert.DoesNotContain(Descendants(panel).OfType<Button>(), b => b.Content as string == "Mais cores…");
                Click(panel, "Laço");
                Click(panel, "Brincos");
                Click(panel, "Óculos de grau");
                Assert.False(FindButton(panel, "Auréola").IsEnabled);
                Assert.Empty(state.Accessories);
                Click(panel, "Salvar visual");
                Assert.Equal(new[] { 3, 15, 21 }, state.Accessories);
                Click(panel, "Brincos");
                Assert.True(FindButton(panel, "Auréola").IsEnabled);
                Click(panel, "Auréola");
                Click(panel, "Descartar alterações");
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Height > 100 && b.EquippedAccessories.SequenceEqual(new[] { 3, 15, 21 }));
                Click(panel, "Nenhum");
                Click(panel, "Salvar visual");
                Assert.Empty(state.Accessories);

                Click(panel, "Bolsa");
                Click(panel, "Saia");
                Click(panel, "Salto alto");
                Click(panel, "Salvar visual");
                Assert.Equal(new[] { 26, 27, 28 }, store.Load(DateTime.UtcNow).State.Accessories);
                Click(panel, "Nenhum");
                Click(panel, "Salvar visual");

                foreach (double level in new[] { 1, .7, .5, .3, .1, 0 })
                {
                    Click(panel, HydrationExpression.Label(HydrationExpression.AtLevel(level)));
                    Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Height > 100 && b.Mood == HydrationExpression.AtLevel(level));
                }
                RenderEveryModelAndAccessory();

                panel.SelectPage(2);
                Assert.DoesNotContain(Descendants(panel).OfType<Button>(), b =>
                    (b.Content as string)?.Contains("Pausar") == true || (b.Content as string)?.Contains("Retomar") == true);
                var interval = Descendants(panel).OfType<TextBox>().Single(t => AutomationProperties.GetName(t) == "Intervalo em minutos");
                interval.Text = "45";
                var serving = Descendants(panel).OfType<TextBox>().Single(t => AutomationProperties.GetName(t) == "Quantidade por gole em ml");
                serving.Text = "333";
                var soundChoice = Descendants(panel).OfType<ComboBox>().Single(t => AutomationProperties.GetName(t) == "Som do personagem");
                Assert.Equal(5, soundChoice.Items.Count);
                soundChoice.SelectedIndex = 2;
                var volume = Descendants(panel).OfType<Slider>().Single(t => AutomationProperties.GetName(t) == "Volume do alerta");
                volume.Value = 35;
                var layout = (FrameworkElement)panel.Content;
                layout.Measure(new Size(1090, 800));
                layout.Arrange(new Rect(0, 0, 1090, 800));
                layout.UpdateLayout();
                var listen = FindButton(panel, "Ouvir");
                Assert.Equal(soundChoice.ActualHeight, listen.ActualHeight, 1);
                var soundPosition = soundChoice.TransformToAncestor(layout).Transform(new Point());
                var buttonPosition = listen.TransformToAncestor(layout).Transform(new Point());
                Assert.Equal(soundPosition.Y, buttonPosition.Y, 1);
                Assert.True(soundPosition.X + soundChoice.ActualWidth < buttonPosition.X);
                foreach (var input in new[] { interval, serving })
                {
                    var unit = ((Grid)input.Parent).Children.OfType<TextBlock>().Single();
                    var inputPosition = input.TransformToAncestor(layout).Transform(new Point());
                    var unitPosition = unit.TransformToAncestor(layout).Transform(new Point());
                    Assert.Equal(inputPosition.Y + input.ActualHeight / 2, unitPosition.Y + unit.ActualHeight / 2, 1);
                }
                Click(panel, "Ouvir");
                Assert.Equal(2, sound.LastId);
                Assert.True(sound.Preview);
                Assert.Equal(35, sound.Volume);
                Assert.Equal(3, state.AlertSoundId);
                Click(panel, "Salvar lembretes");
                Assert.Equal(45, state.Interval);
                Assert.Equal(333, state.ServingMl);
                Assert.Equal(2, state.AlertSoundId);
                Assert.Equal(35, state.AlertVolume);
                Assert.Equal(2, store.Load(DateTime.UtcNow).State.AlertSoundId);
                Assert.Equal(35, store.Load(DateTime.UtcNow).State.AlertVolume);
                Assert.Equal(333, store.Load(DateTime.UtcNow).State.ServingMl);
                serving.Text = "0";
                interval.Text = "60";
                Click(panel, "Salvar lembretes");
                Assert.Equal(45, state.Interval);
                Assert.Equal(333, state.ServingMl);

                panel.SelectPage(0);
                state.VisualAnchor = DateTime.UtcNow.AddHours(-1).Ticks;
                Click(panel, "✓  Já bebi um gole");
                Assert.Equal(333, state.Today(DateTime.UtcNow));
                Assert.Equal(1, state.Level(new DateTime(state.LastDrink, DateTimeKind.Utc)), 8);
                panel.RefreshStats();
                Click(panel, "Desfazer registro");
                Assert.Empty(state.Sips);
                Assert.False(pet.IsVisible);
                Assert.False(panel.IsVisible);
            }
            catch (Exception ex) { error = ex; }
            finally
            {
                panel?.Close();
                pet?.Close();
                var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "Gole.Desktop.Tests")) + Path.DirectorySeparatorChar;
                if (Path.GetFullPath(folder).StartsWith(root, StringComparison.OrdinalIgnoreCase) && Directory.Exists(folder))
                    Directory.Delete(folder, recursive: true);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)), "The STA test exceeded its deadline.");
        if (error is not null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }

    private static void Click(DependencyObject root, string label)
    {
        var button = FindButton(root, label);
        Assert.True(button.IsEnabled, label);
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static Button FindButton(DependencyObject root, string label) => Descendants(root).OfType<Button>()
        .Single(b => AutomationProperties.GetName(b) == label || b.Content as string == label);

    private static void AssertAccessoryBorderTracksSelection(PanelWindow panel)
    {
        var button = FindButton(panel, "Headphone");
        button.ApplyTemplate();
        var border = Assert.IsType<Border>(button.Template.FindName("b", button));
        Assert.DoesNotContain(button.Template.Triggers.OfType<Trigger>(), trigger =>
            trigger.Property == UIElement.IsKeyboardFocusedProperty && trigger.Setters.OfType<Setter>().Any(setter =>
                setter.Property == Border.BorderBrushProperty || setter.Property == Border.BorderThicknessProperty));
        Assert.True(button.Focusable);
        Assert.NotNull(button.FocusVisualStyle);

        var initialColor = Assert.IsType<SolidColorBrush>(border.BorderBrush).Color;
        for (int i = 0; i < 3; i++)
        {
            Click(panel, "Headphone");
            Assert.Equal(new Thickness(2), border.BorderThickness);
            Assert.NotEqual(initialColor, Assert.IsType<SolidColorBrush>(border.BorderBrush).Color);
            Click(panel, "Headphone");
            Assert.Equal(new Thickness(1), border.BorderThickness);
            Assert.Equal(initialColor, Assert.IsType<SolidColorBrush>(border.BorderBrush).Color);
            Assert.Contains(Descendants(panel).OfType<Bottle>(), art => art.Height > 100 && art.EquippedAccessories.Count == 0);
        }
    }

    private static void RenderEveryModelAndAccessory()
    {
        for (int model = 0; model < PetCatalog.Models.Count; model++)
            foreach (double level in new[] { 1, .7, .5, .3, .1, 0 })
                Render(new Bottle { Vessel = model, Level = level, EquippedAccessories = new[] { 17, 15, 20 } });
        for (int item = 1; item < PetCatalog.Accessories.Count; item++)
            Render(new Bottle { Vessel = 0, Level = .1, EquippedAccessories = new[] { item } });
    }

    private static void Render(Bottle art)
    {
        art.Width = 160;
        art.Height = 190;
        art.Measure(new Size(160, 190));
        art.Arrange(new Rect(0, 0, 160, 190));
        art.UpdateLayout();
        var bitmap = new RenderTargetBitmap(160, 190, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(art);
        var pixels = new byte[160 * 190 * 4];
        bitmap.CopyPixels(pixels, 160 * 4, 0);
        Assert.Contains(pixels, p => p != 0);
        Assert.False(art.IsVisible);
    }

    private static IEnumerable<DependencyObject> Descendants(DependencyObject root)
    {
        yield return root;
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            foreach (var descendant in Descendants(child))
                yield return descendant;
    }
}
