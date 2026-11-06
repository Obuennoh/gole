using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public enum PetInteraction
{
    None, Wave, Pet, Land, Accessory
}

public partial class Bottle
{
    public int PersonalityId;
    public bool IsDragging;
    public Vector Gaze;
    PetInteraction interaction;
    double interactionStart = double.NegativeInfinity;
    int activeAccessory, accessoryIndex;
    internal double InteractionAge => Phase - interactionStart;
    internal bool InteractionActive => IsAnimating && InteractionAge is >= 0 and < 2.5;
    public void Interact(PetInteraction kind)
    {
        if (!MotionEnabled || Paused)
            return;
        interaction = kind;
        interactionStart = Phase;
        InvalidateVisual();
    }
    public void PlayAccessories(int item = 0)
    {
        if (!MotionEnabled || Paused)
            return;
        var items = EquippedAccessories.Where(id => id > 0).ToArray();
        if (item != 0 && !items.Contains(item))
            return;
        activeAccessory = item != 0 ? item : items.Length > 0 ? items[accessoryIndex++ % items.Length] : 0;
        Interact(activeAccessory == 0 ? PetInteraction.Wave : PetInteraction.Accessory);
    }
    int ActingPersonality => PersonalityId == 0 ? 1 + (int)(MotionTime / 14) % 11 : PersonalityId;
    double Gesture => IsAnimating && Level > 0 && !Celebrate && !IsDragging && !InteractionActive
        && MotionTime % 14 is > 2 and < 5 ? Math.Sin((MotionTime % 14 - 2) / 3 * Math.PI) : 0;

    CharacterPose InteractivePose(CharacterPose pose)
    {
        if (!IsAnimating)
            return pose;
        double g = Gesture, t = MotionTime;
        var styled = ActingPersonality switch
        {
            1 => pose with { Lean = pose.Lean + g * 3, RightArm = pose.RightArm - g * 18 },
            2 => pose with { LeftArm = pose.LeftArm - g * 16, RightArm = pose.RightArm - g * 16 },
            3 => pose with { Lift = pose.Lift - g * 5, RightArm = pose.RightArm - g * 25 },
            4 => pose with { Blink = g > .3, ScaleY = pose.ScaleY + g * .01 },
            5 => pose with { Lean = pose.Lean - g * 5, LeftArm = pose.LeftArm + g * 25 },
            6 => pose with { Lean = pose.Lean + g, LeftArm = pose.LeftArm - g * 12, RightArm = pose.RightArm - g * 22 },
            7 => pose with { LeftArm = pose.LeftArm + g * Math.Sin(t * 8) * 10, RightArm = pose.RightArm - g * 12 },
            8 => pose with { Lean = pose.Lean + g * 3, RightArm = pose.RightArm - g * 20 },
            9 => pose with { RightArm = pose.RightArm - g * 25, Lean = pose.Lean - g * 3 },
            10 => pose with { LeftArm = pose.LeftArm - g * 20, RightArm = pose.RightArm + g * 20, Lift = pose.Lift - g * Math.Abs(Math.Sin(t * 9)) * 2 },
            11 => pose with { Lean = pose.Lean + g * Math.Sin(t * 3) * 3, RightArm = pose.RightArm - g * 22 },
            _ => pose
        };
        if (IsDragging)
            return styled with
            {
                ScaleX = .97,
                ScaleY = 1.03,
                Lean = Math.Sin(t * 4) * 4,
                LeftArm = -22,
                RightArm = -25
            };
        if (!InteractionActive)
            return styled;
        double envelope = Math.Sin(InteractionAge / 2.5 * Math.PI);
        return interaction switch
        {
            PetInteraction.Wave => pose with { RightArm = -15 + Math.Sin(InteractionAge * 12) * 18 * envelope },
            PetInteraction.Pet => pose with { Blink = true, Lean = Math.Sin(InteractionAge * 4) * 3, Lift = -envelope * 3 },
            PetInteraction.Land => pose with { ScaleX = 1 + .04 * envelope, ScaleY = 1 - .04 * envelope, Lift = -Math.Abs(Math.Sin(InteractionAge * 5)) * 4 * (1 - InteractionAge / 2.5) },
            _ => pose
        };
    }

