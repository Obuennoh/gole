# Segurança e privacidade

O histórico de água, o visual e as preferências ficam neste computador, em `%LOCALAPPDATA%\Gole`. O aplicativo não envia esses dados e não possui telemetria.

A única conexão de rede do aplicativo é usada para consultar e baixar atualizações públicas de `https://github.com/Obuennoh/gole`. Não é necessário informar senha ou token. Os pacotes são verificados pelo Velopack antes da aplicação.

As atualizações são preparadas em segundo plano e aplicadas ao sair. Os dados ficam separados da instalação. Se uma versão anterior tiver armazenado uma credencial de atualização, o aplicativo remove esse arquivo local sem utilizá-lo.

Não publique credenciais, arquivos de preferências, logs pessoais ou dados reais em issues. Para comunicar uma vulnerabilidade, utilize o canal privado de segurança do repositório.
