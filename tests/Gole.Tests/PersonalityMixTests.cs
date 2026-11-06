using Gole.Core;

namespace Gole.Tests;

public sealed class PersonalityMixTests
{
    [Fact]
    public void Selection_preserves_primary_limits_to_three_and_cleans_invalid_choices()
    {
        Assert.Equal(new[] { 13, 6, 11 }, PersonalityMix.Normalize(new[] { 999, 0, 13, 6, 6, 11, 14 }, 13));
        Assert.Empty(PersonalityMix.Normalize(new[] { 6, 11 }, 0));
        Assert.Equal(new[] { 6 }, PersonalityMix.Normalize(null, 6));
    }

    [Fact]
    public void Mix_uses_only_selected_voices_without_immediate_repetition()
    {
        var mix = new PersonalityMix(new Random(42));
        int previous = 0;
        var seen = new HashSet<int>();
        for (int i = 0; i < 100; i++)
        {
            int voice = mix.Next(new[] { 13, 6, 11 });
            Assert.Contains(voice, new[] { 13, 6, 11 });
            Assert.NotEqual(previous, voice);
            seen.Add(previous = voice);
        }
        Assert.Equal(3, seen.Count);
        Assert.Equal(13, mix.Next(new[] { 13 }));
        Assert.Equal(13, mix.Next(new[] { 13 }));
        seen.Clear();
        for (int i = 0; i < 500; i++)
            seen.Add(mix.Next([]));
        Assert.Equal(PersonalityCatalog.Options.Count - 1, seen.Count);
    }
}
