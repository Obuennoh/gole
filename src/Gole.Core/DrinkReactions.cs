namespace Gole.Core;

public sealed class DrinkReactions(Random random)
{
    private static readonly string[] Messages =
    [
        "Aí sim! Você cuidou de mim e de você. Mandou bem!",
        "Meu sorriso voltou! Você é meu humano favorito.",
        "Boa! O deserto que lute, a gente tá hidratado.",
        "Que orgulho desse humano! Segue nesse ritmo.",
        "Renascemos! Toca aqui, parceiro de hidratação.",
        "É disso que eu tô falando! Você arrasou.",
        "A felicidade voltou a circular por aqui. Obrigado!",
        "Você lembrou! Ganhei o dia e um sorrisão.",
        "Meu humano tá brilhando! Cuidar de si fica bem em você.",
        "Isso aí! Missão aguinha cumprida com estilo."
    ];
    private readonly Random random = random ?? throw new ArgumentNullException(nameof(random));
    private int lastIndex = -1;

    public string Next()
    {
        int index = random.Next(lastIndex < 0 ? Messages.Length : Messages.Length - 1);
        if (lastIndex >= 0 && index >= lastIndex)
            index++;
        lastIndex = index;
        return Messages[index];
    }
}
