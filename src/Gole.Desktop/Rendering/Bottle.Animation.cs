using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace Gole.Desktop;

public partial class Bottle
{
    public bool MotionEnabled = true;
    public double CelebrationSeconds = double.PositiveInfinity;
    bool suppressAnimation;
    string frameKey;
    Rect viewFrame;
    static readonly ConditionalWeakTable<DrawingGroup, DrawingBrush> masks = new();
    internal Rect ViewFrame => viewFrame;
    internal Rect DragBounds
    {
        get
        {
            var frame = AnimationFrame(Math.Clamp(Vessel, 0, bodies.Length - 1));
            double width = ActualWidth > 0 ? ActualWidth : Width, height = ActualHeight > 0 ? ActualHeight : Height;
            double scale = Math.Min(width / frame.Width, height / frame.Height);
            // This central patch is inside every vessel and remains easy to grab in every pose.
            return new Rect((width - frame.Width * scale) / 2 + (84 - frame.X) * scale,
                (height - frame.Height * scale) / 2 + (112 - frame.Y) * scale, 52 * scale, 72 * scale);
        }
    }
    double MotionTime => MotionEnabled && !Paused && !suppressAnimation && double.IsFinite(Phase) ? Math.Max(0, Phase) : 0;
    bool IsAnimating => MotionEnabled && !Paused && !suppressAnimation;
    CharacterPose Pose => CharacterMotion.Sample(MotionTime, Level, IsAnimating, Paused, Celebrate, CelebrationSeconds);

    Rect AnimationFrame(int model)
    {
        string key = model + ":" + string.Join(",", EquippedAccessories.OrderBy(id => id)) + ":" + Accessory;
        if (frameKey == key)
            return viewFrame;
        var reference = new DrawingGroup();
        suppressAnimation = true;
        try
        {
            using var context = reference.Open();
            DrawCharacter(context, model, Math.Clamp(Level, 0, 1));
        }
        finally { suppressAnimation = false; }
        var basis = reference.Bounds;
        basis.Union(new Rect(0, 0, 220, 255));
        basis.Inflate(12, 12);
        viewFrame = basis;
        foreach (double angle in new[] { -9.0, 5.0 })
        {
            var envelope = new TransformGroup();
            envelope.Children.Add(new ScaleTransform(1.05, 1.05, 110, 222));
            envelope.Children.Add(new RotateTransform(angle, 110, 222));
            envelope.Children.Add(new TranslateTransform(0, -15));
            viewFrame.Union(envelope.TransformBounds(basis));
        }
        viewFrame.Inflate(3, 3);
        frameKey = key;
        return viewFrame;
    }

    internal Transform ArmTransform(BodyFit fit, bool right)
    {
        var pose = Pose;
        return new RotateTransform(right ? pose.RightArm : pose.LeftArm,
            right ? fit.Right(129) - 3 : fit.Left(129) + 3, 129);
    }

    void DrawAnimatedAccessory(DrawingContext d, int model, int item)
    {
        var fit = Fit(model);
        double t = MotionTime, energy = Pose.Energy;
        if (item is 17 or 18 or 15 or 39 or 35)
        {
            for (int side = 1; side <= 2; side++)
            {
                bool right = side == 2;
                Transform transform;
                if (item is 17 or 18)
                    transform = new ScaleTransform(IsAnimating ? 1 - (.5 + .5 * Math.Sin(t * (2 + energy * 3))) * .33 * energy : 1,
                        1, right ? fit.Right(135) - 3 : fit.Left(135) + 3, 135);
                else if (item == 35)
                {
                    var anchor = AccessoryTransform(fit, model, item).Transform(new Point(right ? 136 : 84, 87));
                    transform = new RotateTransform(IsAnimating ? Math.Sin(t * 2.3 + (right ? .7 : 0)) * 6 * energy : 0, anchor.X, anchor.Y);
                }
                else
                {
                    var anchor = EarringAnchor(fit, right);
                    transform = new RotateTransform(IsAnimating ? Math.Sin(t * 2.2 + (right ? .5 : 0)) * 7 * energy : 0, anchor.X, anchor.Y);
                }
                d.PushTransform(transform);
                DrawAccessorySurface(d, model, item, side);
                d.Pop();
            }
            if (item == 35)
                DrawAccessorySurface(d, model, item, 0);
            return;
        }
        if (item is 44 or 51 or 52 or 54 or 32)
        {
            for (int side = 1; side <= 2; side++)
            {
                d.PushTransform(ArmTransform(fit, side == 2));
                DrawAccessorySurface(d, model, item, side);
                d.Pop();
            }
            if (item == 32)
                return;
        }
        bool inHand = item is 26 or 41 or 47 or 48 or 69 or 78 or 82;
        if (inHand)
            d.PushTransform(ArmTransform(fit, item is not (41 or 69)));
        Transform secondary = Transform.Identity;
        if (item == 45 && IsAnimating)
        {
            double shear = Math.Sin(t * 1.8) * .032 * energy;
            secondary = new MatrixTransform(1, 0, shear, 1, -103 * shear, 0);
        }
        if (item is 48 or 82 && IsAnimating)
        {
            var anchor = AccessoryTransform(fit, model, item).Transform(new Point(189, 110));
            secondary = new RotateTransform(Math.Sin(t * 1.5) * 3 * energy, anchor.X, anchor.Y);
        }
        if (item == 20 && IsAnimating)
            secondary = new TranslateTransform(0, Math.Sin(t * 1.8) * 3 * energy);
        d.PushTransform(secondary);
        DrawAccessorySurface(d, model, item, 0);
        if (item == 46 && Level > 0)
        {
            d.PushTransform(new ScaleTransform(1, IsAnimating ? .8 + .2 * Math.Sin(t * 9) * energy : 1, 110, 193));
            d.DrawDrawing(AccessoryDrawing(model, item, 1));
            d.Pop();
        }
        if (IsAnimating && Level > 0)
            DrawAccessoryEffects(d, fit, model, item);
        d.Pop();
        if (inHand)
            d.Pop();
    }

