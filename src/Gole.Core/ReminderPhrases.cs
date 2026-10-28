namespace Gole.Core;

public static class ReminderPhrases
{
    private static readonly string[][] Groups =
    [
        [
            "Tô feliz da vida! Você e a água: dupla de milhões.",
            "Barriguinha cheia, coração quentinho. Mandou bem!",
            "Se hidratar fosse esporte, você já tava no pódio.",
            "Esse brilho todo é água e autoestima. Segue assim!"
        ],
        [
            "Tô de boa! Um golinho daqui a pouco mantém o charme.",
            "Ainda tô tranquilo. Só não esquece seu parceiro aqui.",
            "Seu café pode esperar. A água também merece carinho.",
            "Passando pra lembrar que a gente fica mais bonito hidratado."
        ],
        [
            "Já fiquei sério aqui. Bora organizar esse próximo gole?",
            "Humano, foco. A água tá perdendo essa disputa pras abas abertas.",
            "Tô na metade da alegria. Um gole resolve o clima.",
            "Minha cara de paisagem é um pedido discreto de água."
        ],
        [
            "Meu sorriso foi de férias. Vai pegar água pra ele voltar?",
            "Tá esperando eu virar uma uva-passa?",
            "A reunião podia ser um e-mail. Essa tristeza podia ser um gole.",
            "Eu tô murchando e você aí. Bora de água, pô."
        ],
        [
            "Tô chorando a pouca água que me resta, porra. Vai beber!",
            "Até a planta da sala tá preocupada comigo. Pega água!",
            "Caralho, falta pouco pra eu fazer cosplay de poeira.",
            "Você quer um pet ou uma uva-passa? Água, porra!"
        ],
        [
            "Morri de sede aqui, porra. Um gole pra ressuscitar o parceiro!",
            "Parabéns, virou dono de um fóssil. Vai beber água!",
            "Alô, além? Meu humano esqueceu a água de novo.",
            "Nem zumbi aguenta essa secura. Levanta e pega água, caralho."
        ]
    ];

    public static string Next(Random random, HydrationMood mood)
    {
        ArgumentNullException.ThrowIfNull(random);
        var group = Groups[(int)mood];
        return group[random.Next(group.Length)];
    }
}
