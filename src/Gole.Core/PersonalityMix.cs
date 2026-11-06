namespace Gole.Core;

public sealed class PersonalityMix(Random random)
{
    public const int Maximum = 3;
    int last;
    public static List<int> Normalize(IEnumerable<int>? choices, int primary)
    {
        primary = PersonalityCatalog.Normalize(primary);
        return primary == 0 ? [] : new[] { primary }.Concat(choices ?? [])
            .Where(id => id > 0 && PersonalityCatalog.Normalize(id) == id).Distinct().Take(Maximum).ToList();
    }

    public int Next(IEnumerable<int> choices)
    {
        var pool = choices.Where(id => id > 0 && PersonalityCatalog.Normalize(id) == id).Distinct().Take(Maximum).ToArray();
        if (pool.Length == 0)
            pool = PersonalityCatalog.Options.Where(p => p.Id > 0).Select(p => p.Id).ToArray();
        var eligible = pool.Length > 1 ? pool.Where(id => id != last).ToArray() : pool;
        return last = eligible[random.Next(eligible.Length)];
    }
}
