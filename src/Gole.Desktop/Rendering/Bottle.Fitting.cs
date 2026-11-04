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
    static readonly Dictionary<(int Model, int Item, int Part, bool Relaxed, bool Layered, string Color, int Pattern), DrawingGroup> fittedAccessories = new();
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
        var style = AccessoryStyles.FirstOrDefault(s => s.AccessoryId == item);
        string color = style != null && HydrationState.IsColor(style.Color) ? style.Color.ToUpperInvariant() : "";
        int pattern = style != null && style.Pattern >= 0 && style.Pattern < AccessoryStyle.Patterns.Count ? style.Pattern : 0;
        bool layered = EquippedAccessories.Any(id => id is 27 or 29 or 53 or 55 or 56);
        var key = (model, item, part, Level < .15, layered, color, pattern);
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
            if (color.Length > 0 || pattern > 0)
                Restyle(drawing, color, pattern);
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
            DrawSleeve(d, fit, item, right, item == 52 ? "#C1848D" : "#647F9F", "#415F7D");
            return;
        }
        if (item == 32)
        {
            DrawBracelet(d, fit, right);
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

    void DrawClothing(DrawingContext d, BodyFit fit, int item, bool layered)
    {
        bool skirt = item is 27 or 55 or 56;
        string fill = skirt ? "#D996B2" : item is 29 or 53 ? "#A997D4" : item is 52 or 58 ? "#C1848D" : "#647F9F";
        string outline = skirt ? "#9F617D" : item is 29 or 53 ? "#756295" : "#415F7D";
        string path = skirt ? "M 20,180 Q 110,194 200,180 L 210,232 L 10,232 Z"
            : item is 44 or 54 or 57 ? "M 20,128 L 75,142 L 92,159 L 99,181 L 94,218 L 15,218 Z M 200,128 L 145,142 L 128,159 L 121,181 L 126,218 L 205,218 Z"
            : item == 29 ? "M 20,160 L 92,160 Q 110,190 128,160 L 200,160 L 210,232 L 10,232 Z"
            : item == 53 ? "M 20,160 L 83,160 L 87,180 L 133,180 L 137,160 L 200,160 L 210,232 L 10,232 Z"
            : item == 58 ? "M 10,155 L 91,155 Q 110,193 129,155 L 210,155 L 210,212 Q 110,228 10,212 Z"
            : "M 10,132 L 75,142 L 92,159 Q 110,190 128,159 L 145,142 L 210,132 L 210,232 L 10,232 Z";
        if (item == 54)
            path = path.Replace("218", "232");
        var garment = Garment(fit, path);
        if (layered && item is 51 or 52 or 58)
            garment = Geometry.Combine(garment, new RectangleGeometry(new Rect(0, 0, 220, 190)), GeometryCombineMode.Intersect, null);
        d.DrawGeometry(B(fill), P(outline, 2.2), garment);
        d.PushClip(garment);
        if (skirt)
        {
            Path(d, "M 20,180 Q 110,194 200,180", null, P(outline, 6));
            for (int i = 0; i < (item == 55 ? 11 : 5); i++)
            {
                double x = 45 + i * (item == 55 ? 13 : 32);
                d.DrawLine(P("#B66F90", 1.8), new Point(x + (110 - x) * .15, 189), new Point(x, 227));
            }
            if (item == 56)
                Path(d, "M 15,203 Q 110,220 205,203 M 15,212 Q 110,232 205,212", null, P("#F3D3E5", 4));
        }
        else if (item is 29 or 53)
        {
            Path(d, "M 20,185 Q 110,197 200,185", null, P("#E8C4DB", 5));
            if (item == 29)
                d.DrawEllipse(B("#EDCF91"), null, new Point(110, 191), 4, 4);
            else
            {
                d.DrawRoundedRectangle(null, P(outline, 2), new Rect(97, 191, 26, 19), 5, 5);
                d.DrawEllipse(B("#EDCF91"), null, new Point(87, 179), 3, 3);
                d.DrawEllipse(B("#EDCF91"), null, new Point(133, 179), 3, 3);
            }
        }
        else
        {
            if (item is 44 or 54 or 57)
                Path(d, "M 81,160 L 94,180 L 80,189 M 139,160 L 126,180 L 140,189", null, P("#BCCDDC", 2));
            if (item == 51)
                d.DrawRoundedRectangle(null, P(outline, 1.5), new Rect(133, 177, 14, 13), 3, 3);
            if (item == 58)
                Path(d, "M 20,195 L 110,212 L 200,195", null, P("#E9C0C8", 4));
            if (item == 52)
            {
                Path(d, "M 99,168 L 99,184 M 121,168 L 121,184", null, P("#F0D2D9", 2));
                d.DrawRoundedRectangle(null, P(outline, 2), new Rect(88, 192, 44, 20), 8, 8);
            }
            if (item == 54)
                for (int y = 182; y < 217; y += 12)
                    d.DrawEllipse(B("#EDCF91"), null, new Point(132, y), 2.5, 2.5);
            Path(d, "M 20,216 Q 110,230 200,216", null, P(outline, 2));
        }
        d.Pop();
    }

    void DrawSleeve(DrawingContext d, BodyFit fit, int item, bool rightSide, string fill, string outline)
    {
        bool longSleeve = item is 44 or 52 or 54;
        double left = fit.Left(129) + 3, right = fit.Right(129) - 3;
        if (!rightSide)
        {
            d.PushTransform(new TranslateTransform(left - 57, 0));
            Path(d, Level < .15
                ? longSleeve ? "M 59,125 Q 33,144 30,159 L 41,163 Q 44,146 64,139 Z" : "M 59,125 Q 45,134 39,143 L 50,151 L 64,139 Z"
                : longSleeve ? "M 59,125 Q 33,124 28,142 L 40,148 Q 44,136 62,139 Z" : "M 59,125 Q 46,124 39,130 L 44,143 L 62,139 Z", B(fill), P(outline, 2));
            d.Pop();
        }
        else
        {
            d.PushTransform(new TranslateTransform(right - 163, 0));
            Path(d, Level < .15
                ? longSleeve ? "M 161,125 Q 187,144 190,159 L 179,163 Q 176,146 156,139 Z" : "M 161,125 Q 175,134 181,143 L 170,151 L 156,139 Z"
                : longSleeve ? "M 161,125 Q 178,124 184,114 L 195,119 Q 188,140 158,139 Z" : "M 161,125 Q 171,125 176,122 L 183,135 Q 173,141 158,139 Z", B(fill), P(outline, 2));
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
