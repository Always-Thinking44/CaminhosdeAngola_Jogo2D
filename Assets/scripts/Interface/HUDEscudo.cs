using UnityEngine;
using UnityEngine.UI;
using CaminhosDeAngola.Jogo;

namespace CaminhosDeAngola.Interface
{
    /// <summary>
    /// Mostra no ecra o tempo que falta ao escudo: um texto e uma barra que
    /// vai esvaziando. So aparece enquanto o escudo esta activo. Cria os
    /// elementos de UI sozinho, por baixo deste objecto.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class HUDEscudo : MonoBehaviour
    {
        [SerializeField] private GerenciadorPowerUps powerUps;

        [Header("Posicao (canto superior esquerdo do HUD)")]
        [SerializeField] private Vector2 posicao = new Vector2(24f, -172f);
        [SerializeField] private Vector2 tamanho = new Vector2(300f, 44f);

        [Header("Cores")]
        [SerializeField] private Color corFundo = new Color(0f, 0f, 0f, 0.45f);
        [SerializeField] private Color corBarra = new Color(0.3f, 0.8f, 1f, 0.85f);
        [SerializeField] private Color corTexto = Color.white;

        private GameObject conteudo;
        private RectTransform barra;
        private Text texto;

        private void Awake()
        {
            if (powerUps == null) powerUps = FindAnyObjectByType<GerenciadorPowerUps>();

            RectTransform rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = posicao;
            rt.sizeDelta = tamanho;

            Construir(rt);
            conteudo.SetActive(false);
        }

        private void Construir(RectTransform pai)
        {
            conteudo = new GameObject("Conteudo", typeof(RectTransform), typeof(Image));
            RectTransform rc = (RectTransform)conteudo.transform;
            rc.SetParent(pai, false);
            Esticar(rc);
            conteudo.GetComponent<Image>().color = corFundo;

            GameObject objBarra = new GameObject("Barra", typeof(RectTransform), typeof(Image));
            barra = (RectTransform)objBarra.transform;
            barra.SetParent(rc, false);
            barra.anchorMin = new Vector2(0f, 0f);
            barra.anchorMax = new Vector2(1f, 1f);
            barra.offsetMin = new Vector2(4f, 4f);
            barra.offsetMax = new Vector2(-4f, -4f);
            objBarra.GetComponent<Image>().color = corBarra;

            GameObject objTexto = new GameObject("TextoEscudo", typeof(RectTransform), typeof(Text));
            RectTransform rtTexto = (RectTransform)objTexto.transform;
            rtTexto.SetParent(rc, false);
            Esticar(rtTexto);
            rtTexto.offsetMin = new Vector2(12f, 0f);
            texto = objTexto.GetComponent<Text>();
            texto.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            texto.fontSize = 24;
            texto.fontStyle = FontStyle.Bold;
            texto.alignment = TextAnchor.MiddleLeft;
            texto.color = corTexto;
            texto.raycastTarget = false;
        }

        private static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            bool activo = powerUps != null && powerUps.EscudoActivo;
            if (conteudo.activeSelf != activo) conteudo.SetActive(activo);
            if (!activo) return;

            float fraccao = powerUps.EscudoDuracao > 0f
                ? Mathf.Clamp01(powerUps.EscudoRestante / powerUps.EscudoDuracao)
                : 0f;

            barra.anchorMax = new Vector2(fraccao, 1f);
            barra.offsetMax = new Vector2(fraccao >= 1f ? -4f : 0f, -4f);
            texto.text = "ESCUDO  " + Mathf.CeilToInt(powerUps.EscudoRestante) + " s";
        }
    }
}
