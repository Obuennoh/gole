namespace Gole.Core;

internal static class PhraseSelection
{
    internal static string Next(Random random, IReadOnlyList<string> messages, string? previous)
    {
        int previousIndex = -1;
        for (int i = 0; i < messages.Count; i++)
            if (messages[i] == previous)
            {
                previousIndex = i;
                break;
            }
        int index = random.Next(messages.Count - (previousIndex >= 0 ? 1 : 0));
        if (previousIndex >= 0 && index >= previousIndex)
            index++;
        return messages[index];
    }
}
