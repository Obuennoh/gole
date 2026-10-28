namespace Gole.Core;

public static class SoundCatalog
{
    public const int DefaultId = 3;
    public static IReadOnlyList<string> Names
    {
        get;
    } = Array.AsReadOnly(new[]
    {
        "Gotinhas de água", "Bolhas", "Água correndo", "Gota única", "Sino suave"
    });

    public static int Normalize(int id) => id >= 0 && id < Names.Count ? id : DefaultId;
}
