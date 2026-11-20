using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CaminhosDeAngola.Interface;
using CaminhosDeAngola.Menu;
using CaminhosDeAngola.Sons;

namespace CaminhosDeAngola.Puzzle
{
    /// <summary>
    /// Mini-jogo Puzzle: a imagem e cortada em colunas x linhas pedacos quadrados,
    /// baralhados; o jogador arrasta um pedaco para cima de outro para os trocar.
    /// Qualquer baralhacao tem solucao, porque so se trocam pedacos de lugar.
    /// Constroi o canvas ao arrancar (como o MenuPrincipal) com o TemaPixel.
    /// As imagens estao em Assets/Resources/Puzzle/Imagens: para juntar uma nova
    /// basta por la o ficheiro. Os niveis estao na lista Niveis.
    /// </summary>
    public class PuzzleJogo : MonoBehaviour
    {
        private struct Nivel
        {
            public readonly string nome;
            public readonly int colunas;
            public readonly int linhas;

            public Nivel(string nome, int colunas, int linhas)
            {
                this.nome = nome;
                this.colunas = colunas;
                this.linhas = linhas;
            }
        }

        // Para juntar um nivel, basta acrescentar uma linha (colunas x linhas).
        private static readonly Nivel[] Niveis =
        {
            new Nivel("FACIL", 4, 5),
            new Nivel("MEDIO", 6, 8),
            new Nivel("DIFICIL", 8, 10)
        };

        private const string CenaMenu = "MenuPrincipal";
        private const float LarguraMax = 700f;
        private const float AlturaMax = 820f;

        private enum Pagina { Niveis, Jogo }

        private Texture2D[] imagens = new Texture2D[0];
        private int imagemAtual;
        private int nivelAtual = -1;
        private int geracao;

        private RectTransform raiz;
        private RectTransform paginaNiveis;
        private RectTransform paginaJogo;
        private RectTransform areaTabuleiro;
        private RectTransform tabuleiro;
        private RectTransform painelInfo;
        private RectTransform fim;
        private RawImage previa;
        private Text textoNivel;
        private Text textoTempo;
        private Text textoTrocas;
        private Text textoResultado;
        private Text textoAviso;

        private PecaPuzzle[] pecas = new PecaPuzzle[0];
        private int[] pecaNoSlot = new int[0];
        private int colunas;
        private int linhas;
        private Vector2 tamPeca;

        private Pagina pagina = Pagina.Niveis;
        private float inicio;
        private float tempoFinal;
        private int trocas;
        private bool concluido;
        private bool aAnimar;
        private bool aVerImagem;
        private bool aTransitar;

        public bool PodeArrastar
        {
            get { return pagina == Pagina.Jogo && !concluido && !aAnimar && !aVerImagem && !aTransitar; }
        }

        // ---------------------------------------------------------------
        // Arranque
        // ---------------------------------------------------------------

        private void Awake()
        {
            Time.timeScale = 1f;
            GarantirEventSystem();
            imagens = Resources.LoadAll<Texture2D>("Puzzle/Imagens");
            ConstruirCanvas();
        }

        private void Start()
        {
            MostrarNiveis();
            if (imagens.Length == 0)
            {
                textoAviso.text = "Sem imagens em Resources/Puzzle/Imagens.";
                Debug.LogWarning("PuzzleJogo: nao ha imagens em Assets/Resources/Puzzle/Imagens.");
            }
        }

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        private void Update()
        {
            if (pagina == Pagina.Jogo && !concluido && textoTempo != null)
                textoTempo.text = "TEMPO " + Formatar(Time.unscaledTime - inicio);

            Keyboard teclado = Keyboard.current;
            if (teclado != null && teclado.escapeKey.wasPressedThisFrame && !aTransitar)
            {
                if (pagina == Pagina.Jogo) MostrarNiveis();
                else VoltarAoMenu();
            }
        }

