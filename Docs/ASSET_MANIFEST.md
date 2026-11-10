# Caminhos de Angola — Manifesto de Assets

Checklist completo do que o jogo precisa. Marcar `[x]` quando o ficheiro estiver dentro de `Assets/` e importado correctamente.

Legenda: ✅ existe no projecto · ⚠️ existe fora do projecto · ❌ falta

---

## 0. Regras técnicas de arte

- Formato: PNG com transparência, fundo removido.
- PPU: **100** (igual à arte actual).
- Pivot: personagens/inimigos = **bottom-center**; props/UI = **center**.
- Todos os sprites virados para a **direita** (o código faz o flip).
- Altura de personagem: ~148 px (igual ao Agostinho Neto).
- Paleta fixa de 24 cores (a extrair da arte existente).
- Contorno de 1 px escuro.
- Sem sombras queimadas na imagem; sem texto dentro da imagem.
- Nome: `categoria_nome_variante_##.png` (ex.: `enemy_candongueiro_01.png`).
- Base de desenho: 1920×1080. Camadas de parallax: 1920 px (3840 px para loop).

> **Nota — powerups existentes:** recortados ao conteúdo e reimportados como *Single* + *Point (no filter)*, PPU 100. Dimensões actuais: `zimbo.png` 585×696 e `pensador.png` 397×987. Como estão em alta resolução, ao criar os prefabs será necessário escalá-los (ou ajustar o PPU por textura) para ~0,6 e ~2,0 unidades de altura.

---

## 1. Sprites — Personagens jogáveis

| Personagem | Animação | Frames | Estado |
|---|---|---|---|
| Agostinho Neto | parado A | 11 | ✅ |
| Agostinho Neto | parado B | 7 | ✅ |
| Agostinho Neto | correr | 8 | ✅ |
| Agostinho Neto | salto | 7 | ✅ |
| Agostinho Neto | agachar | 5 | ✅ |
| Agostinho Neto | dano | 1 | ❌ (opcional) |
| Agostinho Neto | vitória | 1 | ❌ (opcional) |
| Rainha Njinga | parado | 4 | ✅ |
| Rainha Njinga | correr | 8 | ✅ |
| Rainha Njinga | salto | 5 | ✅ |
| Rainha Njinga | agachar | 4 | ❌ |
| Rainha Njinga | dano | 1 | ❌ (opcional) |
| Rainha Njinga | vitória | 1 | ❌ (opcional) |
| Rei Mandume | parado | 4 | ❌ |
| Rei Mandume | correr | 8 | ❌ |
| Rei Mandume | salto | 5 | ❌ |
| Rei Mandume | agachar | 4 | ❌ |
| Rei Mandume | dano | 1 | ❌ (opcional) |
| Rei Mandume | vitória | 1 | ❌ (opcional) |

**Novos frames necessários: ~29** (Njinga 6 + Mandume 23).

---

## 2. Sprites — NPCs

| Item | Frames | Estado |
|---|---|---|
| Zungueira (parada) | 4 (ou 1 + balanço por código) | ❌ |
| Banca da Zungueira (prop) | 1 | ❌ |
| O Pensador (item) | 1 | ✅ `Assets/powerups/pensador.png` (recortar) |
| O Pensador (estátua grande, intro/final) | 1 | ❌ (opcional) |

---

## 3. Sprites — Inimigos e obstáculos

| Nível | Item | Frames | Estado |
|---|---|---|---|
| Luanda | Candongueiro | 1 | ❌ |
| Luanda | Carro A | 1 | ❌ |
| Luanda | Carro B (variante de cor) | 1 | ❌ |
| Luanda | Moto + condutor | 1 | ❌ |
| Luanda | Atirador (parado/mira/disparo) | 3 (ou 1 + flash por código) | ❌ |
| Luanda/Malanje/Huíla | Avião | 1 | ✅ |
| Todos | Bloco suspenso (temporário) | 1 | ❌ |
| Todos | Bloco móvel | 1 | ❌ |
| Malanje | Pássaro (asas cima/baixo) | 2 (ou 1 + código) | ❌ |
| Malanje | Perseguidor | 1 | ❌ |
| Malanje | Palanca Negra Gigante | 1 | ❌ |
| Huíla | Moto, avião, pássaro, atirador | — | reutilizar |
| Final | Grande Esquecimento (idle / atordoado / derrotado) | 3 | ❌ |
| Final | Ponto fraco do Boss | 1 | ❌ |
| Todos | Textura de névoa (tileável) | 1 | ❌ |

