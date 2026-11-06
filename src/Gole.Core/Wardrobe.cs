using System.Globalization;
using System.Runtime.Serialization;

namespace Gole.Core;

[DataContract]
public sealed class SavedLook
{
    [DataMember] public string Name { get; set; } = "Look";
    [DataMember] public string Color { get; set; } = "#26B9C4";
    [DataMember]
    public int Vessel
    {
        get; set;
    }
    [DataMember] public List<int> Accessories { get; set; } = [];
    public SavedLook Copy() => new() { Name = Name, Color = Color, Vessel = Vessel, Accessories = Accessories.ToList() };
}

public static class Wardrobe
{
    public const int MaximumLooks = 30;
    public static readonly string[] Categories = ["Todos", "Vestidos", "Roupas", "Cabeça", "Rosto", "Calçados", "Joias", "Objetos", "Costas", "Maquiagem"];
    public static string Category(int id) => id switch
    {
        29 or 90 => "Vestidos",
        30 or >= 91 and <= 95 => "Maquiagem",
        96 or 97 or 98 => "Joias",
        99 => "Rosto",
        27 or 44 or >= 51 and <= 58 or 63 or >= 83 and <= 89 => "Roupas",
        28 or 42 or 43 or 66 or 79 or 80 => "Calçados",
        17 or 18 or 19 or 45 or 46 or 77 => "Costas",
        2 or 7 or 8 or 21 or 22 or 37 or 38 or 61 or 68 => "Rosto",
        6 or 11 or 14 or 15 or 16 or 32 or 39 or 40 or 41 or 62 or 64 or 71 or 72 or 81 => "Joias",
        26 or 47 or 48 or 49 or 65 or 69 or 78 or 82 => "Objetos",
        _ => "Cabeça"
    };
    public static bool Matches(int id, string? query, string category) => id == 0 ||
        (category == "Todos" || Category(id) == category) && CultureInfo.GetCultureInfo("pt-BR").CompareInfo.IndexOf(
            PetCatalog.Accessories[id], query?.Trim() ?? "", CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
    public static List<SavedLook> Normalize(IEnumerable<SavedLook>? looks) => (looks ?? []).Where(l => l is not null)
        .Take(MaximumLooks).Select(l => new SavedLook
        {
            Name = string.IsNullOrWhiteSpace(l.Name) ? "Look" : l.Name.Trim()[..Math.Min(32, l.Name.Trim().Length)],
            Color = HydrationState.IsColor(l.Color) ? l.Color.ToUpperInvariant() : "#26B9C4",
            Vessel = Math.Clamp(l.Vessel, 0, PetCatalog.Models.Count - 1),
            Accessories = PetCatalog.Normalize(l.Accessories)
        }).ToList();

    // Curated palettes and compatible sets: suggestions never repaint accessories.
    static readonly (string Color, int[] Items)[] Sets =
    [
        ("#9B73CB", [67,71,55,44,79]), ("#26B9C4", [68,69,57,80,81]),
        ("#94BD75", [73,76,77,52,42]), ("#6D69C9", [74,72,78,29,43]),
        ("#D875B3", [70,68,82,56,80]), ("#7EA8B0", [75,69,51,79,81]),
        ("#EDAA61", [59,62,53,66,26]), ("#E694A7", [60,40,55,57,28]),
        ("#456477", [22,12,14,54,43]), ("#3C82D5", [1,38,51,42,19]),
        ("#CFB8A3", [25,61,69,54,43]), ("#45A58C", [35,11,29,80,48]),
        ("#9B73CB", [67,91,93,98,43]), ("#26B9C4", [99,96,57,69,42]),
        ("#D875B3", [68,92,87,80,97])
    ];
    public static SavedLook Suggest(Random random, int vessel, IEnumerable<int>? previous = null)
    {
        var candidates = Sets.Where(s => previous is null || !s.Items.Order().SequenceEqual(previous.Order())).ToArray();
        var set = candidates[random.Next(candidates.Length)];
        return new SavedLook { Name = "Meu look", Vessel = Math.Clamp(vessel, 0, PetCatalog.Models.Count - 1), Color = set.Color, Accessories = set.Items.ToList() };
    }
}
