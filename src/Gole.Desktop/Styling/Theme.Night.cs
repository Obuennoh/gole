using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Gole.Desktop;

static partial class Theme
{
    static readonly Dictionary<string, string> Surfaces = new()
    {
        ["#FFFFFFFF"] = "#FF24343F",
        ["#FFF5F7F9"] = "#FF15232D",
        ["#FFF0F6F7"] = "#FF1B2C36",
        ["#FFEAF2F0"] = "#FF304751",
        ["#FFEDF7F6"] = "#FF345963",
        ["#FFEDF2F5"] = "#FF30414F",
        ["#FFCCE9E7"] = "#FF375E65",
        ["#FFE7EEF0"] = "#FF30414E",
        ["#FFFBEFEF"] = "#FF593E4A",
        ["#FFE2E8ED"] = "#FF48606C"
    };
    static readonly Dictionary<string, string> TextColors = new()
    {
        ["#FF18343D"] = "#FFE8F0F3",
        ["#FF6A8086"] = "#FFAABDC5"
    };
    static Brush Convert(Brush brush, bool night, Dictionary<string, string> palette)
    {
        if (brush is not SolidColorBrush solid)
            return brush;
        string key = solid.Color.ToString();
        if (night && palette.TryGetValue(key, out var dark))
            return B(dark);
        if (!night)
            foreach (var pair in palette)
                if (pair.Value == key)
                    return B(pair.Key);
        return brush;
    }
    internal static void ApplyNight(DependencyObject root, bool night)
    {
        if (root is Bottle)
            return;
        if (root is Button palette && palette.Parent is Border { Tag: string tag } && tag.StartsWith("#"))
            return;
        if (root is Control control)
        {
            control.Background = Convert(control.Background, night, Surfaces);
            control.Foreground = Convert(control.Foreground, night, TextColors);
            control.BorderBrush = Convert(control.BorderBrush, night, Surfaces);
            if (control is TextBox or ComboBox)
            {
                control.Background = B(night ? "#24343F" : "#FFFFFF");
                control.Foreground = B(night ? "#E8F0F3" : "#18343D");
            }
        }
        if (root is TextBlock text)
            text.Foreground = Convert(text.Foreground, night, TextColors);
        if (root is Border border)
        {
            border.Background = Convert(border.Background, night, Surfaces);
            border.BorderBrush = Convert(border.BorderBrush, night, Surfaces);
        }
        if (root is Panel panel)
            panel.Background = Convert(panel.Background, night, Surfaces);
        if (root is FrameworkElement element && root is Window or ComboBox)
        {
            element.Resources[SystemColors.WindowBrushKey] = B(night ? "#24343F" : "#FFFFFF");
            element.Resources[SystemColors.WindowTextBrushKey] = B(night ? "#E8F0F3" : "#18343D");
        }
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            ApplyNight(child, night);
    }
}
