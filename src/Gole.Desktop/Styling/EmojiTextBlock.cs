using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Gole.Desktop;

// WPF's text renderer does not paint color emoji fonts. Bundle the emoji artwork
// as inline images while preserving the original Unicode text for accessibility.
public sealed class EmojiTextBlock : TextBlock
{
    static readonly ConcurrentDictionary<string, BitmapSource> images = new();
    static readonly string[] resources = typeof(EmojiTextBlock).Assembly.GetManifestResourceNames();
    string sourceText = "";
    public new string Text
    {
        get => sourceText;
        set
        {
            sourceText = value ?? "";
            Inlines.Clear();
            var pending = new StringBuilder();
            var elements = StringInfo.GetTextElementEnumerator(sourceText);
            while (elements.MoveNext())
            {
                string element = elements.GetTextElement();
                var bitmap = EmojiImage(element);
                if (bitmap == null)
                {
                    pending.Append(element);
                    continue;
                }
                if (pending.Length > 0)
                {
                    Inlines.Add(new Run(pending.ToString()));
                    pending.Clear();
                }
                var image = new Image { Source = bitmap, Width = FontSize * 1.2, Height = FontSize * 1.2, Margin = new Thickness(1, 0, 1, 0), IsHitTestVisible = false };
                AutomationProperties.SetName(image, element);
                Inlines.Add(new InlineUIContainer(image) { BaselineAlignment = BaselineAlignment.Center });
            }
            if (pending.Length > 0)
                Inlines.Add(new Run(pending.ToString()));
            AutomationProperties.SetHelpText(this, sourceText);
        }
    }

    internal static BitmapSource EmojiImage(string element)
    {
        string key = string.Join("-", element.EnumerateRunes().Where(r => r.Value != 0xfe0f).Select(r => r.Value.ToString("x", CultureInfo.InvariantCulture)));
        string resource = "Gole.Desktop.Emoji." + key + ".png";
        if (!resources.Contains(resource))
            return null;
        return images.GetOrAdd(key, _ =>
        {
            using var stream = typeof(EmojiTextBlock).Assembly.GetManifestResourceStream(resource);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        });
    }
}
