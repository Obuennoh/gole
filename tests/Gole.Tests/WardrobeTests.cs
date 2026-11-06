using Gole.Core;
using Gole.Infrastructure;

namespace Gole.Tests;

public sealed class WardrobeTests
{
    [Fact]
    public void Touch_and_pet_phrases_match_every_personality_without_immediate_repeats()
    {
        var phrases = new InteractionPhrases(new Random(42));
        for (int id = 0; id <= 11; id++)
            foreach (bool pet in new[] { false, true })
            {
                string previous = "";
                for (int i = 0; i < 20; i++)
                {
                    string next = phrases.Next(id, pet);
                    Assert.InRange(next.Length, 1, 160);
                    Assert.NotEqual(previous, next);
                    previous = next;
                }
            }
    }

    [Fact]
    public void Suggestions_have_five_compatible_items_and_never_repeat_the_current_set()
    {
        var random = new Random(42);
        SavedLook? previous = null;
        for (int i = 0; i < 300; i++)
        {
            var look = Wardrobe.Suggest(random, 25, previous?.Accessories);
            Assert.Equal(25, look.Vessel);
            Assert.True(HydrationState.IsColor(look.Color));
            Assert.Equal(5, look.Accessories.Count);
            Assert.Equal(look.Accessories, PetCatalog.Normalize(look.Accessories));
            if (previous is not null)
                Assert.False(previous.Accessories.Order().SequenceEqual(look.Accessories.Order()));
            previous = look;
        }
    }

    [Fact]
    public void Filters_ignore_accents_and_combine_search_with_category()
    {
        Assert.True(Wardrobe.Matches(68, "OCULOS", "Rosto"));
        Assert.False(Wardrobe.Matches(68, "oculos", "Roupas"));
        Assert.False(Wardrobe.Matches(68, "saia", "Todos"));
        Assert.True(Wardrobe.Matches(0, "inexistente", "Roupas"));
        for (int id = 1; id < PetCatalog.Accessories.Count; id++)
            Assert.Contains(Wardrobe.Category(id), Wardrobe.Categories);
    }

    [Fact]
    public void Looks_round_trip_without_changing_position_personality_or_history()
    {
        string directory = Path.Combine(Path.GetTempPath(), "Gole.Tests", Guid.NewGuid().ToString("N"));
        try
        {
            var now = DateTime.UtcNow;
            var state = HydrationState.Fresh(now);
            state.PersonalityId = 11;
            state.X = -30;
            state.Y = -12;
            var look = Wardrobe.Suggest(new Random(2), 25);
            look.Name = "Meu nerd fofo";
            state.Looks.Add(look);
            var store = new JsonStateStore(directory);
            Assert.True(store.Save(state));
            var loaded = store.Load(now).State;
            Assert.Equal(look.Name, Assert.Single(loaded.Looks).Name);
            Assert.Equal(look.Accessories, loaded.Looks[0].Accessories);
            Assert.Equal(11, loaded.PersonalityId);
            Assert.Equal(-30, loaded.X);
            Assert.Equal(-12, loaded.Y);
            Assert.Empty(loaded.Sips);
            state.Looks = null!;
            state.Version = 16;
            state.Validate(now);
            Assert.Empty(state.Looks);
            Assert.Equal(-30, state.X);
        }
        finally { if (Directory.Exists(directory)) Directory.Delete(directory, true); }
    }
}