    static Brush SurfaceMask(DrawingGroup drawing) => masks.GetValue(drawing, value =>
    {
        var bounds = value.Bounds;
        var brush = new DrawingBrush(value)
        {
            ViewboxUnits = BrushMappingMode.Absolute,
            Viewbox = bounds,
            ViewportUnits = BrushMappingMode.Absolute,
            Viewport = bounds,
            Stretch = Stretch.Fill
        };
        brush.Freeze();
        return brush;
    });

    void DrawAccessorySurface(DrawingContext d, int model, int item, int part)
    {
        var drawing = AccessoryDrawing(model, item, part);
        d.DrawDrawing(drawing);
    }

    void DrawAccessoryEffects(DrawingContext d, BodyFit fit, int model, int item)
    {
        double t = MotionTime;
        if (item is 2 or 21 or 37 or 38 or 61 or 68)
        {
            var drawing = AccessoryDrawing(model, item, 0);
            d.PushOpacityMask(SurfaceMask(drawing));
            d.PushOpacity(.22 * (.5 + .5 * Math.Sin(t * .8)));
            double x = 110 + Math.Sin(t * .8) * 48;
            d.DrawLine(P("#FFFFFF", 4), new Point(x - 10, 139), new Point(x + 8, 109));
            d.Pop();
            d.Pop();
        }
        if (item is 9 or 20 or 23 or 36 or 47 or 50 or 64 or 74 or 81)
        {
            var point = item switch
            {
                9 => new Point(110, 48),
                20 => new Point(139, 21),
                23 => new Point(146, 87),
                36 => new Point(110, 63),
                47 => new Point(204, 54),
                50 => new Point(110, 35),
                74 => new Point(104, 48),
                _ => new Point(141, 174)
            };
            Sparkle(d, AccessoryTransform(fit, model, item).Transform(point), 5, .3 + .55 * (.5 + .5 * Math.Sin(t * 1.8)));
        }
        if (item is 1 or 24 or 70 && Level > .6)
        {
            for (int i = 0; i < 2; i++)
            {
                double x = i == 0 ? fit.Left(108) - 12 : fit.Right(108) + 10;
                double y = 94 + Math.Sin(t * 2 + i * 2) * 4;
                d.PushOpacity(.35 + .4 * (.5 + .5 * Math.Sin(t * 2 + i * 2)));
                d.DrawEllipse(B("#8264BC"), null, new Point(x, y + 8), 3, 2);
                d.DrawLine(P("#8264BC", 1.8), new Point(x + 2, y + 8), new Point(x + 2, y - 2));
                d.DrawLine(P("#8264BC", 2), new Point(x + 2, y - 2), new Point(x + 7, y));
                d.Pop();
            }
        }
    }

    static void Sparkle(DrawingContext d, Point point, double size, double opacity)
    {
        d.PushOpacity(opacity);
        d.DrawLine(P("#FFF6CE", 1.8), new Point(point.X - size, point.Y), new Point(point.X + size, point.Y));
        d.DrawLine(P("#FFFFFF", 1.6), new Point(point.X, point.Y - size), new Point(point.X, point.Y + size));
        d.Pop();
    }

    void DrawCelebration(DrawingContext d)
    {
        if (!IsAnimating || !Celebrate || !double.IsFinite(CelebrationSeconds) || CelebrationSeconds < 0 || CelebrationSeconds >= CharacterMotion.CelebrationDuration)
            return;
        double progress = CelebrationSeconds / CharacterMotion.CelebrationDuration;
        d.PushOpacity(Math.Sin(Math.PI * progress) * .8);
        for (int i = 0; i < 5; i++)
        {
            double angle = (i - 2) * .52;
            double distance = 22 + progress * 34;
            var point = new Point(110 + Math.Sin(angle) * distance, 89 - Math.Cos(angle) * distance + progress * progress * 20);
            d.DrawEllipse(B("#B1E8EB"), P("#77BBC8", 1), point, 2.8, 3.8);
        }
        d.Pop();
    }
}
