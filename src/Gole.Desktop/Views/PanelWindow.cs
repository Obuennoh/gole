using Gole.Core;
using Gole.Infrastructure;
using Gole.Desktop.Updates;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using Forms = System.Windows.Forms;

namespace Gole.Desktop
{
    public class PanelWindow : Window
    {
        CompanionWindow pet;
        HydrationState data;
        ContentControl pageHost;
        UIElement[] pages;
        int selectedPage;
        TextBlock title, subtitle, total, progressLabel, countdown, last, footer, petStatus;
        ProgressBar progress;
        Bottle live, preview;
        Button undo, saveVisual, previousHistory, nextHistory, clearHistory;
        StackPanel recordRows;
        TextBlock historyPageLabel;
        List<Sip> historySnapshot;
        int historyPage;
        const int RecordsPerPage = 8;
        internal Func<bool> ConfirmClearHistory
        {
            get; set;
        }
        TextBox nameBox, goalBox, intervalBox, servingBox;
        Slider sizeSlider;
        CheckBox motionBox;
        string draftName, draftColor;
        int draftModel, draftPersonality;
        ComboBox personalityChoice;
        TextBlock personalityDescription, personalityMessage;
        readonly ReminderPhrases previewSpeech = new ReminderPhrases(new Random());
        int lastPreviewPersonality = -1;
        HydrationMood? lastPreviewMood;
        List<int> draftAccessories = new List<int>();
        double draftSize, previewLevel = 1;
        bool draftMotion, syncing;
        double phase;
        DateTime? simulation;
        TextBlock previewName, previewState, accessoryCount, settingsMessage;
        List<Button> nav = new List<Button>();
        List<Border> colorBorders = new List<Border>();
        List<Button> modelButtons = new List<Button>(), accessoryButtons = new List<Button>(), stateButtons = new List<Button>();
        List<Bottle> thumbnails = new List<Bottle>();
        static readonly double[] PreviewLevels = { 1, .7, .5, .3, .1, 0 };
        DispatcherTimer animation;
        static readonly string[] Palette = { "#26B9C4", "#3C82D5", "#6D69C9", "#9B73CB", "#D875B3", "#E694A7", "#DE7867", "#EDAA61", "#D9BE61", "#94BD75", "#45A58C", "#7EA8B0", "#456477", "#788494", "#A58C7C", "#CFB8A3" };
        static readonly string[] ColorNames = { "Turquesa", "Azul", "Índigo", "Lavanda", "Rosa", "Rosé", "Coral", "Pêssego", "Dourado", "Pistache", "Menta", "Petróleo claro", "Azul escuro", "Cinza", "Café", "Areia" };
        public PanelWindow(CompanionWindow companion)
        {
            pet = companion;
            data = pet.Data;
            ConfirmClearHistory = () => MessageBox.Show(this,
                "Apagar todos os " + data.Sips.Count + " registros de água de todos os dias? Suas personalizações e lembretes serão mantidos.",
                "Limpar histórico", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes;
            ReadDraft();
            Title = App.DevelopmentMode ? "Gole · desenvolvimento" : "Gole · painel";
            Width = 1090;
            Height = 800;
            MinWidth = 1000;
            MinHeight = 710;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = Theme.B("#F5F7F9");
            FontFamily = new FontFamily("Segoe UI");
            Foreground = Theme.Ink;
            var shell = new Grid();
            shell.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(176) });
            shell.ColumnDefinitions.Add(new ColumnDefinition());
            var side = new Grid { Background = Theme.B("#162F3A") };
            shell.Children.Add(side);
            var navigation = new StackPanel { Margin = new Thickness(16, 26, 16, 0), VerticalAlignment = VerticalAlignment.Top };
            var logo = Theme.Text("gole.", 36, Brushes.White);
            logo.FontWeight = FontWeights.Bold;
            logo.Margin = new Thickness(12, 0, 0, 34);
            navigation.Children.Add(logo);
            string[] names = { "Visão geral", "Personalizar", "Lembretes" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                var b = Theme.Button(names[i], delegate
                {
                    SelectPage(index);
                }, false);
                b.HorizontalContentAlignment = HorizontalAlignment.Left;
                b.Margin = new Thickness(0, 0, 0, 7);
                b.Padding = new Thickness(15, 13, 15, 13);
                nav.Add(b);
                navigation.Children.Add(b);
            }
            side.Children.Add(navigation);
            var show = Theme.Button("Mostrar pet ↗", pet.ShowPet, false);
            show.Margin = new Thickness(16, 0, 16, 24);
            show.VerticalAlignment = VerticalAlignment.Bottom;
            show.Background = Theme.B("#284650");
            show.Foreground = Theme.B("#D4E5E9");
            side.Children.Add(show);
            var main = new Grid { Margin = new Thickness(30, 25, 30, 17) };
            Grid.SetColumn(main, 1);
            shell.Children.Add(main);
            main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            main.RowDefinitions.Add(new RowDefinition());
            main.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var heading = new StackPanel();
            title = Theme.Text("", 28, Theme.Ink);
            title.FontWeight = FontWeights.SemiBold;
            subtitle = Theme.Text("", 13, Theme.Muted);
            heading.Children.Add(title);
            heading.Children.Add(subtitle);
            main.Children.Add(heading);
            pageHost = new ContentControl { HorizontalContentAlignment = HorizontalAlignment.Stretch, VerticalContentAlignment = VerticalAlignment.Stretch, Margin = new Thickness(0, 15, 0, 0) };
            pages = new[] { Overview(), Editor(), Settings() };
            pageHost.Content = pages[0];
            Grid.SetRow(pageHost, 1);
            main.Children.Add(pageHost);
            footer = Theme.Text("", 11, Theme.Muted);
            footer.Margin = new Thickness(0, 12, 0, 0);
            Grid.SetRow(footer, 2);
            main.Children.Add(footer);
            Content = shell;
            UpdateNavigation();
            RefreshStats();
            UpdateEditor();
            animation = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            animation.Tick += delegate
            {
                if (!IsVisible || WindowState == WindowState.Minimized)
                    return;
                if (data.Motion || draftMotion)
                    phase += .15;
                live.Level = data.Level(DateTime.UtcNow);
                live.Phase = data.Motion ? phase : 0;
                live.Tint = data.Color;
                live.Vessel = data.Vessel;
                live.EquippedAccessories = data.Accessories;
                live.Paused = data.Paused(DateTime.UtcNow);
                live.InvalidateVisual();
                if (simulation.HasValue)
                {
                    previewLevel = Math.Max(0, 1 - (DateTime.UtcNow - simulation.Value).TotalSeconds / 8);
                    if (previewLevel == 0)
                        simulation = null;
                    UpdatePreviewLabel();
                }
                preview.Level = previewLevel;
                preview.Phase = draftMotion ? phase : 0;
                preview.InvalidateVisual();
            };
            animation.Start();
            Closed += delegate
            {
                animation.Stop();
                pet.Save();
            };
        }
        public void SelectPage(int index)
        {
            if (index < 0 || index >= pages.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            selectedPage = index;
            pageHost.Content = pages[index];
            UpdateNavigation();
        }
        void UpdateNavigation()
        {
            int selected = selectedPage;
            string[] titles = { "Visão geral", "Personalizar pet", "Lembretes" };
            string[] subs = { DateTime.Now.ToString("dddd, dd 'de' MMMM", CultureInfo.GetCultureInfo("pt-BR")), "Visual e prévia do personagem", "Intervalo e meta diária" };
            title.Text = titles[selected];
            subtitle.Text = subs[selected];
            for (int i = 0; i < nav.Count; i++)
            {
                nav[i].Background = Theme.B(i == selected ? "#315A63" : "#162F3A");
                nav[i].Foreground = Theme.B(i == selected ? "#FFFFFF" : "#A8BCC5");
            }
        }
        Border Card(UIElement child)
        {
            return new Border { Background = Brushes.White, BorderBrush = Theme.B("#E2E8ED"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(18), Padding = new Thickness(23), Child = child };
        }
        TextBlock Heading(string text)
        {
            var t = Theme.Text(text, 14, Theme.Ink);
            t.FontWeight = FontWeights.SemiBold;
            t.Margin = new Thickness(0, 0, 0, 13);
            return t;
        }
        void Label(StackPanel panel, string text)
        {
            var t = Heading(text);
            t.Margin = new Thickness(0, 20, 0, 10);
            panel.Children.Add(t);
        }
        ScrollViewer Scroll(UIElement child)
        {
            return new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, Content = child };
        }
        UIElement Overview()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.25, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var left = new StackPanel { Margin = new Thickness(0, 0, 18, 0) };
            var consumption = new StackPanel();
            consumption.Children.Add(Heading("Água hoje"));
            total = Theme.Text("", 44, Theme.Ink);
            total.FontWeight = FontWeights.SemiBold;
            consumption.Children.Add(total);
            progressLabel = Theme.Text("", 13, Theme.Muted);
            consumption.Children.Add(progressLabel);
            progress = new ProgressBar { Height = 9, Maximum = 1, Foreground = Theme.Teal, Background = Theme.B("#E7EEF0"), BorderThickness = new Thickness(0), Margin = new Thickness(0, 12, 0, 24) };
            consumption.Children.Add(progress);
            consumption.Children.Add(Theme.Button("✓  Já bebi um gole", delegate
            {
                pet.Drink();
            }, true));
            undo = Theme.Button("Desfazer último gole", () => { pet.Undo(); RefreshStats(); }, false);
            undo.Margin = new Thickness(0, 8, 0, 0);
            consumption.Children.Add(undo);
            left.Children.Add(Card(consumption));
            var history = new StackPanel();
            history.Children.Add(Heading("Histórico de água"));
            recordRows = new StackPanel();
            history.Children.Add(recordRows);
            var pagination = new Grid { Margin = new Thickness(0, 12, 0, 0) };
            pagination.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            pagination.ColumnDefinitions.Add(new ColumnDefinition());
            pagination.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            previousHistory = Theme.Button("Anterior", () => { historyPage--; RefreshHistory(true); }, false);
            nextHistory = Theme.Button("Próxima", () => { historyPage++; RefreshHistory(true); }, false);
            previousHistory.FontSize = nextHistory.FontSize = 12;
            previousHistory.Margin = nextHistory.Margin = new Thickness(0);
            historyPageLabel = Theme.Text("", 12, Theme.Muted);
            historyPageLabel.Margin = new Thickness(4, 0, 4, 0);
            historyPageLabel.VerticalAlignment = VerticalAlignment.Center;
            historyPageLabel.TextAlignment = TextAlignment.Center;
            Grid.SetColumn(historyPageLabel, 1);
            Grid.SetColumn(nextHistory, 2);
            pagination.Children.Add(previousHistory);
            pagination.Children.Add(historyPageLabel);
            pagination.Children.Add(nextHistory);
            history.Children.Add(pagination);
            clearHistory = Theme.Button("Limpar todo o histórico", () =>
            {
                if (data.Sips.Count > 0 && ConfirmClearHistory())
                {
                    pet.ClearDrinks();
                    RefreshStats();
                }
            }, false);
            clearHistory.Foreground = Theme.B("#A63838");
            clearHistory.Background = Theme.B("#FBEFEF");
            clearHistory.Margin = new Thickness(0, 14, 0, 0);
            history.Children.Add(clearHistory);
            var hc = Card(history);
            hc.Margin = new Thickness(0, 18, 0, 0);
            left.Children.Add(hc);
            grid.Children.Add(Scroll(left));
            var right = new StackPanel();
            right.Children.Add(Heading(data.Name));
            live = new Bottle { Height = 260, Width = 225 };
            right.Children.Add(live);
            petStatus = Theme.Text("", 18, Theme.Ink);
            petStatus.FontWeight = FontWeights.SemiBold;
            petStatus.TextAlignment = TextAlignment.Center;
            right.Children.Add(petStatus);
            countdown = Theme.Text("", 15, Theme.Teal);
            countdown.TextAlignment = TextAlignment.Center;
            right.Children.Add(countdown);
            last = Theme.Text("", 11, Theme.Muted);
            last.TextAlignment = TextAlignment.Center;
            right.Children.Add(last);
            right.Children.Add(Theme.Button("Personalizar", delegate
            {
                SelectPage(1);
            }, false));
            var rc = Card(right);
            rc.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumn(rc, 1);
            grid.Children.Add(rc);
            return grid;
        }
        void ReadDraft()
        {
            draftName = data.Name;
            draftColor = data.Color;
            draftModel = data.Vessel;
            draftPersonality = data.PersonalityId;
            draftAccessories = data.Accessories.ToList();
            draftSize = data.Size;
            draftMotion = data.Motion;
        }
        bool Dirty
        {
            get
            {
                return draftName != data.Name || draftColor != data.Color || draftModel != data.Vessel || draftPersonality != data.PersonalityId || !draftAccessories.SequenceEqual(data.Accessories) || Math.Abs(draftSize - data.Size) > .1 || draftMotion != data.Motion;
            }
        }
        UIElement Editor()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(292) });
            var controls = new StackPanel { Margin = new Thickness(0, 0, 19, 0) };
            controls.Children.Add(Heading("Nome"));
            nameBox = new TextBox { Text = draftName, MaxLength = 20 };
            nameBox.TextChanged += delegate
            {
                if (syncing)
                    return;
                draftName = nameBox.Text;
                UpdateEditor();
            };
            controls.Children.Add(nameBox);
            Label(controls, "Personalidade");
            personalityChoice = new ComboBox { ItemsSource = PersonalityCatalog.Options, DisplayMemberPath = "Name", SelectedValuePath = "Id", SelectedValue = draftPersonality, Height = 40, VerticalContentAlignment = VerticalAlignment.Center };
            System.Windows.Automation.AutomationProperties.SetName(personalityChoice, "Personalidade do pet");
            personalityChoice.SelectionChanged += delegate
            {
                if (syncing || personalityChoice.SelectedValue is not int id)
                    return;
                draftPersonality = id;
                UpdateEditor();
            };
            controls.Children.Add(personalityChoice);
            personalityDescription = Theme.Text("", 12, Theme.Muted);
            personalityDescription.Margin = new Thickness(0, 8, 0, 0);
            controls.Children.Add(personalityDescription);
            Label(controls, "Modelo");
            var models = new WrapPanel();
            for (int i = 0; i < Bottle.Models.Length; i++)
            {
                int model = i;
                var b = Tile(Bottle.Models[i], model, 0, 118, 83, delegate
                {
                    draftModel = model;
                    UpdateEditor();
                });
                modelButtons.Add(b);
                models.Children.Add(b);
            }
            controls.Children.Add(models);
            Label(controls, "Cor da água");
            var colors = new WrapPanel();
            for (int i = 0; i < Palette.Length; i++)
            {
                string color = Palette[i];
                var b = Theme.Button("", delegate
                {
                    draftColor = color;
                    UpdateEditor();
                }, false);
                b.Width = 31;
                b.Height = 31;
                b.MinHeight = 31;
                b.Margin = new Thickness(0);
                b.Padding = new Thickness(0);
                b.Background = Theme.B(color);
                b.ToolTip = ColorNames[i];
                System.Windows.Automation.AutomationProperties.SetName(b, "Cor " + ColorNames[i]);
                var border = new Border { Child = b, Padding = new Thickness(3), BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(14), Margin = new Thickness(0, 0, 4, 5), Tag = color };
                colorBorders.Add(border);
                colors.Children.Add(border);
            }
            controls.Children.Add(colors);
            Label(controls, "Acessórios");
            accessoryCount = Theme.Text("", 12, Theme.Muted);
            controls.Children.Add(accessoryCount);
            var accessories = new WrapPanel();
            for (int i = 0; i < Bottle.Accessories.Length; i++)
            {
                int accessory = i;
                var b = Tile(Bottle.Accessories[i], draftModel, accessory, 118, 83, delegate
                {
                    PetCatalog.Toggle(draftAccessories, accessory);
                    UpdateEditor();
                });
                accessoryButtons.Add(b);
                accessories.Children.Add(b);
            }
            controls.Children.Add(accessories);
            Label(controls, "Tamanho na tela");
            sizeSlider = new Slider { Minimum = 100, Maximum = 280, Value = draftSize, TickFrequency = 10, IsSnapToTickEnabled = true };
            sizeSlider.ValueChanged += delegate
            {
                if (syncing)
                    return;
                draftSize = sizeSlider.Value;
                UpdateEditor();
            };
            controls.Children.Add(sizeSlider);
            motionBox = new CheckBox { Content = "Animação suave", IsChecked = draftMotion, Margin = new Thickness(0, 15, 0, 12) };
            motionBox.Click += delegate
            {
                draftMotion = motionBox.IsChecked == true;
                UpdateEditor();
            };
            controls.Children.Add(motionBox);
            grid.Children.Add(Scroll(controls));
            var previewColumn = new Grid();
            previewColumn.RowDefinitions.Add(new RowDefinition());
            previewColumn.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var previewActions = new StackPanel();
            var previewContent = new StackPanel();
            previewContent.Children.Add(Heading("PRÉVIA"));
            previewName = Theme.Text("", 18, Theme.Ink);
            previewName.TextAlignment = TextAlignment.Center;
            previewName.FontWeight = FontWeights.SemiBold;
            previewContent.Children.Add(previewName);
            var stage = new Grid { Height = 238, Background = Theme.B("#F0F6F7") };
            preview = new Bottle { Width = draftSize * 220 / 255, Height = draftSize, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            stage.Children.Add(preview);
            previewContent.Children.Add(new Border { Child = stage, CornerRadius = new CornerRadius(14), ClipToBounds = true, Margin = new Thickness(0, 4, 0, 15) });
            previewState = Theme.Text("", 12, Theme.Muted);
            previewState.TextAlignment = TextAlignment.Center;
            previewContent.Children.Add(previewState);
            var states = new UniformGrid { Columns = 3 };
            string[] stateNames = { "Feliz", "De boa", "Sério", "Triste", "Chorando", "Morto" };
            double[] levels = PreviewLevels;
            for (int i = 0; i < levels.Length; i++)
            {
                double l = levels[i];
                var b = Theme.Button(stateNames[i], delegate
                {
                    simulation = null;
                    previewLevel = l;
                    UpdateEditor();
                }, false);
                b.FontSize = 11;
                b.Padding = new Thickness(7, 9, 7, 9);
                b.Margin = new Thickness(0, 0, 4, 8);
                stateButtons.Add(b);
                states.Children.Add(b);
            }
            previewContent.Children.Add(states);
            personalityMessage = Theme.Text("", 12, Theme.Ink);
            personalityMessage.Margin = new Thickness(0);
            System.Windows.Automation.AutomationProperties.SetName(personalityMessage, "Prévia da fala");
            previewContent.Children.Add(new Border { Child = personalityMessage, Background = Theme.B("#F0F6F7"), CornerRadius = new CornerRadius(10), Padding = new Thickness(12), Margin = new Thickness(0, 4, 0, 8) });
            var anotherPhrase = Theme.Button("Outra frase", () => UpdatePersonalityPreview(true), false);
            anotherPhrase.FontSize = 12;
            previewContent.Children.Add(anotherPhrase);
            var test = Theme.Button("▶  Simular ciclo", delegate
            {
                previewLevel = 1;
                simulation = DateTime.UtcNow;
                UpdateEditor();
            }, false);
            test.FontSize = 12;
            previewContent.Children.Add(test);
            previewColumn.Children.Add(Scroll(Card(previewContent)));
            saveVisual = Theme.Button("Salvar personalização", SaveAppearance, true);
            saveVisual.Margin = new Thickness(0, 16, 0, 6);
            previewActions.Children.Add(saveVisual);
            var discard = Theme.Button("Descartar alterações", delegate
            {
                ReadDraft();
                SyncControls();
                UpdateEditor();
            }, false);
            discard.Margin = new Thickness(0);
            previewActions.Children.Add(discard);
            Grid.SetRow(previewActions, 1);
            previewColumn.Children.Add(previewActions);
            Grid.SetColumn(previewColumn, 1);
            grid.Children.Add(previewColumn);
            return grid;
        }
        Button Tile(string label, int model, int accessory, double width, double height, Action action)
        {
            var content = new StackPanel();
            var art = new Bottle { Vessel = model, Accessory = accessory, Tint = draftColor, Width = 59, Height = 60, Level = 1 };
            thumbnails.Add(art);
            content.Children.Add(art);
            var text = Theme.Text(label, 11, Theme.Ink);
            text.TextAlignment = TextAlignment.Center;
            text.Margin = new Thickness(0);
            content.Children.Add(text);
            var b = Theme.Button(label, action, false);
            b.Content = content;
            b.Width = width;
            b.Height = height;
            b.Margin = new Thickness(0, 0, 6, 6);
            b.Padding = new Thickness(4);
            b.BorderThickness = new Thickness(1);
            b.BorderBrush = Theme.B("#E1E7EC");
            b.Background = Brushes.White;
            System.Windows.Automation.AutomationProperties.SetName(b, label);
            return b;
        }
        void SyncControls()
        {
            syncing = true;
            nameBox.Text = draftName;
            personalityChoice.SelectedValue = draftPersonality;
            sizeSlider.Value = draftSize;
            motionBox.IsChecked = draftMotion;
            syncing = false;
        }
        void UpdateEditor()
        {
            if (preview == null)
                return;
            personalityDescription.Text = PersonalityCatalog.Get(draftPersonality).Description;
            preview.Tint = draftColor;
            preview.Vessel = draftModel;
            preview.EquippedAccessories = draftAccessories.ToArray();
            preview.Level = previewLevel;
            preview.Width = draftSize * 220 / 255;
            preview.Height = draftSize;
            preview.InvalidateVisual();
            previewName.Text = string.IsNullOrWhiteSpace(draftName) ? "Gole" : draftName;
            for (int i = 0; i < modelButtons.Count; i++)
                SelectTile(modelButtons[i], i == draftModel);
            for (int i = 0; i < accessoryButtons.Count; i++)
            {
                bool selected = i == 0 ? draftAccessories.Count == 0 : draftAccessories.Contains(i);
                SelectTile(accessoryButtons[i], selected);
                accessoryButtons[i].IsEnabled = i == 0 || selected || draftAccessories.Count < PetCatalog.MaximumAccessories;
            }
            accessoryCount.Text = draftAccessories.Count + " / " + PetCatalog.MaximumAccessories + " itens · clique para colocar ou remover";
            for (int i = 0; i < thumbnails.Count; i++)
            {
                thumbnails[i].Tint = draftColor;
                if (i >= Bottle.Models.Length)
                    thumbnails[i].Vessel = draftModel;
                else
                    thumbnails[i].EquippedAccessories = draftAccessories.ToArray();
                thumbnails[i].InvalidateVisual();
            }
            foreach (var border in colorBorders)
                border.BorderBrush = border.Tag.ToString().Equals(draftColor, StringComparison.OrdinalIgnoreCase) ? Theme.Teal : Brushes.Transparent;
            for (int i = 0; i < stateButtons.Count; i++)
            {
                double level = PreviewLevels[i];
                stateButtons[i].Background = Math.Abs(level - previewLevel) < .001 ? Theme.B("#CCE9E7") : Theme.B("#EDF2F5");
            }
            UpdatePreviewLabel();
            saveVisual.IsEnabled = Dirty;
        }
        void SelectTile(Button button, bool selected)
        {
            button.BorderBrush = selected ? Theme.Teal : Theme.B("#E1E7EC");
            button.BorderThickness = new Thickness(selected ? 2 : 1);
            button.Background = selected ? Theme.B("#EDF7F6") : Brushes.White;
        }
        void UpdatePersonalityPreview(bool force = false)
        {
            if (personalityMessage == null)
                return;
            var mood = HydrationExpression.AtLevel(previewLevel);
            if (!force && lastPreviewPersonality == draftPersonality && lastPreviewMood == mood)
                return;
            personalityMessage.Text = previewSpeech.Next(mood, draftPersonality);
            lastPreviewPersonality = draftPersonality;
            lastPreviewMood = mood;
        }
        void UpdatePreviewLabel()
        {
            UpdatePersonalityPreview();
            if (previewState != null)
                previewState.Text = simulation.HasValue ? "Simulando · " + Math.Round(previewLevel * 100) + "%" : HydrationExpression.Label(HydrationExpression.AtLevel(previewLevel)) + " · " + Math.Round(previewLevel * 100) + "% de água";
        }
        void SaveAppearance()
        {
            data.Name = string.IsNullOrWhiteSpace(draftName) ? "Gole" : draftName.Trim();
            data.Color = draftColor;
            data.Vessel = draftModel;
            data.PersonalityId = draftPersonality;
            data.Accessories = draftAccessories.ToList();
            data.Accessory = 0;
            data.Size = draftSize;
            data.Motion = draftMotion;
            pet.Apply();
            ReadDraft();
            SyncControls();
            UpdateEditor();
            RefreshStats();
        }
        Grid NumberField(TextBox input, string unit)
        {
            input.Width = 120;
            input.Height = 38;
            input.VerticalContentAlignment = VerticalAlignment.Center;
            input.Margin = new Thickness(0);
            var grid = new Grid { Height = 38, Margin = new Thickness(0, 0, 0, 8) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            var label = Theme.Text(unit, 14, Theme.Muted);
            label.Margin = new Thickness(12, 0, 0, 0);
            label.VerticalAlignment = VerticalAlignment.Center;
            Grid.SetColumn(label, 1);
            grid.Children.Add(input);
            grid.Children.Add(label);
            return grid;
        }
        void SettingsFeedback(string text, bool error)
        {
            settingsMessage.Text = text;
            settingsMessage.Foreground = error ? Theme.B("#AF3434") : Theme.Teal;
        }
        UIElement Settings()
        {
            var page = new StackPanel();
            var timings = new StackPanel();
            timings.Children.Add(Heading("Intervalo do lembrete"));
            timings.Children.Add(Theme.Text("Falas do personagem a cada 5 minutos.", 12, Theme.Muted));
            intervalBox = new TextBox { Text = data.Interval.ToString() };
            System.Windows.Automation.AutomationProperties.SetName(intervalBox, "Intervalo em minutos");
            timings.Children.Add(NumberField(intervalBox, "minutos"));
            timings.Children.Add(Theme.Text("O pet passa de cheio a vazio nesse tempo.", 12, Theme.Muted));
            var presets = new WrapPanel { Margin = new Thickness(0, 8, 0, 0) };
            foreach (int value in new[] { 15, 30, 45, 60 })
            {
                int minutes = value;
                presets.Children.Add(Theme.Button(minutes + " min", delegate
                {
                    intervalBox.Text = minutes.ToString();
                }, false));
            }
            timings.Children.Add(presets);
            Label(timings, "Quantidade por gole");
            servingBox = new TextBox { Text = data.ServingMl.ToString() };
            System.Windows.Automation.AutomationProperties.SetName(servingBox, "Quantidade por gole em ml");
            timings.Children.Add(NumberField(servingBox, "ml"));
            timings.Children.Add(Theme.Text("Cada clique em Já bebi um gole registra essa quantidade e enche o pet.", 12, Theme.Muted));
            Label(timings, "Meta diária de água");
            goalBox = new TextBox { Text = data.Goal.ToString() };
            System.Windows.Automation.AutomationProperties.SetName(goalBox, "Meta diária em ml");
            timings.Children.Add(NumberField(goalBox, "ml por dia"));
            Label(timings, "Alertas");
            var sound = new CheckBox { Content = "Som nas falas e lembretes", IsChecked = data.SpeechSound };
            var notify = new CheckBox { Content = "Notificação do Windows", IsChecked = data.Notifications };
            timings.Children.Add(sound);
            var soundRow = new Grid { Margin = new Thickness(0, 6, 0, 12) };
            soundRow.ColumnDefinitions.Add(new ColumnDefinition());
            soundRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            var soundChoice = new ComboBox { ItemsSource = SoundCatalog.Names, SelectedIndex = data.AlertSoundId, Height = 40, Padding = new Thickness(10, 7, 10, 7), Margin = new Thickness(0, 0, 12, 0), VerticalContentAlignment = VerticalAlignment.Center };
            System.Windows.Automation.AutomationProperties.SetName(soundChoice, "Som do personagem");
            var volume = new Slider { Minimum = 0, Maximum = 100, Value = data.AlertVolume, TickFrequency = 1, IsSnapToTickEnabled = true, IsMoveToPointEnabled = true, SmallChange = 1, LargeChange = 10, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 14, 0) };
            System.Windows.Automation.AutomationProperties.SetName(volume, "Volume do alerta");
            var listen = Theme.Button("Ouvir", () => pet.PreviewSound(soundChoice.SelectedIndex, (int)volume.Value), false);
            listen.Height = 40;
            listen.Margin = new Thickness(0);
            Grid.SetColumn(listen, 1);
            soundRow.Children.Add(soundChoice);
            soundRow.Children.Add(listen);
            timings.Children.Add(soundRow);
            var volumeRow = new Grid { Margin = new Thickness(0, 0, 0, 12), Height = 34 };
            volumeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(140) });
            volumeRow.ColumnDefinitions.Add(new ColumnDefinition());
            volumeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(48) });
            var volumeTitle = Theme.Text("Volume do alerta", 13, Theme.Ink);
            volumeTitle.Margin = new Thickness(0);
            volumeTitle.VerticalAlignment = VerticalAlignment.Center;
            var volumeValue = Theme.Text(data.AlertVolume + "%", 13, Theme.Muted);
            volumeValue.Margin = new Thickness(0);
            volumeValue.VerticalAlignment = VerticalAlignment.Center;
            volumeValue.TextAlignment = TextAlignment.Right;
            volume.ValueChanged += (_, _) => volumeValue.Text = Math.Round(volume.Value) + "%";
            Grid.SetColumn(volume, 1);
            Grid.SetColumn(volumeValue, 2);
            volumeRow.Children.Add(volumeTitle);
            volumeRow.Children.Add(volume);
            volumeRow.Children.Add(volumeValue);
            timings.Children.Add(volumeRow);
            timings.Children.Add(notify);
            settingsMessage = Theme.Text("", 12, Theme.Teal);
            settingsMessage.Margin = new Thickness(0, 10, 0, 7);
            timings.Children.Add(settingsMessage);
            timings.Children.Add(Theme.Button("Salvar lembretes", delegate
            {
                int interval, goal, serving;
                if (!int.TryParse(intervalBox.Text, out interval) || interval < 5 || interval > 180)
                {
                    SettingsFeedback("Defina um intervalo entre 5 e 180 minutos.", true);
                    return;
                }
                if (!int.TryParse(goalBox.Text, out goal) || goal < 250 || goal > 6000)
                {
                    SettingsFeedback("Defina uma meta entre 250 e 6.000 ml.", true);
                    return;
                }
                if (!int.TryParse(servingBox.Text, out serving) || serving < HydrationState.MinimumServingMl || serving > HydrationState.MaximumServingMl)
                {
                    SettingsFeedback("Defina uma quantidade por gole entre 1 e 6.000 ml.", true);
                    return;
                }
                if (interval != data.Interval)
                    pet.ConfigureInterval(interval);
                data.Goal = goal;
                data.ServingMl = serving;
                data.SpeechSound = sound.IsChecked == true;
                data.AlertSoundId = SoundCatalog.Normalize(soundChoice.SelectedIndex);
                data.AlertVolume = (int)volume.Value;
                data.Notifications = notify.IsChecked == true;
                pet.Save();
                RefreshStats();
                SettingsFeedback(pet.Store.SaveFailed ? "Falha ao salvar." : "Lembretes atualizados.", pet.Store.SaveFailed);
            }, true));
            page.Children.Add(Card(timings));
            var pause = new WrapPanel { Margin = new Thickness(0, 18, 0, 0) };
            pause.Children.Add(Theme.Button("Testar frase", pet.PreviewPhrase, false));
            page.Children.Add(pause);
            var updateCard = UpdateSettingsCard();
            updateCard.Margin = new Thickness(0, 18, 0, 0);
            page.Children.Add(updateCard);
            return Scroll(page);
        }
        Border UpdateSettingsCard()
        {
            var updates = pet.Updates;
            var content = new StackPanel();
            content.Children.Add(Heading("Atualizações automáticas"));
            content.Children.Add(Theme.Text("Versão " + (updates?.Version ?? "de desenvolvimento"), 12, Theme.Muted));
            var status = Theme.Text(updates?.Status ?? "Disponível na versão instalada.", 13, Theme.Ink);
            content.Children.Add(status);
            Action refresh = delegate
            {
                if (!Dispatcher.HasShutdownStarted)
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        status.Text = updates.Status;
                    }));
            };
            if (updates != null)
            {
                updates.Changed += refresh;
                Closed += delegate
                {
                    updates.Changed -= refresh;
                };
            }
            var actions = new WrapPanel();
            var check = Theme.Button("Verificar agora", async delegate { await updates.CheckAsync(); }, false);
            check.IsEnabled = updates?.IsInstalled == true;
            actions.Children.Add(check);
            content.Children.Add(actions);
            content.Children.Add(Theme.Text("O Gole busca atualizações ao abrir e instala ao sair pelo menu. Sem conta nem configuração.", 12, Theme.Muted));
            return Card(content);
        }
        void RefreshHistory(bool force = false)
        {
            if (recordRows == null || (!force && historySnapshot != null && historySnapshot.SequenceEqual(data.Sips)))
                return;
            historySnapshot = data.Sips.ToList();
            var ordered = historySnapshot.AsEnumerable().Reverse().OrderByDescending(s => s.Utc).ToList();
            int pages = Math.Max(1, (ordered.Count + RecordsPerPage - 1) / RecordsPerPage);
            historyPage = Math.Clamp(historyPage, 0, pages - 1);
            recordRows.Children.Clear();
            if (ordered.Count == 0)
                recordRows.Children.Add(Theme.Text("Nenhum gole registrado.", 13, Theme.Muted));
            foreach (var sip in ordered.Skip(historyPage * RecordsPerPage).Take(RecordsPerPage))
            {
                var local = new DateTime(sip.Utc, DateTimeKind.Utc).ToLocalTime();
                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition());
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var timestamp = Theme.Text(local.ToString("dd/MM/yyyy HH:mm:ss"), 12, Theme.Muted);
                timestamp.Margin = new Thickness(0, 0, 8, 0);
                timestamp.VerticalAlignment = VerticalAlignment.Center;
                var amount = Theme.Text(sip.Ml.ToString("N0", CultureInfo.GetCultureInfo("pt-BR")) + " ml", 13, Theme.Ink);
                amount.Margin = new Thickness(0, 0, 12, 0);
                amount.VerticalAlignment = VerticalAlignment.Center;
                amount.FontWeight = FontWeights.SemiBold;
                var remove = Theme.Button("Desfazer", () => { pet.RemoveDrink(sip); RefreshStats(); }, false);
                remove.Tag = sip;
                remove.FontSize = 12;
                remove.Padding = new Thickness(10, 6, 10, 6);
                remove.MinHeight = 32;
                remove.Margin = new Thickness(0);
                System.Windows.Automation.AutomationProperties.SetName(remove, "Desfazer gole de " + local.ToString("dd/MM/yyyy HH:mm:ss") + ", " + sip.Ml + " ml");
                Grid.SetColumn(amount, 1);
                Grid.SetColumn(remove, 2);
                row.Children.Add(timestamp);
                row.Children.Add(amount);
                row.Children.Add(remove);
                recordRows.Children.Add(new Border { Child = row, Padding = new Thickness(0, 8, 0, 8), BorderBrush = Theme.B("#E5ECEF"), BorderThickness = new Thickness(0, 0, 0, 1) });
            }
            historyPageLabel.Text = "Página " + (historyPage + 1) + " de " + pages;
            previousHistory.IsEnabled = historyPage > 0;
            nextHistory.IsEnabled = historyPage < pages - 1;
            clearHistory.IsEnabled = ordered.Count > 0;
        }
        public void RefreshStats()
        {
            if (total == null || footer == null)
                return;
            DateTime now = DateTime.UtcNow;
            int ml = data.Today(now);
            double ratio = ml / (double)data.Goal;
            total.Text = ml.ToString("N0", CultureInfo.GetCultureInfo("pt-BR")) + " ml";
            progressLabel.Text = Math.Round(ratio * 100) + "% de " + data.Goal.ToString("N0", CultureInfo.GetCultureInfo("pt-BR")) + " ml";
            progress.Value = Math.Min(1, ratio);
            countdown.Text = pet.NextText();
            last.Text = data.LastDrink == 0 ? "Nenhum registro ainda" : "Último registro · " + new DateTime(data.LastDrink, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM HH:mm");
            petStatus.Text = data.Paused(now) ? "Pausado" : HydrationExpression.Label(HydrationExpression.AtLevel(data.Level(now))) + " · " + Math.Round(data.Level(now) * 100) + "%";
            RefreshHistory();
            undo.IsEnabled = pet.CanUndo;
            footer.Text = pet.Store.SaveFailed ? "Não foi possível salvar os dados." : pet.Store.Recovered ? "Preferências recuperadas. Confira seus ajustes." : "Fechar o painel mantém o pet ativo.";
        }
    }
}
