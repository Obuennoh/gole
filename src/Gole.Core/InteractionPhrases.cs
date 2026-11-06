namespace Gole.Core;

public sealed class InteractionPhrases(Random random)
{
    int last = -1;
    static readonly string[][] Touch =
    [
        ["Autógrafo ou aceno? Hoje o aceno tá liberado.", "Você chamou a estrela desse cantinho? Cheguei."],
        ["Oi! Que bom ter sua companhia por aqui.", "Tô aqui pertinho de você. Recebe meu aceno!"],
        ["Toca aqui, parceiro! Vamos no nosso ritmo.", "Time reunido! Uma comemoração pequenininha pra gente."],
        ["Um encontro breve no meio do dia. Que bom.", "Parei um instante pra te receber com calma."],
        ["Chamaram a protagonista? Preparem minha entrada!", "Luzes em mim! Esse aceno merece um primeiro plano."],
        ["O-oi! Eu tava lendo, mas guardei a página pra falar com você.", "Você me chamou? Até ajeitei os óculos de alegria!"],
        ["Convite de amizade recebido! Toca aqui, player.", "Emote de saudação equipado. Oi, parceiro!"],
        ["Um toque pode ser uma conversa bem pequena, não acha?", "Você passou por aqui e o instante ganhou companhia."],
        ["Arrr! Capitão no convés! Aceno a bombordo.", "Marujo, recebi seu sinal. Seguimos juntos nessa aventura!"],
        ["Tô aqui, criatura! Tá bom, vai um aceno.", "Chamou? Eu ouvi. Não espalha que fiquei contente."],
        ["Oi... afastei a franja só pra te ver melhor.", "Pausei a playlist. Esse toque mereceu minha atenção."]
    ];
    static readonly string[][] Pet =
    [
        ["Pode continuar. Meu ego aprovou esse atendimento.", "Carinho de qualidade. Vou dar cinco estrelinhas."],
        ["Ahh, que carinho gostoso. Um abraço de garrafinha pra você.", "Meu cantinho ficou mais quentinho com esse carinho."],
        ["Pausa de carinho também faz parte do plano!", "Energia de companheirismo renovada. Obrigado!"],
        ["Que tranquilidade... vou ficar aqui mais um instante.", "Esse carinho chegou como uma brisa leve."],
        ["Que cena linda! Não cortem, deixem a câmera rodar!", "Derreti de carinho. Alguém chama o figurinista!"],
        ["E-eu fiquei todo bobo... posso ganhar mais uma estrelinha?", "Meu coraçãozinho fez uma descoberta: adoro esse carinho."],
        ["Buff de carinho recebido. Parceiro feliz!", "Conquista desbloqueada: pet derretido de fofura."],
        ["Às vezes, o melhor argumento é um gesto gentil.", "Esse instante pequeno ganhou um significado bonito."],
        ["Arrr... esse carinho vale mais que um baú de ouro.", "O capitão conquistou o coração da tripulação!"],
        ["Tá, pode continuar. Mas não conta que eu gostei.", "Minha bronca tirou uma folga. Esse carinho tá aprovado."],
        ["A franja escondeu meu sorriso... mas ele tá bem aqui.", "Playlist triste, coração quentinho. Culpa do seu carinho."]
    ];
    public string Next(int personality, bool pet)
    {
        int id = PersonalityCatalog.Normalize(personality);
        if (id == 0)
            id = 1 + random.Next(Touch.Length);
        int pick = random.Next(2);
        if (pick == last)
            pick = 1 - pick;
        last = pick;
        return (pet ? Pet : Touch)[id - 1][pick];
    }
}
