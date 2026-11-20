using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CaminhosDeAngola.Puzzle
{
    /// <summary>
    /// Uma peca do puzzle (um pedaco da imagem). Arrasta-se com o rato e, ao
    /// largar em cima de outra, as duas trocam de lugar. A logica do jogo
    /// fica no PuzzleJogo; esta classe so trata do arrasto.
    /// </summary>
    public class PecaPuzzle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private PuzzleJogo dono;
        private Vector2 deslocamento;
        private bool aArrastar;

        /// <summary>Posicao correcta desta peca (0 = canto de cima, a esquerda).</summary>
        public int Id { get; private set; }
        public RectTransform Rect { get; private set; }
        public RawImage Imagem { get; private set; }
        public Outline Contorno { get; private set; }

        public void Iniciar(PuzzleJogo jogo, int id, RectTransform rect, RawImage imagem, Outline contorno)
        {
            dono = jogo;
            Id = id;
            Rect = rect;
            Imagem = imagem;
            Contorno = contorno;
        }

        public void OnBeginDrag(PointerEventData e)
        {
            aArrastar = dono != null && dono.PodeArrastar;
            if (!aArrastar) return;

            deslocamento = dono.PontoNoTabuleiro(e) - Rect.anchoredPosition;
            Rect.SetAsLastSibling();
            Rect.localScale = new Vector3(1.06f, 1.06f, 1f);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!aArrastar) return;
            Rect.anchoredPosition = dono.PontoNoTabuleiro(e) - deslocamento;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!aArrastar) return;
            aArrastar = false;
            dono.AoSoltar(this, dono.PontoNoTabuleiro(e));
        }
    }

    /// <summary>Botao que so mostra algo enquanto esta premido (ex.: ver a imagem completa).</summary>
    public class SegurarParaVer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action<bool> aoMudar;

        public void OnPointerDown(PointerEventData e) { if (aoMudar != null) aoMudar(true); }
        public void OnPointerUp(PointerEventData e) { if (aoMudar != null) aoMudar(false); }
        public void OnPointerExit(PointerEventData e) { if (aoMudar != null) aoMudar(false); }
    }
}
