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
