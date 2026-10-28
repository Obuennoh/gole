# Validação

## Rotina reproduzível

```powershell
./scripts/check.ps1
```

Ambiente local de referência: Windows, SDK .NET 10.0.403. A solução usa `TreatWarningsAsErrors` e formatação definida em `.editorconfig`.

## Cobertura automatizada

| Área | Verificação |
| --- | --- |
| Ciclo | Cheio no início, metade em 15 minutos e vazio aos 30 minutos |
| Registro | Volume configurado por gole, padrão de 200 ml, reposição completa, total diário, validação de quantidade e reinício do prazo |
| Lembretes | Emissão no prazo, sem duplicação nem repetição de intervalos perdidos |
| Personalidades | Cobertura dos seis humores, frases e reações exclusivas de cada estilo, mistura aleatória, ausência de repetição imediata, fallback de IDs e persistência |
| Falas | Prazo de cinco minutos, mensagem por humor e ausência de duplicação com lembretes |
| Após beber | Elogios sem números ou volume, com variedade e sem repetição consecutiva |
| Pausa | Nível congelado, retomada antecipada e conservação do tempo restante |
| Suspensão | Tick atrasado após a pausa produz no máximo um aviso |
| Configuração | Intervalo alterado durante atividade ou pausa |
| Desfazer | Sem limite de tempo, inclusive após reabrir; remoção individual sem restaurar registros já apagados e recálculo do nível ao apagar o mais recente |
| Histórico | Todos os dias, paginação, registros com mesmo horário, correção da última página após remover, cancelamento e confirmação da limpeza completa |
| Total diário | Contagem pela meia-noite local, independente da meia-noite UTC |
| Persistência | Reabertura com visual, três acessórios, configurações e posição preservados; gravação atômica e falha de escrita sem alteração do principal |
| Recuperação | Fallback para backup e preservação da cópia válida após recuperar |
| Compatibilidade | Migração de registros anteriores e rejeição de schema futuro |
| Editor | Preview de visual e falas, troca de personalidade sem aplicação antecipada, botão Outra frase, descarte e salvamento do estilo |
| Seleção visual | Borda efetiva do template ao marcar e desmarcar repetidamente; foco separado da seleção |
| Acessórios | Três itens simultâneos, bloqueio do quarto, remoção e limpeza |
| Expressões | Seis faixas e seus limites exatos ao longo do esvaziamento |
| Desenho | Renderização em memória de todos os modelos e expressões e de todos os acessórios |
| Integração do painel | Configuração de ml por gole, salvamento, bloqueio de valores inválidos, registro em um clique e desfazer por eventos reais dos botões |
| Sons | Cinco recursos WAV válidos, volume aplicado apenas às amostras, silêncio a 0%, preview com o volume do rascunho e persistência da escolha, sem reprodução real durante testes |
| Atualizador | Download automático sem configuração, falhas de rede com nova tentativa, aplicação ao sair e verificações serializadas |
| Migração do atualizador | Remoção de credenciais antigas sem alterar as preferências |

Os testes de domínio e de persistência usam instantes controlados e diretórios temporários. O teste do painel roda em uma thread STA, sem chamar `Show`, sem ícone de bandeja, sem automação de mouse/teclado e sem ler a tela do usuário.

## Verificação local realizada

- Compilação completa sem erros ou avisos.
- Testes de regras, persistência e comportamento do editor.
- Inicialização local da aplicação e renderização em memória dos modelos, expressões e acessórios.

A folha em `docs/images/characters.png` foi gerada pelo componente de desenho. Ela não é uma captura da área de trabalho.

## Limites

Os testes de controles verificam comportamento e estado; não substituem a validação visual de layout nem testes de integração com notificações e múltiplos monitores.

O job de release instala o pacote silenciosamente em um runner Windows descartável, verifica a versão instalada, gera uma segunda versão e aplica uma atualização real por feed local. Depois confere a versão final e o hash de um arquivo de preferências. O script se recusa a executar fora do GitHub Actions. A publicação só ocorre após esse teste; nenhuma instalação de teste é feita no computador de desenvolvimento.

Após a publicação, o runner consulta o feed público real sem credenciais. Esse teste complementa a instalação e a aplicação ao sair usando as mesmas classes do aplicativo com um feed local controlado.
