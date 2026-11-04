using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Gole.Core;

namespace Gole.Desktop;

public partial class Bottle
{
    internal sealed class BodyFit
    {
        public Geometry Body
        {
            get;
        }
        readonly double[] left = new double[256], right = new double[256];

        public BodyFit(Geometry body)
        {
            Body = body;
            var outline = body.GetFlattenedPathGeometry(.08, ToleranceType.Absolute);
            for (int y = 0; y < left.Length; y++)
            {
                var intersections = new List<double>();
                foreach (var figure in outline.Figures)
                {
                    var previous = figure.StartPoint;
                    foreach (var point in figure.Segments.Cast<PolyLineSegment>().SelectMany(s => s.Points).Append(figure.StartPoint))
                    {
                        if ((previous.Y <= y && point.Y > y) || (point.Y <= y && previous.Y > y))
                            intersections.Add(previous.X + (point.X - previous.X) * (y - previous.Y) / (point.Y - previous.Y));
                        previous = point;
                    }
                }
                left[y] = intersections.Count > 0 ? intersections.Min() : 85;
                right[y] = intersections.Count > 0 ? intersections.Max() : 135;
            }
        }

        public double Left(double y) => left[Math.Clamp((int)Math.Round(y), 0, 255)];
        public double Right(double y) => right[Math.Clamp((int)Math.Round(y), 0, 255)];
        public double Width(double y) => Right(y) - Left(y);
    }

    static readonly Dictionary<int, BodyFit> fits = new();
    static readonly Dictionary<(int Model, int Item, int Part, bool Relaxed, bool Layered), DrawingGroup> fittedAccessories = new();
    internal static BodyFit Fit(int model)
    {
        model = Math.Clamp(model, 0, bodies.Length - 1);
        if (!fits.TryGetValue(model, out var fit))
            fits[model] = fit = new BodyFit(Shape(bodies[model]));
        return fit;
    }

    internal static int AccessoryLayer(int id) => id switch
    {
        17 or 18 or 19 or 45 or 46 => 0,
        27 or 29 or 55 or 56 => 10,
        44 or 51 or 52 or 53 or 54 or 57 or 58 => 20,
        8 => 30,
        11 or 14 or 16 or 40 or 62 or 63 or 65 => 40,
        _ => 50
    };

    static Transform FaceTransform(BodyFit fit) => new ScaleTransform(Math.Min(1, fit.Width(126) / 104), 1, 110, 126);

    void DrawLimbs(DrawingContext d, BodyFit fit, double level, Pen line)
    {
        Path(d, "M 84,216 Q 77,233 69,231 L 57,231 M 137,216 Q 144,233 152,231 L 163,231", null, line);
        // Both poses use the same shoulder and wrist anchors as the accessories.
        double left = fit.Left(129) + 3, right = fit.Right(129) - 3;
        d.PushTransform(ArmTransform(fit, false));
        d.PushTransform(new TranslateTransform(left - 57, 0));
        Path(d, level < .15 ? "M 57,129 Q 34,150 35,168" : "M 57,129 Q 35,128 31,151", null, line);
        d.Pop();
        d.Pop();
        d.PushTransform(ArmTransform(fit, true));
        d.PushTransform(new TranslateTransform(right - 163, 0));
        Path(d, level < .15 ? "M 164,129 Q 185,150 183,168" : "M 163,129 Q 183,126 189,110", null, line);
        d.Pop();
        d.Pop();
    }

    internal void DrawFittedAccessory(DrawingContext d, int model, int item) => DrawAnimatedAccessory(d, model, item);

    DrawingGroup AccessoryDrawing(int model, int item, int part)
    {
        bool layered = EquippedAccessories.Any(id => id is 27 or 29 or 53 or 55 or 56);
        var key = (model, item, part, Level < .15, layered);
        if (!fittedAccessories.TryGetValue(key, out var drawing))
        {
            if (fittedAccessories.Count > 2048)
                fittedAccessories.Clear();
            drawing = new DrawingGroup();
            using (var context = drawing.Open())
            {
                var fit = Fit(model);
                if (part > 0)
                    DrawAccessoryPart(context, fit, model, item, part);
                else if (item is 27 or 29 or 44 or >= 51 and <= 58)
                    DrawClothing(context, fit, item, layered);
                else if (item == 32)
                    DrawBracelets(context, fit);
                else if (item == 49)
                    DrawFloat(context, fit);
                else
                {
                    if (item == 63)
                        context.PushClip(fit.Body);
                    context.PushTransform(AccessoryTransform(fit, model, item));
                    if (item >= 59)
                        DrawNewAccessory(context, item);
                    else
                        DrawAccessoryRaw(context, P("#234957", 3.3), item);
                    context.Pop();
                    if (item == 63)
                        context.Pop();
                }
            }
            drawing.Freeze();
            fittedAccessories[key] = drawing;
        }
        return drawing;
    }

