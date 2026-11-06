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
                AssertOriginalAccessoriesAndReplacement(panel, state, store);
                AssertAnimationPreviewAndSaving(panel, pet, state, store);
                AssertWardrobe(panel, state, store);
                var personality = Descendants(panel).OfType<ComboBox>().Single(c => AutomationProperties.GetName(c) == "Personalidade do pet");
                Assert.Equal(12, personality.Items.Count);
                personality.SelectedValue = 11;
                Assert.Equal(0, state.PersonalityId);
                var phrase = Descendants(panel).OfType<TextBlock>().Single(t => AutomationProperties.GetName(t) == "Prévia da fala");
                Assert.Contains(phrase.Text, PersonalityCatalog.Reminders(11, HydrationMood.Happy));
                string beforePhrase = phrase.Text;
                Click(panel, "Outra frase");
                Assert.NotEqual(beforePhrase, phrase.Text);
                Assert.Equal(1, sound.Plays);
                Click(panel, "Morto");
                Assert.Contains(phrase.Text, PersonalityCatalog.Reminders(11, HydrationMood.Dead));
                Click(panel, "Descartar alterações");
                Assert.Equal(0, (int)personality.SelectedValue);
                personality.SelectedValue = 11;
                Click(panel, "Salvar personalização");
                Assert.Equal(11, state.PersonalityId);
                Assert.Equal(11, store.Load(DateTime.UtcNow).State.PersonalityId);
                personality.SelectedValue = 6;
                Click(panel, "Salvar personalização");
                Assert.Equal(6, state.PersonalityId);
                Assert.Equal(6, store.Load(DateTime.UtcNow).State.PersonalityId);


                Click(panel, "Caneca");
                Assert.Equal(0, state.Vessel);
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Vessel == 5 && b.Height > 100);
                Click(panel, "Morto");
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Level == 0 && b.Height > 100);

                Click(panel, "Descartar alterações");
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Vessel == 0 && b.Height > 100);
                Click(panel, "Caneca");
                Click(panel, "Cor Rosa");
                Click(panel, "Salvar personalização");
                Assert.Equal(5, state.Vessel);
                Assert.Equal("#D875B3", state.Color);
                Assert.Equal(5, store.Load(DateTime.UtcNow).State.Vessel);

                Assert.DoesNotContain(Descendants(panel).OfType<TextBox>(), t => AutomationProperties.GetName(t) == "Cor hexadecimal");
                Assert.DoesNotContain(Descendants(panel).OfType<Button>(), b => b.Content as string == "Mais cores…");
                Click(panel, "Laço");
                Click(panel, "Brincos");
                Click(panel, "Óculos de grau");
                Click(panel, "Livrinho");
                Click(panel, "Pantufas de coelho");
                Assert.False(FindButton(panel, "Auréola").IsEnabled);
                Assert.Empty(state.Accessories);
                Click(panel, "Salvar personalização");
                Assert.Equal(new[] { 3, 15, 21, 69, 80 }, state.Accessories);
                Click(panel, "Brincos");
                Assert.True(FindButton(panel, "Auréola").IsEnabled);
                Click(panel, "Auréola");
                Click(panel, "Descartar alterações");
                Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Height > 100 && b.EquippedAccessories.SequenceEqual(new[] { 3, 15, 21, 69, 80 }));
                Click(panel, "Nenhum");
                Click(panel, "Salvar personalização");
                Assert.Empty(state.Accessories);

                Click(panel, "Bolsa");
                Click(panel, "Saia");
                Click(panel, "Salto alto");
                Click(panel, "Salvar personalização");
                Assert.Equal(new[] { 26, 27, 28 }, store.Load(DateTime.UtcNow).State.Accessories);
                Click(panel, "Nenhum");
                Click(panel, "Salvar personalização");

                foreach (double level in new[] { 1, .7, .5, .3, .1, 0 })
                {
                    Click(panel, HydrationExpression.Label(HydrationExpression.AtLevel(level)));
                    Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Height > 100 && b.Mood == HydrationExpression.AtLevel(level));
                }
                Click(panel, "Frasco de coração");
                Click(panel, "Nenhum");
                Click(panel, "Capa de herói");
                Click(panel, "Orelhas de coelho");
                Click(panel, "Varinha");
                Click(panel, "Óculos com fita");
                Click(panel, "Patins");
                Assert.False(FindButton(panel, "Boia").IsEnabled);
                Click(panel, "Salvar personalização");
                var expandedStyle = store.Load(DateTime.UtcNow).State;
                Assert.Equal(25, expandedStyle.Vessel);
                Assert.Equal(new[] { 45, 35, 47, 68, 79 }, expandedStyle.Accessories);
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
                Assert.Contains(pet.CurrentSpeech, PersonalityCatalog.Reactions(6));
                Assert.Equal(1, state.Level(new DateTime(state.LastDrink, DateTimeKind.Utc)), 8);
                panel.RefreshStats();
                Click(panel, "Desfazer último gole");
                Assert.Empty(state.Sips);
                var old = Enumerable.Range(0, 10).Select(i => new Sip { Utc = DateTime.UtcNow.AddDays(-2).AddMinutes(i).Ticks, Ml = 200 }).ToList();
                state.Sips.AddRange(old);
                panel.RefreshStats();
                Assert.True(FindButton(panel, "Desfazer último gole").IsEnabled);
                Assert.Equal(8, Descendants(panel).OfType<Button>().Count(b => b.Tag is Sip));
                var delete = Descendants(panel).OfType<Button>().Single(b => ReferenceEquals(b.Tag, old[7]));
                delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.DoesNotContain(old[7], state.Sips);
                Assert.Equal(9, store.Load(DateTime.UtcNow).State.Sips.Count);
                Click(panel, "Próxima");
                Assert.Single(Descendants(panel).OfType<Button>(), b => b.Tag is Sip);
                delete = Descendants(panel).OfType<Button>().Single(b => ReferenceEquals(b.Tag, old[0]));
                delete.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                Assert.Equal(8, Descendants(panel).OfType<Button>().Count(b => b.Tag is Sip));
                Assert.False(FindButton(panel, "Próxima").IsEnabled);
                Assert.False(FindButton(panel, "Anterior").IsEnabled);
                panel.ConfirmClearHistory = () => false;
                Click(panel, "Limpar todo o histórico");
                Assert.Equal(8, state.Sips.Count);
                panel.ConfirmClearHistory = () => true;
                Click(panel, "Limpar todo o histórico");
                Assert.Empty(state.Sips);
                Assert.Empty(store.Load(DateTime.UtcNow).State.Sips);
                Assert.False(FindButton(panel, "Limpar todo o histórico").IsEnabled);
                Assert.False(FindButton(panel, "Desfazer último gole").IsEnabled);
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

    private static void AssertWardrobe(PanelWindow panel, HydrationState state, JsonStateStore store)
    {
        var search = Descendants(panel).OfType<TextBox>().Single(c => AutomationProperties.GetName(c) == "Buscar acessórios");
        var category = Descendants(panel).OfType<ComboBox>().Single(c => AutomationProperties.GetName(c) == "Categoria de acessórios");
        search.Text = "oculos";
        Assert.Equal(Visibility.Visible, FindButton(panel, "Óculos com fita").Visibility);
        Assert.Equal(Visibility.Collapsed, FindButton(panel, "Livrinho").Visibility);
        category.SelectedItem = "Roupas";
        Assert.Equal(Visibility.Collapsed, FindButton(panel, "Óculos com fita").Visibility);
        search.Text = "";
        category.SelectedItem = "Todos";
        var originalColor = state.Color;
        Click(panel, "Monte um look");
        Assert.Empty(state.Accessories);
        var art = Descendants(panel).OfType<Bottle>().Single(b => b.Height > 100);
        Assert.Equal(5, art.EquippedAccessories.Count);
        var items = art.EquippedAccessories.ToArray();
        var name = Descendants(panel).OfType<TextBox>().Single(c => AutomationProperties.GetName(c) == "Nome do look");
        name.Text = "Favorito";
        Click(panel, "Guardar novo look");
        Assert.Equal(items, Assert.Single(store.Load(DateTime.UtcNow).State.Looks).Accessories);
        Assert.Empty(state.Accessories);
        Click(panel, "Descartar alterações");
        Assert.Empty(art.EquippedAccessories);
        Click(panel, "Usar look");
        Assert.Equal(items, state.Accessories);
        Assert.Empty(state.Sips);
        name.Text = "Meu favorito";
        Click(panel, "Renomear look");
        Assert.Equal("Meu favorito", Assert.Single(store.Load(DateTime.UtcNow).State.Looks).Name);
        Click(panel, "Excluir look");
        Assert.Empty(store.Load(DateTime.UtcNow).State.Looks);
        Assert.Equal(items, state.Accessories);
        Click(panel, "Nenhum");
        Click(panel, "Cor Turquesa");
        Click(panel, "Salvar personalização");
        Assert.Equal(originalColor, state.Color);
    }

    private static void AssertAnimationPreviewAndSaving(PanelWindow panel, CompanionWindow pet, HydrationState state, JsonStateStore store)
    {
        var motion = Descendants(panel).OfType<CheckBox>().Single(c => AutomationProperties.GetName(c) == "Animar personagem e acessórios");
        var preview = Descendants(panel).OfType<Bottle>().Single(b => b.Height > 100);
        motion.IsChecked = false;
        motion.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
        Assert.False(preview.MotionEnabled);
        Assert.True(state.Motion);
        Assert.False(FindButton(panel, "Ver reação ao beber").IsEnabled);
        Click(panel, "Salvar personalização");
        Assert.False(state.Motion);
        Assert.False(pet.Art.MotionEnabled);
        Assert.False(store.Load(DateTime.UtcNow).State.Motion);
        motion.IsChecked = true;
        motion.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
        Click(panel, "Ver reação ao beber");
        Assert.True(preview.Celebrate);
        Assert.InRange(preview.CelebrationSeconds, 0, 1);
        Assert.Empty(state.Sips);
        Assert.False(pet.Art.Celebrate);
        Assert.False(state.Motion);
        Click(panel, "Descartar alterações");
        Assert.False(preview.Celebrate);
        Assert.False(preview.MotionEnabled);
        motion.IsChecked = true;
        motion.RaiseEvent(new RoutedEventArgs(CheckBox.ClickEvent));
        Click(panel, "Salvar personalização");
        Assert.True(state.Motion);
        Assert.True(pet.Art.MotionEnabled);
        Assert.True(store.Load(DateTime.UtcNow).State.Motion);
    }

    private static void AssertOriginalAccessoriesAndReplacement(PanelWindow panel, HydrationState state, JsonStateStore store)
    {
        Assert.DoesNotContain(Descendants(panel).OfType<ComboBox>(), c =>
            AutomationProperties.GetName(c) is "Acessório para personalizar" or "Estampa do acessório");
        Assert.DoesNotContain(Descendants(panel).OfType<Button>(), b => AutomationProperties.GetName(b).StartsWith("Cor do acessório:"));
        Click(panel, "Saia");
        Click(panel, "Jaqueta");
        Click(panel, "Óculos de grau");
        Click(panel, "Livrinho");
        Click(panel, "Pantufas de coelho");
        Assert.True(FindButton(panel, "Saia plissada").IsEnabled);
        Click(panel, "Saia plissada");
        Assert.Contains(Descendants(panel).OfType<Bottle>(), b => b.Height > 100 && b.EquippedAccessories.Contains(55) && !b.EquippedAccessories.Contains(27));
        Assert.Empty(state.Accessories);
        Click(panel, "Salvar personalização");
        Assert.Equal(new[] { 44, 21, 69, 80, 55 }, store.Load(DateTime.UtcNow).State.Accessories);
        Click(panel, "Nenhum");
        Click(panel, "Salvar personalização");
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
                Render(new Bottle { Vessel = model, Level = level, EquippedAccessories = model >= 13 ? new[] { 45, 35, 47 } : new[] { 17, 15, 20 } });
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