        private static string Formatar(float segundos)
        {
            int s = Mathf.Max(0, Mathf.FloorToInt(segundos));
            return (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
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

            ConstruirFundo();

            paginaNiveis = TemaPixel.Novo("PaginaNiveis", raiz);
            TemaPixel.Esticar(paginaNiveis);
            paginaJogo = TemaPixel.Novo("PaginaJogo", raiz);
            TemaPixel.Esticar(paginaJogo);

            ConstruirPaginaNiveis();
            ConstruirPaginaJogo();
            ConstruirFim();
        }

        private void ConstruirFundo()
        {
            RectTransform fundo = TemaPixel.Novo("Fundo", raiz);
            TemaPixel.Esticar(fundo);
            Image imagem = fundo.gameObject.AddComponent<Image>();
            imagem.sprite = Resources.Load<Sprite>("Menu/menu_fundo");
            imagem.color = imagem.sprite != null ? Color.white : new Color32(40, 50, 70, 255);
            imagem.raycastTarget = false;
            AspectRatioFitter ajuste = fundo.gameObject.AddComponent<AspectRatioFitter>();
            ajuste.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            ajuste.aspectRatio = 16f / 9f;

            RectTransform escuro = TemaPixel.Novo("Escurecer", raiz);
            TemaPixel.Esticar(escuro);
            Image im2 = escuro.gameObject.AddComponent<Image>();
            im2.color = new Color32(12, 10, 24, 150);
            im2.raycastTarget = false;
        }

        private void ConstruirPaginaNiveis()
        {
            RectTransform caixa = TemaPixel.CriarPainel(paginaNiveis, 900f, true);
            TemaPixel.CriarFaixa(caixa, "PUZZLE", TemaPixel.Amarelo);

            Text intro = TemaPixel.CriarTexto("Intro", caixa,
                "Escolhe a dificuldade e monta a imagem arrastando as partes.", 24, TemaPixel.Creme);
            intro.gameObject.AddComponent<LayoutElement>().minHeight = 70f;

            for (int i = 0; i < Niveis.Length; i++)
            {
                int indice = i;
                Nivel n = Niveis[i];
                string rotulo = n.nome + "  " + n.colunas + " X " + n.linhas + "  (" + (n.colunas * n.linhas) + " PARTES)";
                TemaPixel.CriarBotao(caixa, rotulo, VarianteBotao.Terra, () => Iniciar(indice, false));
            }

            TemaPixel.CriarBotao(caixa, "VOLTAR", VarianteBotao.Asfalto, VoltarAoMenu);
            textoAviso = TemaPixel.CriarTexto("Aviso", caixa, "", 22, TemaPixel.Amarelo, false, TextAnchor.MiddleCenter, true);
            TemaPixel.CriarTexto("Ajuda", caixa, "Esc: voltar", 20, TemaPixel.Castanho);
        }

        private void ConstruirPaginaJogo()
        {
            areaTabuleiro = TemaPixel.Novo("AreaTabuleiro", paginaJogo);
            areaTabuleiro.anchorMin = areaTabuleiro.anchorMax = new Vector2(0.5f, 0.5f);
            areaTabuleiro.pivot = new Vector2(0.5f, 0.5f);
            areaTabuleiro.anchoredPosition = new Vector2(-300f, -10f);

            CriarMoldura("MolduraExterior", 26f, TemaPixel.Terracota);
            CriarMoldura("MolduraInterior", 12f, TemaPixel.Contorno);

            tabuleiro = TemaPixel.Novo("Tabuleiro", areaTabuleiro);
            tabuleiro.anchorMin = tabuleiro.anchorMax = new Vector2(0f, 1f);
            tabuleiro.pivot = new Vector2(0f, 1f);
            tabuleiro.anchoredPosition = Vector2.zero;

            RectTransform rp = TemaPixel.Novo("Previa", areaTabuleiro);
            TemaPixel.Esticar(rp);
            previa = rp.gameObject.AddComponent<RawImage>();
            previa.raycastTarget = false;
            rp.gameObject.SetActive(false);

            painelInfo = TemaPixel.CriarPainel(paginaJogo, 560f, true);
            painelInfo.anchoredPosition = new Vector2(520f, 0f);
            TemaPixel.CriarFaixa(painelInfo, "PUZZLE", TemaPixel.Amarelo);

            RectTransform placa = TemaPixel.CriarPlaca(painelInfo, "Placa");
            textoNivel = TemaPixel.CriarTexto("Nivel", placa, "", 28, TemaPixel.Amarelo);
            textoTempo = TemaPixel.CriarTexto("Tempo", placa, "TEMPO 00:00", 28, TemaPixel.Creme);
            textoTrocas = TemaPixel.CriarTexto("Trocas", placa, "TROCAS 0", 28, TemaPixel.Creme);

            Button bVer = TemaPixel.CriarBotao(painelInfo, "VER IMAGEM", VarianteBotao.Asfalto, null);
            bVer.gameObject.AddComponent<SegurarParaVer>().aoMudar = AoVerImagem;
            TemaPixel.CriarBotao(painelInfo, "BARALHAR", VarianteBotao.Terra, () => Iniciar(nivelAtual, false));
            TemaPixel.CriarBotao(painelInfo, "VOLTAR", VarianteBotao.Asfalto, MostrarNiveis);

            Text ajuda = TemaPixel.CriarTexto("Ajuda", painelInfo,
                "Arrasta uma parte para cima de outra para as trocar. Segura VER IMAGEM para espreitar.",
                20, TemaPixel.Castanho);
            ajuda.gameObject.AddComponent<LayoutElement>().minHeight = 80f;
        }

        private void CriarMoldura(string nome, float margem, Color cor)
        {
            RectTransform m = TemaPixel.Novo(nome, areaTabuleiro);
            TemaPixel.Esticar(m);
            m.offsetMin = new Vector2(-margem, -margem);
            m.offsetMax = new Vector2(margem, margem);
            Image im = m.gameObject.AddComponent<Image>();
            im.color = cor;
            im.raycastTarget = false;
        }

        private void ConstruirFim()
        {
            fim = TemaPixel.CriarFundoEcra(raiz, "Fim");
            RectTransform caixa = TemaPixel.CriarPainel(fim, 760f, true);
            TemaPixel.CriarFaixa(caixa, "BEM FEITO!", TemaPixel.Amarelo);

            RectTransform placa = TemaPixel.CriarPlaca(caixa, "Resultado");
            textoResultado = TemaPixel.CriarTexto("Texto", placa, "", 32, TemaPixel.Creme, false, TextAnchor.MiddleCenter, true);

            TemaPixel.CriarBotao(caixa, "OUTRA IMAGEM", VarianteBotao.Terra, () => Iniciar(nivelAtual, true));
            TemaPixel.CriarBotao(caixa, "DIFICULDADE", VarianteBotao.Asfalto, MostrarNiveis);
            TemaPixel.CriarBotao(caixa, "MENU", VarianteBotao.Asfalto, VoltarAoMenu);
            fim.gameObject.SetActive(false);
        }

        // ---------------------------------------------------------------
        // Navegacao
        // ---------------------------------------------------------------

        private void MostrarNiveis()
        {
            StopAllCoroutines();
            geracao++;
            aAnimar = false;
            aVerImagem = false;
            pagina = Pagina.Niveis;
            paginaNiveis.gameObject.SetActive(true);
            paginaJogo.gameObject.SetActive(false);
            fim.gameObject.SetActive(false);
            previa.gameObject.SetActive(false);
        }

        private void VoltarAoMenu()
        {
            if (aTransitar) return;
            aTransitar = true;
            MenuPrincipal.AbrirEscolhaDeJogos = true;
            SceneManager.LoadScene(CenaMenu);
        }

        private void AoVerImagem(bool ver)
        {
            if (ver && !PodeArrastar) return;
            aVerImagem = ver;
            if (previa != null) previa.gameObject.SetActive(ver);
        }

        // ---------------------------------------------------------------
        // Jogo
        // ---------------------------------------------------------------

        /// <summary>Comeca um nivel. outraImagem = true passa para a imagem seguinte.</summary>
        private void Iniciar(int indiceNivel, bool outraImagem)
        {
            if (imagens.Length == 0)
            {
                textoAviso.text = "Sem imagens em Resources/Puzzle/Imagens.";
                return;
            }

            if (indiceNivel < 0 || indiceNivel >= Niveis.Length) indiceNivel = 0;

            StopAllCoroutines();
            geracao++;
            aAnimar = false;
            aVerImagem = false;
            concluido = false;
            nivelAtual = indiceNivel;
            if (outraImagem) imagemAtual = (imagemAtual + 1) % imagens.Length;

            Nivel nivel = Niveis[indiceNivel];
            colunas = nivel.colunas;
            linhas = nivel.linhas;
            Texture2D tex = imagens[imagemAtual];

            ConstruirPecas(tex);
            Baralhar();
            Posicionar();
            AtualizarContornos();

            previa.texture = tex;
            previa.gameObject.SetActive(false);

            trocas = 0;
            inicio = Time.unscaledTime;
            textoNivel.text = nivel.nome + " " + colunas + " X " + linhas;
            textoTempo.text = "TEMPO 00:00";
            textoTrocas.text = "TROCAS 0";

            pagina = Pagina.Jogo;
            paginaNiveis.gameObject.SetActive(false);
            paginaJogo.gameObject.SetActive(true);
            fim.gameObject.SetActive(false);
            GerenciadorAudio.Tocar(Som.Clique);
        }

        private void ConstruirPecas(Texture2D tex)
        {
            for (int i = tabuleiro.childCount - 1; i >= 0; i--) Destroy(tabuleiro.GetChild(i).gameObject);

            // Tamanho do tabuleiro: cabe na area livre, com a proporcao da imagem.
            float aspecto = tex.width / (float)tex.height;
            float w = LarguraMax;
            float h = w / aspecto;
            if (h > AlturaMax)
            {
                h = AlturaMax;
                w = h * aspecto;
            }
            w = Mathf.Floor(w / colunas) * colunas;
            h = Mathf.Floor(h / linhas) * linhas;
            tamPeca = new Vector2(w / colunas, h / linhas);
            areaTabuleiro.sizeDelta = new Vector2(w, h);
            tabuleiro.sizeDelta = new Vector2(w, h);

            int total = colunas * linhas;
            pecas = new PecaPuzzle[total];
            for (int id = 0; id < total; id++)
            {
                int c = id % colunas;
                int r = id / colunas;

                GameObject go = new GameObject("Peca" + id, typeof(RectTransform));
                RectTransform rt = (RectTransform)go.transform;
                rt.SetParent(tabuleiro, false);
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0f, 1f);
                rt.sizeDelta = tamPeca;

                RawImage ri = go.AddComponent<RawImage>();
                ri.texture = tex;
                ri.uvRect = new Rect(c / (float)colunas, (linhas - 1 - r) / (float)linhas, 1f / colunas, 1f / linhas);
                ri.raycastTarget = true;

                Outline contorno = go.AddComponent<Outline>();
                contorno.effectColor = TemaPixel.Contorno;
                contorno.effectDistance = new Vector2(2f, -2f);
                contorno.useGraphicAlpha = false;

                PecaPuzzle peca = go.AddComponent<PecaPuzzle>();
                peca.Iniciar(this, id, rt, ri, contorno);
                pecas[id] = peca;
            }
        }

