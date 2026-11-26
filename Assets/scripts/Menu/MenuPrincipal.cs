using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CaminhosDeAngola.Jogo;
using CaminhosDeAngola.Interface;
using CaminhosDeAngola.Jogador;

namespace CaminhosDeAngola.Menu
{
    /// <summary>
    /// Menu principal: Jogar -> escolher o jogo (Corrida, Quiz, Memoria, Puzzle)
    /// -> escolher o personagem (Neto, Njinga, Mandume) -> escolher o caminho (Luanda, Malanje, Huila, Memoria).
    /// Constroi todo o canvas ao arrancar, com o TemaPixel. As imagens estao em
    /// Assets/Resources/Menu. Esc volta atras.
    /// </summary>
    public class MenuPrincipal : MonoBehaviour
    {
        [Header("Cenas")]
        [SerializeField] private string cenaLuanda = "SampleScene";
        [SerializeField] private string cenaHuila = "Huila";

        [Header("Nevoa de fundo")]
        [SerializeField, Min(0f)] private float velocidadeNevoa = 28f;

        /// <summary>Quem volta de um mini-jogo (ex.: Puzzle) pede ao menu para abrir a escolha de jogo.</summary>
        public static bool AbrirEscolhaDeJogos;

        private enum Pagina { Principal, Jogos, Personagem, Caminhos }
        private enum EstadoCartao { Aberto, Bloqueado, EmBreve }

        private static readonly string[] NomesNiveis = { "LUANDA", "MALANJE", "HUILA", "MEMORIA" };
        private static readonly string[] ChavesNiveis = { "luanda", "malanje", "huila", "memoria" };
        private static readonly string[] DescricoesNiveis =
        {
            "A Marginal e os monumentos da capital.",
            "As Pedras Negras e as palancas.",
            "A Serra da Leba e o planalto.",
            "O último desafio. Boss final."
        };

        private RectTransform raiz;
        private CanvasGroup grupoRaiz;
        private RectTransform paginaPrincipal;
        private RectTransform paginaJogos;
        private RectTransform paginaCaminhos;
        private RectTransform paginaPersonagem;
        private Button[] botoesPersonagem;
        private Pagina atual = Pagina.Principal;
        private Text aviso;
        private float fimAviso;
        private readonly RectTransform[] camadasNevoa = new RectTransform[2];
        private readonly float[] larguraNevoa = new float[2];
        private bool aTransitar;

        // ---------------------------------------------------------------
        // Arranque
        // ---------------------------------------------------------------

        private void Awake()
        {
            Time.timeScale = 1f;
            GarantirEventSystem();
            ConstruirCanvas();
        }

        private void Start()
        {
            Pagina inicial = Pagina.Principal;
            if (AbrirEscolhaDeJogos)
            {
                AbrirEscolhaDeJogos = false;
                inicial = Pagina.Jogos;
            }
            if (ProgressoJogo.AbrirEscolhaDeNiveis)
            {
                ProgressoJogo.AbrirEscolhaDeNiveis = false;
                inicial = Pagina.Caminhos;
            }
            MostrarPagina(inicial, true);
            StartCoroutine(AparecerRaiz());
        }

        private IEnumerator AparecerRaiz()
        {
            grupoRaiz.alpha = 0f;
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.unscaledDeltaTime;
                grupoRaiz.alpha = Mathf.Clamp01(t / 0.6f);
                yield return null;
            }
            grupoRaiz.alpha = 1f;
        }

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        // ---------------------------------------------------------------
        // Teclado, nevoa e avisos
        // ---------------------------------------------------------------

