using System;
using System.Collections.Generic;
using System.Windows;

namespace Gole.Desktop;

internal static class DesktopPlacement
{
    // Transparent window padding and the speech balloon are deliberately excluded.
    public static Vector RecoveryOffset(Rect grip, IReadOnlyList<Rect> monitors, double minimumGrip = 8)
    {
        if (grip.IsEmpty || monitors.Count == 0)
            return new Vector();
        double requiredWidth = Math.Min(minimumGrip, grip.Width);
        double requiredHeight = Math.Min(minimumGrip, grip.Height);
        foreach (var monitor in monitors)
        {
            var visible = Rect.Intersect(grip, monitor);
            if (!visible.IsEmpty && visible.Width >= requiredWidth && visible.Height >= requiredHeight)
                return new Vector();
        }
        Vector nearest = new Vector();
        double distance = double.PositiveInfinity;
        foreach (var monitor in monitors)
        {
            double x = Math.Clamp(grip.X, monitor.Left, Math.Max(monitor.Left, monitor.Right - grip.Width));
            double y = Math.Clamp(grip.Y, monitor.Top, Math.Max(monitor.Top, monitor.Bottom - grip.Height));
            var delta = new Vector(x - grip.X, y - grip.Y);
            if (delta.LengthSquared < distance)
            {
                distance = delta.LengthSquared;
                nearest = delta;
            }
        }
        return nearest;
    }
}
