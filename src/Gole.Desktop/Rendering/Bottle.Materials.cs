using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    static readonly Dictionary<int, Brush> patterns = new();

    static bool IsMaterial(Brush brush) => brush is SolidColorBrush b
        && Math.Max(b.Color.R, Math.Max(b.Color.G, b.Color.B)) - Math.Min(b.Color.R, Math.Min(b.Color.G, b.Color.B)) > 18
        && (b.Color.R + b.Color.G + b.Color.B) / 3.0 > 95;

    static Brush Recolor(Brush brush, string color, double reference, bool fill = false)
    {
        if (color.Length == 0 || brush is not SolidColorBrush || (!fill && !IsMaterial(brush)))
            return brush;
        var original = ((SolidColorBrush)brush).Color;
        var chosen = (Color)ColorConverter.ConvertFromString(color);
        double light = (original.R + original.G + original.B) / 765.0;
        double offset = (light - reference) * .6;
        byte Channel(byte value) => (byte)Math.Clamp(offset > 0 ? value + (255 - value) * offset : value * (1 + offset), 0, 255);
        return new SolidColorBrush(Color.FromArgb(original.A, Channel(chosen.R), Channel(chosen.G), Channel(chosen.B)));
    }

    static IEnumerable<GeometryDrawing> Surfaces(DrawingGroup group) => group.Children.SelectMany(child => child is DrawingGroup nested
        ? Surfaces(nested) : child is GeometryDrawing geometry ? new[] { geometry } : Array.Empty<GeometryDrawing>());

    static void Restyle(DrawingGroup group, string color, int pattern, double? reference = null)
    {
        reference ??= Surfaces(group).Where(g => g.Brush is SolidColorBrush)
            .OrderByDescending(g => g.Geometry.Bounds.Width * g.Geometry.Bounds.Height)
            .Select(g => { var c = ((SolidColorBrush)g.Brush).Color; return (c.R + c.G + c.B) / 765.0; }).FirstOrDefault(.62);
        var children = group.Children.Select(child => child.CloneCurrentValue()).ToArray();
        group.Children.Clear();
        foreach (var child in children)
        {
            if (child is DrawingGroup nested)
                Restyle(nested, color, pattern, reference);
            if (child is GeometryDrawing geometry)
            {
                bool material = geometry.Brush is SolidColorBrush solid && solid.Color.A > 20;
                geometry.Brush = Recolor(geometry.Brush, color, reference.Value, true);
                if (geometry.Pen != null)
                {
                    geometry.Pen = geometry.Pen.CloneCurrentValue();
                    geometry.Pen.Brush = Recolor(geometry.Pen.Brush, color, reference.Value);
                }
                group.Children.Add(geometry);
                if (material && pattern > 0 && geometry.Geometry.Bounds.Width > 12 && geometry.Geometry.Bounds.Height > 10)
                {
                    // Pattern fills share the exact garment geometry, including neckline and body clipping.
                    group.Children.Add(new GeometryDrawing(PatternBrush(pattern), null, geometry.Geometry));
                    if (geometry.Pen != null)
                        group.Children.Add(new GeometryDrawing(null, geometry.Pen, geometry.Geometry));
                }
            }
            else
                group.Children.Add(child);
        }
    }

    static Brush PatternBrush(int pattern)
    {
        if (patterns.TryGetValue(pattern, out var cached))
            return cached;
        var tile = new DrawingGroup();
        using (var d = tile.Open())
        {
            var light = B("#B3FFFFFF");
            var dark = B("#38234957");
            // A transparent tile fixes the repeat size independently of the motif bounds.
            d.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 16, 16));
            switch (pattern)
            {
                case 1:
                    d.DrawEllipse(light, null, new Point(4, 4), 1.7, 1.7);
                    d.DrawEllipse(light, null, new Point(12, 12), 1.7, 1.7);
                    break;
                case 2:
                    d.DrawGeometry(light, null, Shape("M -4,0 L 1,0 L 17,16 L 12,16 Z M 12,0 L 17,0 L 33,16 L 28,16 Z"));
                    break;
                case 3:
                    d.DrawGeometry(light, null, Shape("M 5,1 L 6,5 L 10,6 L 6,7 L 5,11 L 4,7 L 0,6 L 4,5 Z"));
                    d.DrawEllipse(B("#D9FFFFFF"), null, new Point(12, 13), .9, .9);
                    d.DrawEllipse(B("#75FFFFFF"), null, new Point(13, 3), .7, .7);
                    break;
                case 4:
                    d.DrawRectangle(light, null, new Rect(0, 0, 8, 8));
                    d.DrawRectangle(dark, null, new Rect(8, 0, 8, 8));
                    d.DrawRectangle(light, null, new Rect(8, 8, 8, 8));
                    break;
                case 5:
                    d.DrawGeometry(light, null, Shape("M 8,2 L 10,6 L 14,6 L 11,9 L 12,13 L 8,11 L 4,13 L 5,9 L 2,6 L 6,6 Z"));
                    break;
                case 6:
                    d.DrawGeometry(light, null, Shape("M 8,13 C -3,6 3,0 8,5 C 13,0 19,6 8,13 Z"));
                    break;
            }
        }
        var brush = new DrawingBrush(tile)
        {
            Viewport = new Rect(0, 0, 16, 16),
            ViewportUnits = BrushMappingMode.Absolute,
            Viewbox = new Rect(0, 0, 16, 16),
            ViewboxUnits = BrushMappingMode.Absolute,
            TileMode = TileMode.Tile,
            Stretch = Stretch.None
        };
        brush.Freeze();
        patterns[pattern] = brush;
        return brush;
    }
}

