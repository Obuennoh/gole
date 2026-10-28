using System.Text.Json;

namespace Gole.Core;

public sealed record PersonalityOption(int Id, string Name, string Description);

public static class PersonalityCatalog
{
    private sealed class Voice
    {
        public int Id
        {
            get; set;
        }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string[][] Reminders { get; set; } = [];
        public string[] Reactions { get; set; } = [];
    }

    private static readonly Voice[] Voices = Load();
    public static IReadOnlyList<PersonalityOption> Options
    {
        get;
    } = Array.AsReadOnly(
        new[] { new PersonalityOption(0, "Aleatória", "Mistura todos os estilos a cada fala.") }
        .Concat(Voices.Select(v => new PersonalityOption(v.Id, v.Name, v.Description))).ToArray());
    private static readonly IReadOnlyList<string>[][] ReminderPools = Enumerable.Range(0, Voices.Length + 1)
        .Select(id => Enumerable.Range(0, 6).Select(mood => (IReadOnlyList<string>)Array.AsReadOnly(
            id == 0 ? Voices.SelectMany(v => v.Reminders[mood]).ToArray() : Voices[id - 1].Reminders[mood])).ToArray()).ToArray();
    private static readonly IReadOnlyList<string>[] ReactionPools = Enumerable.Range(0, Voices.Length + 1)
        .Select(id => (IReadOnlyList<string>)Array.AsReadOnly(id == 0 ? Voices.SelectMany(v => v.Reactions).ToArray() : Voices[id - 1].Reactions)).ToArray();

    public static int Normalize(int id) => id >= 0 && id <= Voices.Length ? id : 0;
    public static PersonalityOption Get(int id) => Options[Normalize(id)];
    public static IReadOnlyList<string> Reminders(int id, HydrationMood mood)
    {
        if (!Enum.IsDefined(mood))
            throw new ArgumentOutOfRangeException(nameof(mood));
        return ReminderPools[Normalize(id)][(int)mood];
    }
    public static IReadOnlyList<string> Reactions(int id) => ReactionPools[Normalize(id)];

    private static Voice[] Load()
    {
        using var stream = typeof(PersonalityCatalog).Assembly.GetManifestResourceStream("Gole.Core.Content.personalities.json")
            ?? throw new InvalidOperationException("Missing personality catalog.");
        var voices = JsonSerializer.Deserialize<Voice[]>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Invalid personality catalog.");
        voices = voices.OrderBy(v => v.Id).ToArray();
        for (int i = 0; i < voices.Length; i++)
        {
            var voice = voices[i];
            if (voice.Id != i + 1 || string.IsNullOrWhiteSpace(voice.Name) || string.IsNullOrWhiteSpace(voice.Description)
                || voice.Reminders.Length != 6 || voice.Reminders.Any(p => p.Length < 8 || p.Any(string.IsNullOrWhiteSpace) || p.Distinct().Count() != p.Length)
                || voice.Reactions.Length < 8 || voice.Reactions.Any(string.IsNullOrWhiteSpace) || voice.Reactions.Distinct().Count() != voice.Reactions.Length)
                throw new InvalidOperationException("Incomplete personality content.");
        }
        return voices.Length > 0 ? voices : throw new InvalidOperationException("Empty personality catalog.");
    }
}
