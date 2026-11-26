using UnityEngine;
using UnityEngine.UI;

namespace CaminhosDeAngola.Menu
{
    /// <summary>
    /// Anima o "parado" de um personagem numa Image do menu. Mantem a escala
    /// de pixel igual para todos (escalaPixel canvas-unidades por pixel de
    /// arte) e alinha o pivo do sprite ao centro, para nao tremer.
    /// </summary>
    public class AnimadorRetrato : MonoBehaviour
    {
        private Image imagem;
        private RectTransform rt;
        private Sprite[] frames;
        private float fotogramas = 8f;
        private float escala = 1.6f;
        private float margemBaixo;
        private int ultimo = -1;

        public void Configurar(Image alvo, Sprite[] sprites, float fotogramasPorSegundo, float escalaPixel, float margem)
        {
            imagem = alvo;
            rt = (RectTransform)alvo.transform;
            frames = sprites;
            fotogramas = Mathf.Max(1f, fotogramasPorSegundo);
            escala = escalaPixel;
            margemBaixo = margem;
            ultimo = -1;
            if (frames != null && frames.Length > 0) Aplicar(0);
        }

        private void Update()
        {
            if (frames == null || frames.Length == 0) return;
            int i = (int)(Time.unscaledTime * fotogramas) % frames.Length;
            if (i != ultimo) Aplicar(i);
        }

        private void Aplicar(int i)
        {
            Sprite s = frames[i];
            if (s == null) return;

            float k = escala * (100f / s.pixelsPerUnit);
            Vector2 tamanho = s.rect.size * k;
            imagem.sprite = s;
            rt.sizeDelta = tamanho;
            rt.anchoredPosition = new Vector2((tamanho.x * 0.5f) - (s.pivot.x * k), margemBaixo);
            ultimo = i;
        }
    }
}
