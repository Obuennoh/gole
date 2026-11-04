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
                        AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = items[i][0], Color = i % 2 == 0 ? "#D875B3" : "#6D69C9", Pattern = i is 0 or 5 ? 3 : 0 } }
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
            for (int page = 0; page < 3; page++)
            {
                var canvas = new Canvas { Width = 1400, Height = 60 + clothes.Length * 190, Background = Theme.B("#F1F5F7") };
                for (int column = 0; column < 7; column++)
                {
                    int model = page * 7 + column;
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
                            AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = item, Color = row % 2 == 0 ? "#D875B3" : "#3C82D5", Pattern = row % 7 } }
                        }, column * 200 + 10, 45 + row * 190);
                        Add(canvas, Theme.Text(PetCatalog.Accessories[item], 12, Theme.Ink), column * 200 + 20, 207 + row * 190);
                    }
                }
                Save(canvas, Path.Combine(folder, "roupas-" + (page + 1) + ".png"));
            }
            for (int page = 0; page < 3; page++)
            {
                var canvas = new Canvas { Width = 1400, Height = 60 + 10 * 190, Background = Theme.B("#F1F5F7") };
                int[][] outfits = { new[] { 29, 11, 33 }, new[] { 51, 42, 5 }, new[] { 52, 38, 46 }, new[] { 55, 44, 61 }, new[] { 56, 40, 35 },
                    new[] { 53, 59, 26 }, new[] { 54, 16, 43 }, new[] { 58, 62, 66 }, new[] { 49, 1, 47 }, new[] { 27, 32, 48 } };
                for (int column = 0; column < 7; column++)
                {
                    int model = page * 7 + column;
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
                            AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = items[0], Color = "#D875B3", Pattern = row % 7 } }
                        }, column * 200 + 10, 45 + row * 190);
                    }
                }
                Save(canvas, Path.Combine(folder, "combinacoes-" + (page + 1) + ".png"));
            }
            var preview = new Canvas { Width = 1400, Height = 550, Background = Theme.B("#F1F5F7") };
            Add(preview, Theme.Text("GOLE · cores e estampas por acessório", 24, Theme.Ink), 24, 16);
            string[] colors = { "#D875B3", "#3C82D5", "#45A58C", "#6D69C9", "#DE7867", "#EDAA61", "#456477" };
            for (int i = 0; i < AccessoryStyle.Patterns.Count; i++)
            {
                Add(preview, new Bottle
                {
                    Vessel = 4,
                    Accessory = 27,
                    Width = 180,
                    Height = 175,
                    AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = 27, Color = colors[i], Pattern = i } }
                }, i * 200 + 10, 57);
                Add(preview, Theme.Text(AccessoryStyle.Patterns[i], 14, Theme.Ink), i * 200 + 25, 237);
                int item = 51 + i;
                Add(preview, new Bottle
                {
                    Vessel = new[] { 0, 8, 19, 5, 15, 20, 14 }[i],
                    Accessory = item,
                    Width = 180,
                    Height = 175,
                    AccessoryStyles = new[] { new AccessoryStyle { AccessoryId = item, Color = colors[6 - i], Pattern = i % 4 } }
                }, i * 200 + 10, 292);
                Add(preview, Theme.Text(PetCatalog.Accessories[item], 14, Theme.Ink), i * 200 + 25, 477);
            }
            Save(preview, Path.Combine(folder, "personalizacao.png"));
            return 0;
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
                Add(canvas, Theme.Text("Acessórios · combine até três", 20, Theme.Ink), 24, accessoryTop - 40);
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
