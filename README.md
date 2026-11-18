# Caminhos de Angola

Jogo 2D *side-scroller* em desenvolvimento, feito em **Unity 6.4 (6000.4.4f1)**. O jogador percorre lugares reais de Angola, recolhe itens históricos e foge do **Esquecimento** — uma névoa que avança sem parar.

> **Regra de ouro:** o jogador **não ataca**. Só corre, salta, desvia e esconde-se. Não existe botão de ataque e não deve ser adicionado.

Estado atual: **fatia vertical jogável de Luanda** (protótipo). Malanje, Huíla e a Sala da Memória estão planeadas mas ainda por implementar.

---

## Requisitos

- **Unity 6.4 (6000.4.4f1)** — a versão está registada em `ProjectSettings/ProjectVersion.txt`.
- **PC** (platforma alvo do protótipo).
- Package **Unity Input System** (já incluído em `Packages/manifest.json`).

Não é preciso instalar nada à parte: ao abrir o projeto, o Unity resolve as dependências do `Packages/manifest.json`.

---

## Como abrir e jogar

1. Abrir o Unity Hub e selecionar esta pasta como projeto (`TestandoJogo`).
2. Deixar o Unity importar os assets na primeira abertura.
3. Abrir a cena `Assets/Scenes/MenuPrincipal.unity` e carregar **Play**.
   - Alternativamente, abrir `Assets/Scenes/SampleScene.unity` para entrar diretamente no nível de Luanda.

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

## O jogo

- **Personagem:** Agostinho Neto (o único jogável no protótipo).
- **Moeda:** **Zimbo** — usado na loja da Zungueira.
- **Vidas:** 3. Ao chegar a zero, o nível recomeça.
- **Checkpoints:** as **Zungueiras**. Com `E` o jogo pausa e abre a loja; entrar e sair não perde nada.
- **O Pensador:** item escondido que faz a névoa recuar.
- **Fim de nível:** mostra o que foi recolhido vs. o que devia ter sido. Se faltar algum item histórico, o nível repete-se.

### Perigos de Luanda

Carros e candongueiros (apenas no troço da Marginal até à igreja), moto, atirador, blocos suspensos e valas. Todos têm colisão real; os que ainda não têm arte definitiva usam *placeholders* com comportamento completo.

### Percurso

Marginal → BNA → Palácio de Ferro → Monumento aos Soldados Desconhecidos → Largo 1.º de Maio → Prédios da FILDA.

---

## Estrutura do projeto

```
Assets/
├── Scenes/
│   ├── MenuPrincipal.unity   # Menu (Jogar → Corrida → Caminho)
│   └── SampleScene.unity     # Nível de Luanda
├── scripts/
│   ├── Jogador/              # ControladorJogador, Movimento
│   ├── Jogo/                 # vidas, colectáveis, checkpoints, névoa, progresso
│   ├── Perigos/              # trânsito, moto, atirador, bala, dano
│   ├── Interface/            # HUD, fim de nível/jogo, loja, tema pixel
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
- Não alterar arte nem apagar objetos existentes da cena sem avisar a equipa.
- Em caso de dúvida sobre uma regra de jogo, perguntar em vez de inventar.

---

## Documentação

| Ficheiro | Para que serve |
|---|---|
| [`CONTEXTO.md`](CONTEXTO.md) | Visão geral, regras técnicas, mecânicas, perigos, ordem de trabalho |
| [`Docs/ASSET_MANIFEST.md`](Docs/ASSET_MANIFEST.md) | Inventário de sprites, áudio, UI e textos (✅ existe / ⚠️ fora do projeto / ❌ falta) |

---

## Estado do protótipo

Já implementado: menu principal com seleção de caminhos e progresso em `PlayerPrefs`; movimento do jogador (salto variável, *coyote time*, *jump buffer*, agachar, subida de degraus); vidas, dano e respawn; colectáveis e HUD; névoa com recuo pelo Pensador; Zungueira com loja; atirador, trânsito, moto e blocos; ecrãs de fim de nível/jogo.

Por fazer nesta fase: afinar valores no Inspector, arte definitiva dos perigos, textos históricos dos 7 monumentos de Luanda, música e SFX. Fora do âmbito atual: Malanje, Huíla, boss, Sala da Memória, Android.

---

## Equipa

Projeto de 3 pessoas para o Trabalho Multidisciplinar (12.ª Classe). Plataforma: PC.
