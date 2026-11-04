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
        "Squeeze", "Slim", "Retrô", "Quadrada", "Arredondada", "Com alça",
        "Com canudo", "Dobrável", "Cantil", "Long neck", "Galão fitness", "Com capa", "Triangular", "Gota"
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
        "Bolsa", "Saia", "Salto alto", "Vestido", "Batom", "Tiara floral", "Pulseiras",
        "Chapéu de bruxa", "Gorro", "Orelhas de coelho", "Tiara de coração", "Óculos de coração",
        "Óculos futuristas", "Brincos de estrela", "Colar de coração", "Relógio", "Tênis", "Botas",
        "Jaqueta", "Capa de herói", "Jetpack", "Varinha", "Balão de coração", "Boia", "Tiara de unicórnio",
        "Camiseta", "Moletom", "Jardineira", "Casaco", "Saia plissada", "Saia de tule",
        "Colete", "Poncho", "Chapéu de pescador", "Tiara de laço", "Óculos redondos", "Lenço de pescoço",
        "Cinto", "Broche de coração", "Bolsa transversal", "Chinelo"
    });

    static int Slot(int id) => id switch
    {
        5 or 9 or 12 or 25 or 33 or 34 or 59 => 1,
        2 or 21 or 22 or 37 or 38 or 61 => 2,
        27 or 29 or 53 or 55 or 56 => 3,
        44 or 51 or 52 or 54 or 57 or 58 => 4,
        28 or 42 or 43 or 66 => 5,
        13 or 31 or 35 or 36 or 50 or 60 => 6,
        _ => 0
    };

    public static bool Conflicts(int first, int second) => first != second && Slot(first) != 0 && Slot(first) == Slot(second);

    public static bool CanEquip(IReadOnlyCollection<int> items, int accessory) => items.Count < MaximumAccessories
        || items.Contains(accessory) || items.Any(id => Conflicts(id, accessory));

    public static List<int> Normalize(IEnumerable<int>? items)
    {
        var result = new List<int>();
        foreach (int id in (items ?? []).Where(id => id > 0 && id < Accessories.Count).Distinct())
        {
            if (!CanEquip(result, id))
                continue;
            result.RemoveAll(other => Conflicts(other, id));
            result.Add(id);
        }
        return result;
    }

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
        if (!CanEquip(items, accessory))
            return false;
        items.RemoveAll(id => Conflicts(id, accessory));
        items.Add(accessory);
        return true;
    }
}
