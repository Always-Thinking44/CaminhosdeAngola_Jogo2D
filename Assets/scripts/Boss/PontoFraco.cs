using UnityEngine;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// Ponto fraco do Grande Esquecimento. So esta activo enquanto o boss esta
    /// atordoado. Quando o jogador lhe cai em cima, tira 1 ponto de vida ao
    /// boss e o jogador ressalta para cima. Precisa de um Collider2D (trigger).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PontoFraco : MonoBehaviour
    {
        [SerializeField] private GrandeEsquecimento boss;
        [Tooltip("Impulso vertical dado ao jogador depois de acertar.")]
        [SerializeField, Min(0f)] private float ressalto = 9f;
        [Tooltip("Tempo minimo entre dois acertos, para um salto nao contar duas vezes.")]
        [SerializeField, Min(0f)] private float intervaloEntreAcertos = 0.6f;

        private float proximoAcerto;

        private void Reset()
        {
            Collider2D c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            if (boss == null) boss = GetComponentInParent<GrandeEsquecimento>();
        }

        private void OnTriggerEnter2D(Collider2D outro) { Tentar(outro); }
        private void OnTriggerStay2D(Collider2D outro) { Tentar(outro); }

        private void Tentar(Collider2D outro)
        {
            if (boss == null || !boss.EstaAtordoado || Time.time < proximoAcerto) return;
            if (!outro.CompareTag("Player")) return;

            Rigidbody2D corpo = outro.attachedRigidbody;
            if (corpo == null) return;

            // So conta se o jogador vem de cima (a cair) e esta acima do centro do ponto fraco.
            bool aCair = corpo.linearVelocity.y <= 0.1f;
            bool porCima = outro.bounds.min.y >= GetComponent<Collider2D>().bounds.center.y - 0.1f;
            if (!aCair || !porCima) return;

            if (boss.ReceberSalto())
            {
                proximoAcerto = Time.time + intervaloEntreAcertos;
                corpo.linearVelocity = new Vector2(corpo.linearVelocity.x, ressalto);
            }
        }
    }
}
