using UnityEngine;
using UnityEngine.InputSystem;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// Pedestal do Fragmento de Memoria. O jogador chega junto dele, carrega em E
    /// e o boss fica atordoado. Cada pedestal pertence a uma fase (1 a 3) e
    /// so funciona quando o boss esta nessa fase e protegido. Precisa de um
    /// Collider2D (trigger) a cobrir a zona de interaccao.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PedestalFragmento : MonoBehaviour
    {
        [SerializeField] private GrandeEsquecimento boss;
        [Tooltip("Fase a que este pedestal pertence (1 = Luanda, 2 = Malanje, 3 = Huila).")]
        [SerializeField, Range(1, 3)] private int fase = 1;
        [Tooltip("Mostra-se quando o jogador pode interagir (ex.: texto 'E').")]
        [SerializeField] private GameObject indicadorInteragir;
        [SerializeField] private SpriteRenderer desenho;
        [SerializeField] private Color corVazio = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private Color corActivo = new Color(1f, 0.85f, 0.3f, 1f);

        public int Fase => fase;

        private bool jogadorPerto;
        private bool fragmentoColocado;

        private void Reset()
        {
            Collider2D c = GetComponent<Collider2D>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            if (boss == null) boss = FindFirstObjectByType<GrandeEsquecimento>();
        }

        private void OnEnable()
        {
            if (boss != null) boss.AoRecuperar += AoBossRecuperar;
            Actualizar();
        }

        private void OnDisable()
        {
            if (boss != null) boss.AoRecuperar -= AoBossRecuperar;
        }

        private void Update()
        {
            if (!jogadorPerto || !PodeUsar()) return;

            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                fragmentoColocado = true;
                boss.Atordoar();
                Actualizar();
            }
        }

        private bool PodeUsar()
        {
            return boss != null
                && !fragmentoColocado
                && boss.EstadoActual == GrandeEsquecimento.Estado.Resistir
                && boss.FaseActual == fase;
        }

        // Se o tempo de atordoamento acabar sem concluir a fase, o pedestal reabre.
        private void AoBossRecuperar()
        {
            if (boss.FaseActual == fase) fragmentoColocado = false;
            Actualizar();
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;
            jogadorPerto = true;
            Actualizar();
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;
            jogadorPerto = false;
            Actualizar();
        }

        private void Actualizar()
        {
            if (indicadorInteragir != null) indicadorInteragir.SetActive(jogadorPerto && PodeUsar());
            if (desenho != null) desenho.color = fragmentoColocado ? corActivo : corVazio;
        }
    }
}