        private void Update()
        {
            AnimarNevoa();

            if (aviso != null && aviso.text.Length > 0 && Time.unscaledTime > fimAviso) aviso.text = "";

            Keyboard teclado = Keyboard.current;
            if (teclado == null) return;

            if (teclado.escapeKey.wasPressedThisFrame)
            {
                if (atual == Pagina.Caminhos) MostrarPagina(PaginaAntesDeCaminhos(), false);
                else if (atual == Pagina.Personagem) MostrarPagina(Pagina.Jogos, false);
                else if (atual == Pagina.Jogos) MostrarPagina(Pagina.Principal, false);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (teclado.f9Key.wasPressedThisFrame)
            {
                ProgressoJogo.DesbloquearTudo();
                ReconstruirCaminhos();
                Avisar("Teste: todos os níveis desbloqueados.");
            }
            else if (teclado.f10Key.wasPressedThisFrame)
            {
                ProgressoJogo.Reiniciar();
                ReconstruirCaminhos();
                Avisar("Teste: progresso reposto.");
            }
#endif
        }

        private void AnimarNevoa()
        {
            for (int i = 0; i < camadasNevoa.Length; i++)
            {
                RectTransform camada = camadasNevoa[i];
                if (camada == null || larguraNevoa[i] <= 0f) continue;
                float velocidade = velocidadeNevoa * (i == 0 ? 1f : 0.55f);
                float x = -((Time.unscaledTime * velocidade) % larguraNevoa[i]);
                camada.anchoredPosition = new Vector2(x, camada.anchoredPosition.y);
            }
        }

        private void Avisar(string texto)
        {
            if (aviso == null) return;
            aviso.text = texto;
            fimAviso = Time.unscaledTime + 3f;
        }

        // ---------------------------------------------------------------
        // Canvas
        // ---------------------------------------------------------------

        private void ConstruirCanvas()
        {
            GameObject cgo = new GameObject("Canvas", typeof(RectTransform));
            cgo.transform.SetParent(transform, false);
            Canvas canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler escala = cgo.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escala.matchWidthOrHeight = 0.5f;
            cgo.AddComponent<GraphicRaycaster>();

            raiz = (RectTransform)cgo.transform;
            grupoRaiz = cgo.AddComponent<CanvasGroup>();

            ConstruirFundo();

            paginaPrincipal = CriarPagina("PaginaPrincipal");
            paginaJogos = CriarPagina("PaginaJogos");
            paginaCaminhos = CriarPagina("PaginaCaminhos");
            paginaPersonagem = CriarPagina("PaginaPersonagem");

            ConstruirPaginaPrincipal();
            ConstruirPaginaJogos();
            ConstruirPaginaPersonagem();
            ConstruirPaginaCaminhos();
        }

        private RectTransform CriarPagina(string nome)
        {
            RectTransform rt = TemaPixel.Novo(nome, raiz);
            TemaPixel.Esticar(rt);
            rt.gameObject.AddComponent<CanvasGroup>();
            return rt;
        }

        private void ConstruirFundo()
        {
            // Paisagem de Luanda (capturada do proprio nivel), a cobrir o ecra.
            RectTransform fundo = TemaPixel.Novo("Fundo", raiz);
            TemaPixel.Esticar(fundo);
            Image imagem = fundo.gameObject.AddComponent<Image>();
            imagem.sprite = Resources.Load<Sprite>("Menu/menu_fundo");
            imagem.color = imagem.sprite != null ? Color.white : new Color32(40, 50, 70, 255);
            imagem.raycastTarget = false;
            AspectRatioFitter ajuste = fundo.gameObject.AddComponent<AspectRatioFitter>();
            ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            ajuste.aspectRatio = 16f / 9f;

            // Escurece para dar destaque ao menu.
            RectTransform escuro = TemaPixel.Novo("Escurecer", raiz);
            TemaPixel.Esticar(escuro);
            Image im2 = escuro.gameObject.AddComponent<Image>();
            im2.color = new Color32(12, 10, 24, 120);
            im2.raycastTarget = false;

            // Duas camadas de nevoa (o Esquecimento) a deslizar.
            Sprite nevoa = CriarSpriteNevoa();
            for (int i = 0; i < 2; i++)
            {
                RectTransform camada = TemaPixel.Novo("Nevoa" + i, raiz);
                camada.anchorMin = new Vector2(0f, 0f);
                camada.anchorMax = new Vector2(0f, 0f);
                camada.pivot = new Vector2(0f, 0f);
                float largura = 2304f;
                float altura = i == 0 ? 360f : 260f;
                camada.sizeDelta = new Vector2(largura * 2f, altura);
                camada.anchoredPosition = new Vector2(0f, i == 0 ? 0f : 120f);
                for (int k = 0; k < 2; k++)
                {
                    RectTransform bloco = TemaPixel.Novo("Bloco" + k, camada);
                    bloco.anchorMin = new Vector2(0f, 0f);
                    bloco.anchorMax = new Vector2(0f, 1f);
                    bloco.pivot = new Vector2(0f, 0.5f);
                    bloco.sizeDelta = new Vector2(largura, 0f);
                    bloco.anchoredPosition = new Vector2(k * largura, 0f);
                    Image n = bloco.gameObject.AddComponent<Image>();
                    n.sprite = nevoa;
                    n.color = new Color(1f, 1f, 1f, i == 0 ? 0.75f : 0.45f);
                    n.raycastTarget = false;
                    if (i == 1 && k == 0) bloco.localScale = new Vector3(1f, 1f, 1f);
                }
                camadasNevoa[i] = camada;
                larguraNevoa[i] = largura;
            }
        }

        /// <summary>Textura de nevoa que emenda nas pontas (ondas com frequencias inteiras).</summary>
        private static Sprite CriarSpriteNevoa()
        {
            const int w = 512;
            const int h = 96;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            Color32[] px = new Color32[w * h];
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)w * Mathf.PI * 2f;
                float n = 0.55f + 0.22f * Mathf.Sin(u * 2f + 1f) + 0.14f * Mathf.Sin(u * 3f + 2f) + 0.09f * Mathf.Sin(u * 5f + 4f);
                for (int y = 0; y < h; y++)
                {
                    float v = y / (float)(h - 1);
                    float perfil = Mathf.Sin(v * Mathf.PI);
                    float a = Mathf.Clamp01(perfil * perfil * n * 1.1f);
                    px[y * w + x] = new Color32(196, 208, 222, (byte)(a * 150f));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        // ---------------------------------------------------------------
        // Paginas
        // ---------------------------------------------------------------

        private void ConstruirPaginaPrincipal()
        {
            RectTransform logo = TemaPixel.Novo("Logo", paginaPrincipal);
            logo.anchorMin = logo.anchorMax = new Vector2(0.5f, 1f);
            logo.pivot = new Vector2(0.5f, 1f);
            logo.anchoredPosition = new Vector2(0f, -40f);
            logo.sizeDelta = new Vector2(800f, 472f);
            Image imagem = logo.gameObject.AddComponent<Image>();
            imagem.sprite = Resources.Load<Sprite>("Menu/menu_logo");
            imagem.raycastTarget = false;
            if (imagem.sprite == null) imagem.enabled = false;

            RectTransform botoes = TemaPixel.Novo("Botoes", paginaPrincipal);
            botoes.anchorMin = botoes.anchorMax = new Vector2(0.5f, 0.5f);
            botoes.pivot = new Vector2(0.5f, 1f);
            botoes.anchoredPosition = new Vector2(0f, -70f);
            botoes.sizeDelta = new Vector2(520f, 220f);
            VerticalLayoutGroup v = botoes.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 24f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            Button bJogar = TemaPixel.CriarBotao(botoes, "JOGAR", VarianteBotao.Terra, () => MostrarPagina(Pagina.Jogos, false));
            Button bSair = TemaPixel.CriarBotao(botoes, "SAIR", VarianteBotao.Asfalto, Sair);
            bJogar.GetComponentInChildren<Text>().fontSize = 36;
            bSair.GetComponentInChildren<Text>().fontSize = 36;

            Text rodape = TemaPixel.CriarTexto("Rodape", paginaPrincipal, "PROTÓTIPO DE LUANDA", 20, TemaPixel.Creme, false, TextAnchor.MiddleCenter, true);
            RectTransform rr = rodape.rectTransform;
            rr.anchorMin = new Vector2(0f, 0f);
            rr.anchorMax = new Vector2(1f, 0f);
            rr.pivot = new Vector2(0.5f, 0f);
            rr.anchoredPosition = new Vector2(0f, 24f);
            rr.sizeDelta = new Vector2(0f, 36f);
        }

        private void ConstruirPaginaJogos()
        {
            RectTransform caixa = TemaPixel.CriarPainel(paginaJogos, 1700f, true);
            TemaPixel.CriarFaixa(caixa, "ESCOLHE O JOGO", TemaPixel.Amarelo);

            RectTransform fila = CriarFila(caixa);
            CriarCartao(fila, "corrida", "CORRIDA", "Foge do Esquecimento pelas províncias.", EstadoCartao.Aberto, "JOGAR",
                () => MostrarPagina(SelecaoPersonagem.Todos.Length > 0 ? Pagina.Personagem : Pagina.Caminhos, false), false);
            CriarCartao(fila, "quiz", "QUIZ", "Testa o que sabes sobre Angola.", EstadoCartao.EmBreve, "EM BREVE", null, false);
            CriarCartao(fila, "memoriaj", "MEMORIA", "Encontra os pares da nossa história.", EstadoCartao.EmBreve, "EM BREVE", null, false);
            CriarCartao(fila, "puzzle", "PUZZLE", "Monta as imagens do nosso património.", EstadoCartao.Aberto, "JOGAR",
                () => IrParaCena("Puzzle"), false);

            BotaoCentrado(caixa, "VOLTAR", VarianteBotao.Asfalto, () => MostrarPagina(Pagina.Principal, false), 420f);
            TemaPixel.CriarTexto("Ajuda", caixa, "Esc: voltar", 20, TemaPixel.Castanho);
        }

        private void ConstruirPaginaCaminhos()
        {
            for (int i = paginaCaminhos.childCount - 1; i >= 0; i--) Destroy(paginaCaminhos.GetChild(i).gameObject);

            RectTransform caixa = TemaPixel.CriarPainel(paginaCaminhos, 1700f, true);
            TemaPixel.CriarFaixa(caixa, "ESCOLHE O CAMINHO", TemaPixel.Amarelo);

            RectTransform fila = CriarFila(caixa);
            for (int i = 0; i < ProgressoJogo.TotalNiveis; i++)
            {
                int indice = i;
                bool aberto = ProgressoJogo.NivelDesbloqueado(i);
                EstadoCartao estado = aberto ? EstadoCartao.Aberto : EstadoCartao.Bloqueado;
                string descricao = aberto ? DescricoesNiveis[i] : "Conclui o nível anterior.";
                CriarCartao(fila, ChavesNiveis[i], NomesNiveis[i], descricao, estado, aberto ? "ENTRAR" : "BLOQUEADO",
                    () => EscolherNivel(indice), ProgressoJogo.NivelConcluido(i));
            }

            BotaoCentrado(caixa, "VOLTAR", VarianteBotao.Asfalto, () => MostrarPagina(PaginaAntesDeCaminhos(), false), 420f);
            aviso = TemaPixel.CriarTexto("Aviso", caixa, "", 24, TemaPixel.Amarelo, false, TextAnchor.MiddleCenter, true);
            aviso.gameObject.AddComponent<LayoutElement>().minHeight = 32f;
            TemaPixel.CriarTexto("Ajuda", caixa, "Esc: voltar", 20, TemaPixel.Castanho);
        }

        /// <summary>De onde se volta a partir dos caminhos: do personagem (ou dos jogos, se nao houver personagens).</summary>
        private Pagina PaginaAntesDeCaminhos()
        {
            return SelecaoPersonagem.Todos.Length > 0 ? Pagina.Personagem : Pagina.Jogos;
        }

        private void ConstruirPaginaPersonagem()
        {
            for (int i = paginaPersonagem.childCount - 1; i >= 0; i--) Destroy(paginaPersonagem.GetChild(i).gameObject);

            PersonagemDados[] todos = SelecaoPersonagem.Todos;
            int actual = SelecaoPersonagem.Indice;

            RectTransform caixa = TemaPixel.CriarPainel(paginaPersonagem, 1700f, true);
            TemaPixel.CriarFaixa(caixa, "ESCOLHE O PERSONAGEM", TemaPixel.Amarelo);

            RectTransform fila = CriarFila(caixa);
            botoesPersonagem = new Button[todos.Length];
            for (int i = 0; i < todos.Length; i++)
            {
                int indice = i;
                botoesPersonagem[i] = CriarCartaoPersonagem(fila, todos[i], i == actual, () => EscolherPersonagem(indice));
            }

            BotaoCentrado(caixa, "VOLTAR", VarianteBotao.Asfalto, () => MostrarPagina(Pagina.Jogos, false), 420f);
            TemaPixel.CriarTexto("Ajuda", caixa, "Setas: escolher     Enter: confirmar     Esc: voltar", 20, TemaPixel.Castanho);
        }

        private Button CriarCartaoPersonagem(RectTransform pai, PersonagemDados dados, bool escolhido, UnityAction accao)
        {
            RectTransform cartao = TemaPixel.Novo("Cartao_" + dados.Nome, pai);
            Image moldura = cartao.gameObject.AddComponent<Image>();
            moldura.sprite = TemaPixel.Sprite("ui_cartao");
            moldura.type = Image.Type.Sliced;
            LayoutElement leCartao = cartao.gameObject.AddComponent<LayoutElement>();
            leCartao.preferredWidth = 500f;
            leCartao.minWidth = 500f;

            VerticalLayoutGroup v = cartao.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(12, 12, 12, 14);
            v.spacing = 10f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            // Palco escuro com o personagem a respirar (animacao do parado).
            RectTransform palco = TemaPixel.Novo("Palco", cartao);
            Image fundo = palco.gameObject.AddComponent<Image>();
            fundo.sprite = TemaPixel.Sprite("ui_placa_escura");
            fundo.type = Image.Type.Sliced;
            fundo.raycastTarget = false;
            LayoutElement lePalco = palco.gameObject.AddComponent<LayoutElement>();
            lePalco.preferredHeight = 300f;
            lePalco.minHeight = 300f;

            RectTransform figura = TemaPixel.Novo("Figura", palco);
            figura.anchorMin = figura.anchorMax = new Vector2(0.5f, 0f);
            figura.pivot = new Vector2(0.5f, 0f);
            Image imagem = figura.gameObject.AddComponent<Image>();
            imagem.raycastTarget = false;
            figura.gameObject.AddComponent<AnimadorRetrato>().Configurar(imagem, dados.AnimacaoParado, 8f, 1.6f, 24f);

            if (escolhido) CriarEtiqueta(palco, "ACTUAL", "ui_placa_ok", TemaPixel.VerdeEscuro, true);

            Text nome = TemaPixel.CriarTexto("Nome", cartao, dados.Nome.ToUpperInvariant(), 28, TemaPixel.Amarelo, true, TextAnchor.MiddleCenter, true);
            nome.gameObject.AddComponent<LayoutElement>().minHeight = 44f;

            Text traco = TemaPixel.CriarTexto("Traco", cartao, dados.Traco, 20, TemaPixel.Creme, false, TextAnchor.UpperCenter);
            traco.gameObject.AddComponent<LayoutElement>().minHeight = 80f;

            return TemaPixel.CriarBotao(cartao, escolhido ? "CONTINUAR" : "ESCOLHER", VarianteBotao.Terra, accao, 80f);
        }

        private void EscolherPersonagem(int indice)
        {
            if (aTransitar) return;
            SelecaoPersonagem.Indice = indice;
            MostrarPagina(Pagina.Caminhos, false);
        }

        private IEnumerator SelecionarPersonagemActual()
        {
            yield return null;
            if (EventSystem.current == null || botoesPersonagem == null || botoesPersonagem.Length == 0) yield break;
            int i = Mathf.Clamp(SelecaoPersonagem.Indice, 0, botoesPersonagem.Length - 1);
            if (botoesPersonagem[i] != null) EventSystem.current.SetSelectedGameObject(botoesPersonagem[i].gameObject);
        }

        private void ReconstruirCaminhos()
        {
            ConstruirPaginaCaminhos();
            if (atual == Pagina.Caminhos) StartCoroutine(SelecionarPrimeiro(paginaCaminhos));
        }

        private RectTransform CriarFila(RectTransform pai)
        {
            RectTransform fila = TemaPixel.Novo("Fila", pai);
            HorizontalLayoutGroup h = fila.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = 28f;
            h.childAlignment = TextAnchor.UpperCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            return fila;
        }

        private Button BotaoCentrado(RectTransform pai, string rotulo, VarianteBotao variante, UnityAction accao, float largura)
        {
            RectTransform linha = TemaPixel.Novo("LinhaBotao", pai);
            linha.gameObject.AddComponent<LayoutElement>().minHeight = 96f;
            HorizontalLayoutGroup h = linha.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.childAlignment = TextAnchor.MiddleCenter;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = false;
            h.childForceExpandHeight = false;
            Button botao = TemaPixel.CriarBotao(linha, rotulo, variante, accao);
            LayoutElement le = botao.GetComponent<LayoutElement>();
            le.preferredWidth = largura;
            le.minWidth = largura;
            return botao;
        }

        // ---------------------------------------------------------------
        // Cartoes
        // ---------------------------------------------------------------

        private void CriarCartao(RectTransform pai, string chaveQuadro, string nome, string descricao,
            EstadoCartao estado, string rotuloBotao, UnityAction accao, bool concluido)
        {
            RectTransform cartao = TemaPixel.Novo("Cartao_" + nome, pai);
            Image moldura = cartao.gameObject.AddComponent<Image>();
            moldura.sprite = TemaPixel.Sprite("ui_cartao");
            moldura.type = Image.Type.Sliced;
            LayoutElement leCartao = cartao.gameObject.AddComponent<LayoutElement>();
            leCartao.preferredWidth = 376f;
            leCartao.minWidth = 376f;

            VerticalLayoutGroup v = cartao.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(12, 12, 12, 14);
            v.spacing = 10f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;

            // Quadro pintado
            RectTransform quadro = TemaPixel.Novo("Quadro", cartao);
            Image imagem = quadro.gameObject.AddComponent<Image>();
            imagem.sprite = Resources.Load<Sprite>("Menu/quadro_" + chaveQuadro);
            imagem.raycastTarget = false;
            if (imagem.sprite == null) imagem.color = new Color32(60, 60, 80, 255);
            LayoutElement leQuadro = quadro.gameObject.AddComponent<LayoutElement>();
            leQuadro.preferredHeight = 272f;
            leQuadro.minHeight = 272f;
            if (estado == EstadoCartao.Bloqueado) imagem.color = new Color(0.26f, 0.26f, 0.34f, 1f);
            else if (estado == EstadoCartao.EmBreve) imagem.color = new Color(0.55f, 0.55f, 0.62f, 1f);

            if (estado == EstadoCartao.Bloqueado)
            {
                RectTransform cadeado = TemaPixel.Novo("Cadeado", quadro);
                cadeado.anchorMin = cadeado.anchorMax = new Vector2(0.5f, 0.5f);
                cadeado.pivot = new Vector2(0.5f, 0.5f);
                cadeado.sizeDelta = new Vector2(112f, 140f);
                Image ic = cadeado.gameObject.AddComponent<Image>();
                ic.sprite = TemaPixel.Sprite("ui_cadeado");
                ic.raycastTarget = false;
            }

            if (estado == EstadoCartao.EmBreve)
                CriarEtiqueta(quadro, "EM BREVE", "ui_placa_escura", TemaPixel.Amarelo, false);
            else if (concluido)
                CriarEtiqueta(quadro, "CONCLUÍDO", "ui_placa_ok", TemaPixel.VerdeEscuro, true);

            // Nome
            Text titulo = TemaPixel.CriarTexto("Nome", cartao, nome, 28, estado == EstadoCartao.Aberto ? TemaPixel.Amarelo : TemaPixel.Castanho,
                true, TextAnchor.MiddleCenter, true);
            titulo.gameObject.AddComponent<LayoutElement>().minHeight = 44f;

            // Descricao
            Text texto = TemaPixel.CriarTexto("Descricao", cartao, descricao, 20,
                estado == EstadoCartao.Aberto ? TemaPixel.Creme : TemaPixel.Castanho, false, TextAnchor.UpperCenter);
            texto.gameObject.AddComponent<LayoutElement>().minHeight = 60f;

            // Botao
            Button botao = TemaPixel.CriarBotao(cartao, rotuloBotao,
                estado == EstadoCartao.Aberto ? VarianteBotao.Terra : VarianteBotao.Inactivo, accao, 80f);
            botao.interactable = estado == EstadoCartao.Aberto;
        }

        private void CriarEtiqueta(RectTransform quadro, string texto, string sprite, Color cor, bool noTopo)
        {
            RectTransform e = TemaPixel.Novo("Etiqueta", quadro);
            e.anchorMin = new Vector2(0f, noTopo ? 1f : 0f);
            e.anchorMax = new Vector2(1f, noTopo ? 1f : 0f);
            e.pivot = new Vector2(0.5f, noTopo ? 1f : 0f);
            e.anchoredPosition = Vector2.zero;
            e.sizeDelta = new Vector2(0f, 56f);
            Image im = e.gameObject.AddComponent<Image>();
            im.sprite = TemaPixel.Sprite(sprite);
            im.type = Image.Type.Sliced;
            im.raycastTarget = false;
            Text t = TemaPixel.CriarTexto("Texto", e, texto, 22, cor, false, TextAnchor.MiddleCenter, true);
            TemaPixel.Esticar(t.rectTransform);
        }

        // ---------------------------------------------------------------
        // Navegacao
        // ---------------------------------------------------------------

        private void MostrarPagina(Pagina destino, bool imediato)
        {
            atual = destino;
            if (destino == Pagina.Caminhos) ConstruirPaginaCaminhos();
            if (destino == Pagina.Personagem) ConstruirPaginaPersonagem();

            RectTransform alvo = destino == Pagina.Principal ? paginaPrincipal
                : (destino == Pagina.Jogos ? paginaJogos : (destino == Pagina.Personagem ? paginaPersonagem : paginaCaminhos));
            paginaPrincipal.gameObject.SetActive(alvo == paginaPrincipal);
            paginaJogos.gameObject.SetActive(alvo == paginaJogos);
            paginaPersonagem.gameObject.SetActive(alvo == paginaPersonagem);
            paginaCaminhos.gameObject.SetActive(alvo == paginaCaminhos);

            if (!imediato) StartCoroutine(Aparecer(alvo.GetComponent<CanvasGroup>()));
            StartCoroutine(destino == Pagina.Personagem ? SelecionarPersonagemActual() : SelecionarPrimeiro(alvo));
        }

        private IEnumerator Aparecer(CanvasGroup grupo)
        {
            float t = 0f;
            grupo.alpha = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                grupo.alpha = Mathf.Clamp01(t / 0.18f);
                yield return null;
            }
            grupo.alpha = 1f;
        }

        private IEnumerator SelecionarPrimeiro(RectTransform pagina)
        {
            yield return null;
            if (EventSystem.current == null) yield break;
            Button[] botoes = pagina.GetComponentsInChildren<Button>();
            foreach (Button b in botoes)
            {
                if (b.interactable)
                {
                    EventSystem.current.SetSelectedGameObject(b.gameObject);
                    yield break;
                }
            }
        }

        private void EscolherNivel(int indice)
        {
            if (aTransitar) return;
            if (!ProgressoJogo.NivelDesbloqueado(indice))
            {
                Avisar("Conclui o nível anterior para abrir este caminho.");
                return;
            }

            string cena = indice == 0 ? cenaLuanda : (indice == 2 ? cenaHuila : "");
            if (string.IsNullOrEmpty(cena) || !Application.CanStreamedLevelBeLoaded(cena))
            {
                Avisar(NomesNiveis[indice] + " ainda está em criação.");
                return;
            }

            StartCoroutine(CarregarCena(cena));
        }

        private IEnumerator CarregarCena(string cena)
        {
            aTransitar = true;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                grupoRaiz.alpha = 1f - Mathf.Clamp01(t / 0.35f);
                yield return null;
            }
            SceneManager.LoadScene(cena);
        }

        private void IrParaCena(string cena)
        {
            if (aTransitar) return;
            if (!Application.CanStreamedLevelBeLoaded(cena)) return;
            StartCoroutine(CarregarCena(cena));
        }

        private void Sair()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
