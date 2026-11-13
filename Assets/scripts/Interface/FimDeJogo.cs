using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CaminhosDeAngola.Jogo;

namespace CaminhosDeAngola.Interface
{
    /// <summary>
    /// Ecra de "Fim de jogo", mostrado quando o jogador fica sem vidas.
    /// O jogo pára e o jogador escolhe entre jogar outra vez (recarrega o
    /// nivel) ou sair. Teclado: Enter = jogar outra vez, Esc = sair.
    /// A UI e criada por este script, no mesmo estilo do FimDeNivel.
    /// </summary>
    public class FimDeJogo : MonoBehaviour
    {
        [Header("Ligacoes (procuradas sozinhas se ficarem vazias)")]
        [SerializeField] private GerenciadorVidas vidas;
        [SerializeField] private GerenciadorColectaveis colectaveis;

        [Header("Textos")]
        [SerializeField] private string titulo = "FIM DE JOGO";
        [SerializeField] private string subtitulo = "O Esquecimento apanhou-te. Ficaste sem vidas.";
        [SerializeField] private string textoJogarOutraVez = "Jogar outra vez";
        [SerializeField] private string textoSair = "Sair do jogo";

        [Header("Cores")]
        [SerializeField] private Color corCaixa = new Color(0.13f, 0.1f, 0.07f, 0.97f);
        [SerializeField] private Color corTitulo = new Color(1f, 0.45f, 0.4f);
        [SerializeField] private Color corBotaoPrincipal = new Color(0.55f, 0.36f, 0.14f, 1f);
        [SerializeField] private Color corBotaoSecundario = new Color(0.30f, 0.22f, 0.18f, 1f);

        private Font fonte;
        private bool mostrado;

        private void Awake()
        {
            if (vidas == null) vidas = FindAnyObjectByType<GerenciadorVidas>();
            if (colectaveis == null) colectaveis = FindAnyObjectByType<GerenciadorColectaveis>();

            fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GarantirEventSystem();
        }

        private void OnEnable()
        {
            if (vidas != null) vidas.AoSemVidas += Mostrar;
        }

        private void OnDisable()
        {
            if (vidas != null) vidas.AoSemVidas -= Mostrar;
        }

        private void OnDestroy()
        {
            // Se a cena recarregar, o tempo nao pode ficar parado.
            if (mostrado) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (!mostrado) return;

            Keyboard teclado = Keyboard.current;
            if (teclado == null) return;

            if (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame)
                JogarOutraVez();
            else if (teclado.escapeKey.wasPressedThisFrame)
                Sair();
        }

        private void JogarOutraVez()
        {
            Time.timeScale = 1f;
            mostrado = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void Sair()
        {
#if UNITY_EDITOR
            Time.timeScale = 1f;
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ---------------------------------------------------------------
        // Canvas
        // ---------------------------------------------------------------

        private void Mostrar()
        {
            if (mostrado) return;
            mostrado = true;
            Time.timeScale = 0f;

            RectTransform raiz = (RectTransform)transform;
            Esticar(raiz);

            RectTransform painel = Novo("PainelFimDeJogo", raiz);
            Esticar(painel);
            painel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);

            RectTransform caixa = Novo("Caixa", painel);
            caixa.anchorMin = caixa.anchorMax = new Vector2(0.5f, 0.5f);
            caixa.pivot = new Vector2(0.5f, 0.5f);
            caixa.anchoredPosition = Vector2.zero;
            caixa.sizeDelta = new Vector2(760f, 100f);
            caixa.gameObject.AddComponent<Image>().color = corCaixa;

            VerticalLayoutGroup vertical = caixa.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(32, 32, 28, 28);
            vertical.spacing = 12f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;

            ContentSizeFitter ajuste = caixa.gameObject.AddComponent<ContentSizeFitter>();
            ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            NovoTexto("Titulo", caixa, titulo, 52, corTitulo, 70f, FontStyle.Bold);
            NovoTexto("Subtitulo", caixa, subtitulo, 26, Color.white, 40f, FontStyle.Normal);

            if (colectaveis != null)
            {
                string resumo = "Itens históricos: " + colectaveis.ItensHistoricos + " de " + colectaveis.TotalItensHistoricos
                    + "     |     Zimbos: " + colectaveis.Zimbos;
                NovoTexto("Resumo", caixa, resumo, 24, new Color(1f, 0.82f, 0.4f), 44f, FontStyle.Bold);
            }

            ConstruirLinhaDeBotoes(caixa);

            NovoTexto("Ajuda", caixa, "Enter: jogar outra vez     |     Esc: sair", 20,
                new Color(0.65f, 0.65f, 0.65f), 30f, FontStyle.Normal);
        }

        private void ConstruirLinhaDeBotoes(RectTransform pai)
        {
            RectTransform linha = Novo("Botoes", pai);
            linha.gameObject.AddComponent<LayoutElement>().preferredHeight = 70f;

            HorizontalLayoutGroup horizontal = linha.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 24f;
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = true;
            horizontal.childForceExpandHeight = true;

            ConstruirBotao(linha, textoJogarOutraVez, corBotaoPrincipal, JogarOutraVez);
            ConstruirBotao(linha, textoSair, corBotaoSecundario, Sair);
        }

        private void ConstruirBotao(RectTransform pai, string rotulo, Color cor, UnityEngine.Events.UnityAction accao)
        {
            RectTransform rt = Novo("Botao", pai);
            Image fundo = rt.gameObject.AddComponent<Image>();
            fundo.color = cor;

            Button botao = rt.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            botao.onClick.AddListener(accao);

            RectTransform rTexto = Novo("Rotulo", rt);
            Esticar(rTexto);
            Text texto = rTexto.gameObject.AddComponent<Text>();
            texto.font = fonte;
            texto.fontSize = 28;
            texto.fontStyle = FontStyle.Bold;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.color = Color.white;
            texto.raycastTarget = false;
            texto.text = rotulo;
        }

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        private void NovoTexto(string nome, RectTransform pai, string conteudo, int tamanho,
            Color cor, float altura, FontStyle estilo)
        {
            RectTransform rt = Novo(nome, pai);
            Text texto = rt.gameObject.AddComponent<Text>();
            texto.font = fonte;
            texto.fontSize = tamanho;
            texto.fontStyle = estilo;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.color = cor;
            texto.raycastTarget = false;
            texto.text = conteudo;
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = altura;
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
