using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Base dos objectos que o jogador recolhe. O colisor tem de ser um
    /// trigger e o objecto fica na layer Colectavel.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class ColectavelBase : MonoBehaviour
    {
        [Header("Ligacoes")]
        [SerializeField] protected GerenciadorColectaveis gerenciador;

        protected virtual void Awake()
        {
            if (gerenciador == null)
                gerenciador = FindAnyObjectByType<GerenciadorColectaveis>();

            Collider2D colisor = GetComponent<Collider2D>();
            if (colisor != null) colisor.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D outro)
        {
            if (!outro.CompareTag("Player")) return;
            if (!Recolher()) return;

            Sons.GerenciadorAudio.Tocar(SomDeRecolha);

            Destroy(gameObject);
        }

        /// <summary>Efectua a recolha. Devolve falso para manter o objecto na cena.</summary>
        protected abstract bool Recolher();

        /// <summary>Som tocado quando o objecto e recolhido. Cada colectavel pode escolher o seu.</summary>
        protected virtual Sons.Som SomDeRecolha => Sons.Som.Zimbo;
    }
}
