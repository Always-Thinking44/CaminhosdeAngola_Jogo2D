using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using CaminhosDeAngola.Jogo;

namespace CaminhosDeAngola.Interface
{
    public enum TipoPowerUp
    {
        Escudo,
        VidaExtra
    }

    /// <summary>Um item da loja. Tudo afinavel no Inspector (preco, segundos, icone).</summary>
    [Serializable]
    public class OpcaoLoja
    {
        public string nome = "Escudo";
        public TipoPowerUp tipo = TipoPowerUp.Escudo;
        [Min(0)] public int custo = 5;
        [Tooltip("So conta para o Escudo: quantos segundos protege.")]
        [Min(1f)] public float duracaoSegundos = 10f;
        [Tooltip("Icone do power-up. Se vazio, usa um quadrado colorido (placeholder).")]
        public Sprite icone;
        public Color corPlaceholder = new Color(0.3f, 0.8f, 1f, 1f);
    }

    /// <summary>
    /// Loja da zungueira. Ao abrir, o jogo pára (Time.timeScale = 0). Mostra a
    /// lista de power-ups com icone, descricao, duracao e preco; ao escolher,
    /// gasta os Zimbos. Podes comprar varios. Escolher com o rato ou com as
    /// teclas 1, 2, ...; fechar com E ou Esc. A UI e criada por este script.
    /// </summary>
    public class LojaZungueira : MonoBehaviour
    {
        [Header("Itens a venda")]
        [SerializeField] private List<OpcaoLoja> opcoes = new List<OpcaoLoja>
        {
            new OpcaoLoja { nome = "Escudo", tipo = TipoPowerUp.Escudo, custo = 5, duracaoSegundos = 10f, corPlaceholder = new Color(0.3f, 0.8f, 1f, 1f) },
            new OpcaoLoja { nome = "Vida extra", tipo = TipoPowerUp.VidaExtra, custo = 8, duracaoSegundos = 1f, corPlaceholder = new Color(0.86f, 0.16f, 0.16f, 1f) }
        };

        [Header("Ligacoes (procuradas sozinhas se ficarem vazias)")]
        [SerializeField] private GerenciadorColectaveis colectaveis;
        [SerializeField] private GerenciadorVidas vidas;
        [SerializeField] private GerenciadorPowerUps powerUps;

        [Header("Cores")]
        [SerializeField] private Color corCaixa = new Color(0.13f, 0.1f, 0.07f, 0.97f);
        [SerializeField] private Color corLinha = new Color(0.24f, 0.19f, 0.13f, 1f);
        [SerializeField] private Color corLinhaSemSaldo = new Color(0.16f, 0.14f, 0.12f, 1f);

        private class Linha
        {
            public Image fundo;
            public Text custo;
        }

        private readonly List<Linha> linhas = new List<Linha>();
        private Font fonte;
        private GameObject painel;
        private Text textoZimbos;
        private Text textoMensagem;

        private CheckpointZungueira origem;
        private bool houveCompra;
        private float escalaTempoAnterior = 1f;
        private int frameAbertura = -1;
        private int frameFecho = -1;

        /// <summary>Verdadeiro enquanto a loja esta aberta.</summary>
        public bool Aberta { get; private set; }

        private void Awake()
        {
            if (colectaveis == null) colectaveis = FindAnyObjectByType<GerenciadorColectaveis>();
            if (vidas == null) vidas = FindAnyObjectByType<GerenciadorVidas>();
            if (powerUps == null) powerUps = FindAnyObjectByType<GerenciadorPowerUps>();

            fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GarantirEventSystem();
            ConstruirInterface();
            painel.SetActive(false);
        }

        private void OnDestroy()
        {
            // Se a cena recarregar com a loja aberta, o tempo nao pode ficar parado.
            if (Aberta) Time.timeScale = 1f;
        }

        /// <summary>Abre a loja e pára o jogo. Chamado pela zungueira.</summary>
        public void Abrir(CheckpointZungueira zungueira)
        {
            if (Aberta || Time.frameCount <= frameFecho) return;

            origem = zungueira;
            houveCompra = false;
            frameAbertura = Time.frameCount;

            escalaTempoAnterior = Time.timeScale > 0f ? Time.timeScale : 1f;
            Time.timeScale = 0f;

            Aberta = true;
            DefinirMensagem("", Color.white);
            painel.SetActive(true);
            Actualizar();
        }

