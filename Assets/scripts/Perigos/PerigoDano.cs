using UnityEngine;
using CaminhosDeAngola.Jogo;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Qualquer perigo que tira uma vida ao jogador quando o toca.
    /// Precisa de um Collider2D com "Is Trigger" ligado. Reutilizado pelos
    /// veiculos, pela moto e pela bala do atirador.
    /// </summary>
    public class PerigoDano : MonoBehaviour
    {
        [Tooltip("Opcional: se vazio, procura o GerenciadorVidas na cena.")]
        [SerializeField] private GerenciadorVidas gerenciadorVidas;

        [Tooltip("Ligar para a bala: destroi-se ao tocar no jogador.")]
        [SerializeField] private bool destruirAoTocar;

        private void Awake()
        {
            if (gerenciadorVidas == null) gerenciadorVidas = FindAnyObjectByType<GerenciadorVidas>();
        }

        private void OnTriggerEnter2D(Collider2D outro) => Tocar(outro);

        // Stay: se o jogador ficar dentro do perigo quando acaba a invulnerabilidade, leva dano.
        private void OnTriggerStay2D(Collider2D outro) => Tocar(outro);

        private void Tocar(Collider2D outro)
        {
            if (gerenciadorVidas == null || !outro.CompareTag("Player")) return;
            if (gerenciadorVidas.Invulneravel) return;

            gerenciadorVidas.PerderVida();

            if (destruirAoTocar) Destroy(gameObject);
        }
    }
}
