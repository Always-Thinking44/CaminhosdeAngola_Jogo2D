# Caminhos de Angola — Contexto para desenvolvimento (Protótipo de Luanda)

Ficheiro de contexto para o OpenCode. Ler por inteiro antes de qualquer tarefa.
Documentos complementares: GDD v1.0 e Manifesto de Assets.

## 1. O projecto

- Jogo 2D side-scroller em **Unity 6.4 (6000.4.4f1)**, projecto `TestandoJogo`, cena base `SampleScene`.
- Já existem na cena: Main Camera, CinemachineCamera, Global Light 2D, `player`, Grid com Tilemap (estrada, chão, blocos suspensos) e os planos de fundo de Luanda (terciário, secundário).
- Equipa de 3 pessoas. Plataforma: PC. Moeda: **Zimbo**.
- Ideia central: uma travessia pela memória de Angola. O jogador percorre lugares reais, recolhe itens históricos e foge do **Esquecimento** (a névoa).
- **O jogador não ataca. Só foge, salta e desvia.** Nunca criar botão de ataque.

## 2. Âmbito do protótipo

**Dentro:** apenas o nível de Luanda, com um personagem (Agostinho Neto).
**Fora (não fazer agora):** menu principal, escolha de nível, escolha de personagem, Malanje, Huíla, boss, Sala da Memória, música e SFX finais, Android.

Usar a arte que já existe (Neto, cenários de Luanda, Zimbo, Pensador, avião). Tudo o que não tem sprite usa **placeholder** (caixa colorida com o nome do objecto), com colisores e comportamento reais, para depois trocar só o sprite.

## 3. Regras técnicas

- Sprites com PPU **100**, filtro **Point (no filter)**, pivot bottom-center (personagens/inimigos) ou center (props/UI). Todos virados para a direita; o código faz o flip.
- `zimbo.png` e `pensador.png` estão importados como *Multiple* com filtro Bilinear: reimportar como *Single* + Point e recortar ao conteúdo.
- Scripts em `Assets/scripts`, um script por responsabilidade, nomes em inglês ou português coerentes (escolher um e manter), valores afináveis expostos no Inspector (`[SerializeField]`).
- Usar o **novo Input System** com Input Actions (teclado agora, toque no futuro).
- Layers: Player, Chao, Perigo, Colectavel, Inimigo, Plataforma, Trigger.
- Tags: Player, Checkpoint, Monumento, Zungueira, Colectavel.
- Configurar a matriz de colisão e Physics2D (gravidade, material sem atrito).

## 4. Mecânicas (decididas)

### Personagem
- Andar, correr, salto variável, coyote time, jump buffer, agachar.
- Animações existentes do Neto: parado A/B, correr, salto, agachar.
- **3 vidas.** Ao chegar a 0, o nível recomeça.

### Névoa (Esquecimento)
- Segue o jogador por trás, a velocidade **constante** (valor ajustável no Inspector). Pode ganhar variações mais tarde, não agora.
- Visual: faixa larga semi-transparente com gradiente e partículas. Tudo o que fica atrás da frente da névoa aparece nublado, como em nevoeiro.
- Se a névoa apanha o jogador, ele volta ao **último checkpoint**.
- A névoa recua um pouco quando o jogador apanha **O Pensador**.
- No fim do nível, se o jogador recolheu tudo, a névoa desaparece.
- *Assumir e confirmar:* apanhar o jogador custa 1 vida; ao voltar ao checkpoint a névoa é reposta a uma distância fixa atrás do jogador.

### Checkpoints = Zungueiras
- Cada zungueira é um checkpoint.
- Com uma tecla (usar **E**) o jogador entra na zungueira: o jogo **pára** (névoa e perigos em pausa) e abre-se a loja.
- Ao entrar/sair **não se perde nada** (Zimbos, itens).
- Loja (opcional no protótipo, fazer no fim se houver tempo): Asas, Escudo, vida extra, pagos com Zimbos.

### Colectáveis
- **Zimbos**: economia da loja.
- **Itens históricos**: um por monumento (7 em Luanda segundo o manifesto; confirmar a lista final).
- **O Pensador**: escondido no nível, faz recuar a névoa.
- HUD: vidas, Zimbos, itens históricos recolhidos / total.

