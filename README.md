# Caminhos de Angola

Jogo 2D em **Unity 6.4 (6000.4.4f1)** feito por uma equipa de 3 pessoas. O projeto é pensado como uma **colecção de mini-jogos** com identidade angolana, ligados por um menu principal. O primeiro mini-jogo — e o único jogável por agora — é a **Corrida**: um *side-scroller* em que o jogador percorre lugares reais de Angola, recolhe itens históricos e foge do **Esquecimento**, uma névoa que avança sem parar.

> **Regra de ouro (Corrida):** o jogador **não ataca**. Só corre, salta, desvia e esconde-se. Não existe botão de ataque e não deve ser adicionado.

**Estado atual:** menu principal funcional e **fatia vertical jogável de Luanda** (protótipo). Os restantes caminhos e mini-jogos estão planeados e aparecem no menu como bloqueados ou "em breve".

---

## Índice

1. [Requisitos](#requisitos)
2. [Como abrir e jogar](#como-abrir-e-jogar)
3. [Menu principal e fluxo de jogo](#menu-principal-e-fluxo-de-jogo)
4. [Mini-jogos](#mini-jogos)
5. [Caminhos (níveis) da Corrida](#caminhos-níveis-da-corrida)
6. [A Corrida em detalhe](#a-corrida-em-detalhe)
7. [Identidade visual](#identidade-visual)
8. [Estrutura do projeto](#estrutura-do-projeto)
9. [Convenções de desenvolvimento](#convenções-de-desenvolvimento)
10. [Documentação](#documentação)
11. [Equipa](#equipa)

---

## Requisitos

- **Unity 6.4 (6000.4.4f1)** — a versão está registada em `ProjectSettings/ProjectVersion.txt`.
- **PC** (plataforma alvo do protótipo; Android fica para o futuro).
- Package **Unity Input System** (já incluído em `Packages/manifest.json`).

Não é preciso instalar nada à parte: ao abrir o projeto, o Unity resolve as dependências do `Packages/manifest.json`.

---

## Como abrir e jogar

1. Abrir o Unity Hub e selecionar esta pasta como projeto (`TestandoJogo`).
2. Deixar o Unity importar os assets na primeira abertura.
3. Abrir a cena `Assets/Scenes/MenuPrincipal.unity` e carregar **Play** (é a cena inicial do jogo).
   - Para testar só o nível de Luanda, abrir `Assets/Scenes/SampleScene.unity`.

### Controlos (teclado)

| Ação | Teclas |
|---|---|
| Andar | `A` / `D` ou `←` / `→` |
| Saltar (salto variável) | `Espaço`, `W` ou `↑` |
| Agachar | `S` ou `↓` |
| Interagir (ex.: entrar na Zungueira) | `E` |
| Pausa | `Esc` |
| Voltar no menu | `Esc` |

Em builds de teste (`UNITY_EDITOR` / `DEVELOPMENT_BUILD`), `F9` desbloqueia todos os níveis e `F10` repõe o progresso guardado.

---

## Menu principal e fluxo de jogo

O menu tem três passos, sempre com a mesma identidade visual:

```
Jogar  →  Escolher jogo  →  Escolher caminho (nível)  →  Jogo
```

1. **Jogar** — entrada do menu principal.
2. **Escolher jogo** — lista de mini-jogos. Só a **Corrida** está aberta; **Quiz**, **Jogo da Memória** e **Puzzle** aparecem como "em breve".
3. **Escolher caminho** — lista de níveis do mini-jogo escolhido. Só **Luanda** está aberta; os restantes ficam **bloqueados até concluir o anterior**.

O progresso (níveis desbloqueados) é guardado em `PlayerPrefs` (`ProgressoJogo`). Ao concluir Luanda, o caminho seguinte deve ficar desbloqueado.

Notas:
- O menu de escolha de jogo e de caminho é a base do jogo final; o protótipo de Luanda pode ser testado sem passar por ele (`SampleScene`).
- Os ecrãs de **Zungueira (loja)**, **Fim de jogo** e **Nível concluído** usam o mesmo tema visual do menu.

---

## Mini-jogos

O projeto vai ter vários mini-jogos, todos acessíveis a partir do mesmo menu:

| Mini-jogo | Descrição | Estado |
|---|---|---|
| **Corrida** | *Side-scroller* por lugares de Angola, com itens históricos, perigos e a névoa do Esquecimento | Jogável (só Luanda) |
| **Quiz** | Perguntas sobre a história e a cultura de Angola | Planeado ("em breve") |
| **Jogo da Memória** | Jogo de pares com imagens ligadas aos lugares e figuras do jogo | Planeado ("em breve") |
| **Puzzle** | Montar imagens em grelhas de dificuldade crescente (4×4, 6×6, 10×10, …) | Planeado ("em breve") |

---

## Caminhos (níveis) da Corrida

Cada província tem o seu cenário, a sua jogabilidade e os seus itens históricos.

| Caminho | Estado | Notas |
|---|---|---|
| **Luanda** | Jogável (protótipo) | Marginal → BNA → Palácio de Ferro → Soldados Desconhecidos → Largo 1.º de Maio → FILDA |
| **Malanje** | Bloqueado / por implementar | Pedras Negras de Pungo Andongo, a palanca negra gigante e as Quedas de Kalandula |
| **Huíla** | Bloqueado / por implementar | Província com mais paisagens (a confirmar pela equipa) |
| **Sala da Memória** | Bloqueado / por implementar | Nível final; o GDD prevê um confronto final (boss), por definir |

A ordem de desbloqueio é a da tabela. Os níveis ainda não implementados aparecem no menu com cadeado.

---

## A Corrida em detalhe

- **Personagem:** Agostinho Neto (o único jogável no protótipo). Está previsto o jogador **escolher a personagem** antes de jogar (ex.: Rainha Nzinga Mbande, Rei Mandume).
- **Moeda:** **Zimbo**, usado na loja da Zungueira.
- **Vidas:** 3. Ao chegar a zero, o nível recomeça.
- **Névoa (o Esquecimento):** segue o jogador desde o início; por onde passa, tudo fica nublado. Se apanhar o jogador, este volta ao último checkpoint e perde uma vida. Quando o jogador recolhe tudo e chega ao fim, a névoa desaparece.
- **Itens históricos:** em cada monumento há um item especial que o jogador tem de apanhar (7 em Luanda: igreja, BNA, palácio, soldados, largo, prédio do largo e FILDA).
- **O Pensador:** item escondido que faz a névoa recuar.
- **Checkpoints:** as **Zungueiras**. Com `E` o jogo pausa e abre a loja; entrar e sair não perde Zimbos nem itens.
- **Loja da Zungueira:** vende Asas (voar), Escudo e vida extra (esta última opcional no protótipo).
- **Fim de nível:** mostra o que foi recolhido vs. o que devia ter sido. Se faltar algum item histórico, o nível repete-se.

### Perigos de Luanda

Carros e candongueiros (apenas no troço da Marginal até à igreja, a andar da direita para a esquerda), moto (aparece de repente e tira 1 vida; pode ser saltada no momento certo), atirador (só dispara quando o jogador está a certa distância), blocos suspensos que aparecem e desaparecem, e valas. Os que ainda não têm arte definitiva usam *placeholders* com colisor e comportamento reais.

### Percurso e planos do cenário

O cenário de Luanda tem planos separados: **primeiro plano** (chão e camada de jogo), **plano principal** (monumentos e edifícios), **plano secundário** (prédios com menos cor), **plano terciário** (silhuetas de prédios e casas) e **céu/horizonte**.

---

## Identidade visual

O jogo tem uma identidade angolana aplicada a **todos** os ecrãs (menu, loja, fim de jogo, nível concluído) através do script `TemaPixel`:

- painéis escuros com losangos inspirados nos **lusona**;
- faixa decorativa inspirada na **capulana**;
- botões em forma de **bilhete**.

Estilo de arte: *pixel art* nítida e consistente (um só tamanho de pixel, paleta partilhada com tons de terra de Angola, mesma regra de contorno e mesma direção de luz). Ver as convenções abaixo.

---

## Estrutura do projeto

```
Assets/
├── Scenes/
│   ├── MenuPrincipal.unity   # Menu (Jogar → escolher jogo → escolher caminho)
│   └── SampleScene.unity     # Nível de Luanda
├── scripts/
│   ├── Jogador/              # ControladorJogador, Movimento
│   ├── Jogo/                 # vidas, colectáveis, checkpoints, névoa, progresso
│   ├── Perigos/              # trânsito, moto, atirador, bala, dano
│   ├── Interface/            # HUD, fim de nível/jogo, loja, TemaPixel
│   └── Menu/                 # menu principal
├── Prefabs/, Resources/, Placeholders/, powerups/
├── personagens_v1/, luanda_bg_separados/, Fisica/, Shaders/, Settings/
├── CaminhosDeAngola.inputactions   # Input Actions (mapa "Jogo")
└── Screenshots/
Docs/
├── ASSET_MANIFEST.md         # Checklist de arte e conteúdo
└── BackupArte/
CONTEXTO.md                   # Regras de jogo e contexto para a equipa/IA
```

Os scripts usam o namespace `CaminhosDeAngola.*` e valores afináveis no Inspector via `[SerializeField]`.

---

## Convenções de desenvolvimento

- **Uma tarefa de cada vez:** testar em Play Mode e fazer commit antes de avançar.
- **Naming de sprites:** `categoria_nome_variante_##.png`, PPU **100**, filtro **Point (no filter)**, pivot *bottom-center* para personagens/inimigos e *center* para props/UI. Todos os sprites virados para a direita (o código faz o *flip*).
- **Input:** usar o novo Input System (`Assets/CaminhosDeAngola.inputactions`). Nada de `Input.GetKey` legado.
- **Layers:** `Player`, `Chao`, `Perigo`, `Colectavel`, `Inimigo`, `Plataforma`, `Trigger`.
- **Tags:** `Player`, `Checkpoint`, `Monumento`, `Zungueira`, `Colectavel`.
- **Arte em falta:** usar placeholder (caixa colorida com o nome) com colisor e comportamento reais, para trocar só o sprite depois.
- **Interface nova:** usar sempre o `TemaPixel` para manter o mesmo estilo em todos os ecrãs.
- **Novos mini-jogos:** cada um deve ser independente da Corrida (cena e scripts próprios) e entrar no menu "Escolher jogo".
- Não alterar arte nem apagar objetos existentes da cena sem avisar a equipa.
- Em caso de dúvida sobre uma regra de jogo, perguntar em vez de inventar.

---

## Documentação

| Ficheiro | Para que serve |
|---|---|
| [`CONTEXTO.md`](CONTEXTO.md) | Visão geral, regras técnicas, mecânicas, perigos, ordem de trabalho |
| [`Docs/ASSET_MANIFEST.md`](Docs/ASSET_MANIFEST.md) | Inventário de sprites, áudio, UI e textos (✅ existe / ⚠️ fora do projeto / ❌ falta) |
| `HANDOFF_*.md` | Notas de passagem entre sessões de trabalho (o mais recente é o `HANDOFF_3.md`) |
| GDD v1.0 (Word) | Documento de design do jogo, feito a partir do modelo `MODELO-DE-GDD-CSJ.docx` |

---

## Equipa

Projeto de 3 pessoas para o Trabalho Multidisciplinar (12.ª Classe). Plataforma: PC.

*Última atualização: 18 de novembro de 2026.*