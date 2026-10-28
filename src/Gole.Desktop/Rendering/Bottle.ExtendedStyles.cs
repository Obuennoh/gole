using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    private static bool IsBackgroundAccessory(int id) => id is 17 or 18 or 19 or 45 or 46;

    private void DrawExtendedBottleBack(DrawingContext d, int model, Pen line)
    {
        if (model == 15)
            Path(d, "M 83,58 C 21,74 17,194 50,219 M 137,58 C 199,74 203,194 170,219", null, P("#B39873", 8));
        if (model == 17)
        {
            Path(d, "M 143,64 C 194,43 205,108 177,123 L 174,107 C 188,96 182,69 149,80 Z", B("#B5CFD8"), line);
            Path(d, "M 70,57 Q 110,9 151,57", null, P("#3F6682", 9));
        }
    }

    private void DrawExtendedBottleDetails(DrawingContext d, int model, Pen line)
    {
        switch (model)
        {
            case 13:
                Path(d, "M 116,59 L 129,20 L 148,20", null, P("#D884AC", 8));
                Path(d, "M 116,59 L 129,20 L 148,20", null, P("#F8C9DF", 3));
                d.DrawRoundedRectangle(B("#B2D3C9"), line, new Rect(78, 48, 64, 18), 6, 6);
                Path(d, "M 67,205 Q 110,218 154,205", null, P("#6F9E9B", 3));
                break;
            case 14:
                d.DrawRoundedRectangle(B("#7774A8"), line, new Rect(77, 35, 66, 23), 6, 6);
                Path(d, "M 66,103 Q 110,114 154,103 M 67,145 Q 110,157 153,145 M 66,175 Q 110,187 154,175 M 69,199 Q 110,208 151,199", null, P("#759CA6", 3));
                Path(d, "M 70,98 Q 110,108 150,98 M 71,171 Q 110,180 149,171", null, P("#90FFFFFF", 2));
                break;
            case 15:
                d.DrawRoundedRectangle(B("#81735F"), line, new Rect(82, 35, 56, 23), 6, 6);
                d.DrawRoundedRectangle(B("#C3B28D"), null, new Rect(90, 41, 40, 6), 3, 3);
                Path(d, "M 56,172 Q 64,212 110,214 Q 156,212 164,172", null, P("#7C9F98", 4));
                Path(d, "M 87,87 Q 110,80 133,87", null, P("#90FFFFFF", 3));
                break;
            case 16:
                d.DrawRoundedRectangle(B("#D4A56B"), line, new Rect(89, 24, 42, 20), 5, 5);
                Path(d, "M 96,61 L 124,61 M 96,69 L 124,69", null, P("#94BFC0", 2));
                Path(d, "M 66,201 Q 110,216 154,201", null, P("#85BCD0CD", 4));
                break;
            case 17:
                d.DrawRoundedRectangle(B("#3F6682"), line, new Rect(70, 37, 80, 25), 7, 7);
                d.DrawRoundedRectangle(B("#A9CCCA"), null, new Rect(83, 43, 54, 7), 3, 3);
                Path(d, "M 55,92 L 66,92 M 54,106 L 63,106 M 55,163 L 66,163 M 55,178 L 69,178 M 55,193 L 66,193", null, P("#77A6AE", 2));
                d.DrawRoundedRectangle(B("#3F6682"), null, new Rect(56, 205, 108, 10), 5, 5);
                break;
            case 18:
                d.DrawRoundedRectangle(B("#C59D88"), line, new Rect(79, 30, 62, 25), 7, 7);
                Path(d, "M 137,42 Q 167,41 174,76", null, P("#9E776A", 4));
                d.DrawRoundedRectangle(B("#755E779C"), P("#65758F", 2), new Rect(58, 163, 104, 52), 9, 9);
                Path(d, "M 66,169 L 154,206 M 154,169 L 66,206", null, P("#9DB1C1", 2));
                d.DrawRoundedRectangle(B("#E0C5A1"), null, new Rect(99, 179, 23, 17), 4, 4);
                break;
            case 19:
                d.DrawRoundedRectangle(B("#8D99BC"), line, new Rect(85, 33, 50, 24), 6, 6);
                Path(d, "M 56,203 Q 110,216 164,203", null, P("#71A1B0", 3));
                Path(d, "M 79,116 L 61,189", null, P("#90FFFFFF", 4));
                break;
            case 20:
                d.DrawRoundedRectangle(B("#85B7AB"), line, new Rect(88, 30, 44, 24), 9, 9);
                Path(d, "M 94,40 Q 110,33 126,40", null, P("#D7EBDE", 2));
                Path(d, "M 60,166 Q 55,198 83,211 M 137,211 Q 165,198 160,166", null, P("#88FFFFFF", 4));
                break;
        }
    }

    private void DrawStar(DrawingContext d, double x, double y, double radius, string fill, string outline)
    {
        d.PushTransform(new TranslateTransform(x, y));
        d.PushTransform(new ScaleTransform(radius / 10, radius / 10));
        Path(d, "M 0,-10 L 3,-3 L 10,-3 L 5,2 L 7,10 L 0,6 L -7,10 L -5,2 L -10,-3 L -3,-3 Z", B(fill), P(outline, 1.5));
        d.Pop();
        d.Pop();
    }

    private void DrawHeart(DrawingContext d, double x, double y, double radius, string fill, string outline)
    {
        d.PushTransform(new TranslateTransform(x, y));
        d.PushTransform(new ScaleTransform(radius / 10, radius / 10));
        Path(d, "M 0,9 C -18,-2 -9,-15 0,-6 C 9,-15 18,-2 0,9 Z", B(fill), P(outline, 1.7));
        d.Pop();
        d.Pop();
    }

    private void DrawExtendedAccessory(DrawingContext d, Pen line, int accessory)
    {
        switch (accessory)
        {
            case 33:
                Path(d, "M 75,77 L 101,14 Q 116,46 144,77 Z", B("#74628D"), P("#4C4266", 2.5));
                Path(d, "M 81,64 Q 109,72 136,63 L 145,77 L 75,77 Z", B("#C38EA5"), null);
                d.DrawRoundedRectangle(B("#D5B96D"), P("#9A7A45", 1.5), new Rect(104, 66, 15, 12), 2, 2);
                d.DrawEllipse(B("#74628D"), P("#4C4266", 2.5), new Point(110, 83), 65, 10);
                break;
            case 34:
                Path(d, "M 64,84 Q 66,39 110,39 Q 154,39 156,84 Z", B("#BD8F9F"), P("#8D657C", 2.5));
                Path(d, "M 83,75 Q 84,52 99,45 M 108,74 L 108,44 M 133,74 Q 132,53 119,45", null, P("#E0B9C5", 2));
                d.DrawEllipse(B("#E2BCC9"), P("#8D657C", 2), new Point(110, 32), 12, 12);
                d.DrawRoundedRectangle(B("#A77991"), P("#8D657C", 2), new Rect(60, 76, 100, 18), 6, 6);
                break;
            case 35:
                d.DrawEllipse(B("#FCF4ED"), P("#B99AA8", 2.5), new Point(84, 39), 12, 31);
                d.DrawEllipse(B("#FCF4ED"), P("#B99AA8", 2.5), new Point(136, 39), 12, 31);
                d.DrawEllipse(B("#E9B5C8"), null, new Point(84, 38), 6, 22);
                d.DrawEllipse(B("#E9B5C8"), null, new Point(136, 38), 6, 22);
                Path(d, "M 61,100 Q 110,61 159,100", null, P("#B99AA8", 5));
                break;
            case 36:
                Path(d, "M 61,96 Q 110,59 159,96", null, P("#B67C9C", 5));
                DrawHeart(d, 110, 68, 13, "#EB9DBA", "#A56584");
                d.DrawEllipse(B("#F4D487"), null, new Point(78, 83), 3, 3);
                d.DrawEllipse(B("#F4D487"), null, new Point(142, 83), 3, 3);
                break;
            case 37:
                DrawHeart(d, 87, 124, 15, "#40E9A2BD", "#AF6A8A");
                DrawHeart(d, 133, 124, 15, "#40E9A2BD", "#AF6A8A");
                Path(d, "M 103,120 Q 110,114 117,120 M 70,119 L 60,115 M 150,119 L 160,115", null, P("#AF6A8A", 3));
                break;
            case 38:
                d.DrawRoundedRectangle(B("#555DCED8"), P("#506D85", 3), new Rect(66, 111, 88, 27), 11, 11);
                Path(d, "M 73,115 L 93,115 M 119,134 L 143,134 M 66,118 L 56,112 M 154,118 L 164,112", null, P("#9AE4DE", 3));
                d.DrawLine(P("#A0FFFFFF", 2), new Point(105, 116), new Point(99, 130));
                break;
            case 39:
                Path(d, "M 58,132 L 52,148 M 162,132 L 168,148", null, P("#BC9A55", 2));
                DrawStar(d, 51, 156, 9, "#EACD80", "#B48D48");
                DrawStar(d, 169, 156, 9, "#EACD80", "#B48D48");
                break;
            case 40:
                Path(d, "M 73,161 Q 110,190 147,161", null, P("#C3A560", 2.5));
                DrawHeart(d, 110, 181, 9, "#D28CA6", "#A86A83");
                break;
            case 41:
                d.DrawRoundedRectangle(B("#536F8B"), P("#35566E", 1.5), new Rect(28, 136, 13, 29), 4, 4);
                d.DrawEllipse(B("#F8F4E8"), P("#B79A61", 2.5), new Point(34, 150), 9, 9);
                Path(d, "M 34,144 L 34,150 L 39,153", null, P("#526879", 1.8));
                break;
            case 42:
                Path(d, "M 49,224 L 66,224 L 73,232 L 82,233 Q 88,244 78,244 L 49,244 Q 43,242 46,233 Z M 171,224 L 154,224 L 147,232 L 138,233 Q 132,244 142,244 L 171,244 Q 177,242 174,233 Z", B("#B6CCD9"), P("#547189", 2.5));
                Path(d, "M 46,239 L 83,239 M 137,239 L 174,239 M 57,228 L 67,233 M 55,233 L 64,236 M 163,228 L 153,233 M 165,233 L 156,236", null, P("#F9F6ED", 2.5));
                break;
            case 43:
                Path(d, "M 47,218 L 67,218 L 68,233 L 81,235 Q 86,246 77,246 L 47,246 Z M 173,218 L 153,218 L 152,233 L 139,235 Q 134,246 143,246 L 173,246 Z", B("#8C7470"), P("#5F555E", 2.5));
                Path(d, "M 47,240 L 82,240 M 138,240 L 173,240 M 51,224 L 62,224 M 158,224 L 169,224", null, P("#CFB58E", 3));
                break;
            case 44:
                Path(d, "M 68,155 L 91,162 L 86,180 L 80,213 L 54,208 L 58,174 L 41,164 L 49,151 Z M 152,155 L 129,162 L 134,180 L 140,213 L 166,208 L 162,174 L 179,164 L 171,151 Z", B("#647F9F"), P("#415F7D", 2.5));
                Path(d, "M 91,162 L 102,178 L 86,187 M 129,162 L 118,178 L 134,187 M 67,181 L 77,183 M 143,183 L 153,181", null, P("#BCCDDC", 2));
                break;
            case 45:
                Path(d, "M 65,103 Q 41,152 25,219 Q 62,207 80,226 Q 110,211 140,226 Q 159,207 195,219 Q 179,152 155,103 Z", B("#D1858E"), P("#9E596C", 2.5));
                Path(d, "M 64,123 L 43,207 M 156,123 L 177,207", null, P("#E5ACB1", 3));
                break;
            case 46:
                d.DrawRoundedRectangle(B("#889BAF"), P("#4E647C", 2.5), new Rect(32, 103, 31, 94), 10, 10);
                d.DrawRoundedRectangle(B("#889BAF"), P("#4E647C", 2.5), new Rect(157, 103, 31, 94), 10, 10);
                Path(d, "M 35,193 L 60,193 L 55,222 L 47,210 L 40,222 Z M 160,193 L 185,193 L 180,222 L 172,210 L 165,222 Z", B("#EBB568"), P("#C18B55", 1.5));
                Path(d, "M 40,113 L 55,113 M 165,113 L 180,113", null, P("#D6E1EA", 4));
                break;
            case 47:
                d.DrawLine(P("#B5975B", 5), new Point(187, 114), new Point(202, 67));
                DrawStar(d, 204, 54, 13, "#F0D493", "#B5975B");
                Path(d, "M 192,44 L 187,40 M 205,34 L 205,28 M 215,69 L 218,73", null, P("#CCAE76", 2));
                break;
            case 48:
                Path(d, "M 189,110 Q 202,91 196,65", null, P("#A78395", 1.8));
                DrawHeart(d, 196, 43, 17, "#E9A2BC", "#AE6E8B");
                Path(d, "M 192,61 L 198,61 L 197,66 Z", B("#C485A0"), null);
                Path(d, "M 184,37 Q 183,30 188,29", null, P("#F9D7E3", 3));
                break;
            case 49:
                d.DrawEllipse(B("#F1C991"), P("#BA8D62", 2.5), new Point(110, 193), 74, 19);
                d.DrawEllipse(B("#C3E5DF"), P("#BA8D62", 2), new Point(110, 189), 47, 9);
                Path(d, "M 48,186 Q 54,203 67,204 M 155,204 Q 169,203 173,187", null, P("#E29BAA", 10));
                break;
            case 50:
                Path(d, "M 61,98 Q 110,60 159,98", null, P("#AC97C3", 5));
                Path(d, "M 98,66 L 110,14 L 122,66 Z", B("#EAD9AA"), P("#B39B78", 2));
                Path(d, "M 103,45 L 116,51 M 106,33 L 113,38 M 100,57 L 119,63", null, P("#CEAF8C", 2));
                Path(d, "M 69,82 L 70,58 L 91,74 M 129,74 L 150,58 L 151,82", B("#EAD1DE"), P("#AC97C3", 2));
                break;
        }
    }
}
