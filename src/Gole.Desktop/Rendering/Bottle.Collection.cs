using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    void DrawCollectionBottleBack(DrawingContext d, int model, Pen line)
    {
        if (model == 24)
            Path(d, "M 144,30 C 179,22 179,62 147,59 L 147,49 C 164,50 165,36 146,40 Z", B("#B5A4CC"), line);
        if (model == 28)
        {
            Path(d, "M 174,104 C 220,87 218,160 166,171 L 170,156 C 199,143 202,113 173,119 Z", B("#F0D6CB"), line);
            d.DrawEllipse(B("#E5C3B4"), P("#AA8277", 2), new Point(110, 219), 64, 8);
            Path(d, "M 73,217 Q 110,226 147,217", null, P("#F8ECE2", 2));
        }
    }

    void DrawCollectionBottleDetails(DrawingContext d, int model, Pen line)
    {
        DrawNewVesselDetails(d, model, line);
        switch (model)
        {
            case 21:
                d.DrawRoundedRectangle(B("#D79A79"), line, new Rect(76, 33, 68, 27), 10, 10);
                Path(d, "M 90,33 Q 110,8 130,33 M 87,43 L 133,43", null, P("#A76C58", 4));
                Path(d, "M 52,177 Q 54,208 80,213 M 140,213 Q 166,208 168,177", null, P("#90FFFFFF", 4));
                break;
            case 22:
                d.DrawRoundedRectangle(B("#BBA1D4"), line, new Rect(83, 29, 54, 24), 5, 5);
                Path(d, "M 96,57 L 96,84 M 75,155 L 85,155 M 69,172 L 83,172 M 63,189 L 80,189 M 60,205 L 84,205", null, P("#93BAC6", 2.5));
                d.DrawEllipse(null, P("#80FFFFFF", 2), new Point(140, 181), 6, 6);
                d.DrawEllipse(null, P("#80FFFFFF", 2), new Point(130, 200), 3, 3);
                break;
            case 23:
                Path(d, "M 84,54 L 84,34 L 96,23 L 124,23 L 136,34 L 136,54 Z", B("#B6C7E2"), line);
                Path(d, "M 84,35 L 136,35 M 96,24 L 98,53 M 124,24 L 122,53", null, P("#EEF1FF", 2));
                Path(d, "M 52,99 L 76,105 L 76,205 L 52,197 M 168,99 L 144,105 L 144,205 L 168,197 M 76,205 L 110,216 L 144,205", null, P("#70FFFFFF", 3));
                break;
            case 24:
                d.DrawRoundedRectangle(B("#B5A4CC"), line, new Rect(70, 24, 80, 41), 9, 9);
                Path(d, "M 80,34 L 140,34 M 72,55 L 148,55", null, P("#E3DAED", 3));
                d.DrawRoundedRectangle(B("#9283AC"), null, new Rect(63, 205, 94, 12), 5, 5);
                Path(d, "M 149,82 L 149,185", null, P("#99FFFFFF", 4));
                break;
            case 25:
                d.DrawRoundedRectangle(B("#D28DAB"), line, new Rect(84, 31, 52, 27), 7, 7);
                DrawHeart(d, 110, 27, 9, "#F0BED0", "#9C6689");
                Path(d, "M 57,103 Q 56,85 75,91 M 152,170 Q 141,191 125,201", null, P("#AAFFFFFF", 4));
                break;
            case 26:
                Path(d, "M 130,83 L 141,29 L 161,29", null, P("#9B88BE", 8));
                Path(d, "M 130,83 L 141,29 L 161,29", null, P("#D7C9EB", 3));
                d.DrawEllipse(B("#BBDACB"), line, new Point(110, 84), 56, 8);
                Path(d, "M 65,90 Q 110,97 155,90", null, P("#E9F7E4", 2));
                foreach (double x in new[] { 72.0, 91, 112, 133, 151 })
                    d.DrawEllipse(B("#79849F"), P("#53617E", 1), new Point(x, 204 + (x % 3) * 3), 5, 5);
                break;
            case 27:
                d.DrawRoundedRectangle(B("#C1B079"), line, new Rect(70, 30, 80, 29), 7, 7);
                Path(d, "M 81,36 L 81,53 M 95,34 L 95,54 M 125,34 L 125,54 M 139,36 L 139,53", null, P("#E5D7A6", 2));
                Path(d, "M 63,91 Q 110,99 157,91 M 63,194 Q 110,202 157,194", null, P("#C6B580", 6));
                Path(d, "M 70,102 L 70,187 M 150,101 L 150,186", null, P("#80D5CF9D", 3));
                Path(d, "M 110,43 Q 98,28 105,24 Q 119,27 110,43 M 111,44 Q 119,30 127,33 Q 126,44 111,44", B("#84A878"), null);
                break;
            case 28:
                Path(d, "M 45,95 Q 110,111 175,95 M 50,104 Q 110,119 170,104", null, P("#E2B8A3", 3));
                Path(d, "M 86,210 Q 110,219 134,210", null, P("#E2B8A3", 3));
                break;
        }
    }

    void DrawCollectionAccessory(DrawingContext d, int item)
    {
        switch (item)
        {
            case 67:
                Path(d, "M 53,106 Q 49,64 94,63 Q 144,49 164,87 L 163,141 L 149,127 L 145,104 Q 121,114 99,106 L 116,91 Q 82,109 60,112 L 64,94 Z", B("#363344"), P("#242334", 2.3));
                Path(d, "M 143,77 Q 125,94 103,100 M 151,85 Q 142,103 151,119", null, P("#A77DAD", 4));
                break;
            case 68:
                d.DrawRoundedRectangle(B("#22B4DDE5"), P("#4D5271", 3.4), new Rect(65, 109, 39, 33), 11, 11);
                d.DrawRoundedRectangle(B("#22B4DDE5"), P("#4D5271", 3.4), new Rect(116, 109, 39, 33), 11, 11);
                Path(d, "M 104,123 Q 110,118 116,123 M 65,119 L 57,115 M 155,119 L 163,115", null, P("#4D5271", 3));
                d.DrawRoundedRectangle(B("#EADBB9"), P("#C0A679", 1), new Rect(105, 117, 10, 9), 1, 1);
                Path(d, "M 108,118 L 108,125 M 112,118 L 112,125", null, P("#FFF2D5", 1));
                break;
            case 69:
                Path(d, "M 10,133 Q 22,128 34,136 Q 46,128 58,133 L 58,166 Q 46,161 34,169 Q 22,161 10,166 Z", B("#FFF1D7"), P("#78678F", 3));
                Path(d, "M 34,136 L 34,168 M 16,140 L 27,142 M 16,147 L 27,149 M 41,141 L 52,139 M 41,148 L 52,146", null, P("#B4A5AD", 1.5));
                Path(d, "M 45,131 L 45,145 L 49,141 L 53,143 L 53,131", B("#C990AB"), null);
                break;
            case 70:
                Path(d, "M 58,119 C 54,57 166,57 162,119", null, P("#7F6D9D", 10));
                Path(d, "M 65,83 L 66,51 L 92,69 M 128,69 L 154,51 L 155,83", B("#B9A2D1"), P("#7F6D9D", 3));
                Path(d, "M 72,70 L 72,61 L 82,69 M 138,69 L 148,61 L 148,71", null, P("#F1C5DC", 4));
                d.DrawRoundedRectangle(B("#B9A2D1"), P("#7F6D9D", 2), new Rect(46, 110, 17, 38), 7, 7);
                d.DrawRoundedRectangle(B("#B9A2D1"), P("#7F6D9D", 2), new Rect(157, 110, 17, 38), 7, 7);
                DrawHeart(d, 55, 129, 5, "#F1C5DC", "#D7A3C0");
                DrawHeart(d, 165, 129, 5, "#F1C5DC", "#D7A3C0");
                break;
            case 71:
                Path(d, "M 60,166 Q 110,184 160,166", null, P("#3D394C", 7));
                Path(d, "M 112,178 C 97,177 96,195 111,198 C 119,199 124,194 124,189 C 114,194 105,186 112,178 Z", B("#D9CDE7"), P("#93839F", 1.4));
                break;
            case 72:
                Path(d, "M 61,163 Q 110,194 159,163", null, P("#BC9C66", 2.2));
                DrawStar(d, 88, 178, 5, "#F1D990", "#BC9C66");
                DrawStar(d, 110, 184, 7, "#F1D990", "#BC9C66");
                DrawStar(d, 132, 178, 5, "#F1D990", "#BC9C66");
                break;
            case 73:
                d.DrawEllipse(B("#93B989"), P("#587C64", 2.5), new Point(79, 45), 14, 16);
                d.DrawEllipse(B("#93B989"), P("#587C64", 2.5), new Point(141, 45), 14, 16);
                Path(d, "M 60,87 Q 60,49 110,49 Q 160,49 160,87 Z", B("#A6CA91"), P("#587C64", 2.5));
                d.DrawEllipse(B("#2E4B42"), null, new Point(79, 42), 4, 5);
                d.DrawEllipse(B("#2E4B42"), null, new Point(141, 42), 4, 5);
                Path(d, "M 101,66 Q 110,74 119,66", null, P("#587C64", 2));
                d.DrawRoundedRectangle(B("#83AA78"), P("#587C64", 2), new Rect(57, 80, 106, 14), 5, 5);
                break;
            case 74:
                Path(d, "M 72,80 L 105,8 Q 119,38 146,78 Z", B("#6677AF"), P("#414E80", 2.5));
                d.DrawEllipse(B("#6677AF"), P("#414E80", 2.5), new Point(110, 85), 64, 10);
                Path(d, "M 77,70 Q 110,78 140,69", null, P("#B9A1CA", 7));
                DrawStar(d, 104, 48, 7, "#F0D69A", "#D2B67B");
                DrawStar(d, 121, 62, 4, "#F0D69A", "#D2B67B");
                break;
            case 75:
                Path(d, "M 61,99 Q 110,60 159,99", null, P("#8CA5C5", 5));
                Path(d, "M 89,79 C 74,79 74,59 89,59 C 93,43 113,44 118,57 C 135,50 144,79 128,79 Z", B("#F1F4FC"), P("#8CA5C5", 2));
                d.DrawEllipse(B("#8CA5C5"), null, new Point(98, 65), 1.7, 2);
                d.DrawEllipse(B("#8CA5C5"), null, new Point(115, 65), 1.7, 2);
                Path(d, "M 103,70 Q 107,74 111,70", null, P("#8CA5C5", 1.3));
                break;
            case 76:
                Path(d, "M 150,91 Q 151,74 162,69 Q 160,85 170,96", null, P("#648762", 2.5));
                Path(d, "M 162,70 Q 145,60 144,72 Q 153,78 162,70", B("#93B889"), P("#648762", 1));
                d.DrawEllipse(B("#D57A90"), P("#9F526F", 2), new Point(149, 96), 8, 8);
                d.DrawEllipse(B("#D57A90"), P("#9F526F", 2), new Point(171, 99), 8, 8);
                d.DrawEllipse(B("#F6C7D2"), null, new Point(147, 93), 2, 2);
                break;
            case 77:
                d.DrawRoundedRectangle(B("#BEA0BA"), P("#8C6E8C", 3), new Rect(35, 110, 150, 89), 20, 20);
                Path(d, "M 37,134 L 38,93 L 63,112 M 157,112 L 182,93 L 183,134", B("#BEA0BA"), P("#8C6E8C", 3));
                d.DrawRoundedRectangle(B("#E4CADD"), P("#8C6E8C", 2), new Rect(34, 153, 23, 31), 6, 6);
                d.DrawRoundedRectangle(B("#E4CADD"), P("#8C6E8C", 2), new Rect(163, 153, 23, 31), 6, 6);
                DrawHeart(d, 45, 165, 5, "#AA7A9D", "#AA7A9D");
                DrawHeart(d, 175, 165, 5, "#AA7A9D", "#AA7A9D");
                break;
            case 78:
                Path(d, "M 177,138 Q 174,104 188,110 Q 201,113 202,138", null, P("#8B6B8C", 3));
                d.DrawRoundedRectangle(B("#9B82AF"), P("#665379", 2.5), new Rect(168, 134, 42, 35), 8, 8);
                Path(d, "M 179,144 L 179,135 L 185,135 L 185,144 Q 192,152 185,160 L 178,160 Q 172,152 179,144 Z", B("#A5D3CD"), P("#547C86", 1.5));
                d.DrawRoundedRectangle(B("#D5B98D"), null, new Rect(177, 132, 10, 5), 1, 1);
                DrawStar(d, 199, 149, 5, "#EAD6A4", "#BA9A66");
                break;
            case 79:
                Path(d, "M 47,215 L 66,215 L 68,229 L 82,234 L 82,242 L 45,242 Z M 173,215 L 154,215 L 152,229 L 138,234 L 138,242 L 175,242 Z", B("#C78DAF"), P("#865C88", 2));
                Path(d, "M 51,223 L 62,223 M 158,223 L 169,223 M 45,240 L 82,240 M 138,240 L 175,240", null, P("#F1D4D9", 3));
                foreach (double x in new[] { 52.0, 75, 145, 168 })
                    d.DrawEllipse(B("#E7C889"), P("#9B8057", 1.8), new Point(x, 248), 5, 5);
                break;
            case 80:
                foreach (double x in new[] { 62.0, 158 })
                {
                    d.DrawEllipse(B("#F2D6E2"), P("#B185A2", 1.5), new Point(x - 6, 221), 4, 11);
                    d.DrawEllipse(B("#F2D6E2"), P("#B185A2", 1.5), new Point(x + 6, 221), 4, 11);
                    d.DrawEllipse(B("#F2D6E2"), P("#B185A2", 2), new Point(x, 237), 20, 10);
                    d.DrawEllipse(B("#695575"), null, new Point(x - 5, 235), 1.5, 2);
                    d.DrawEllipse(B("#695575"), null, new Point(x + 5, 235), 1.5, 2);
                    d.DrawEllipse(B("#CF8FAA"), null, new Point(x, 240), 2, 1.5);
                }
                break;
            case 81:
                d.DrawEllipse(B("#9BA2D0"), P("#676F9D", 1.6), new Point(140, 175), 8, 8);
                d.PushTransform(new RotateTransform(-22, 140, 175));
                d.DrawEllipse(null, P("#DEC48C", 2.5), new Point(140, 175), 14, 4);
                d.Pop();
                break;
            case 82:
                Path(d, "M 189,110 Q 205,85 198,68", null, P("#8D87AD", 1.6));
                DrawStar(d, 198, 43, 23, "#D2BDE8", "#9781B5");
                Path(d, "M 194,66 L 201,66 L 199,72 Z", B("#B49ACF"), null);
                Path(d, "M 188,39 L 195,37 L 197,29", null, P("#F3E9FC", 3));
                break;
        }
    }
}