### Fim de nível
- Aparece um canvas com **o que o jogador apanhou vs o que devia ter apanhado**.
- Se faltar algum item: mostra que **falhou** e o nível **repete-se** (Zimbos e itens recolhidos são reiniciados com o nível).
- Se estiver tudo: sucesso, a névoa desaparece e o nível termina.

## 5. Perigos de Luanda

Todos são obstáculos reais no plano principal, com colisão com o jogador.

| Perigo | Comportamento | Sprite |
|---|---|---|
| Carros e candongueiros | Só no **troço de estrada** (do início até à igreja). Passam sempre, **da direita para a esquerda**. Depois da igreja o chão é normal. | placeholder |
| Moto | Aparece de repente, de tempos a tempos, muito rápida. Pode matar. Pode ser saltada no timing certo. | placeholder |
| Atirador | Fica parado num ou em vários pontos do nível. **Só dispara quando o jogador está a uma certa distância.** | placeholder |
| Blocos suspensos | Aparecem e desaparecem por timing; o jogador cai se estiver em cima. | já existem na cena |
| Valas | Zonas baixas que o jogador tem de saltar, além de zonas elevadas. | tilemap |

### Atirador e bala (detalhe)

- Sprites do atirador: `enemy_atirador_01` (parado), `_02` (a mirar), `_03` (disparo com clarão). Canvas 192×160 px, PPU 100, pivot bottom-center, virado para a direita.
- Prefab `Bala` com `enemy_bala_01.png` (48×16 px, aponta para a direita), `Rigidbody2D` (Kinematic ou Gravity Scale 0) e collider com *Is Trigger* ligado. Layer Perigo.
- Objecto filho vazio `FirePoint` na ponta do cano, posição local aproximada **(0.72, 1.06)** (afinar no Inspector).
- Comportamento: só dispara quando o jogador está dentro de uma distância configurável. Mostra o frame 3 (clarão) cerca de 0,1 s e cria a bala no `FirePoint`. Tem tempo de recarga configurável entre disparos.
- A bala vai em linha recta (não persegue). Ao tocar no jogador tira 1 vida e destrói-se. Destrói-se também ao tocar no chão ou ao fim de cerca de 3 s.
- Se o atirador estiver virado para a esquerda, o script faz o flip do sprite e do `FirePoint`.
- Todos os valores (distância, recarga, velocidade da bala, tempo de vida) expostos no Inspector.

Fora do protótipo inicial (fazer só depois): perseguidor, avião/pássaros com o power-up de voar.

## 6. Estrutura do nível Luanda

- Percurso da esquerda para a direita: Marginal → BNA → Palácio de Ferro → Monumento aos Soldados Desconhecidos → Largo 1.º de Maio → Prédios da FILDA.
- Espaço entre monumentos para o jogador andar uns segundos antes do seguinte.
- Subidas e descidas (o nível não deve ser linear e plano).
- Sem coqueiros do Largo 1.º de Maio em diante.
- Planos: principal (monumentos), secundário, terciário, céu/horizonte, chão. Cada plano com parallax próprio.

## 7. Ordem de trabalho

Fazer **uma tarefa de cada vez**, testar em Play Mode e fazer commit antes de avançar.

1. Base do projecto: layers, tags, colisões, Physics2D, Input Actions, import settings.
2. Player: movimento, salto variável, coyote, buffer, agachar, câmara.
3. Vidas, dano, respawn e checkpoints.
4. Colectáveis e HUD.
5. Névoa (movimento + visual + recuo com o Pensador).
6. Perigos, um a um: carros/candongueiros, blocos suspensos, valas, moto, atirador.
7. Zungueira (entrar com E, pausa, loja simples).
8. Fim de nível (canvas apanhado vs devido, repetir se faltar, fim da névoa).

## 8. Como trabalhar

- Antes de cada tarefa, dizer em 2–3 linhas o que vai fazer e que scripts/objectos vai criar ou alterar.
- Não alterar a arte nem apagar objectos existentes da cena sem avisar.
- Não implementar nada fora do âmbito da secção 2.
- Depois de cada tarefa: indicar como testar em Play Mode e o que ficou por afinar.
- Em caso de dúvida sobre uma regra de jogo, perguntar em vez de inventar.
