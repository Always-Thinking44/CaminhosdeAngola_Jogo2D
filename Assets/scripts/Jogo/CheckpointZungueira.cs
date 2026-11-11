using UnityEngine;
using UnityEngine.InputSystem;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Zungueira que serve de checkpoint. Tem uma zona invisivel (o collider
    /// trigger) que diz se o jogador esta dentro. So com o jogador dentro da
    /// zona e que a tecla E activa o checkpoint. A bancada da loja sera
    /// acrescentada na Tarefa 7.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointZungueira : MonoBehaviour
    {
        [SerializeField] private GerenciadorVidas gerenciador;

        [Tooltip("Asset de accoes (CaminhosDeAngola.inputactions). Usa a accao 'Interagir'.")]
        [SerializeField] private InputActionAsset accoes;

        [Tooltip("Deslocamento, a partir do centro da zungueira, onde o jogador reaparece.")]
        [SerializeField] private Vector3 deslocamentoRespawn = new Vector3(0f, -0.28f, 0f);

        [Tooltip("Objecto mostrado quando o jogador esta dentro da zona (ex.: o aviso 'Premir E').")]
        [SerializeField] private GameObject aviso;

        [SerializeField] private Color corInactiva = new Color(0.95f, 0.55f, 0.12f, 1f);
        [SerializeField] private Color corActiva = new Color(0.20f, 0.80f, 0.32f, 1f);

        private InputAction accaoInteragir;
        private SpriteRenderer desenho;
        private bool jogadorDentro;
        private bool activo;

        private void Awake()
        {
            desenho = GetComponent<SpriteRenderer>();
            if (gerenciador == null) gerenciador = FindAnyObjectByType<GerenciadorVidas>();
            if (desenho != null) desenho.color = corInactiva;
            if (aviso != null) aviso.SetActive(false);

            if (accoes != null)
            {
                InputActionMap mapa = accoes.FindActionMap("Jogo", false);
                if (mapa != null) accaoInteragir = mapa.FindAction("Interagir", false);
            }

            if (accaoInteragir == null)
                Debug.LogError("CheckpointZungueira: falta a accao 'Interagir' (asset de accoes).", this);
        }

        private void OnEnable()
        {
            accaoInteragir?.Enable();
        }

        private void OnDisable()
        {
            accaoInteragir?.Disable();
        }

        private void Update()
        {
            if (!jogadorDentro || accaoInteragir == null) return;

            if (accaoInteragir.WasPressedThisFrame())
                Activar();
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;

            jogadorDentro = true;
            if (aviso != null) aviso.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;

            jogadorDentro = false;
            if (aviso != null) aviso.SetActive(false);
        }

        private void Activar()
        {
            if (activo) return;

            activo = true;

            if (gerenciador != null)
                gerenciador.DefinirCheckpoint(transform.position + deslocamentoRespawn);

            if (desenho != null)
                desenho.color = corActiva;

            if (aviso != null) aviso.SetActive(false);
        }
    }
}
