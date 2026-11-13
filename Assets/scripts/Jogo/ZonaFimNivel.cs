using UnityEngine;
using CaminhosDeAngola.Interface;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Zona invisivel no fim do nivel. Quando o jogador a toca, avisa o
    /// FimDeNivel, que decide se o nivel foi concluido ou se tem de se repetir.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ZonaFimNivel : MonoBehaviour
    {
        [Tooltip("Opcional: se vazio, procura o FimDeNivel da cena.")]
        [SerializeField] private FimDeNivel fimDeNivel;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            if (fimDeNivel == null) fimDeNivel = FindAnyObjectByType<FimDeNivel>();
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;
            if (fimDeNivel != null) fimDeNivel.Concluir();
        }

        private void OnDrawGizmos()
        {
            BoxCollider2D caixa = GetComponent<BoxCollider2D>();
            if (caixa == null) return;

            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(caixa.offset, caixa.size);
        }
    }
}
