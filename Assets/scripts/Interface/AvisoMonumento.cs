using UnityEngine;
using UnityEngine.UI;

namespace CaminhosDeAngola.Interface
{
    /// <summary>
    /// Aviso no topo do ecra com o nome do monumento, mostrado quando o
    /// jogador apanha um item historico. Entra a deslizar, fica alguns
    /// segundos e desaparece. Usa tempo real (continua mesmo com o jogo
    /// em pausa). Cria-se sozinho na primeira utilizacao.
    /// Uso: AvisoMonumento.Mostrar("Palacio de Ferro");
    /// </summary>
    public class AvisoMonumento : MonoBehaviour
    {
        private const float TempoEntrada = 0.35f;
        private const float TempoVisivel = 3.5f;
        private const float TempoSaida = 0.5f;
        private const float YEscondido = 90f;   // acima do ecra
        private const float YVisivel = -48f;    // abaixo do bordo de cima

        private static AvisoMonumento instancia;

        private RectTransform placa;
        private CanvasGroup grupo;
        private Text textoNome;
        private float inicio;
        private bool activo;

        /// <summary>Mostra o aviso com o nome do monumento (substitui o que estiver a aparecer).</summary>
        public static void Mostrar(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome)) return;

            if (instancia == null) instancia = Criar();
            instancia.Iniciar(nome);
        }

        private static AvisoMonumento Criar()
        {
            GameObject go = new GameObject("AvisoMonumento");
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400; // por baixo da pausa (500)

            CanvasScaler escala = go.AddComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            escala.matchWidthOrHeight = 0.5f;
            escala.referencePixelsPerUnit = 100f;

            AvisoMonumento aviso = go.AddComponent<AvisoMonumento>();
            aviso.Construir();
            return aviso;
        }

        private void Construir()
        {
            placa = TemaPixel.CriarPlaca(transform, "Placa");
            placa.anchorMin = placa.anchorMax = new Vector2(0.5f, 1f);
            placa.pivot = new Vector2(0.5f, 1f);
            placa.anchoredPosition = new Vector2(0f, YEscondido);

            ContentSizeFitter ajuste = placa.gameObject.AddComponent<ContentSizeFitter>();
            ajuste.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            grupo = placa.gameObject.AddComponent<CanvasGroup>();
            grupo.interactable = false;
            grupo.blocksRaycasts = false;
            grupo.alpha = 0f;

            Text etiqueta = TemaPixel.CriarTexto("Etiqueta", placa, "ITEM HIST\u00D3RICO", 20, TemaPixel.Amarelo);
            etiqueta.horizontalOverflow = HorizontalWrapMode.Overflow;

            textoNome = TemaPixel.CriarTexto("Nome", placa, "", 32, TemaPixel.Creme, true, TextAnchor.MiddleCenter, true);
            textoNome.horizontalOverflow = HorizontalWrapMode.Overflow;

            placa.gameObject.SetActive(false);
        }

        private void Iniciar(string nome)
        {
            textoNome.text = nome.ToUpperInvariant();
            placa.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(placa);

            inicio = Time.unscaledTime;
            activo = true;
            Aplicar(0f);
        }

        private void Update()
        {
            if (!activo) return;

            float t = Time.unscaledTime - inicio;
            if (t >= TempoEntrada + TempoVisivel + TempoSaida)
            {
                activo = false;
                placa.gameObject.SetActive(false);
                return;
            }

            float k;
            if (t < TempoEntrada) k = t / TempoEntrada;
            else if (t < TempoEntrada + TempoVisivel) k = 1f;
            else k = 1f - (t - TempoEntrada - TempoVisivel) / TempoSaida;

            Aplicar(Mathf.SmoothStep(0f, 1f, k));
        }

        // k: 0 = escondido, 1 = totalmente visivel
        private void Aplicar(float k)
        {
            grupo.alpha = k;
            placa.anchoredPosition = new Vector2(0f, Mathf.Lerp(YEscondido, YVisivel, k));
        }

        private void OnDestroy()
        {
            if (instancia == this) instancia = null;
        }
    }
}
