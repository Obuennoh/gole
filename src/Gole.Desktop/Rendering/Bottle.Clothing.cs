using System;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    internal readonly record struct SkirtCut(double Left, double Right, double HemLeft, double HemRight, double WaistY, double HemY)
    {
        public Point Waist(double u) => new Point(Left + (Right - Left) * u, WaistY + 20 * u * (1 - u));
        public Point Hem(double u) => new Point(HemLeft + (HemRight - HemLeft) * u, HemY + 36 * u * (1 - u));
        public Geometry Outline()
        {
            var shape = new StreamGeometry();
            using (var c = shape.Open())
            {
                c.BeginFigure(new Point(Left, WaistY), true, true);
                c.QuadraticBezierTo(new Point(110, WaistY + 10), new Point(Right, WaistY), true, false);
                c.LineTo(new Point(HemRight, HemY), true, false);
                c.QuadraticBezierTo(new Point(110, HemY + 18), new Point(HemLeft, HemY), true, false);
            }
            return shape;
        }
    }

    internal static SkirtCut CutSkirt(BodyFit fit, int item)
    {
        double waistY = item == 29 ? 184 : 180;
        double flare = item == 56 ? 25 : item == 29 ? 23 : item == 55 ? 20 : 18;
        return new SkirtCut(fit.Left(waistY) - 1, fit.Right(waistY) + 1,
            Math.Min(fit.Left(waistY) - flare, fit.Left(212) - 12),
            Math.Max(fit.Right(waistY) + flare, fit.Right(212) + 12), waistY, item == 29 ? 220 : 218);
    }

    static (string Fill, string Outline, string Detail) ClothColors(int item) => item switch
    {
        27 => ("#D996B2", "#9F617D", "#B97596"),
        29 => ("#A997D4", "#756295", "#D7C9E9"),
        51 => ("#E8DFCE", "#8A8173", "#79A9AA"),
        52 => ("#B09FCB", "#75638E", "#DCD0E9"),
        53 => ("#6F91AB", "#415F7D", "#C2D4DE"),
        54 => ("#B69479", "#775E50", "#DEC7AD"),
        55 => ("#83A3C6", "#526F92", "#ADC5DE"),
        56 => ("#D8B1D7", "#A37CA5", "#F0DDED"),
        57 => ("#88A390", "#526E60", "#C2D3C7"),
        58 => ("#C1848D", "#8E5A6F", "#E9C0C8"),
        _ => ("#647F9F", "#415F7D", "#BCCDDC")
    };

    void DrawClothing(DrawingContext d, BodyFit fit, int item, bool layered)
    {
        if (item is 27 or 55 or 56)
        {
            DrawSkirt(d, fit, item);
            return;
        }
        if (item == 29)
        {
            DrawDress(d, fit);
            return;
        }
        if (item == 53)
        {
            DrawOveralls(d, fit);
            return;
        }
        if (item == 58)
        {
            DrawPoncho(d, fit, layered);
            return;
        }
        DrawTop(d, fit, item, layered);
    }

    void DrawSkirt(DrawingContext d, BodyFit fit, int item)
    {
        var cut = CutSkirt(fit, item);
        var cloth = cut.Outline();
        var colors = ClothColors(item);
        d.DrawGeometry(B(colors.Fill), P(colors.Outline, 2.2), cloth);
        d.PushClip(cloth);
        int folds = item == 55 ? 12 : 5;
        for (int i = 1; i < folds; i++)
        {
            double u = i / (double)folds;
            var top = cut.Waist(u);
            var bottom = cut.Hem(u);
            if (item == 55 && i % 2 == 0)
            {
                var panel = new StreamGeometry();
                using (var c = panel.Open())
                {
                    c.BeginFigure(top, true, true);
                    c.LineTo(bottom, true, false);
                    c.LineTo(cut.Hem((i + 1.0) / folds), true, false);
                    c.LineTo(cut.Waist((i + 1.0) / folds), true, false);
                }
                d.DrawGeometry(B(colors.Detail), null, panel);
            }
            d.DrawLine(P(item == 55 ? colors.Outline : colors.Detail, item == 55 ? 1.2 : 1.8),
                new Point(top.X, top.Y + 4), new Point(bottom.X, bottom.Y - 3));
        }
        if (item == 56)
        {
            for (int row = 0; row < 3; row++)
            {
                double y = 197 + row * 10;
                var ruffle = new StreamGeometry();
                using (var c = ruffle.Open())
                {
                    c.BeginFigure(new Point(cut.HemLeft, y), false, false);
                    for (int i = 0; i < 5; i++)
                    {
                        double x = cut.HemLeft + (cut.HemRight - cut.HemLeft) * i / 5;
                        double next = cut.HemLeft + (cut.HemRight - cut.HemLeft) * (i + 1) / 5;
                        c.QuadraticBezierTo(new Point((x + next) / 2, y + 9), new Point(next, y), true, false);
                    }
                }
                d.DrawGeometry(null, P(colors.Detail, 3), ruffle);
            }
        }
        d.Pop();
        var band = new StreamGeometry();
        using (var c = band.Open())
        {
            c.BeginFigure(new Point(cut.Left, cut.WaistY), false, false);
            c.QuadraticBezierTo(new Point(110, cut.WaistY + 10), new Point(cut.Right, cut.WaistY), true, false);
        }
        d.DrawGeometry(null, P(colors.Outline, 4), band);
        if (item == 29)
            d.DrawEllipse(B("#EDCF91"), P("#A8864E", 1), cut.Waist(.5), 4, 3);
    }

    void DrawDress(DrawingContext d, BodyFit fit)
    {
        var colors = ClothColors(29);
        var bodice = new StreamGeometry();
        using (var c = bodice.Open())
        {
            c.BeginFigure(new Point(fit.Left(158) + 2, 158), true, true);
            c.LineTo(new Point(94, 158), true, false);
            c.QuadraticBezierTo(new Point(110, 186), new Point(126, 158), true, false);
            c.LineTo(new Point(fit.Right(158) - 2, 158), true, false);
            c.LineTo(new Point(fit.Right(184) - 2, 189), true, false);
            c.QuadraticBezierTo(new Point(110, 199), new Point(fit.Left(184) + 2, 189), true, false);
        }
        d.DrawGeometry(B(colors.Fill), P(colors.Outline, 2.2), bodice);
        DrawSkirt(d, fit, 29);
    }

    Geometry TopOutline(BodyFit fit, int item, bool layered)
    {
        bool open = item is 44 or 54 or 57;
        double hem = layered ? item == 54 ? 207 : 190 : item == 54 ? 218 : item == 57 ? 210 : 212;
        double ease = item == 54 ? 5 : 2;
        var shape = new StreamGeometry();
        using (var c = shape.Open())
        {
            if (open)
            {
                for (int side = 0; side < 2; side++)
                {
                    bool right = side == 1;
                    double Sign(double x) => right ? 220 - x : x;
                    double edge = right ? 220 - fit.Right(132) : fit.Left(132);
                    double hip = right ? 220 - fit.Right(hem - 2) : fit.Left(hem - 2);
                    c.BeginFigure(new Point(Sign(edge - 1), item == 57 ? 140 : 132), true, true);
                    c.QuadraticBezierTo(new Point(Sign(79), 146), new Point(Sign(92), 158), true, false);
                    c.LineTo(new Point(Sign(99), 176), true, false);
                    c.LineTo(new Point(Sign(96), hem), true, false);
                    c.QuadraticBezierTo(new Point(Sign((hip + 96) / 2), hem + 4), new Point(Sign(hip - ease), hem), true, false);
                    c.LineTo(new Point(Sign(edge - 1), 154), true, false);
                }
            }
            else
            {
                c.BeginFigure(new Point(fit.Left(132) - 1, 132), true, true);
                c.QuadraticBezierTo(new Point(78, 144), new Point(94, 157), true, false);
                c.QuadraticBezierTo(new Point(110, 185), new Point(126, 157), true, false);
                c.QuadraticBezierTo(new Point(142, 144), new Point(fit.Right(132) + 1, 132), true, false);
                c.LineTo(new Point(fit.Right(hem - 2) + ease, hem), true, false);
                c.QuadraticBezierTo(new Point(110, hem + 7), new Point(fit.Left(hem - 2) - ease, hem), true, false);
            }
        }
        return shape;
    }

    void DrawTop(DrawingContext d, BodyFit fit, int item, bool layered)
    {
        var colors = ClothColors(item);
        var cloth = TopOutline(fit, item, layered);
        d.DrawGeometry(B(colors.Fill), P(colors.Outline, 2.2), cloth);
        d.PushClip(cloth);
        if (item is 44 or 54 or 57)
        {
            Path(d, "M 83,148 L 101,175 L 85,183 L 91,159 Z M 137,148 L 119,175 L 135,183 L 129,159 Z", B(colors.Detail), P(colors.Outline, 1.4));
            double pocketY = layered ? 176 : 190;
            d.DrawLine(P(colors.Detail, 2), new Point(fit.Left(pocketY) + 8, pocketY), new Point(83, pocketY + 2));
            d.DrawLine(P(colors.Detail, 2), new Point(137, pocketY + 2), new Point(fit.Right(pocketY) - 8, pocketY));
            if (item == 54)
                for (int y = 181; y <= (layered ? 199 : 211); y += 10)
                    d.DrawEllipse(B("#E7CFAC"), null, new Point(128, y), 2.3, 2.3);
        }
        else if (item == 51)
        {
            double x = fit.Right(177) - 20;
            d.DrawRoundedRectangle(null, P(colors.Detail, 1.7), new Rect(x, 176, 12, 11), 2, 2);
        }
        else if (item == 52)
        {
            Path(d, "M 100,167 L 99,181 M 120,167 L 121,181", null, P(colors.Detail, 2));
            d.DrawRoundedRectangle(null, P(colors.Outline, 1.6), new Rect(88, layered ? 177 : 190, 44, layered ? 10 : 17), 5, 5);
        }
        d.Pop();
    }

    void DrawOveralls(DrawingContext d, BodyFit fit)
    {
        var colors = ClothColors(53);
        var trousers = Garment(fit, "M 20,188 Q 110,198 200,188 L 210,230 L 10,230 Z");
        d.DrawGeometry(B(colors.Fill), P(colors.Outline, 2.2), trousers);
        double left = Math.Max(fit.Left(163) + 12, 84), right = Math.Min(fit.Right(163) - 12, 136);
        d.DrawLine(P(colors.Fill, 8), new Point(left, 158), new Point(91, 180));
        d.DrawLine(P(colors.Fill, 8), new Point(right, 158), new Point(129, 180));
        d.DrawRoundedRectangle(B(colors.Fill), P(colors.Outline, 2), new Rect(85, 174, 50, 24), 5, 5);
        d.DrawRoundedRectangle(null, P(colors.Detail, 1.6), new Rect(98, 180, 24, 12), 3, 3);
        d.DrawEllipse(B("#EDCF91"), null, new Point(91, 178), 2.5, 2.5);
        d.DrawEllipse(B("#EDCF91"), null, new Point(129, 178), 2.5, 2.5);
        d.DrawLine(P(colors.Outline, 1.7), new Point(110, 202), new Point(110, 221));
        d.DrawLine(P(colors.Detail, 1.2), new Point(114, 203), new Point(114, 218));
    }

    void DrawPoncho(DrawingContext d, BodyFit fit, bool layered)
    {
        var colors = ClothColors(58);
        double hem = layered ? 184 : 201, point = layered ? 198 : 224;
        var cloth = new StreamGeometry();
        using (var c = cloth.Open())
        {
            c.BeginFigure(new Point(fit.Left(132) - 2, 132), true, true);
            c.QuadraticBezierTo(new Point(77, 144), new Point(94, 157), true, false);
            c.QuadraticBezierTo(new Point(110, 185), new Point(126, 157), true, false);
            c.QuadraticBezierTo(new Point(143, 144), new Point(fit.Right(132) + 2, 132), true, false);
            c.LineTo(new Point(fit.Right(hem) + 17, hem), true, false);
            c.LineTo(new Point(110, point), true, false);
            c.LineTo(new Point(fit.Left(hem) - 17, hem), true, false);
        }
        d.DrawGeometry(B(colors.Fill), P(colors.Outline, 2.2), cloth);
        d.PushClip(cloth);
        for (int row = 0; row < 2; row++)
        {
            double y = hem - 12 + row * 7;
            d.DrawLine(P(colors.Detail, 3), new Point(fit.Left(hem) - 20, y), new Point(110, point - 12 + row * 7));
            d.DrawLine(P(colors.Detail, 3), new Point(110, point - 12 + row * 7), new Point(fit.Right(hem) + 20, y));
        }
        d.Pop();
    }
}
