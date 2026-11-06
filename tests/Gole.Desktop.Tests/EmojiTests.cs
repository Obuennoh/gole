using System.Globalization;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Gole.Core;
using Gole.Desktop;

namespace Gole.Desktop.Tests;

[Collection("Rendering")]
public sealed class EmojiTests
{
    [Fact]
    public void Catalog_emoji_are_bundled_and_smiley_is_colored_in_both_themes()
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var phrases = PersonalityCatalog.Options.Where(p => p.Id > 0).SelectMany(p =>
                    Enum.GetValues<HydrationMood>().SelectMany(m => PersonalityCatalog.Reminders(p.Id, m)).Concat(PersonalityCatalog.Reactions(p.Id)));
                foreach (string phrase in phrases)
                {
                    var elements = StringInfo.GetTextElementEnumerator(phrase);
                    while (elements.MoveNext())
                    {
                        string element = elements.GetTextElement();
                        if (element.EnumerateRunes().Any(r => r.Value >= 0x1f000 || r.Value is >= 0x2600 and <= 0x27bf or 0x2b50))
                            Assert.NotNull(EmojiTextBlock.EmojiImage(element));
                    }
                }
                foreach (var brush in new[] { Brushes.Black, Brushes.White })
                {
                    var text = new EmojiTextBlock { FontSize = 24, Foreground = brush, Text = "Oi 😊 👉👈", Width = 250, Height = 50 };
                    Assert.Equal("Oi 😊 👉👈", text.Text);
                    Assert.Equal(text.Text, AutomationProperties.GetHelpText(text));
                    Assert.Equal(3, text.Inlines.OfType<InlineUIContainer>().Count());
                    text.Measure(new Size(250, 50));
                    text.Arrange(new Rect(0, 0, 250, 50));
                    var bitmap = new RenderTargetBitmap(250, 50, 96, 96, PixelFormats.Pbgra32);
                    bitmap.Render(text);
                    var pixels = new byte[250 * 50 * 4];
                    bitmap.CopyPixels(pixels, 250 * 4, 0);
                    Assert.Contains(Enumerable.Range(0, pixels.Length / 4), i => pixels[i * 4 + 2] > 180 && pixels[i * 4 + 1] > 100 && pixels[i * 4] < 100);
                    text.Text = "Sem emoji";
                    Assert.Empty(text.Inlines.OfType<InlineUIContainer>());
                }
            }
            catch (Exception ex) { error = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(20)));
        if (error != null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }
}
