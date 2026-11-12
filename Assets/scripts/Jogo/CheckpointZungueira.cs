using UnityEngine;
using UnityEngine.InputSystem;
using CaminhosDeAngola.Interface;

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

        [Tooltip("Loja que abre ao carregar E. Se vazio, procura a LojaZungueira da cena.")]
        [SerializeField] private LojaZungueira loja;

        private InputAction accaoInteragir;
        // So fica gasta depois de o jogador comprar alguma coisa: entrar sem comprar nao gasta.
        private bool lojaGasta;
        private bool jogadorDentro;

        protected override void Awake()
        {
            base.Awake();

            if (loja == null) loja = FindAnyObjectByType<LojaZungueira>();

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
            if (lojaGasta || !jogadorDentro || accaoInteragir == null) return;
            if (loja != null && loja.Aberta) return;

            if (accaoInteragir.WasPressedThisFrame())
            {
                Activar();
                if (loja != null) loja.Abrir(this);
            }
        }

        /// <summary>Chamado pela loja ao fechar. Depois de comprar, esta zungueira nao abre mais.</summary>
        public void AoFecharLoja(bool houveCompra)
        {
            if (houveCompra)
            {
                lojaGasta = true;
                EsconderAvisoAgora();
            }
            else if (jogadorDentro)
            {
                MostrarAvisoPermanente(textoDentro);
            }
        }

private void OnTriggerEnter2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;

            jogadorDentro = true;
            if (!lojaGasta) MostrarAvisoPermanente(textoDentro);
        }

private void OnTriggerExit2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;

            jogadorDentro = false;
            if (!lojaGasta) EsconderAvisoAgora();
        }
    }
}