**Novos: ~16.**

---

## 4. Sprites — Colectáveis e power-ups

| Item | Frames | Estado |
|---|---|---|
| Zimbo | 1 (opcional: 4 de rotação) | ✅ `Assets/powerups/zimbo.png` (recortar) |
| O Pensador | 1 | ✅ `Assets/powerups/pensador.png` (recortar) |
| Asas (power-up) | 1 | ❌ |
| Escudo (pickup) | 1 | ❌ |
| Fragmento de Memória | 1 (3 variantes por cor) | ❌ |
| Item histórico (genérico) | 1 | ❌ |
| Miniaturas dos monumentos (menu Memória) | 13 | ✅ reutilizar PNGs dos edifícios |

**Novos: ~5.**

---

## 5. Sprites — Objectos de mundo

| Item | Frames | Estado |
|---|---|---|
| Checkpoint / pedestal de monumento | 1 | ❌ |
| Pedestal do Boss | 1 | ❌ |
| Portal / porta de fim de nível | 1 | ❌ |
| Placa/letreiro de monumento | 1 | ❌ |
| Alerta "moto" (UI de perigo) | 1 | ❌ |
| Alerta "avião" (UI de perigo) | 1 | ❌ |

**Novos: ~6.**

---

## 6. Cenários

### Luanda — ✅ pronto
Céu, secundário, terciário, chão, 18 edifícios/monumentos, skyline.

### Malanje — ❌
| Item | Estado |
|---|---|
| Céu | ❌ |
| Parallax longe (Pedras Negras) | ❌ |
| Parallax perto | ❌ |
| Tiles de chão | 4 ❌ |
| Pedras Negras de Pungo Andongo | ❌ |
| Quedas de Kalandula (2 frames de água) | ❌ |
| Vegetação (3 variantes) | ❌ |

### Huíla — ❌
| Item | Estado |
|---|---|
| Céu | ❌ |
| Parallax longe (Serra) | ❌ |
| Parallax perto (Planalto) | ❌ |
| Tiles de rocha/chão | 4 ❌ |
| Serra da Leba | ❌ |
| Fenda da Tundavala | ❌ |
| Vegetação (2 variantes) | ❌ |

### Sala da Memória — ❌
| Item | Estado |
|---|---|
| Fundo principal | ❌ |
| Fundo secundário | ❌ |
| Elementos das 3 províncias | reutilizar |

**Novos: ~25 imagens.**

---

## 7. Efeitos visuais (VFX)

| Efeito | Frames | Estado |
|---|---|---|
| Poeira (correr/aterrar) | 4 | ❌ |
| Faísca ao recolher | 1 | ❌ |
| Névoa (textura) | 1 | ❌ |
| Rasto das Asas | 1 | ❌ |
| Impacto/dano | 1 | ❌ |
| Brilho do Fragmento | 1 | ❌ |

**Novos: ~6.**

---

## 8. Interface (UI)

### Ícones
| Item | Estado |
|---|---|
| Vida | ❌ |
| Zimbo | ✅ reutilizar sprite |
| Item histórico | ❌ |
| Fragmento | ✅ reutilizar sprite |
| Asas | ✅ reutilizar sprite |
| Escudo | ✅ reutilizar sprite |
| Pausa | ❌ |
| Som ligado / desligado | 2 ❌ |
| Seta de navegação | ❌ |

### Barras e painéis
| Item | Estado |
|---|---|
| Barra de voo (moldura + preenchimento) | 2 ❌ |
| Barra de vida do Boss (moldura + preenchimento) | 2 ❌ |
| Painel genérico | 1 ❌ |
| Botão (normal / hover / pressionado) | 3 ❌ |
| Botão de ícone (moldura) | 1 ❌ |

### Ecrãs
| Item | Estado |
|---|---|
| Fundo do menu principal | ❌ |
| Logo "Caminhos de Angola" | ❌ |
| Caixa de diálogo | ❌ |
| Painel da loja | ❌ |
| Painel do menu Memória | ❌ |
| Painel de selecção de personagem | ❌ |
| Painel de pausa | ❌ |
| Cartão de personagem (moldura) | 1 ❌ |
| Retratos dos 3 personagens | ✅ reutilizar frames de idle |

**Novos: ~25.**

---

