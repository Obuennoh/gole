using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public enum PetInteraction
{
    None, Wave, Pet, Land, Hop, Cheer, Dance, Shy, Bow, Wiggle, Stretch
}

public partial class Bottle
{
    public int PersonalityId;
    public bool IsDragging;
    public Vector Gaze;
    public double DragLean;
    int touchCount;
    public void Touch() => Interact((touchCount++ % 8) switch
    {
        0 => PetInteraction.Wave,
        1 => PetInteraction.Hop,
        2 => PetInteraction.Cheer,
        3 => PetInteraction.Dance,
        4 => PetInteraction.Shy,
        5 => PetInteraction.Bow,
        6 => PetInteraction.Wiggle,
        _ => PetInteraction.Stretch
    });
    PetInteraction interaction;
    double interactionStart = double.NegativeInfinity;
    double dragVertical, dragSpeed, lastDrag = double.NegativeInfinity;
    double speechUntil, speechStart;
    int speechCount;
    bool warmSpeech;
    internal PetInteraction CurrentInteraction => interaction;
    public void TrackDrag(double dx, double dy)
    {
        IsDragging = true;
        DragLean = DragLean * .65 + Math.Clamp(-dx * .45, -7, 4) * .35;
        dragVertical = dragVertical * .65 + Math.Clamp(dy / 30, -1, 1) * .35;
        dragSpeed = Math.Clamp(Math.Abs(dx) + Math.Abs(dy), 0, 30) / 30;
        lastDrag = Phase;
    }
    public void EndDrag()
    {
        IsDragging = false;
        Interact(PetInteraction.Land);
    }
    public void SpeakGesture(string text, int seconds)
    {
        speechStart = Phase;
        speechUntil = Phase + Math.Clamp(seconds, 0, 12);
        speechCount++;
        warmSpeech = text.Contains("💗") || text.Contains("💖") || text.Contains("carinho", StringComparison.OrdinalIgnoreCase)
            || text.Contains("abraço", StringComparison.OrdinalIgnoreCase);
        // Gestures accompany speech without replacing a click, cuddle or celebration.
        InvalidateVisual();
    }
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
    int ActingPersonality => PersonalityId == 0 ? 1 + (int)(MotionTime / 14) % (Gole.Core.PersonalityCatalog.Options.Count - 1) : PersonalityId;
    double Gesture => IsAnimating && Level > 0 && !Celebrate && !IsDragging && !InteractionActive
        && MotionTime % 14 is > 2 and < 5 ? Math.Sin((MotionTime % 14 - 2) / 3 * Math.PI) : 0;

