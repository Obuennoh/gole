namespace Gole.Core;

public static class SoundCatalog
{
    public const int DefaultId = 3;
    public const int DefaultVolume = 70;
    public static IReadOnlyList<string> Names
    {
        get;
    } = Array.AsReadOnly(new[]
    {
        "Água no copo", "Bolhas", "Água correndo", "Gota única", "Sino suave"
    });

    public static int Normalize(int id) => id >= 0 && id < Names.Count ? id : DefaultId;
}
