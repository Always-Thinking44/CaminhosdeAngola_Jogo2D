using UnityEngine;
using UnityEngine.InputSystem;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Zungueira: checkpoint com zona invisivel. So com o jogador dentro da
    /// zona e que a tecla E activa o checkpoint. A bancada da loja sera
    /// acrescentada na Tarefa 7.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CheckpointZungueira : CheckpointBase
    {
        [Header("Accoes")]
        [Tooltip("Asset de accoes (CaminhosDeAngola.inputactions). Usa a accao 'Interagir'.")]
        [SerializeField] private InputActionAsset accoes;

        [SerializeField] private string textoDentro = "Premir E";

        private InputAction accaoInteragir;
        private bool jogadorDentro;

        protected override void Awake()
        {
            base.Awake();

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
            if (Activo || !jogadorDentro || accaoInteragir == null) return;

            if (accaoInteragir.WasPressedThisFrame())
                Activar();
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;

            jogadorDentro = true;
            if (!Activo) MostrarAvisoPermanente(textoDentro);
        }

        private void OnTriggerExit2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;

            jogadorDentro = false;
            if (!Activo) EsconderAvisoAgora();
        }
    }
}