    CharacterPose InteractivePose(CharacterPose pose)
    {
        if (!IsAnimating)
            return pose;
        double g = Gesture, t = MotionTime;
        bool speaking = Phase >= speechStart && Phase < speechUntil && !IsDragging && !InteractionActive && !Celebrate;
        if (speaking)
            g = Math.Sin(Math.Min(1, (Phase - speechStart) / .3) * Math.PI / 2)
            * Math.Min(1, (speechUntil - Phase) / .4) * (.65 + .35 * Math.Sin((Phase - speechStart) * 4));
        var styled = ActingPersonality switch
        {
            1 => pose with { Lean = pose.Lean + g * 3, RightArm = pose.RightArm - g * 18 },
            2 => pose with { LeftArm = pose.LeftArm - g * 16, RightArm = pose.RightArm - g * 16 },
            3 => pose with { Lift = pose.Lift - g * 5, RightArm = pose.RightArm - g * 25 },
            4 => pose with { Blink = g > .3, ScaleY = pose.ScaleY + g * .01 },
            5 => pose with { Lean = pose.Lean - g * 5, LeftArm = pose.LeftArm + g * 25 },
            6 or 12 => pose with { Lean = pose.Lean + g, LeftArm = pose.LeftArm - g * 12, RightArm = pose.RightArm - g * 22 },
            7 => pose with { LeftArm = pose.LeftArm + g * Math.Sin(t * 8) * 10, RightArm = pose.RightArm - g * 12 },
            8 => pose with { Lean = pose.Lean + g * 3, RightArm = pose.RightArm - g * 20 },
            9 => pose with { RightArm = pose.RightArm - g * 25, Lean = pose.Lean - g * 3 },
            10 => pose with { LeftArm = pose.LeftArm - g * 20, RightArm = pose.RightArm + g * 20, Lift = pose.Lift - g * Math.Abs(Math.Sin(t * 9)) * 2 },
            11 => pose with { Lean = pose.Lean + g * Math.Sin(t * 3) * 3, RightArm = pose.RightArm - g * 22 },
            _ => pose
        };
        if (!speaking && g > 0)
            styled = ((int)(t / 14) % 3) switch
            {
                1 => styled with { ScaleY = 1 + .018 * g, LeftArm = -18 * g, RightArm = -18 * g },
                2 => styled with { Lean = styled.Lean - 3 * g, Blink = g > .8 },
                _ => styled
            };
        if (speaking)
            styled = styled with
            {
                Lean = styled.Lean + Math.Sin(t * 3 + speechCount) * g,
                Blink = styled.Blink || (warmSpeech && g > .9),
                Lift = styled.Lift - g,
                LeftArm = styled.LeftArm - (speechCount % 2) * g * 9
            };
        double momentum = Math.Exp(-Math.Max(0, Phase - lastDrag) * 5);
        if (IsDragging)
            return styled with
            {
                ScaleX = .985 - dragVertical * momentum * .018 + Math.Sin(t * 5) * .006,
                ScaleY = 1.015 + dragVertical * momentum * .018 - Math.Sin(t * 5) * .006,
                Lean = Math.Clamp(DragLean * momentum + Math.Sin(t * 3) * (1.2 + dragSpeed * momentum * 2), -8, 5),
                Lift = -3 - Math.Sin(t * 4),
                LeftArm = -20 - dragSpeed * momentum * 8 + Math.Sin(t * 5) * 7,
                RightArm = -27 - Math.Sin(t * 5) * 7
            };
        if (!InteractionActive)
            return styled;
        double envelope = Math.Sin(InteractionAge / 2.5 * Math.PI);
        return interaction switch
        {
            PetInteraction.Dance => pose with { Lean = Math.Sin(InteractionAge * 9) * 5 * envelope, Lift = -Math.Abs(Math.Sin(InteractionAge * 9)) * 5 * envelope, LeftArm = Math.Sin(InteractionAge * 9) * 22 * envelope, RightArm = -Math.Sin(InteractionAge * 9) * 22 * envelope },
            PetInteraction.Shy => pose with { Lean = -5 * envelope, Blink = envelope > .5, LeftArm = -20 * envelope, RightArm = 18 * envelope, ScaleY = 1 - .025 * envelope },
            PetInteraction.Bow => pose with { Lean = 4 * envelope, ScaleX = 1 + .025 * envelope, ScaleY = 1 - .045 * envelope, LeftArm = 12 * envelope, RightArm = -12 * envelope, Blink = envelope > .7 },
            PetInteraction.Wiggle => pose with { Lean = Math.Sin(InteractionAge * 15) * 4 * envelope, ScaleX = 1 + .025 * envelope, ScaleY = 1 - .025 * envelope },
            PetInteraction.Stretch => pose with { ScaleY = 1 + .035 * envelope, ScaleX = 1 - .025 * envelope, LeftArm = -28 * envelope, RightArm = -28 * envelope, Blink = envelope > .8 },
            PetInteraction.Hop => pose with { Lift = -Math.Abs(Math.Sin(InteractionAge * 6)) * 11 * envelope, LeftArm = -18 * envelope, RightArm = -20 * envelope },
            PetInteraction.Cheer => pose with { Lean = Math.Sin(InteractionAge * 7) * 4 * envelope, LeftArm = -25 * envelope, RightArm = -25 * envelope, Blink = envelope > .8 },
            PetInteraction.Wave => pose with { RightArm = -15 + Math.Sin(InteractionAge * 12) * 18 * envelope },
            PetInteraction.Pet => pose with { Blink = true, Lean = Math.Sin(InteractionAge * 4) * 3, Lift = -envelope * 3 },
            PetInteraction.Land => pose with { ScaleX = 1 + .04 * envelope, ScaleY = 1 - .04 * envelope, Lift = -Math.Abs(Math.Sin(InteractionAge * 5)) * 4 * (1 - InteractionAge / 2.5) },
            _ => pose
        };
    }

    internal void DrawPersonality(DrawingContext d, BodyFit fit)
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
            DrawHeart(d, 88 + Math.Sin(InteractionAge * 3) * 3, 102 - InteractionAge * 12, 5, "#D8ACD3", "#AE708E");
            DrawHeart(d, 132 - Math.Sin(InteractionAge * 3) * 3, 99 - InteractionAge * 10, 5, "#E6C09F", "#AE708E");
            d.Pop();
        }
    }
}
