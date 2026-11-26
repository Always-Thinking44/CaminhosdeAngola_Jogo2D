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
    /// Pausa com Esc. Cria-se sozinho (como o GerenciadorAudio), por isso nao
    /// e preciso pôr nada nas cenas. So funciona em cenas de jogo (as que
    /// tem GerenciadorVidas): no menu e no puzzle o Esc nao faz nada.
    /// Nao abre se o tempo ja estiver parado por outro ecra (loja, fim de
    /// jogo, fim de nivel). O aspecto vem do TemaPixel.
    /// </summary>
    public class GerenciadorPausa : MonoBehaviour
    {
        private static GerenciadorPausa instancia;

        private bool pausado;
        private float escalaAnterior = 1f;
        private float escalaNoFimDoFrame = 1f;
        private GameObject canvas;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Criar()
        {
            if (instancia != null) return;
            GameObject go = new GameObject("GerenciadorPausa");
            instancia = go.AddComponent<GerenciadorPausa>();
            DontDestroyOnLoad(go);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += AoCarregarCena;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
        }

        // Se a cena mudar com o jogo em pausa, limpa tudo.
        private void AoCarregarCena(Scene cena, LoadSceneMode modo)
        {
            if (!pausado) return;
            FecharCanvas();
            pausado = false;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            Keyboard teclado = Keyboard.current;
            if (teclado == null || !teclado.escapeKey.wasPressedThisFrame) return;

            if (pausado)
            {
                Retomar();
                return;
            }

            // Tempo ja parado por outro ecra (loja, fim de jogo, fim de nivel): nao mexer.
            if (escalaNoFimDoFrame <= 0f || Time.timeScale <= 0f) return;
            if (FindAnyObjectByType<GerenciadorVidas>() == null) return;

            Pausar();
        }

        private void LateUpdate()
        {
            escalaNoFimDoFrame = Time.timeScale;
        }

        private void OnDestroy()
        {
            if (instancia == this) instancia = null;
        }

        // ---------------------------------------------------------------

        private void Pausar()
        {
            pausado = true;
            escalaAnterior = Time.timeScale;
            Time.timeScale = 0f;
            ConstruirInterface();
        }

        private void Retomar()
        {
            pausado = false;
            Time.timeScale = escalaAnterior > 0f ? escalaAnterior : 1f;
            FecharCanvas();
        }

        private void Reiniciar()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void IrParaMenu()
        {
            Time.timeScale = 1f;
            if (Application.CanStreamedLevelBeLoaded(ProgressoJogo.CenaMenu))
            {
                SceneManager.LoadScene(ProgressoJogo.CenaMenu);
                return;
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void FecharCanvas()
        {
            if (canvas != null) Destroy(canvas);
            canvas = null;
        }

        // ---------------------------------------------------------------
        // Interface
        // ---------------------------------------------------------------

        private void ConstruirInterface()
        {
            GarantirEventSystem();

            canvas = new GameObject("CanvasPausa", typeof(RectTransform));
            canvas.transform.SetParent(transform, false);
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 500;

            CanvasScaler escala = canvas.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escala.matchWidthOrHeight = 0.5f;
            escala.referencePixelsPerUnit = 100f;

            canvas.AddComponent<GraphicRaycaster>();

            RectTransform ecra = TemaPixel.CriarFundoEcra(canvas.transform, "PainelPausa");
            RectTransform caixa = TemaPixel.CriarPainel(ecra, 760f, true);
            TemaPixel.CriarFaixa(caixa, "PAUSA", TemaPixel.Creme);

            TemaPixel.CriarTexto("Subtitulo", caixa, "O jogo está parado.", 32, TemaPixel.Creme);
            TemaPixel.CriarDivisor(caixa);

            Button continuar = TemaPixel.CriarBotao(caixa, "Continuar", VarianteBotao.Terra, Retomar);
            TemaPixel.CriarBotao(caixa, "Reiniciar nível", VarianteBotao.Asfalto, Reiniciar);
            TemaPixel.CriarBotao(caixa, "Menu principal", VarianteBotao.Asfalto, IrParaMenu);

            TemaPixel.CriarTexto("Ajuda", caixa, "Esc: continuar     Setas + Enter: escolher", 24, TemaPixel.Castanho);

            // Teclado: o primeiro botao fica seleccionado (Enter confirma).
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(continuar.gameObject);
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
