# Fontes dos sons

Os efeitos adaptados abaixo foram publicados por **rubberduck** sob [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/), permitindo adaptação e redistribuição.

| Arquivo do app | Original | Fonte |
| --- | --- | --- |
| `water-drops.wav` | `loop_water_01.ogg` | [40 CC0 water / splash / slime SFX](https://opengameart.org/content/40-cc0-water-splash-slime-sfx) |
| `bubbles.wav` | `loop_bubbles_1.ogg` | Mesmo pacote de água |
| `water-stream.wav` | `loop_water_02.ogg` | Mesmo pacote de água |
| `soft-chime.wav` | `bell_01.ogg` | [100 CC0 SFX](https://opengameart.org/content/100-cc0-sfx) |

Os originais foram obtidos pela [cópia pública dos pacotes](https://github.com/lavenderdotpet/CC0-Public-Domain-Sounds). O processamento aplica recorte, conversão para mono PCM 16-bit/44,1 kHz, ajuste de nível e fades curtos nas extremidades. Não são incluídos arquivos executáveis dessa coleção.

`single-drop.wav` é o efeito original do Gole e continua como padrão.

Para reconstruir os quatro efeitos adaptados, instale `numpy` e `soundfile==0.13.1` em um ambiente de desenvolvimento e execute:

```powershell
python scripts/prepare-audio.py pasta-dos-originais
```

O aplicativo instalado usa os WAVs incorporados e não precisa dessas ferramentas nem de conexão para tocar os sons.
