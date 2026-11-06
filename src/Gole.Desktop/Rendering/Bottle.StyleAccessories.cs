using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    void DrawStyleAccessory(DrawingContext d, int item)
    {
        switch (item)
        {
            case 91:
            case 92:
                var shade = new RadialGradientBrush();
                shade.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(item == 91 ? "#C044384E" : "#CFDC7E9D"), 0));
                shade.GradientStops.Add(new GradientStop(Colors.Transparent, 1));
                foreach (double x in item == 91 ? new[] { 87.0, 133 } : new[] { 74.0, 146 })
                    d.DrawEllipse(shade, null, new Point(x, item == 91 ? 126 : 141), item == 91 ? 22 : 15, item == 91 ? 17 : 9);
                break;
            case 93:
            case 95:
                Path(d, "M 98,147 Q 105,138 110,145 Q 115,138 122,147 Q 111,161 98,147 Z", B(item == 93 ? "#332D3D" : "#8D466C"), P("#493040", 1.2));
                Path(d, "M 102,150 Q 110,154 118,150", null, P(item == 93 ? "#83778F" : "#D68FB2", 1.5));
                break;
            case 94:
                Path(d, "M 75,123 Q 86,115 98,123 M 75,123 L 68,116 L 72,126 M 122,123 Q 134,115 145,123 L 152,116 L 148,126", null, P("#3C3045", 2.4));
                break;
            case 96:
                Path(d, "M 123,179 L 123,165 M 130,180 L 130,162 M 137,180 L 137,168", null, P("#425678", 3));
                Path(d, "M 123,168 L 123,164 M 130,166 L 130,162 M 137,172 L 137,168", null, P("#D9B45E", 3));
                Path(d, "M 117,177 L 143,177 L 142,195 Q 130,203 118,195 Z", B("#E4E7ED"), P("#6E8399", 1.7));
                Path(d, "M 121,182 L 139,182", null, P("#ADBAC9", 1));
                break;
            case 97:
                foreach (double angle in new[] { 0.0, 60, 120 })
                {
                    d.PushTransform(new RotateTransform(angle, 138, 177));
                    d.DrawEllipse(null, P("#697EA8", 1.6), new Point(138, 177), 11, 4.5);
                    d.Pop();
                }
                d.DrawEllipse(B("#D59CC2"), P("#82638F", 1), new Point(138, 177), 3, 3);
                break;
            case 98:
                Path(d, "M 61,184 Q 110,213 159,184 M 119,200 Q 142,222 159,187", null, P("#545368", 3.8));
                Path(d, "M 61,184 Q 110,213 159,184 M 119,200 Q 142,222 159,187", null, P("#C9C5D7", 1.7));
                d.DrawEllipse(B("#C9C5D7"), P("#545368", 1), new Point(61, 184), 3, 3);
                d.DrawEllipse(B("#C9C5D7"), P("#545368", 1), new Point(159, 184), 3, 3);
                break;
            case 99:
                d.DrawRoundedRectangle(B("#10FFFFFF"), P("#37364A", 4.5), new Rect(69, 113, 35, 26), 5, 5);
                d.DrawRoundedRectangle(B("#10FFFFFF"), P("#37364A", 4.5), new Rect(116, 113, 35, 26), 5, 5);
                Path(d, "M 104,122 Q 110,118 116,122 M 69,119 L 60,116 M 151,119 L 160,116", null, P("#37364A", 3));
                break;
        }
    }
}
