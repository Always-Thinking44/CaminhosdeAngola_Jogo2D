using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Zungueira que serve de checkpoint. Quando o jogador lhe toca, guarda o
    /// ponto de respawn. A loja (tecla E) sera acrescentada na Tarefa 7.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointZungueira : MonoBehaviour
    {
        [SerializeField] private GerenciadorVidas gerenciador;

        [Tooltip("Deslocamento, a partir do centro da zungueira, onde o jogador reaparece.")]
        [SerializeField] private Vector3 deslocamentoRespawn = new Vector3(0f, -0.28f, 0f);

        [SerializeField] private Color corInactiva = new Color(0.95f, 0.55f, 0.12f, 1f);
        [SerializeField] private Color corActiva = new Color(0.20f, 0.80f, 0.32f, 1f);

        private SpriteRenderer desenho;
        private bool activo;

        private void Awake()
        {
            desenho = GetComponent<SpriteRenderer>();
            if (gerenciador == null) gerenciador = FindAnyObjectByType<GerenciadorVidas>();
            if (desenho != null) desenho.color = corInactiva;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (activo || !outro.CompareTag("Player")) return;
            Activar();
        }

        private void Activar()
        {
            activo = true;

            if (gerenciador != null)
                gerenciador.DefinirCheckpoint(transform.position + deslocamentoRespawn);

            if (desenho != null)
                desenho.color = corActiva;
        }
    }
}