        /// <summary>Baralha (Fisher-Yates) ate nao ficar ja resolvido.</summary>
        private void Baralhar()
        {
            int total = colunas * linhas;
            pecaNoSlot = new int[total];
            do
            {
                for (int i = 0; i < total; i++) pecaNoSlot[i] = i;
                for (int i = total - 1; i > 0; i--)
                {
                    int j = UnityEngine.Random.Range(0, i + 1);
                    int tmp = pecaNoSlot[i];
                    pecaNoSlot[i] = pecaNoSlot[j];
                    pecaNoSlot[j] = tmp;
                }
            }
            while (Resolvido());
        }

        private void Posicionar()
        {
            for (int s = 0; s < pecaNoSlot.Length; s++)
                pecas[pecaNoSlot[s]].Rect.anchoredPosition = PosSlot(s);
        }

        private Vector2 PosSlot(int slot)
        {
            return new Vector2((slot % colunas) * tamPeca.x, -(slot / colunas) * tamPeca.y);
        }

        private bool Resolvido()
        {
            for (int i = 0; i < pecaNoSlot.Length; i++)
                if (pecaNoSlot[i] != i) return false;
            return true;
        }

        private void AtualizarContornos()
        {
            for (int s = 0; s < pecaNoSlot.Length; s++)
            {
                PecaPuzzle p = pecas[pecaNoSlot[s]];
                p.Contorno.effectColor = p.Id == s ? TemaPixel.VerdeEscuro : TemaPixel.Contorno;
                p.Imagem.SetVerticesDirty();
            }
        }