    void DrawAccessoryPart(DrawingContext d, BodyFit fit, int model, int item, int part)
    {
        bool right = part == 2;
        if (item is 44 or 51 or 52 or 54)
        {
            var colors = ClothColors(item);
            DrawSleeve(d, fit, item, right, colors.Fill, colors.Outline);
            return;
        }
        if (item == 32)
        {
            DrawBracelet(d, fit, right);
            return;
        }
        if (item is 15 or 39)
        {
            var anchor = EarringAnchor(fit, right);
            double x = anchor.X + (right ? 4 : -4);
            d.DrawLine(P("#BC9A55", 1.7), anchor, new Point(x, item == 15 ? 140 : 148));
            if (item == 15)
                d.DrawEllipse(null, P("#D4AE5B", 3), new Point(x, 147), 5.5, 8);
            else
                DrawStar(d, x, 155, 8, "#EACD80", "#B48D48");
            return;
        }
        d.PushTransform(AccessoryTransform(fit, model, item));
        if (item == 35)
        {
            double x = right ? 136 : 84;
            d.DrawEllipse(B("#FCF4ED"), P("#B99AA8", 2.5), new Point(x, 49), 12, 38);
            d.DrawEllipse(B("#E9B5C8"), null, new Point(x, 48), 6, 28);
        }
        else if (item == 46)
            Path(d, "M 35,193 L 60,193 L 55,222 L 47,210 L 40,222 Z M 160,193 L 185,193 L 180,222 L 172,210 L 165,222 Z", B("#EBB568"), P("#C18B55", 1.5));
        else
        {
            d.PushClip(new RectangleGeometry(new Rect(right ? 110 : -110, -100, 220, 500)));
            DrawAccessoryRaw(d, P("#234957", 3.3), item);
            d.Pop();
        }
        d.Pop();
    }

    static Point EarringAnchor(BodyFit fit, bool right) => new Point(right ? fit.Right(132) + 1 : fit.Left(132) - 1, 132);

    Transform AccessoryTransform(BodyFit fit, int model, int item)
    {
        if (item is 2 or 7 or 8 or 21 or 30 or 37 or 38 or 61)
            return FaceTransform(fit);
        if (item is 28 or 42 or 43 or 66)
            return Transform.Identity;
        // Small side pieces retain their shape and follow the actual body edge.
        if (item is 4 or 23)
            return new TranslateTransform(fit.Right(88) - 162, 0);
        if (item is 26 or 65)
            return new TranslateTransform(fit.Right(165) - 165, item == 26 ? Level < .15 ? 30 : -28 : 0);
        if (item == 41)
            return new TranslateTransform(fit.Left(129) - 54, Level < .15 ? 18 : 0);
        if (item is 47 or 48)
            return new TranslateTransform(fit.Right(129) - 166, Level < .15 ? 58 : 0);
        if (item is 5 or 9 or 12 or 25 or 33 or 34 or 59)
        {
            double[] seats = { 52, 60, 63, 70, 77, 87, 74, 54, 47, 43, 53, 55, 59, 66, 58, 58, 44, 62, 55, 57, 54 };
            double seat = seats[model];
            double baseline = item == 9 ? 70 : item == 25 ? 85 : 87;
            double width = Math.Clamp(fit.Width(100) / 106, .82, 1.15);
            return new MatrixTransform(width, 0, 0, 1, 110 * (1 - width), seat - baseline);
        }
        double y = item == 46 ? 193 : item is 1 or 24 ? 126 : item is 15 or 39 ? 143 : item is 6 or 11 or 14 or 16 or 40 or 62 or 63 or 64 ? 169 : item is 17 or 18 or 19 or 45 or 46 ? 135 : item == 20 ? 60 : 100;
        double ratio = Math.Clamp(fit.Width(y) / 116, .65, 1.25);
        return new ScaleTransform(ratio, 1, 110, y);
    }

    internal Geometry Garment(BodyFit fit, string path) => Geometry.Combine(fit.Body, Shape(path), GeometryCombineMode.Intersect, null, .1, ToleranceType.Absolute);

    void DrawSleeve(DrawingContext d, BodyFit fit, int item, bool rightSide, string fill, string outline)
    {
        bool longSleeve = item is 44 or 52 or 54;
        double left = fit.Left(129) + 3, right = fit.Right(129) - 3;
        if (!rightSide)
        {
            d.PushTransform(new TranslateTransform(left - 57, 0));
            Path(d, Level < .15
                ? longSleeve ? "M 59,125 Q 34,143 30,160 Q 33,166 41,164 Q 43,146 64,139 Z" : "M 59,125 Q 45,134 39,143 L 50,151 L 64,139 Z"
                : longSleeve ? "M 60,125 Q 39,123 29,140 Q 27,145 36,149 L 43,143 Q 47,137 62,139 Z" : "M 59,125 Q 46,124 39,130 L 44,143 L 62,139 Z", B(fill), P(outline, 2));
            d.Pop();
        }
        else
        {
            d.PushTransform(new TranslateTransform(right - 163, 0));
            Path(d, Level < .15
                ? longSleeve ? "M 161,125 Q 186,143 190,160 Q 187,166 179,164 Q 177,146 156,139 Z" : "M 161,125 Q 175,134 181,143 L 170,151 L 156,139 Z"
                : longSleeve ? "M 160,125 Q 175,125 181,115 Q 185,110 194,117 L 191,125 Q 181,142 158,139 Z" : "M 161,125 Q 171,125 176,122 L 183,135 Q 173,141 158,139 Z", B(fill), P(outline, 2));
            d.Pop();
        }
    }

