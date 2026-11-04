using System.Runtime.Serialization;

namespace Gole.Core;

[DataContract]
public sealed record AccessoryStyle
{
    [DataMember]
    public int AccessoryId
    {
        get; set;
    }
    [DataMember] public string Color { get; set; } = "";
    [DataMember]
    public int Pattern
    {
        get; set;
    }

    public static IReadOnlyList<string> Patterns
    {
        get;
    } = Array.AsReadOnly(new[]
    {
        "Liso", "Bolinhas", "Listras", "Brilho", "Xadrez", "Estrelas", "Corações"
    });

    public static List<AccessoryStyle> Normalize(IEnumerable<AccessoryStyle>? styles) => (styles ?? [])
        .Where(s => s is not null && s.AccessoryId > 0 && s.AccessoryId < PetCatalog.Accessories.Count)
        .GroupBy(s => s.AccessoryId).Select(g => g.Last())
        .Select(s => new AccessoryStyle
        {
            AccessoryId = s.AccessoryId,
            Color = HydrationState.IsColor(s.Color) ? s.Color.ToUpperInvariant() : "",
            Pattern = s.Pattern >= 0 && s.Pattern < Patterns.Count ? s.Pattern : 0
        }).Where(s => s.Color.Length > 0 || s.Pattern != 0).OrderBy(s => s.AccessoryId).ToList();
}
