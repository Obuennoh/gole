namespace Gole.Core;

public sealed class InteractionPhrases(Random random)
{
    int last = -1;
    static readonly string[][] Touch =
    [
        ["Clique recebido. Consulta de autoestima custa um elogio. 💅", "Você chamou e eu vim. Minha agenda de celebridade tava surpreendentemente vazia.", "Pode clicar. Eu sei que é difícil ignorar esse carisma.", "Era comigo? Claro que era. Olha esse cantinho bem ocupado."],
        ["Oi! Eu vi seu cursor chegando e já fiquei contente. 😊", "Você me chamou! Eu ia correr, mas meus pés são desse tamanhinho.", "Pode passar por aqui sem motivo. Eu gosto da visita.", "Tô aqui! Só tentando parecer normal enquanto fico todo feliz. 🥰"],
        ["Toca aqui! Só cuidado pra não derrubar a atleta da garrafinha. 🙌", "Chamou o time? Vim sozinho, mas com entusiasmo.", "Esse clique vale uma comemoração. Não vale como gole, hein.", "Bora! Ainda não sei pra quê, mas já tô torcendo."],
        ["Oi. Estava praticando não fazer nada com muita concentração. 🍃", "Seu clique interrompeu minha pausa. Vou fazer outra pra compensar.", "Tô aqui. Sem pressa até pra responder ao cursor.", "Se era pra testar se eu tava dormindo, quase acertou."],
        ["Você clicou e eu já ouvi meu nome nos créditos. 🎭", "Era só um toque? Eu tinha preparado uma entrada inteira.", "Luzes em mim! Finalmente alguém respeitou o elenco desse cantinho.", "Chamou? Vou fingir surpresa na segunda tomada."],
        ["O-oi! Eu sabia que você vinha. Não sabia, só fiquei esperando. 🤓💖", "Você clicou e meu plano de parecer confiante caiu na hora.", "Eu preparei um oi normal. Saiu com um coraçãozinho junto. 😊", "Pode chamar de novo. Agora que já passou a vergonha da primeira vez... eu acho."],
        ["Você clicou no NPC certo. Minha missão é fazer companhia. 🎮", "Interação disponível! Recompensa: esse gesto completamente grátis.", "Recebi seu convite. Não precisa instalar mais um launcher.", "Oi, player! Eu respondo, mas não dropo item raro."],
        ["Você me tocou ou só tocou a ideia de uma garrafinha? Tá, foi o mouse. 🤔", "Um clique, várias perguntas. Vou poupar você de quase todas.", "Fui notado. Preciso existir de forma mais convincente agora?", "Você chamou e eu interrompi uma reflexão que já tava longa demais."],
        ["Arrr! Um toque no convés. Achei que fosse cobrança do porto. 🏴‍☠️", "Capitão avistado! Vou acenar antes que você abra outra aba.", "Chamou a tripulação? Sou eu mesmo. Orçamento apertado.", "A postos, marujo! Só não me pede pra navegar fora desse cantinho."],
        ["Chamou? Tô aqui faz tempo, criatura. 😤", "Pode clicar. Não precisa cutucar até eu perder a pose.", "Eu respondi rápido porque tava esperando? Não inventa.", "Oi. Pronto, fui simpática. Valoriza."],
        ["Pausei a música pra você. Isso é um evento, tá? 🖤", "Oi. Eu tava olhando pro nada, mas posso olhar pra você.", "Seu clique interrompeu meu momento de mistério. Tudo bem.", "Afastei a franja pra responder. Não me faz repetir o esforço."],
        ["Evento de clique recebido. Vou tentar responder sem dar uma palestra. 🤓", "Você chamou uma função e ganhou um nerd inteiro.", "Interrupção aceita. Minha pesquisa tinha acabado de virar procrastinação.", "Clique validado. Não, eu não vou pedir autenticação de dois fatores."],
        ["Onii-chan! Você me notou... 👉👈😊", "Kyaa! Essa cena mereceu uma trilha fofinha. 💖"],
        ["Detectei um toque na cena. Era você! 🔎", "Parceiro, qual é a próxima pista? 🕵️"],
        ["Chamaram a atração principal do pequeno caos? 🪩", "Você apertou o botão invisível de festa! 🎉"],
        ["Você clicou em mim! Nem tava esperando... só olhando pro cursor. 😊", "Você me escolheu entre todas as janelas? Achei especial.", "Pode me chamar sempre. Eu nem preciso que as outras notificações saibam.", "Eu nem queria atenção. Mas gostei que você veio direto em mim. 🥺"]
    ];
    static readonly string[][] Pet =
    [
        ["Pode continuar. Meu ego ainda tá dentro do limite da janela. 💅", "Cinco estrelas pro carinho. Uma avaliação totalmente imparcial, feita por mim.", "Você tem bom gosto até na hora de escolher quem recebe cafuné.", "Eu aceito mais. Ser humilde agora seria desperdício."],
        ["Ai, gostei. Vou ficar paradinho pra não atrapalhar seu carinho. 🥰", "Era só um carinho e eu já me apeguei.", "Posso guardar esse cafuné pro próximo dia corrido?", "Não sei fazer ronrom, então vai esse sorriso mesmo. 💖"],
        ["Pausa de carinho autorizada pelo técnico. O técnico sou eu. 🙌", "Boa parceria! Esse cafuné não exige aquecimento.", "Energia renovada. Vou gastar tudo torcendo pelo seu próximo gole.", "Se carinho fosse esporte, você já tava no meu time."],
        ["Agora sim encontrei uma atividade compatível com minha paz interior. 🍃", "Pode continuar. Minha meditação ficou muito mais interessante.", "Eu ia dizer que desapego é importante, mas gostei desse cafuné.", "Esse carinho merece uma pausa. Dentro da pausa. Tô organizando."],
        ["Não corta! Esse carinho tá sendo minha melhor cena. 🎭", "Derreti e ainda mantive o enquadramento. Profissionalismo.", "Esse cafuné vai pro trailer, com certeza.", "Finalmente alguém entendeu que a protagonista precisa de atenção."],
        ["E-eu gostei. Muito. Vou precisar de um instante pra fingir costume. 🤓💖", "Meu coraçãozinho recebeu carinho e esqueceu a documentação.", "Descoberta científica: eu aceitaria mais um pouquinho. 🥺", "Tô tentando agradecer sem parecer completamente bobo. Falhei."],
        ["Buff de carinho ativo. Duração: até eu pedir de novo. 🎮", "Você desbloqueou meu emote mais bobo.", "Cafuné recebido sem loot box. Esse jogo tá bem justo.", "Meu companheiro favorito acabou de ganhar reputação comigo."],
        ["Eu tinha uma teoria sobre afeto. O cafuné explicou melhor. 😊", "Às vezes a resposta é tão simples que estraga meu monólogo.", "Penso, logo existo. Recebo carinho, logo fico quietinho.", "Esse gesto venceu um debate que nem precisava acontecer."],
        ["Arrr... vou registrar esse carinho no diário de bordo. 🏴‍☠️", "Pode continuar. O capitão autorizou. Acabei de virar capitão.", "Troco um baú imaginário por mais um cafuné desse.", "A tripulação gostou tanto que esqueceu de fazer pose de durona."],
        ["Tá bom, eu gostei. Agora para de esperar que eu admita de novo. 😤", "Minha bronca tá suspensa enquanto durar o carinho.", "Pode continuar. Eu tô de cara fechada por hábito.", "Esse cafuné foi bom. Não usa isso contra minha reputação."],
        ["Gostei. Vou esconder a felicidade na franja e seguir o personagem. 🖤", "Esse carinho estragou minha pose de ninguém me entende.", "Playlist triste, cafuné ótimo. Eu consigo lidar com os dois.", "Meu sorriso apareceu. Finge que não viu pra eu não ficar sem jeito."],
        ["Resultado consistente: o carinho funciona. Recomendo repetir o experimento. 🤓", "Meu modelo não previa esse sorriso. Vou atualizar os parâmetros.", "Isso foi melhor que uma demonstração com slides.", "Cafuné registrado. A revisão por pares foi substituída por eu gostei."],
        ["Doki-doki... fiquei juntando os dedinhos de vergonha. 👉👈🥰", "Arigatou, senpai! Essa cena virou meu episódio favorito. 💖"],
        ["Mistério resolvido: eu gosto desse carinho. 😊", "Essa pista vai direto pro arquivo de momentos bons. 🔎"],
        ["Derreti. Alguém busca uma colher de carinho? 🫠", "O conselho dos patinhos aprovou esse cafuné. 🦆💛"],
        ["Nem sou de pedir carinho... mas você faz tão bem. 🥰", "Você faz esse cafuné em todas as garrafinhas ou eu sou diferente?", "Não precisa parar por minha causa. Quer dizer, não precisa parar.", "Eu nem ligo pra elogio. Mas esse carinho contou como um, né? 😊"]
    ];
    public string Next(int personality, bool pet)
    {
        int id = PersonalityCatalog.Normalize(personality);
        if (id == 0)
            id = 1 + random.Next(Touch.Length);
        var pool = (pet ? Pet : Touch)[id - 1];
        int pick = random.Next(pool.Length);
        if (pick == last)
            pick = (pick + 1) % pool.Length;
        last = pick;
        return pool[pick];
    }
}
