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
    /// <summary>
    /// Loja da zungueira. Ao abrir, o jogo pára (Time.timeScale = 0). Mostra a
    /// lista de power-ups com icone, descricao e preco; ao escolher,
    /// gasta os Zimbos. Podes comprar varios. Escolher com o rato ou com as
    /// teclas 1, 2, ...; fechar com E ou Esc. O aspecto vem do TemaPixel.
    /// </summary>
    public class LojaZungueiraNova : MonoBehaviour
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

        private class Linha
        {
            public Button botao;
            public Text custo;
        }

        private readonly List<Linha> linhas = new List<Linha>();
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
            DefinirMensagem("", TemaPixel.Contorno);
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
                DefinirMensagem("Não tens Zimbos suficientes.", TemaPixel.TerracotaEscura);
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
                        DefinirMensagem("Já tens o máximo de vidas (" + vidas.VidasLimite + ").", TemaPixel.Terracota);
                        return;
                    }
                    vidas.GanharVida();
                    break;
            }

            colectaveis.GastarZimbos(opcao.custo);
            houveCompra = true;
            DefinirMensagem(opcao.nome + " comprado!", TemaPixel.VerdeEscuro);
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

                TemaPixel.DefinirVariante(linhas[i].botao, (temSaldo && !vidasCheias) ? VarianteBotao.Asfalto : VarianteBotao.Inactivo);
                linhas[i].custo.color = temSaldo ? TemaPixel.Creme : TemaPixel.VermelhoClaro;
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
            TemaPixel.Esticar(raiz);

            // Fundo escuro que cobre o ecra e bloqueia cliques no jogo.
            RectTransform ecra = TemaPixel.CriarFundoEcra(raiz, "Painel");
            painel = ecra.gameObject;

            RectTransform caixa = TemaPixel.CriarPainel(ecra, 960f, true);
            TemaPixel.CriarFaixa(caixa, "ZUNGUEIRA", TemaPixel.Amarelo);

            RectTransform saldo = TemaPixel.CriarPlaca(caixa, "Saldo");
            textoZimbos = TemaPixel.CriarTexto("Zimbos", saldo, "Zimbos: 0", 32, TemaPixel.Amarelo,
                false, TextAnchor.MiddleCenter, true);

            TemaPixel.CriarDivisor(caixa);

            linhas.Clear();
            for (int i = 0; i < opcoes.Count; i++)
                linhas.Add(ConstruirLinha(caixa, i));

            textoMensagem = TemaPixel.CriarTexto("Mensagem", caixa, "", 24, TemaPixel.Contorno);
            textoMensagem.gameObject.AddComponent<LayoutElement>().minHeight = 36f;

            TemaPixel.CriarTexto("Ajuda", caixa, "Clica num item ou prime o número     E ou Esc: sair", 24, TemaPixel.Castanho);
        }

        private Linha ConstruirLinha(RectTransform pai, int indice)
        {
            OpcaoLoja opcao = opcoes[indice];

            RectTransform rt = TemaPixel.Novo("Opcao_" + (indice + 1), pai);
            Image fundo = TemaPixel.AplicarSprite(rt.gameObject.AddComponent<Image>(), "ui_botao_asfalto");

            Button botao = rt.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            botao.transition = Selectable.Transition.SpriteSwap;
            TemaPixel.DefinirVariante(botao, VarianteBotao.Asfalto);
            int copia = indice;
            botao.onClick.AddListener(() => Comprar(copia));

            rt.gameObject.AddComponent<LayoutElement>().minHeight = 128f;

            HorizontalLayoutGroup horizontal = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(40, 40, 24, 24);
            horizontal.spacing = 24f;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            // Icone numa moldura escura (sprite do power-up ou quadrado colorido).
            RectTransform moldura = TemaPixel.Novo("Icone", rt);
            Image imgMoldura = TemaPixel.AplicarSprite(moldura.gameObject.AddComponent<Image>(), "ui_placa_escura");
            imgMoldura.raycastTarget = false;
            LayoutElement leMoldura = moldura.gameObject.AddComponent<LayoutElement>();
            leMoldura.preferredWidth = 80f;
            leMoldura.preferredHeight = 80f;

            RectTransform dentro = TemaPixel.Novo("Imagem", moldura);
            dentro.anchorMin = dentro.anchorMax = new Vector2(0.5f, 0.5f);
            dentro.pivot = new Vector2(0.5f, 0.5f);
            dentro.anchoredPosition = Vector2.zero;
            dentro.sizeDelta = new Vector2(48f, 48f);
            Image icone = dentro.gameObject.AddComponent<Image>();
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

            // Nome + descricao.
            RectTransform coluna = TemaPixel.Novo("Texto", rt);
            VerticalLayoutGroup vertical = coluna.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.spacing = 8f;
            vertical.childAlignment = TextAnchor.MiddleLeft;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;
            coluna.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            TemaPixel.CriarTexto("Nome", coluna, "[" + (indice + 1) + "] " + opcao.nome, 24, TemaPixel.Amarelo,
                true, TextAnchor.MiddleLeft, true);
            TemaPixel.CriarTexto("Descricao", coluna, Descricao(opcao), 24, TemaPixel.Creme,
                false, TextAnchor.MiddleLeft, true);

            // Preco.
            Text custo = TemaPixel.CriarTexto("Custo", rt, opcao.custo + " Zimbos", 24, TemaPixel.Creme,
                true, TextAnchor.MiddleRight, true);
            custo.gameObject.AddComponent<LayoutElement>().preferredWidth = 250f;

            return new Linha { botao = botao, custo = custo };
        }
    }
}