        // ---------------------------------------------------------------
        // Arrasto (chamado pelas pecas)
        // ---------------------------------------------------------------

        /// <summary>Posicao do rato no tabuleiro (origem no canto de cima, a esquerda; y para baixo e negativo).</summary>
        public Vector2 PontoNoTabuleiro(PointerEventData e)
        {
            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(tabuleiro, e.position, e.pressEventCamera, out local);
            return local;
        }

        /// <summary>Largou-se uma peca no ponto dado: troca com a peca que la esta, ou volta ao sitio.</summary>
        public void AoSoltar(PecaPuzzle p, Vector2 ponto)
        {
            p.Rect.localScale = Vector3.one;

            int origem = Array.IndexOf(pecaNoSlot, p.Id);
            int col = Mathf.FloorToInt(ponto.x / tamPeca.x);
            int lin = Mathf.FloorToInt(-ponto.y / tamPeca.y);
            bool dentro = col >= 0 && col < colunas && lin >= 0 && lin < linhas;
            int destino = dentro ? lin * colunas + col : -1;

            if (destino < 0 || destino == origem)
            {
                StartCoroutine(Mover(new[] { p.Rect }, new[] { PosSlot(origem) }, 0.12f, null));
                return;
            }

            PecaPuzzle outra = pecas[pecaNoSlot[destino]];
            pecaNoSlot[origem] = outra.Id;
            pecaNoSlot[destino] = p.Id;
            trocas++;
            textoTrocas.text = "TROCAS " + trocas;
            GerenciadorAudio.Tocar(Som.Clique);
            AtualizarContornos();

            bool completo = Resolvido();
            if (completo)
            {
                concluido = true;
                tempoFinal = Time.unscaledTime - inicio;
            }

            aAnimar = true;
            StartCoroutine(Mover(
                new[] { p.Rect, outra.Rect },
                new[] { PosSlot(destino), PosSlot(origem) },
                0.14f,
                () =>
                {
                    aAnimar = false;
                    if (completo) StartCoroutine(Vitoria());
                }));
        }

        private IEnumerator Mover(RectTransform[] itens, Vector2[] destinos, float duracao, Action aoFim)
        {
            Vector2[] origem = new Vector2[itens.Length];
            for (int i = 0; i < itens.Length; i++) origem[i] = itens[i].anchoredPosition;

            float t = 0f;
            while (t < duracao)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / duracao));
                for (int i = 0; i < itens.Length; i++) itens[i].anchoredPosition = Vector2.Lerp(origem[i], destinos[i], k);
                yield return null;
            }

            for (int i = 0; i < itens.Length; i++) itens[i].anchoredPosition = destinos[i];
            if (aoFim != null) aoFim();
        }

        private IEnumerator Vitoria()
        {
            int minha = geracao;
            textoTempo.text = "TEMPO " + Formatar(tempoFinal);
            GerenciadorAudio.Tocar(Som.ItemHistorico);

            yield return new WaitForSecondsRealtime(1.4f);
            if (minha != geracao || pagina != Pagina.Jogo) yield break;

            textoResultado.text = "TEMPO  " + Formatar(tempoFinal) + "\nTROCAS  " + trocas;
            fim.gameObject.SetActive(true);
        }
    }
}
