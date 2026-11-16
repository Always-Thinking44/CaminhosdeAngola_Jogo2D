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
    /// O aspecto vem do TemaPixel (pixel art, paleta de Luanda).
    /// </summary>
    public class FimDeJogoNovo : MonoBehaviour
    {
        [Header("Ligacoes (procuradas sozinhas se ficarem vazias)")]
        [SerializeField] private GerenciadorVidas vidas;
        [SerializeField] private GerenciadorColectaveis colectaveis;

        [Header("Textos")]
        [SerializeField] private string titulo = "FIM DE JOGO";
        [SerializeField] private string subtitulo = "O Esquecimento apanhou-te. Ficaste sem vidas.";
        [SerializeField] private string textoJogarOutraVez = "Jogar outra vez";
        [SerializeField] private string textoSair = "Sair do jogo";

        private bool mostrado;

        private void Awake()
        {
            if (vidas == null) vidas = FindAnyObjectByType<GerenciadorVidas>();
            if (colectaveis == null) colectaveis = FindAnyObjectByType<GerenciadorColectaveis>();
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
            TemaPixel.Esticar(raiz);

            RectTransform ecra = TemaPixel.CriarFundoEcra(raiz, "PainelFimDeJogo");
            RectTransform caixa = TemaPixel.CriarPainel(ecra, 760f, true);
            TemaPixel.CriarFaixa(caixa, titulo, TemaPixel.Creme);

            TemaPixel.CriarTexto("Subtitulo", caixa, subtitulo, 32, TemaPixel.Contorno);
            TemaPixel.CriarDivisor(caixa);

            if (colectaveis != null)
            {
                RectTransform placa = TemaPixel.CriarPlaca(caixa, "Resumo");
                TemaPixel.CriarTexto("Itens", placa,
                    "Itens históricos: " + colectaveis.ItensHistoricos + " de " + colectaveis.TotalItensHistoricos,
                    32, TemaPixel.Amarelo, false, TextAnchor.MiddleCenter, true);
                TemaPixel.CriarTexto("Zimbos", placa, "Zimbos: " + colectaveis.Zimbos,
                    32, TemaPixel.Creme, false, TextAnchor.MiddleCenter, true);
            }

            RectTransform linha = TemaPixel.Novo("Botoes", caixa);
            linha.gameObject.AddComponent<LayoutElement>().minHeight = 96f;
            HorizontalLayoutGroup horizontal = linha.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.spacing = 24f;
            horizontal.childAlignment = TextAnchor.MiddleCenter;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = true;
            horizontal.childForceExpandHeight = true;

            TemaPixel.CriarBotao(linha, textoJogarOutraVez, VarianteBotao.Terra, JogarOutraVez);
            TemaPixel.CriarBotao(linha, textoSair, VarianteBotao.Asfalto, Sair);

            TemaPixel.CriarTexto("Ajuda", caixa, "Enter: jogar outra vez     Esc: sair", 24, TemaPixel.Castanho);
        }

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }
    }
}