## 9. Áudio

### Música (5 faixas)
| Momento | Estilo | Estado |
|---|---|---|
| Menu | Tema calmo angolano | ❌ |
| Luanda | Semba / kizomba / kuduro instrumental | ❌ |
| Malanje | Percussão e ritmos tradicionais | ❌ |
| Huíla | Épica e aberta | ❌ |
| Boss | Épica e tensa | ❌ |

### Efeitos sonoros (~25)
salto · aterragem · passos (loop) · moeda · item histórico · fragmento · asas activar · voo (loop) · escudo quebrar · dano · morte · checkpoint · loja abrir · loja fechar · compra · clique de botão · atirador mira · disparo · buzina de candongueiro · moto a passar · avião a passar · pássaro · boss atingido · boss rugido · ambiente de névoa · vitória · game over.

---

## 10. Conteúdo educativo (textos)

| Bloco | Quantidade | Estado |
|---|---|---|
| Biografias dos 3 personagens | 3 | ❌ (eu redijo, tu validas) |
| Monumentos de Luanda | 7 | ❌ |
| Monumentos de Malanje | 3 | ❌ |
| Monumentos da Huíla | 3 | ❌ |
| O Pensador / Grande Esquecimento | 2 | ❌ |
| Descrições da loja | 2–4 | ❌ |
| Tutoriais e prompts ("Pressiona E") | ~8 | ❌ |

Cada entrada: título, data/época, 2–4 frases, fonte.

---

## 11. Fonte

| Item | Estado |
|---|---|
| 1 fonte pixel com acentos portugueses (Latin Extended) | ❌ |

Sugestões: Silkscreen, Pixelify Sans, VT323 (verificar acentos).

---

## 12. Sistemas de código (implemento eu)

- Input Actions definitivo (teclado + futuro toque).
- PlayerController: salto variável, coyote time, jump buffer, agachar, voo.
- Vidas, dano, respawn e checkpoints.
- Zimbos, itens históricos, fragmento, Pensador, Asas, Escudo.
- Perigos: trânsito, moto, atirador, blocos, pássaros, aviões, névoa, perseguidor.
- Monumentos e Zungueira (loja).
- HUD, pausa, menu Memória, selecção de personagem.
- Save system (JSON) e progressão.
- Boss com 3 fases e 9 saltos.
- Áudio manager, transições, parallax.
- Build Windows.

---

## 13. Cenas

| Cena | Estado |
|---|---|
| MainMenu | ❌ |
| CharacterSelect | ❌ |
| Nivel1_Luanda | ⚠️ (SampleScene serve de base) |
| Nivel2_Malanje | ❌ |
| Nivel3_Huila | ❌ |
| Nivel4_Memoria | ❌ |

---

## 14. Configuração do projecto

- Layers: Player, Chao, Perigo, Colectavel, Inimigo, Plataforma, Trigger.
- Tags: Player, Checkpoint, Monumento, Zungueira, Colectavel.
- Matriz de colisão (quem colide com quem).
- Physics2D: gravidade e material sem atrito.
- PPU e import settings de todas as texturas.
- Input Actions e rebinding.
- Build settings com as 6 cenas.

---

## 15. Prioridades

### P0 — Fatia vertical de Luanda
Neto (✅), Njinga agachar, Mandume placeholder, Zungueira, banca, candongueiro, carro A/B, moto, atirador, blocos, Asas, Escudo, Fragmento, item histórico, checkpoint, névoa, UI essencial, fonte, ~10 SFX, música de Luanda, 7 textos de Luanda.

### P1 — Malanje e Huíla
Pássaro, perseguidor, palanca, cenários completos, música, textos, tiles.

### P2 — Boss
3 sprites do boss, ponto fraco, pedestal, música, barra de vida.

### P3 — Polimento
Poses de vitória/dano, VFX extra, retratos, Android, localização.

---

## 16. O que preciso de ti

1. Onde estão o **Zimbo** e **O Pensador**? (caminho ou copiar para o projecto)
2. Que outros assets já tens? (moto, candongueiro, atirador, UI, áudio, fonte)
3. Autorizas a geração de arte por IA? (começaria por 1 candongueiro + 1 moto como teste)
4. Aceitas que eu redija os textos históricos para tu validares?
5. O áudio será fornecido por ti/grupo ou usamos placeholders?
6. Preferes fonte pixel com acentos ou uma fonte normal?
