using Gole.Core;
using Gole.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop
{
    public class Bottle : FrameworkElement
    {
        public static readonly string[] Models = PetCatalog.Models.ToArray();
        public static readonly string[] Accessories = PetCatalog.Accessories.ToArray();
        public double Level = 1, Phase;
        public string Tint = "#26B9C4";
        public int Accessory, Vessel;
        public IReadOnlyList<int> EquippedAccessories = Array.Empty<int>();
        public HydrationMood Mood => Celebrate ? HydrationMood.Happy : HydrationExpression.AtLevel(Level);
        public bool Paused, Celebrate;
        static readonly Dictionary<string, Geometry> paths = new Dictionary<string, Geometry>();
        static readonly Dictionary<string, Brush> brushes = new Dictionary<string, Brush>();
        static readonly Dictionary<string, Pen> pens = new Dictionary<string, Pen>();
        static Geometry Shape(string path)
        {
            Geometry shape;
            if (!paths.TryGetValue(path, out shape))
            {
                shape = Geometry.Parse(path);
                shape.Freeze();
                paths[path] = shape;
            }
            return shape;
        }
        static Brush B(string color)
        {
            Brush brush;
            if (!brushes.TryGetValue(color, out brush))
            {
                brush = Theme.B(color);
                brush.Freeze();
                brushes[color] = brush;
            }
            return brush;
        }
        static Pen P(string color, double width)
        {
            string key = color + width;
            Pen pen;
            if (!pens.TryGetValue(key, out pen))
            {
                pen = new Pen(B(color), width) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
                pen.Freeze();
                pens[key] = pen;
            }
            return pen;
        }
        void Path(DrawingContext d, string path, Brush brush, Pen pen)
        {
            d.DrawGeometry(brush, pen, Shape(path));
        }
        static string[] bodies = {
   "M 85,48 L 135,48 L 135,62 C 135,73 166,77 168,100 L 168,193 Q 168,219 144,222 L 76,222 Q 52,219 52,193 L 52,100 C 54,77 85,73 85,62 Z",
   "M 84,57 L 136,57 L 136,70 Q 164,78 167,101 L 161,195 Q 160,221 140,222 L 80,222 Q 60,221 59,195 L 53,101 Q 56,78 84,70 Z",
   "M 77,55 L 143,55 Q 160,57 160,78 L 160,202 Q 160,222 140,222 L 80,222 Q 60,222 60,202 L 60,78 Q 60,57 77,55 Z",
   "M 65,70 L 145,70 L 163,59 L 159,91 L 171,193 Q 173,220 147,222 L 73,222 Q 47,220 51,193 L 61,91 Z",
   "M 49,77 L 171,77 L 157,205 Q 155,222 139,222 L 81,222 Q 65,222 63,205 Z",
   "M 55,87 L 163,87 L 163,195 Q 163,222 139,222 L 79,222 Q 55,222 55,195 Z",
   "M 65,70 L 155,70 L 155,83 Q 169,90 169,105 L 169,202 Q 169,222 148,222 L 72,222 Q 51,222 51,202 L 51,105 Q 51,90 65,83 Z",
   "M 88,51 L 132,51 L 132,67 Q 157,77 160,98 L 153,125 L 160,154 L 160,201 Q 160,222 140,222 L 80,222 Q 60,222 60,201 L 60,154 L 67,125 L 60,98 Q 63,77 88,67 Z",
   "M 93,43 L 127,43 L 127,65 Q 148,76 148,95 L 148,202 Q 148,222 130,222 L 90,222 Q 72,222 72,202 L 72,95 Q 72,76 93,65 Z",
   "M 90,40 L 130,40 L 130,81 Q 159,91 159,116 L 159,202 Q 159,222 138,222 L 82,222 Q 61,222 61,202 L 61,116 Q 61,91 90,81 Z",
   "M 84,49 L 136,49 L 136,70 L 158,82 Q 168,85 168,96 L 168,207 Q 168,222 153,222 L 67,222 Q 52,222 52,207 L 52,96 Q 52,85 62,82 L 84,70 Z",
   "M 86,51 L 134,51 L 134,74 C 197,102 184,222 135,222 L 85,222 C 36,222 23,102 86,74 Z",
   "M 80,55 L 140,55 Q 160,56 160,79 L 160,202 Q 160,222 140,222 L 80,222 Q 60,222 60,202 L 60,79 Q 60,56 80,55 Z"
  };
        protected override void OnRender(DrawingContext d)
        {
            base.OnRender(d);
            int model = Math.Clamp(Vessel, 0, bodies.Length - 1);
            double level = Math.Max(0, Math.Min(1, Level));
            double scale = Math.Min(ActualWidth / 220, ActualHeight / 255);
            d.PushTransform(new TranslateTransform((ActualWidth - 220 * scale) / 2, (ActualHeight - 255 * scale) / 2));
            d.PushTransform(new ScaleTransform(scale, scale));
            double bob = Paused ? 0 : Math.Sin(Phase) * (level == 0 ? .6 : 2.3);
            d.DrawEllipse(B("#160B5056"), null, new Point(110, 238), 57 - bob, 7);
            d.PushTransform(new TranslateTransform(0, bob));
            if (level == 0 && !Paused)
                d.PushTransform(new RotateTransform(-7, 110, 215));
            var line = P("#234957", 3.3);
            IReadOnlyList<int> equipped = EquippedAccessories;
            if (equipped.Count == 0 && Accessory > 0)
                equipped = new[] { Accessory };
            foreach (int item in equipped)
                if (item is 17 or 18 or 19)
                    DrawAccessory(d, line, item);
            Path(d, "M 84,216 Q 77,233 69,231 L 57,231 M 137,216 Q 144,233 152,231 L 163,231", null, line);
            if (level < .15)
                Path(d, "M 57,129 Q 34,150 35,168 M 164,129 Q 185,150 183,168", null, line);
            else
                Path(d, "M 57,129 Q 35,128 31,151 M 163,129 Q 183,126 189,110", null, line);
            // Separate handles make each silhouette a genuinely different vessel.
            if (model == 3)
                Path(d, "M 161,88 C 217,79 221,169 166,177 L 164,160 C 198,152 195,107 163,107 Z", B("#E2F0F1"), line);
            if (model == 5)
                Path(d, "M 162,105 C 214,94 215,173 163,173 L 163,155 C 192,156 191,122 163,123 Z", B("#E2F0F1"), line);
            Geometry body = Shape(bodies[model]);
            d.DrawGeometry(B(level == 0 ? "#F2EEE6" : "#F2FAFB"), null, body);
            d.PushClip(body);
            Color color = (Color)ColorConverter.ConvertFromString(Tint);
            Color deep = Color.FromRgb((byte)(color.R * .70), (byte)(color.G * .76), (byte)(color.B * .83));
            if (level > 0)
            {
                double top = model == 0 ? 78 : model == 1 ? 82 : model == 2 ? 70 : model == 5 ? 94 : 84;
                double waterY = 224 - (224 - top) * level;
                var wave = new StreamGeometry();
                using (var c = wave.Open())
                {
                    c.BeginFigure(new Point(40, waterY), true, true);
                    c.BezierTo(new Point(77, waterY - 10 + Math.Sin(Phase) * 4), new Point(125, waterY + 8), new Point(180, waterY), true, false);
                    c.LineTo(new Point(180, 230), true, false);
                    c.LineTo(new Point(40, 230), true, false);
                }
                d.DrawGeometry(new LinearGradientBrush(color, deep, 90), null, wave);
                if (level > .3)
                {
                    d.DrawEllipse(B("#70FFFFFF"), null, new Point(76, 185), 3.5, 3.5);
                    d.DrawEllipse(B("#50FFFFFF"), null, new Point(143, 174), 2.5, 2.5);
                }
            }
            d.DrawRoundedRectangle(B("#A0FFFFFF"), null, new Rect(64, model == 5 ? 104 : 98, 6, 87), 3, 3);
            d.Pop();
            d.DrawGeometry(null, line, body);
            if (model == 0)
            {
                d.DrawRoundedRectangle(B("#B9DDD5"), line, new Rect(79, 28, 62, 24), 7, 7);
                for (int i = 0; i < 5; i++)
                    d.DrawLine(P("#78AAAB", 2), new Point(89 + i * 10, 34), new Point(89 + i * 10, 45));
            }
            if (model == 1)
            {
                d.DrawRoundedRectangle(B("#314D64"), line, new Rect(78, 40, 64, 20), 6, 6);
                d.DrawRoundedRectangle(B("#93CABD"), line, new Rect(98, 24, 25, 17), 4, 4);
                Path(d, "M 140,44 Q 164,30 157,14", null, P("#314D64", 5));
            }
            if (model == 2)
            {
                d.DrawRoundedRectangle(B("#314D64"), line, new Rect(72, 34, 76, 29), 8, 8);
                d.DrawLine(P("#AAC2CA", 2), new Point(81, 45), new Point(139, 45));
                d.DrawRoundedRectangle(B("#25536E"), null, new Rect(75, 197, 70, 14), 5, 5);
            }
            if (model == 3)
            {
                Path(d, "M 64,70 L 145,70 L 163,59", null, line);
                d.DrawLine(P("#D1E6E7", 3), new Point(82, 86), new Point(136, 86));
            }
            if (model == 4)
            {
                Path(d, "M 132,79 L 143,29 L 164,29", null, P("#E898A9", 7));
                d.DrawLine(P("#234957", 3), new Point(49, 77), new Point(171, 77));
            }
            if (model == 5)
                d.DrawLine(P("#A5BFC4", 3), new Point(66, 94), new Point(153, 94));
            if (model == 6)
            {
                d.DrawRoundedRectangle(B("#D2B284"), line, new Rect(60, 52, 100, 22), 5, 5);
                for (int i = 0; i < 6; i++)
                    d.DrawLine(P("#A98F6C", 1.5), new Point(69 + i * 16, 57), new Point(69 + i * 16, 68));
            }
            DrawBottleDetails(d, model, line);
            DrawFace(d);
            d.DrawEllipse(B("#85EE9DAA"), null, new Point(74, 141), 8, 4);
            d.DrawEllipse(B("#85EE9DAA"), null, new Point(146, 141), 8, 4);
            if (level > .18)
                Path(d, "M 110,177 C 110,177 101,187 101,192 C 101,203 119,203 119,192 C 119,187 110,177 110,177 Z", B("#AAFFFFFF"), null);
            foreach (int item in equipped.OrderBy(id => id == 8 ? 0 : 1))
                if (item is not (17 or 18 or 19))
                    DrawAccessory(d, line, item);
            if (level == 0 && !Paused)
                d.Pop();
            d.Pop();
            d.Pop();
            d.Pop();
        }
        void DrawBottleDetails(DrawingContext d, int model, Pen line)
        {
            switch (model)
            {
                case 7:
                    d.DrawRoundedRectangle(B("#355B68"), line, new Rect(82, 35, 56, 19), 5, 5);
                    d.DrawRoundedRectangle(B("#B8D9D1"), line, new Rect(102, 20, 16, 15), 3, 3);
                    Path(d, "M 66,111 Q 80,120 79,136 M 154,111 Q 140,120 141,136", null, P("#709EA7", 2));
                    break;
                case 8:
                    d.DrawRoundedRectangle(B("#D7B499"), line, new Rect(88, 24, 44, 23), 6, 6);
                    d.DrawLine(P("#F6D8C2", 2), new Point(96, 32), new Point(124, 32));
                    break;
                case 9:
                    d.DrawRoundedRectangle(B("#B66F75"), line, new Rect(86, 26, 48, 17), 4, 4);
                    d.DrawLine(P("#A0C6C6", 3), new Point(91, 65), new Point(129, 65));
                    Path(d, "M 69,181 Q 110,192 151,181", null, P("#A0FFFFFF", 3));
                    break;
                case 10:
                    d.DrawRoundedRectangle(B("#4F6A8E"), line, new Rect(80, 30, 60, 23), 4, 4);
                    Path(d, "M 151,102 L 151,202 Q 151,208 145,208", null, P("#60FFFFFF", 4));
                    break;
                case 11:
                    d.DrawRoundedRectangle(B("#D5B079"), line, new Rect(80, 30, 60, 25), 8, 8);
                    d.DrawLine(P("#F4DEC0", 3), new Point(89, 40), new Point(131, 40));
                    break;
                case 12:
                    Path(d, "M 140,50 C 182,46 185,91 158,99 M 141,61 C 166,60 168,80 158,85", null, P("#4B6C82", 5));
                    d.DrawRoundedRectangle(B("#4B6C82"), line, new Rect(75, 34, 70, 25), 7, 7);
                    d.DrawRoundedRectangle(B("#B8D3DE"), null, new Rect(84, 41, 51, 8), 4, 4);
                    break;
            }
        }

        void DrawFace(DrawingContext d)
        {
            var mood = Mood;
            var ink = P("#234957", 3);
            bool blink = Math.Sin(Phase * .57) > .993;
            if (Paused)
            {
                Path(d, "M 80,125 Q 87,130 94,125 M 126,125 Q 133,130 140,125", null, ink);
                d.DrawEllipse(null, ink, new Point(110, 148), 4, 5);
                return;
            }
            if (mood == HydrationMood.Dead)
            {
                Path(d, "M 81,121 L 93,133 M 93,121 L 81,133 M 127,121 L 139,133 M 139,121 L 127,133", null, ink);
                d.DrawEllipse(B("#46525A"), null, new Point(110, 150), 8, 6);
                Path(d, "M 109,152 L 121,152 Q 127,162 117,164 Q 110,164 109,152 Z", B("#EA9CA7"), P("#A95D71", 1.5));
                return;
            }
            if (mood == HydrationMood.Happy)
            {
                Path(d, "M 80,128 Q 87,114 94,128 M 126,128 Q 133,114 140,128", null, ink);
                Path(d, "M 98,143 Q 110,167 122,143 Z", B("#234957"), ink);
                d.DrawEllipse(B("#EAA4AE"), null, new Point(110, 152), 6, 3);
                return;
            }
            if (mood == HydrationMood.Crying)
            {
                Path(d, "M 79,128 Q 87,119 95,128 M 125,128 Q 133,119 141,128 M 79,113 Q 88,118 96,110 M 124,110 Q 132,118 141,113", null, ink);
                d.DrawRoundedRectangle(B("#8DBBDDFA"), null, new Rect(82, 129, 10, 32), 4, 4);
                d.DrawRoundedRectangle(B("#8DBBDDFA"), null, new Rect(128, 129, 10, 32), 4, 4);
                double drop = (Math.Sin(Phase * 1.5) + 1) * 4;
                d.DrawEllipse(B("#77BFE7"), null, new Point(87, 166 + drop), 4, 5);
                d.DrawEllipse(B("#77BFE7"), null, new Point(133, 171 - drop), 4, 5);
                Path(d, "M 101,155 Q 110,139 119,155 Z", B("#46525A"), ink);
                return;
            }
            if (blink)
                Path(d, "M 80,125 Q 87,130 94,125 M 126,125 Q 133,130 140,125", null, ink);
            else
            {
                double eyeHeight = mood == HydrationMood.Content ? 6 : mood == HydrationMood.Neutral ? 5 : 4;
                d.DrawEllipse(B("#234957"), null, new Point(87, 126), 5, eyeHeight);
                d.DrawEllipse(B("#234957"), null, new Point(133, 126), 5, eyeHeight);
                d.DrawEllipse(Brushes.White, null, new Point(88, 124), 1.4, 1.5);
                d.DrawEllipse(Brushes.White, null, new Point(134, 124), 1.4, 1.5);
            }
            if (mood == HydrationMood.Content)
                Path(d, "M 100,145 Q 110,154 120,145", null, ink);
            else if (mood == HydrationMood.Neutral)
                d.DrawLine(ink, new Point(101, 147), new Point(119, 147));
            else
            {
                Path(d, "M 101,153 Q 110,141 119,153 M 79,116 L 95,111 M 125,111 L 141,116", null, ink);
            }
        }

        void DrawAccessory(DrawingContext d, Pen line, int accessory)
        {
            switch (accessory)
            {
                case 1:
                    Path(d, "M 62,112 C 58,60 161,60 158,112", null, P("#8264BC", 9));
                    d.DrawRoundedRectangle(B("#B59BDC"), line, new Rect(47, 109, 18, 33), 8, 8);
                    d.DrawRoundedRectangle(B("#B59BDC"), line, new Rect(156, 109, 18, 33), 8, 8);
                    break;
                case 2:
                    d.DrawRoundedRectangle(B("#E0294155"), line, new Rect(73, 115, 30, 21), 7, 7);
                    d.DrawRoundedRectangle(B("#E0294155"), line, new Rect(118, 115, 30, 21), 7, 7);
                    d.DrawLine(line, new Point(103, 122), new Point(118, 122));
                    d.DrawLine(P("#80FFFFFF", 2), new Point(80, 120), new Point(86, 125));
                    d.DrawLine(P("#80FFFFFF", 2), new Point(125, 120), new Point(131, 125));
                    break;
                case 3:
                    Path(d, "M 110,70 Q 76,39 75,64 Q 75,88 110,70 Z M 110,70 Q 143,39 145,64 Q 145,88 110,70 Z", B("#EB91AD"), P("#9D5C78", 2));
                    d.DrawEllipse(B("#F8C5D5"), P("#9D5C78", 2), new Point(110, 69), 7, 7);
                    break;
                case 4:
                    for (int i = 0; i < 5; i++)
                    {
                        double a = i * Math.PI * 2 / 5;
                        d.DrawEllipse(B("#F2ABCB"), P("#B46E94", 1.5), new Point(145 + Math.Sin(a) * 10, 88 + Math.Cos(a) * 10), 8, 8);
                    }
                    d.DrawEllipse(B("#EFC762"), null, new Point(145, 88), 6, 6);
                    break;
                case 5:
                    Path(d, "M 62,89 Q 66,49 110,49 Q 155,49 158,89 Z", B("#477DB2"), line);
                    d.DrawRoundedRectangle(B("#274D77"), line, new Rect(102, 83, 78, 12), 6, 6);
                    d.DrawEllipse(B("#B4D8EE"), null, new Point(109, 68), 7, 7);
                    break;
                case 6:
                    Path(d, "M 110,165 L 85,153 Q 80,165 85,177 Z M 110,165 L 135,153 Q 140,165 135,177 Z", B("#BE647C"), P("#754A60", 2));
                    d.DrawEllipse(B("#EBA8B8"), null, new Point(110, 165), 6, 6);
                    break;
                case 7:
                    Path(d, "M 110,146 C 98,132 94,159 82,146 C 86,165 102,163 110,154 C 118,163 134,165 138,146 C 126,159 122,132 110,146 Z", B("#624C43"), null);
                    break;
                case 8:
                    Path(d, "M 82,121 L 77,116 M 87,119 L 85,112 M 92,121 L 94,114 M 128,121 L 125,114 M 133,119 L 134,112 M 138,121 L 142,116", null, P("#234957", 2.3));
                    break;
                case 9:
                    Path(d, "M 75,70 L 67,37 L 93,49 L 110,23 L 128,49 L 153,37 L 145,70 Z", B("#EFC65B"), P("#9D7A33", 2.5));
                    d.DrawEllipse(B("#D47893"), null, new Point(110, 54), 5, 7);
                    break;
                case 10:
                    Path(d, "M 58,92 Q 109,71 165,92 L 165,104 Q 109,87 58,104 Z", B("#BE6D72"), P("#824B57", 2));
                    Path(d, "M 155,100 L 177,112 L 172,88 Z", B("#BE6D72"), null);
                    break;
                case 11:
                    Path(d, "M 72,163 Q 110,197 148,163", null, P("#9DA6B3", 2));
                    for (int i = 0; i < 9; i++)
                    {
                        double x = 75 + i * 8.7;
                        double y = 166 + Math.Sin(i * Math.PI / 8) * 17;
                        d.DrawEllipse(B("#FFF9E9"), P("#BEC5CE", 1), new Point(x, y), 4.2, 4.2);
                    }
                    break;
                case 12:
                    d.DrawRoundedRectangle(B("#71606E"), line, new Rect(78, 30, 65, 51), 7, 7);
                    d.DrawRectangle(B("#CDA7A1"), null, new Rect(78, 65, 65, 12));
                    d.DrawRoundedRectangle(B("#71606E"), line, new Rect(59, 77, 103, 11), 5, 5);
                    break;
                case 13:
                    Path(d, "M 62,101 Q 108,61 158,101 M 68,85 L 68,48 L 94,70 M 129,70 L 153,48 L 153,85", null, P("#785C78", 6));
                    Path(d, "M 74,76 L 74,59 L 85,70 M 138,70 L 147,59 L 147,76", null, P("#E9A5BB", 4));
                    break;
                case 14:
                    Path(d, "M 104,158 L 116,158 L 120,166 L 114,173 L 122,199 L 110,210 L 98,199 L 106,173 L 100,166 Z", B("#687FB5"), P("#415274", 2));
                    break;
                case 15:
                    d.DrawEllipse(B("#E2B95E"), P("#A5813D", 2), new Point(59, 143), 7, 10);
                    d.DrawEllipse(B("#E2B95E"), P("#A5813D", 2), new Point(161, 143), 7, 10);
                    break;
                case 16:
                    Path(d, "M 64,165 Q 110,183 157,165 L 158,178 Q 112,199 62,178 Z M 132,181 L 145,183 L 143,218 L 128,214 Z", B("#C1848D"), P("#8E5A6F", 2));
                    d.DrawLine(P("#E9C0C8", 2), new Point(71, 173), new Point(116, 181));
                    break;
                case 17:
                    Path(d, "M 62,126 C 22,76 -6,89 12,113 C -7,111 3,138 19,141 C -2,147 19,174 57,167 Z M 159,126 C 199,76 227,89 209,113 C 228,111 218,138 202,141 C 223,147 202,174 164,167 Z", B("#F5F7FE"), P("#A4B4C9", 2.5));
                    break;
                case 18:
                    Path(d, "M 61,130 L 20,94 L 10,156 L 32,146 L 43,168 L 59,158 Z M 160,130 L 201,94 L 211,156 L 189,146 L 178,168 L 162,158 Z", B("#8C79A6"), P("#594C73", 2.5));
                    break;
                case 19:
                    d.DrawRoundedRectangle(B("#D19C66"), P("#916B49", 3), new Rect(40, 106, 140, 100), 18, 18);
                    Path(d, "M 82,109 Q 110,70 139,109", null, P("#916B49", 5));
                    break;
                case 20:
                    d.DrawEllipse(null, P("#E2BF62", 6), new Point(110, 22), 38, 9);
                    break;
                case 21:
                    d.DrawEllipse(B("#15FFFFFF"), P("#855F5A", 2.5), new Point(87, 126), 17, 13);
                    d.DrawEllipse(B("#15FFFFFF"), P("#855F5A", 2.5), new Point(133, 126), 17, 13);
                    Path(d, "M 104,124 Q 110,120 116,124 M 70,121 L 60,118 M 150,121 L 160,118", null, P("#855F5A", 2.5));
                    break;
                case 22:
                    d.DrawLine(P("#3A495B", 3), new Point(61, 107), new Point(157, 145));
                    d.DrawEllipse(B("#3A495B"), line, new Point(87, 126), 14, 12);
                    break;
                case 23:
                    Path(d, "M 146,76 L 150,86 L 161,86 L 152,93 L 156,104 L 146,97 L 136,104 L 140,93 L 131,86 L 142,86 Z", B("#EDCD76"), P("#B7974D", 2));
                    break;
                case 24:
                    d.DrawRoundedRectangle(B("#F4F5FC"), P("#9AAFC4", 2), new Rect(49, 117, 12, 27), 5, 5);
                    d.DrawRoundedRectangle(B("#F4F5FC"), P("#9AAFC4", 2), new Rect(159, 117, 12, 27), 5, 5);
                    break;
                case 25:
                    d.DrawEllipse(B("#B5657E"), P("#82445E", 2.5), new Point(107, 66), 49, 20);
                    d.DrawRoundedRectangle(B("#82445E"), null, new Rect(76, 78, 71, 7), 3, 3);
                    d.DrawLine(P("#82445E", 4), new Point(110, 46), new Point(115, 39));
                    break;
                case 26:
                    Path(d, "M 173,164 C 169,134 201,133 202,164", null, P("#915E7B", 4));
                    Path(d, "M 167,159 L 207,159 L 210,194 Q 188,202 165,194 Z", B("#D493B4"), P("#915E7B", 2.5));
                    d.DrawRoundedRectangle(B("#E7C681"), P("#AD894F", 1), new Rect(182, 169, 10, 7), 2, 2);
                    Path(d, "M 171,164 Q 187,181 203,164", null, P("#EFC9DE", 2));
                    break;
                case 27:
                    Path(d, "M 64,180 Q 110,194 157,180 L 177,215 Q 110,237 44,215 Z", B("#D996B2"), P("#9F617D", 2.5));
                    Path(d, "M 64,180 Q 110,194 157,180", null, P("#9F617D", 6));
                    Path(d, "M 82,188 L 75,222 M 109,192 L 109,225 M 137,188 L 146,222", null, P("#B66F90", 2));
                    break;
                case 28:
                    Path(d, "M 51,225 L 65,225 L 71,235 L 81,237 Q 83,242 77,242 L 56,242 L 52,234 L 49,245 L 45,245 L 47,230 Z", B("#B45F83"), P("#7D3E60", 2));
                    Path(d, "M 169,225 L 155,225 L 149,235 L 139,237 Q 137,242 143,242 L 164,242 L 168,234 L 171,245 L 175,245 L 173,230 Z", B("#B45F83"), P("#7D3E60", 2));
                    break;
                case 29:
                    Path(d, "M 79,156 L 95,156 Q 110,173 125,156 L 142,156 L 134,185 L 170,218 Q 110,236 50,218 L 86,185 Z", B("#A997D4"), P("#756295", 2.5));
                    Path(d, "M 87,185 Q 110,191 134,185", null, P("#E8C4DB", 5));
                    d.DrawEllipse(B("#EDCF91"), null, new Point(111, 188), 4, 4);
                    break;
                case 30:
                    Path(d, "M 98,147 Q 105,138 110,145 Q 115,138 122,147 Q 111,161 98,147 Z", B("#C86485"), P("#95435F", 1.5));
                    Path(d, "M 99,148 Q 110,152 121,148", null, P("#F3BACB", 1.5));
                    break;
                case 31:
                    Path(d, "M 61,100 Q 110,58 159,100", null, P("#779D86", 5));
                    for (int i = 0; i < 5; i++)
                    {
                        double x = 76 + i * 17, y = 81 - Math.Sin(i * Math.PI / 4) * 9;
                        for (int petal = 0; petal < 5; petal++)
                        {
                            double angle = petal * Math.PI * 2 / 5;
                            d.DrawEllipse(B(i % 2 == 0 ? "#E9B3CD" : "#F4E7C3"), null, new Point(x + Math.Sin(angle) * 5, y + Math.Cos(angle) * 5), 4, 4);
                        }
                        d.DrawEllipse(B("#D8B663"), null, new Point(x, y), 3, 3);
                    }
                    break;
                case 32:
                    d.DrawEllipse(null, P("#D7B366", 4), new Point(35, 148), 8, 5);
                    d.DrawEllipse(null, P("#C18CAB", 3), new Point(37, 142), 8, 5);
                    d.DrawEllipse(null, P("#D7B366", 4), new Point(183, 118), 8, 5);
                    d.DrawEllipse(null, P("#C18CAB", 3), new Point(181, 124), 8, 5);
                    break;
            }
        }
    }
}