    void DrawBracelets(DrawingContext d, BodyFit fit)
    {
        DrawBracelet(d, fit, false);
        DrawBracelet(d, fit, true);
    }

    void DrawBracelet(DrawingContext d, BodyFit fit, bool right)
    {
        double x = right ? fit.Right(129) + 17 : fit.Left(129) - 19;
        double y = Level < .15 ? 162 : right ? 118 : 148;
        d.DrawEllipse(null, P("#D7B366", 4), new Point(x, y), 7, 4);
        d.DrawEllipse(null, P("#C18CAB", 3), new Point(x + (right ? -1 : 1), y + 6), 7, 4);
    }

    void DrawFloat(DrawingContext d, BodyFit fit)
    {
        double radius = fit.Width(193) / 2 + 16;
        var outer = new EllipseGeometry(new Point(110, 193), radius, 19);
        var inner = new EllipseGeometry(new Point(110, 188), radius - 10, 10);
        var ring = Geometry.Combine(outer, inner, GeometryCombineMode.Exclude, null);
        var backOfBody = Geometry.Combine(fit.Body, new RectangleGeometry(new Rect(0, 0, 220, 189)), GeometryCombineMode.Intersect, null);
        var visible = Geometry.Combine(ring, backOfBody, GeometryCombineMode.Exclude, null);
        d.DrawGeometry(B("#F1C991"), P("#BA8D62", 2), visible);
        d.DrawEllipse(B("#E29BAA"), null, new Point(110 - radius + 11, 199), 5, 3);
        d.DrawEllipse(B("#E29BAA"), null, new Point(110 + radius - 11, 199), 5, 3);
    }

    void DrawNewAccessory(DrawingContext d, int item)
    {
        switch (item)
        {
            case 59:
                Path(d, "M 78,48 L 142,48 L 153,78 L 174,91 Q 110,103 46,91 L 67,78 Z", B("#94BD75"), P("#5A7953", 2.5));
                Path(d, "M 67,78 Q 110,87 153,78", null, P("#D9D59B", 6));
                break;
            case 60:
                Path(d, "M 61,99 Q 110,60 159,99", null, P("#9D5C78", 5));
                Path(d, "M 136,76 Q 113,44 115,67 Q 113,87 136,76 Q 165,51 159,75 Q 156,89 136,76 Z", B("#EB91AD"), P("#9D5C78", 2));
                break;
            case 61:
                d.DrawEllipse(B("#30D9BE61"), P("#AA824F", 3), new Point(87, 126), 18, 18);
                d.DrawEllipse(B("#30D9BE61"), P("#AA824F", 3), new Point(133, 126), 18, 18);
                Path(d, "M 105,124 Q 110,120 115,124 M 69,124 L 60,120 M 151,124 L 160,120", null, P("#AA824F", 2));
                break;
            case 62:
                Path(d, "M 65,160 Q 110,185 155,160 L 154,175 L 119,187 L 110,208 L 100,187 L 66,175 Z", B("#94BD75"), P("#5A7953", 2));
                break;
            case 63:
                d.DrawRoundedRectangle(B("#A58C7C"), P("#69584D", 2), new Rect(50, 190, 120, 12), 4, 4);
                d.DrawRoundedRectangle(B("#EDCF91"), P("#A5813D", 2), new Rect(101, 188, 18, 16), 3, 3);
                break;
            case 64:
                DrawHeart(d, 141, 174, 10, "#E694A7", "#9D5C78");
                break;
            case 65:
                Path(d, "M 67,103 L 184,178", null, P("#915E7B", 5));
                d.DrawRoundedRectangle(B("#D493B4"), P("#915E7B", 2.5), new Rect(163, 166, 39, 32), 8, 8);
                d.DrawRoundedRectangle(B("#EDCF91"), null, new Rect(176, 175, 12, 7), 2, 2);
                break;
            case 66:
                d.DrawEllipse(B("#94BD75"), P("#5A7953", 2), new Point(63, 239), 20, 6);
                d.DrawEllipse(B("#94BD75"), P("#5A7953", 2), new Point(157, 239), 20, 6);
                Path(d, "M 48,237 L 61,229 L 72,237 M 148,237 L 159,229 L 172,237", null, P("#DDE9B9", 4));
                break;
        }
    }
}
