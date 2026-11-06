using System;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    static (string Fill, string Ink, string Detail) NewClothColors(int item) => item switch
    {
        83 => ("#B97596", "#80506D", "#FAE5EE"),
        84 => ("#7089AF", "#405D81", "#C9DAEF"),
        85 => ("#93AE9D", "#526E60", "#E2EBD8"),
        86 => ("#F0E7D7", "#857969", "#7F9DB4"),
        87 => ("#E6B4C7", "#A77195", "#B9688E"),
        88 => ("#A69ACA", "#6B5A92", "#F0D9A3"),
        89 => ("#C7A682", "#897055", "#F4DFC2"),
        _ => ("#8EAAC4", "#526F92", "#F2E9D8")
    };
    void DrawExpandedClothing(DrawingContext d, BodyFit fit, int item, bool layered)
    {
        var colors = NewClothColors(item);
        Geometry cloth;
        if (item is 83 or 84 or 85 or 90)
        {
            var cut = CutSkirt(fit, item == 90 ? 29 : 55);
            if (item == 85)
                cut = cut with
                {
                    HemY = 231,
                    HemLeft = cut.Left - 12,
                    HemRight = cut.Right + 12
                };
            cloth = cut.Outline();
            if (item == 90)
            {
                var bodice = Garment(fit, "M 40,157 L 94,157 Q 110,182 126,157 L 180,157 L 180,189 L 40,189 Z");
                d.DrawGeometry(B(colors.Fill), P(colors.Ink, 2), bodice);
            }
            d.DrawGeometry(B(colors.Fill), P(colors.Ink, 2), cloth);
            d.DrawLine(P(colors.Ink, 3), cut.Waist(0), cut.Waist(1));
        }
        else
        {
            cloth = TopOutline(fit, item == 89 ? 54 : 51, layered);
            d.DrawGeometry(B(colors.Fill), P(colors.Ink, 2), cloth);
        }
        d.PushClip(cloth);
        if (item is 83 or 90)
            for (int y = 191; y < 233; y += 12)
                for (int x = 35; x < 190; x += 15)
                    d.DrawEllipse(B(colors.Detail), null, new Point(x + (y % 2) * 5, y), 2.5, 2.5);
        if (item is 84 or 86)
        {
            for (int y = 168; y < 236; y += 10)
                d.DrawLine(P(colors.Detail, item == 84 ? 3 : 4), new Point(20, y), new Point(200, y));
            if (item == 84)
                for (int x = 35; x < 190; x += 15)
                    d.DrawLine(P(colors.Detail, 2), new Point(x, 180), new Point(x, 235));
        }
        if (item == 85)
            for (int x = 55; x < 170; x += 18)
                d.DrawLine(P(colors.Detail, 1.5), new Point(110 + (x - 110) * .6, 183), new Point(x, 233));
        if (item == 87)
            DrawHeart(d, 110, layered ? 181 : 190, 10, colors.Detail, colors.Ink);
        if (item == 88)
        {
            DrawStar(d, 110, 184, 8, colors.Detail, colors.Detail);
            DrawStar(d, 85, 201, 4, colors.Detail, colors.Detail);
            DrawStar(d, 137, 200, 4, colors.Detail, colors.Detail);
        }
        if (item == 89)
            for (int y = 175; y < 216; y += 12)
                d.DrawEllipse(B(colors.Detail), P(colors.Ink, 1), new Point(126, y), 2.5, 2.5);
        d.Pop();
    }
    void DrawNewVesselDetails(DrawingContext d, int model, Pen line)
    {
        if (model == 29)
        {
            d.DrawRoundedRectangle(B("#D5B2C5"), line, new Rect(58, 43, 104, 19), 5, 5);
            Path(d, "M 128,43 L 135,21 L 150,21", null, P("#B389A9", 6));
            Path(d, "M 155,87 C 204,73 203,152 152,159", null, P("#9EB8C6", 8));
        }
        if (model == 30)
        {
            Path(d, "M 67,76 Q 110,30 153,76 M 61,87 L 159,87", null, P("#BCABD4", 7));
            Path(d, "M 70,198 Q 110,226 150,198", null, P("#BCABD4", 5));
        }
        if (model == 31)
        {
            d.DrawRoundedRectangle(B("#C3B17C"), line, new Rect(80, 31, 60, 25), 6, 6);
            Path(d, "M 64,177 Q 110,190 156,177 M 72,94 Q 110,103 148,94", null, P("#A5C6B4", 4));
        }
        if (model == 32)
        {
            d.DrawEllipse(B("#C0CFDA"), line, new Point(110, 67), 45, 8);
            d.DrawEllipse(null, P("#667F8E", 3), new Point(110, 64), 10, 4);
            Path(d, "M 68,213 Q 110,225 152,213", null, P("#A5BBC7", 4));
        }
    }
}