        /// <summary>Fecha a loja e o jogo continua.</summary>
        public void Fechar()
        {
            if (!Aberta) return;

            Aberta = false;
            frameFecho = Time.frameCount;
            painel.SetActive(false);
            Time.timeScale = escalaTempoAnterior;

            if (origem != null) origem.AoFecharLoja(houveCompra);
            origem = null;
        }

        private void Update()
        {
            if (!Aberta) return;

            Keyboard teclado = Keyboard.current;
            if (teclado == null) return;

            if (teclado.escapeKey.wasPressedThisFrame ||
                (teclado.eKey.wasPressedThisFrame && Time.frameCount > frameAbertura))
            {
                Fechar();
                return;
            }

            int maximo = Mathf.Min(opcoes.Count, 9);
            for (int i = 0; i < maximo; i++)
            {
                Key tecla = (Key)((int)Key.Digit1 + i);
                if (teclado[tecla].wasPressedThisFrame) Comprar(i);
            }
        }

        private void Comprar(int indice)
        {
            if (indice < 0 || indice >= opcoes.Count || colectaveis == null) return;

            OpcaoLoja opcao = opcoes[indice];

            if (colectaveis.Zimbos < opcao.custo)
            {
                DefinirMensagem("Não tens Zimbos suficientes.", new Color(1f, 0.45f, 0.4f));
                return;
            }

            switch (opcao.tipo)
            {
                case TipoPowerUp.Escudo:
                    if (powerUps == null) return;
                    powerUps.ActivarEscudo(opcao.duracaoSegundos);
                    break;

                case TipoPowerUp.VidaExtra:
                    if (vidas == null) return;
                    if (!vidas.PodeGanharVida)
                    {
                        DefinirMensagem("Já tens o máximo de vidas (" + vidas.VidasLimite + ").", new Color(1f, 0.85f, 0.4f));
                        return;
                    }
                    vidas.GanharVida();
                    break;
            }

            colectaveis.GastarZimbos(opcao.custo);
            houveCompra = true;
            DefinirMensagem(opcao.nome + " comprado!", new Color(0.5f, 1f, 0.55f));
            Actualizar();
        }

        private void Actualizar()
        {
            if (colectaveis != null) textoZimbos.text = "Zimbos: " + colectaveis.Zimbos;

            for (int i = 0; i < linhas.Count && i < opcoes.Count; i++)
            {
                OpcaoLoja opcao = opcoes[i];
                bool temSaldo = colectaveis != null && colectaveis.Zimbos >= opcao.custo;
                bool vidasCheias = opcao.tipo == TipoPowerUp.VidaExtra && vidas != null && !vidas.PodeGanharVida;

                linhas[i].fundo.color = (temSaldo && !vidasCheias) ? corLinha : corLinhaSemSaldo;
                linhas[i].custo.color = temSaldo ? Color.white : new Color(1f, 0.45f, 0.4f);
            }
        }

        private void DefinirMensagem(string texto, Color cor)
        {
            if (textoMensagem == null) return;
            textoMensagem.text = texto;
            textoMensagem.color = cor;
        }

        private string Descricao(OpcaoLoja opcao)
        {
            switch (opcao.tipo)
            {
                case TipoPowerUp.Escudo:
                    return "Protege de carros, moto e balas durante " + opcao.duracaoSegundos.ToString("0.#") + " s";
                case TipoPowerUp.VidaExtra:
                    int limite = vidas != null ? vidas.VidasLimite : 4;
                    return "Ganhas 1 vida (máximo " + limite + ")";
                default:
                    return "";
            }
        }

        // ---------------------------------------------------------------
        // Construcao da interface
        // ---------------------------------------------------------------

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        private void ConstruirInterface()
        {
            RectTransform raiz = (RectTransform)transform;
            Esticar(raiz);

            // Fundo escuro que cobre o ecra e bloqueia cliques no jogo.
            RectTransform rPainel = Novo("Painel", raiz);
            painel = rPainel.gameObject;
            Esticar(rPainel);
            painel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            RectTransform caixa = Novo("Caixa", rPainel);
            caixa.anchorMin = caixa.anchorMax = new Vector2(0.5f, 0.5f);
            caixa.pivot = new Vector2(0.5f, 0.5f);
            caixa.anchoredPosition = Vector2.zero;
            caixa.sizeDelta = new Vector2(820f, 100f);
            caixa.gameObject.AddComponent<Image>().color = corCaixa;

            VerticalLayoutGroup vertical = caixa.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(28, 28, 24, 24);
            vertical.spacing = 12f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;

            ContentSizeFitter ajuste = caixa.gameObject.AddComponent<ContentSizeFitter>();
            ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            NovoTexto("Titulo", caixa, "ZUNGUEIRA", 40, TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.4f), 52f, FontStyle.Bold);
            textoZimbos = NovoTexto("Zimbos", caixa, "Zimbos: 0", 28, TextAnchor.MiddleCenter, Color.white, 36f, FontStyle.Normal);