    void DrawPersonality(DrawingContext d, BodyFit fit)
    {
        double g = Gesture;
        if (g > 0)
        {
            d.PushOpacity(g);
            d.PushTransform(new ScaleTransform(Math.Min(1, fit.Width(160) / 110), 1, 110, 160));
            switch (ActingPersonality)
            {
                case 1:
                    Path(d, "M 77,111 L 96,114", null, P("#234957", 2));
                    break;
                case 2:
                    DrawHeart(d, 110, 93 - g * 5, 9, "#E694A7", "#AE708E");
                    break;
                case 3:
                    DrawStar(d, 110, 92, 10, "#EDCF91", "#AD8953");
                    break;
                case 4:
                    Path(d, "M 91,181 Q 110,172 129,181 M 96,189 Q 110,182 124,189", null, P("#B3D4BF", 3));
                    break;
                case 5:
                    DrawHeart(d, 110, 179, 10, "#C58CAB", "#916D91");
                    break;
                case 6:
                    Path(d, "M 83,170 Q 96,166 110,173 Q 124,166 137,170 L 137,196 Q 124,191 110,198 Q 96,191 83,196 Z", B("#FFF1D7"), P("#78678F", 2));
                    Path(d, "M 110,173 L 110,196 M 89,178 L 103,180 M 117,180 L 131,178", null, P("#B4A5AD", 1.5));
                    break;
                case 7:
                    d.DrawRoundedRectangle(B("#777393"), P("#4D5271", 2), new Rect(82, 173, 56, 25), 9, 9);
                    Path(d, "M 91,185 L 103,185 M 97,179 L 97,191", null, P("#E4DCF0", 3));
                    d.DrawEllipse(B("#D9B1CE"), null, new Point(127, 182), 3, 3);
                    break;
                case 8:
                    d.DrawEllipse(null, P("#99A6C7", 2), new Point(139, 99), 9, 7);
                    d.DrawEllipse(B("#99A6C7"), null, new Point(128, 110), 2, 2);
                    break;
                case 9:
                    Path(d, "M 123,113 L 154,105 L 158,117 L 127,125 Z", B("#D6B67D"), P("#92794F", 2));
                    break;
                case 10:
                    Path(d, "M 73,171 Q 110,188 145,171 M 75,181 Q 110,167 143,181", null, P("#234957", 4));
                    break;
                case 11:
                    Path(d, "M 144,87 Q 141,76 131,79 M 144,87 L 151,95", null, P("#A77DAD", 3));
                    break;
            }
            d.Pop();
            d.Pop();
        }
        if (InteractionActive && interaction == PetInteraction.Pet)
        {
            d.PushOpacity(Math.Sin(InteractionAge / 2.5 * Math.PI));
            DrawHeart(d, 110, 92 - InteractionAge * 9, 9, "#E694A7", "#AE708E");
            d.Pop();
        }
        if (InteractionActive && interaction == PetInteraction.Accessory && activeAccessory is not (69 or 47 or 48 or 82 or 1 or 24 or 70))
            Sparkle(d, new Point(110, 80 - InteractionAge * 5), 7, Math.Sin(InteractionAge / 2.5 * Math.PI));
    }

    void DrawInteractiveAccessory(DrawingContext d, BodyFit fit, int model, int item)
    {
        if (!InteractionActive || interaction != PetInteraction.Accessory || item != activeAccessory)
            return;
        double p = InteractionAge / 2.5;
        d.PushOpacity(Math.Sin(p * Math.PI));
        d.PushTransform(AccessoryTransform(fit, model, item));
        if (item == 69)
            Path(d, "M 34,136 Q 34,119 52,133 L 52,160 Q 40,153 34,169 Z", B("#FFF1D7"), P("#B4A5AD", 1.5));
        else if (item is 47 or 48 or 82)
            DrawStar(d, 193 - p * 15, 62 - p * 22, 3 + p * 4, "#F0D493", "#B5975B");
        else if (item is 1 or 24 or 70)
            Path(d, "M 169,86 L 169,70 L 179,73 M 169,86 Q 160,81 162,89 Q 168,94 169,86", null, P("#8264BC", 2));
        else
            Sparkle(d, new Point(110, 95 - p * 15), 4 + p * 3, 1);
        d.Pop();
        d.Pop();
    }
}
