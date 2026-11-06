using Gole.Core;
using Gole.Infrastructure;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Gole.Desktop
{
    static class VisualTests
    {
        public static int RenderAnimation(string folder)
        {
            Directory.CreateDirectory(folder);
            int[][] items = { new[] { 35, 27, 11 }, new[] { 17, 50, 29 }, new[] { 1, 51, 42 },
                new[] { 44, 32, 48 }, new[] { 45, 46, 38 }, new[] { 29, 47, 9 } };
            string[] labels = { "Respiração e orelhas", "Asas e brilhos", "Gestos e headphone", "Cansaço e balão", "Capa e jetpack", "Comemoração ao beber" };
            int[] models = { 0, 5, 1, 8, 19, 4 };
            for (int frame = 0; frame < 72; frame++)
            {
                double seconds = frame / 10.0;
                var canvas = new Canvas { Width = 840, Height = 640, Background = Theme.B("#F1F5F7") };
                Add(canvas, Theme.Text("GOLE · personagem e acessórios animados", 22, Theme.Ink), 24, 15);
                for (int i = 0; i < items.Length; i++)
                {
                    double x = i % 3 * 280, y = 50 + i / 3 * 290;
                    Add(canvas, new Bottle
                    {
                        Vessel = models[i],
                        EquippedAccessories = items[i],
                        Width = 260,
                        Height = 235,
                        Level = i == 3 ? .1 : 1,
                        Phase = seconds,
                        Celebrate = i == 5,
                        CelebrationSeconds = seconds % 4.8,
                    }, x + 10, y);
                    Add(canvas, Theme.Text(labels[i], 14, Theme.Ink), x + 28, y + 244);
                }
                Save(canvas, Path.Combine(folder, "frame-" + frame.ToString("D3") + ".png"));
            }
            return 0;
        }

        public static int RenderFitting(string folder)
        {
            Directory.CreateDirectory(folder);
            int[] clothes = { 27, 29, 44, 51, 52, 53, 54, 55, 56, 57, 58 };
            for (int page = 0; page * 7 < PetCatalog.Models.Count; page++)
            {
                var canvas = new Canvas { Width = 1400, Height = 60 + clothes.Length * 190, Background = Theme.B("#F1F5F7") };
                for (int column = 0; column < 7; column++)
                {
                    int model = page * 7 + column;
                    if (model >= PetCatalog.Models.Count)
                        break;
                    Add(canvas, Theme.Text(PetCatalog.Models[model], 15, Theme.Ink), column * 200 + 20, 15);
                    for (int row = 0; row < clothes.Length; row++)
                    {
                        int item = clothes[row];
                        Add(canvas, new Bottle
                        {
                            Vessel = model,
                            Accessory = item,
                            Width = 180,
                            Height = 158,
                            MotionEnabled = false,
                        }, column * 200 + 10, 45 + row * 190);
                        Add(canvas, Theme.Text(PetCatalog.Accessories[item], 12, Theme.Ink), column * 200 + 20, 207 + row * 190);
                    }
                }
                Save(canvas, Path.Combine(folder, "roupas-" + (page + 1) + ".png"));
            }
            for (int page = 0; page * 7 < PetCatalog.Models.Count; page++)
            {
                var canvas = new Canvas { Width = 1400, Height = 60 + 10 * 190, Background = Theme.B("#F1F5F7") };
                int[][] outfits = { new[] { 29, 11, 33 }, new[] { 51, 42, 5 }, new[] { 52, 38, 46 }, new[] { 55, 44, 61 }, new[] { 56, 40, 35 },
                    new[] { 53, 59, 26 }, new[] { 54, 16, 43 }, new[] { 58, 62, 66 }, new[] { 49, 1, 47 }, new[] { 27, 32, 48 } };
                for (int column = 0; column < 7; column++)
                {
                    int model = page * 7 + column;
                    if (model >= PetCatalog.Models.Count)
                        break;
                    Add(canvas, Theme.Text(PetCatalog.Models[model], 15, Theme.Ink), column * 200 + 20, 15);
                    for (int row = 0; row < outfits.Length; row++)
                    {
                        var items = outfits[row];
                        Add(canvas, new Bottle
                        {
                            Vessel = model,
                            EquippedAccessories = items,
                            Width = 180,
                            Height = 170,
                            Level = row % 2 == 0 ? 1 : 0,
                            Phase = 1.5,
                        }, column * 200 + 10, 45 + row * 190);
                    }
                }
                Save(canvas, Path.Combine(folder, "combinacoes-" + (page + 1) + ".png"));
            }
            var preview = new Canvas { Width = 1400, Height = 550, Background = Theme.B("#F1F5F7") };
            Add(preview, Theme.Text("GOLE · roupas com caimento próprio e cores originais", 24, Theme.Ink), 24, 16);
            int[] examples = { 27, 55, 56, 29, 51, 52, 53, 44, 54, 57, 58, 15, 39, 17 };
            for (int i = 0; i < examples.Length; i++)
            {
                int column = i % 7, row = i / 7;
                Add(preview, new Bottle
                {
                    Vessel = new[] { 4, 8, 19, 20, 0, 5, 15 }[column],
                    Accessory = examples[i],
                    MotionEnabled = false,
                    Width = 180,
                    Height = 175
                }, column * 200 + 10, 57 + row * 235);
                Add(preview, Theme.Text(PetCatalog.Accessories[examples[i]], 14, Theme.Ink), column * 200 + 25, 237 + row * 235);
            }
            Save(preview, Path.Combine(folder, "roupas-corrigidas.png"));
            RenderMix(folder);
            RenderStyles(folder);
            RenderCollection(folder);
            RenderWardrobePanel(folder);
            var gestures = new Canvas { Width = 1100, Height = 65 + ((PersonalityCatalog.Options.Count - 2) / 4 + 1) * 270, Background = Theme.B("#F1F5F7") };
            Add(gestures, Theme.Text("GOLE · gestos das personalidades", 24, Theme.Ink), 24, 15);
            for (int id = 1; id < PersonalityCatalog.Options.Count; id++)
            {
                int x = (id - 1) % 4 * 275, y = 65 + (id - 1) / 4 * 270;
                Add(gestures, new Bottle { Vessel = 0, PersonalityId = id, Phase = 3.5, Level = .7, Width = 250, Height = 220 }, x + 10, y);
                Add(gestures, Theme.Text(PersonalityCatalog.Get(id).Name, 15, Theme.Ink), x + 90, y + 225);
            }
            Save(gestures, Path.Combine(folder, "personalidades-gestos.png"));
            return 0;
        }

        static void RenderMix(string folder)
        {
            var canvas = new Canvas { Width = 1200, Height = 760, Background = Theme.B("#F1F5F7") };
            Add(canvas, Theme.Text("GOLE · Dorameiro + Nerd fofo + Emo", 26, Theme.Ink), 25, 15);
            string[] phrases = { "Onii-chan! Você me notou... 👉👈😊", "Meu coraçãozinho fez um beep. 🤓💗", "Playlist triste, coração quentinho. 🖤" };
            int[] ids = { 13, 6, 11 };
            for (int i = 0; i < 3; i++)
            {
                var pet = new Bottle { Vessel = 0, PersonalityId = ids[i], Width = 370, Height = 320, Level = .7 };
                pet.SpeakGesture(phrases[i], 6);
                pet.Phase = 1.5;
                Add(canvas, pet, 15 + i * 400, 65);
                var text = Theme.EmojiText(phrases[i], 19, Theme.Ink);
                text.Width = 350;
                Add(canvas, text, 25 + i * 400, 395);
            }
            var dark = new Border { Width = 1150, Height = 160, CornerRadius = new CornerRadius(15), Background = Theme.B("#24343F"), Padding = new Thickness(24) };
            dark.Child = Theme.EmojiText("Emojis coloridos também no modo noturno: 😊 🥰 🤓 👉👈 💖 🦆 🕵️ 🏴‍☠️\nOs três estilos se alternam nas falas e nos gestos.", 23, Theme.B("#E8F0F3"));
            Add(canvas, dark, 25, 535);
            Save(canvas, Path.Combine(folder, "personalidades-mistura-emojis.png"));
        }

        static void RenderStyles(string folder)
        {
            var canvas = new Canvas { Width = 1200, Height = 1030, Background = Theme.B("#F1F5F7") };
            Add(canvas, Theme.Text("GOLE · Emo, Nerd e Nerd fofo", 26, Theme.Ink), 25, 18);
            int[][] sets = { new[] { 67, 91, 93, 98, 43 }, new[] { 99, 96, 57, 69, 42 }, new[] { 68, 92, 87, 80, 97 } };
            string[] labels = { "Emo · franja e maquiagem", "Nerd · óculos, canetas e livro escolhido", "Nerd fofo · blush e broche de átomo" };
            for (int i = 0; i < 3; i++)
            {
                Add(canvas, new Bottle { Vessel = i == 1 ? 22 : 0, EquippedAccessories = sets[i], Width = 360, Height = 290, MotionEnabled = false }, 20 + i * 400, 60);
                Add(canvas, Theme.Text(labels[i], 15, Theme.Ink), 20 + i * 400, 355);
            }
            for (int i = 0; i < 9; i++)
            {
                int x = i % 5 * 240, y = 410 + i / 5 * 260;
                Add(canvas, new Bottle { Vessel = new[] { 0, 8, 4, 31, 28 }[i % 5], Accessory = 91 + i, Width = 225, Height = 215, MotionEnabled = false }, x, y);
                Add(canvas, Theme.Text(PetCatalog.Accessories[91 + i], 14, Theme.Ink), x + 20, y + 220);
            }
            var text = Theme.EmojiText("O-oi! Meu coraçãozinho fez um beep. 🤓💗   |   Evento de gole recebido. 🔬", 18, Theme.Ink);
            text.FontFamily = new FontFamily("Segoe UI, Segoe UI Emoji");
            Add(canvas, text, 25, 970);
            Save(canvas, Path.Combine(folder, "emo-nerds-maquiagem.png"));
            var reactions = new Canvas { Width = 1200, Height = 600, Background = Theme.B("#F1F5F7") };
            var pet = new Bottle { PersonalityId = 6 };
            for (int i = 0; i < 8; i++)
            {
                pet = new Bottle { PersonalityId = 6, Width = 280, Height = 235, EquippedAccessories = sets[2] };
                for (int n = 0; n <= i; n++)
                    pet.Touch();
                pet.Phase = .8;
                Add(reactions, pet, i % 4 * 300, i / 4 * 290);
                Add(reactions, Theme.Text(pet.CurrentInteraction.ToString(), 16, Theme.Ink), i % 4 * 300 + 110, i / 4 * 290 + 240);
            }
            Save(reactions, Path.Combine(folder, "reacoes-clique.png"));
        }

        static void RenderCollection(string folder)
        {
            var canvas = new Canvas { Width = 1200, Height = 1170, Background = Theme.B("#F1F5F7") };
            Add(canvas, Theme.Text("GOLE · novos modelos e combinações com 5 acessórios", 24, Theme.Ink), 24, 16);
            for (int i = 0; i < 8; i++)
            {
                double x = i % 4 * 300, y = 65 + i / 4 * 230;
                Add(canvas, new Bottle { Vessel = 21 + i, Width = 270, Height = 185, MotionEnabled = false }, x + 15, y);
                Add(canvas, Theme.Text(PetCatalog.Models[21 + i], 15, Theme.Ink), x + 60, y + 190);
            }
            int[][] outfits = { new[] { 67, 71, 55, 44, 79 }, new[] { 68, 69, 57, 80, 81 },
                new[] { 73, 76, 77, 52, 42 }, new[] { 74, 72, 78, 29, 43 }, new[] { 70, 68, 82, 56, 80 }, new[] { 75, 69, 51, 79, 81 } };
            string[] labels = { "Franja e choker de lua", "Nerdzinho com livrinho", "Sapinho e mochila de gatinho", "Mago e bolsa de poções", "Fone de gatinho e balão", "Nuvem e broche de planeta" };
            Add(canvas, Theme.Text("Monte seu estilo · personalidade e visual são escolhas independentes", 20, Theme.Ink), 24, 542);
            for (int i = 0; i < outfits.Length; i++)
            {
                double x = i % 3 * 400, y = 592 + i / 3 * 280;
                Add(canvas, new Bottle
                {
                    Vessel = new[] { 23, 22, 21, 25, 26, 28 }[i],
                    EquippedAccessories = outfits[i],
                    Width = 370,
                    Height = 225,
                    MotionEnabled = false,
                    Tint = i % 2 == 0 ? "#9B73CB" : "#26B9C4"
                }, x + 15, y);
                Add(canvas, Theme.Text(labels[i], 15, Theme.Ink), x + 60, y + 235);
            }
            Save(canvas, Path.Combine(folder, "novos-estilos.png"));
        }

        static void RenderWardrobePanel(string folder)
        {
            var state = HydrationState.Fresh(DateTime.UtcNow);
            state.PersonalityId = 13;
            state.PersonalityIds = new() { 13, 6, 11 };
            var store = new JsonStateStore(Path.Combine(folder, "preview-state"));
            var pet = new CompanionWindow(new HydrationSession(state), store);
            foreach (bool dark in new[] { false, true })
            {
                state.NightMode = dark;
                state.Looks = new System.Collections.Generic.List<SavedLook>();
                state.Looks.Add(new SavedLook { Name = "Nerd de biblioteca", Vessel = 30, Color = "#26B9C4", Accessories = new() { 68, 69, 89, 80, 81 } });
                state.Looks.Add(new SavedLook { Name = "Saia de bolinhas", Vessel = 31, Color = "#D875B3", Accessories = new() { 83, 87, 60, 28, 40 } });
                var panel = new PanelWindow(pet) { Resources = Theme.CreateResources() };
                panel.SelectPage(1);
                var content = (FrameworkElement)panel.Content;
                ((Grid)content).Background = panel.Background;
                content.Measure(new Size(1090, 800));
                content.Arrange(new Rect(0, 0, 1090, 800));
                content.UpdateLayout();
                var bitmap = new RenderTargetBitmap(1090, 800, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(content);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                using (var stream = File.Create(Path.Combine(folder, dark ? "looks-noturno.png" : "looks-claro.png")))
                    encoder.Save(stream);
                var queue = new System.Collections.Generic.Queue<DependencyObject>();
                queue.Enqueue(content);
                CheckBox selection = null;
                while (queue.Count > 0)
                {
                    var node = queue.Dequeue();
                    if (node is CheckBox check && System.Windows.Automation.AutomationProperties.GetName(check) == "Personalidade Pick-me girl")
                    {
                        selection = check;
                        break;
                    }
                    for (int child = 0; child < VisualTreeHelper.GetChildrenCount(node); child++)
                        queue.Enqueue(VisualTreeHelper.GetChild(node, child));
                }
                DependencyObject parent = selection;
                while (parent != null && parent is not ScrollViewer)
                    parent = VisualTreeHelper.GetParent(parent);
                if (parent is ScrollViewer scroll)
                {
                    scroll.ScrollToVerticalOffset(430);
                    content.UpdateLayout();
                    var selectionBitmap = new RenderTargetBitmap(1090, 800, 96, 96, PixelFormats.Pbgra32);
                    selectionBitmap.Render(content);
                    var selectionEncoder = new PngBitmapEncoder();
                    selectionEncoder.Frames.Add(BitmapFrame.Create(selectionBitmap));
                    using var selectionFile = File.Create(Path.Combine(folder, dark ? "personalidades-noturno.png" : "personalidades-claro.png"));
                    selectionEncoder.Save(selectionFile);
                }
                panel.Close();
            }
            pet.Close();
        }

        static void Save(Canvas canvas, string path)
        {
            canvas.Measure(new Size(canvas.Width, canvas.Height));
            canvas.Arrange(new Rect(0, 0, canvas.Width, canvas.Height));
            canvas.UpdateLayout();
            var bitmap = new RenderTargetBitmap((int)canvas.Width, (int)canvas.Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(canvas);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }

        public static int Render(string output)
        {
            try
            {
                const int columns = 7, cellWidth = 200, cellHeight = 240;
                int modelRows = (PetCatalog.Models.Count + columns - 1) / columns;
                int accessoryRows = (PetCatalog.Accessories.Count + columns - 1) / columns;
                int moodTop = 55 + modelRows * cellHeight + 55;
                int accessoryTop = moodTop + cellHeight + 55;
                int height = accessoryTop + accessoryRows * cellHeight;
                var canvas = new Canvas { Width = 1400, Height = height, Background = Theme.B("#F1F5F7") };
                Add(canvas, Theme.Text("Modelos", 20, Theme.Ink), 24, 15);
                for (int i = 0; i < PetCatalog.Models.Count; i++)
                {
                    int x = (i % columns) * cellWidth, y = 55 + (i / columns) * cellHeight;
                    Add(canvas, new Bottle { Vessel = i, Level = 1, Width = 180, Height = 200 }, x + 10, y);
                    Add(canvas, Theme.Text(Bottle.Models[i], 14, Theme.Ink), x + 25, y + 205);
                }
                Add(canvas, Theme.Text("Expressões ao longo do ciclo", 20, Theme.Ink), 24, moodTop - 40);
                double[] levels = { 1, .7, .5, .3, .1, 0 };
                for (int i = 0; i < levels.Length; i++)
                {
                    Add(canvas, new Bottle { Vessel = 0, Level = levels[i], Width = 180, Height = 200 }, i * cellWidth + 10, moodTop);
                    Add(canvas, Theme.Text(HydrationExpression.Label(HydrationExpression.AtLevel(levels[i])), 14, Theme.Ink), i * cellWidth + 25, moodTop + 205);
                }
                Add(canvas, Theme.Text("Acessórios · combine até " + PetCatalog.MaximumAccessories, 20, Theme.Ink), 24, accessoryTop - 40);
                for (int i = 0; i < PetCatalog.Accessories.Count; i++)
                {
                    int x = (i % columns) * cellWidth, y = accessoryTop + (i / columns) * cellHeight;
                    Add(canvas, new Bottle { Vessel = i % PetCatalog.Models.Count, Accessory = i, Level = 1, Width = 180, Height = 200, Tint = i % 2 == 0 ? "#9B73CB" : "#26B9C4" }, x + 10, y);
                    Add(canvas, Theme.Text(Bottle.Accessories[i], 14, Theme.Ink), x + 20, y + 205);
                }
                canvas.Measure(new Size(1400, height));
                canvas.Arrange(new Rect(0, 0, 1400, height));
                canvas.UpdateLayout();
                var target = new RenderTargetBitmap(1400, height, 96, 96, PixelFormats.Pbgra32);
                target.Render(canvas);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(target));
                using (var stream = File.Create(output))
                    encoder.Save(stream);
                return 0;
            }
            catch (Exception ex) { File.WriteAllText(output + ".error.txt", ex.ToString()); return 1; }
        }
        static void Add(Canvas canvas, UIElement child, double x, double y)
        {
            Canvas.SetLeft(child, x);
            Canvas.SetTop(child, y);
            canvas.Children.Add(child);
        }
    }
}
