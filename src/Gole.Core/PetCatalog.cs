namespace Gole.Core;

public static class PetCatalog
{
    public const int MaximumAccessories = 3;
    public static IReadOnlyList<string> Models
    {
        get;
    } = Array.AsReadOnly(new[]
    {
        "Clássica", "Esportiva", "Térmica", "Jarra", "Copo", "Caneca", "Pote de vidro",
        "Squeeze", "Slim", "Retrô", "Quadrada", "Arredondada", "Com alça"
    });
    public static IReadOnlyList<string> Accessories
    {
        get;
    } = Array.AsReadOnly(new[]
    {
        "Nenhum", "Headphone", "Óculos de sol", "Laço", "Flor", "Boné", "Gravata borboleta",
        "Bigode", "Cílios", "Coroa", "Bandana", "Colar de pérolas", "Chapéu", "Tiara de gatinho",
        "Gravata", "Brincos", "Cachecol", "Asas de anjo", "Asas de morcego", "Mochila",
        "Auréola", "Óculos de grau", "Tapa-olho", "Presilha de estrela", "Fones sem fio", "Boina",
        "Bolsa", "Saia", "Salto alto", "Vestido", "Batom", "Tiara floral", "Pulseiras"
    });

    public static List<int> Normalize(IEnumerable<int>? items) => (items ?? [])
        .Where(id => id > 0 && id < Accessories.Count)
        .Distinct().Take(MaximumAccessories).ToList();

    public static bool Toggle(List<int> items, int accessory)
    {
        if (accessory < 0 || accessory >= Accessories.Count)
            return false;
        if (accessory == 0)
        {
            items.Clear();
            return true;
        }
        if (items.Remove(accessory))
            return true;
        if (items.Count >= MaximumAccessories)
            return false;
        items.Add(accessory);
        return true;
    }
}