            linhas.Clear();
            for (int i = 0; i < opcoes.Count; i++)
                linhas.Add(ConstruirLinha(caixa, i));

            textoMensagem = NovoTexto("Mensagem", caixa, "", 24, TextAnchor.MiddleCenter, Color.white, 34f, FontStyle.Normal);
            NovoTexto("Ajuda", caixa, "Clica num item ou prime o número   |   E ou Esc para sair", 20, TextAnchor.MiddleCenter, new Color(0.75f, 0.75f, 0.75f), 30f, FontStyle.Normal);
        }

        private Linha ConstruirLinha(RectTransform pai, int indice)
        {
            OpcaoLoja opcao = opcoes[indice];

            RectTransform rt = Novo("Opcao_" + (indice + 1), pai);
            Image fundo = rt.gameObject.AddComponent<Image>();
            fundo.color = corLinha;

            Button botao = rt.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            int copia = indice;
            botao.onClick.AddListener(() => Comprar(copia));

            LayoutElement alturaLinha = rt.gameObject.AddComponent<LayoutElement>();
            alturaLinha.preferredHeight = 96f;

            HorizontalLayoutGroup horizontal = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(14, 18, 12, 12);
            horizontal.spacing = 18f;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            // Icone (sprite do power-up ou quadrado colorido).
            RectTransform rIcone = Novo("Icone", rt);
            Image icone = rIcone.gameObject.AddComponent<Image>();
            icone.raycastTarget = false;
            if (opcao.icone != null)
            {
                icone.sprite = opcao.icone;
                icone.preserveAspect = true;
            }
            else
            {
                icone.color = opcao.corPlaceholder;
            }
            LayoutElement leIcone = rIcone.gameObject.AddComponent<LayoutElement>();
            leIcone.preferredWidth = 72f;
            leIcone.preferredHeight = 72f;

            // Nome + descricao.
            RectTransform rTexto = Novo("Texto", rt);
            Text texto = rTexto.gameObject.AddComponent<Text>();
            texto.font = fonte;
            texto.supportRichText = true;
            texto.alignment = TextAnchor.MiddleLeft;
            texto.color = Color.white;
            texto.raycastTarget = false;
            texto.fontSize = 22;
            texto.text = "<b><size=30>[" + (indice + 1) + "] " + opcao.nome + "</size></b>\n" + Descricao(opcao);
            LayoutElement leTexto = rTexto.gameObject.AddComponent<LayoutElement>();
            leTexto.flexibleWidth = 1f;

            // Preco.
            RectTransform rCusto = Novo("Custo", rt);
            Text custo = rCusto.gameObject.AddComponent<Text>();
            custo.font = fonte;
            custo.fontSize = 30;
            custo.fontStyle = FontStyle.Bold;
            custo.alignment = TextAnchor.MiddleRight;
            custo.color = Color.white;
            custo.raycastTarget = false;
            custo.text = opcao.custo + " Zimbos";
            LayoutElement leCusto = rCusto.gameObject.AddComponent<LayoutElement>();
            leCusto.preferredWidth = 170f;

            return new Linha { fundo = fundo, custo = custo };
        }

        private Text NovoTexto(string nome, RectTransform pai, string conteudo, int tamanho,
            TextAnchor alinhamento, Color cor, float altura, FontStyle estilo)
        {
            RectTransform rt = Novo(nome, pai);
            Text texto = rt.gameObject.AddComponent<Text>();
            texto.font = fonte;
            texto.fontSize = tamanho;
            texto.fontStyle = estilo;
            texto.alignment = alinhamento;
            texto.color = cor;
            texto.raycastTarget = false;
            texto.text = conteudo;

            LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = altura;
            return texto;
        }

        private static RectTransform Novo(string nome, Transform pai)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(pai, false);
            return rt;
        }

        private static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
