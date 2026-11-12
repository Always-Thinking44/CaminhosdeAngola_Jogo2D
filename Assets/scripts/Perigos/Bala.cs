using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Bala do atirador. Vai em linha recta (nao persegue). Tira uma vida ao
    /// jogador (o PerigoDano trata disso e destroi-a). Destroi-se tambem ao
    /// tocar no chao ou ao fim do tempo de vida.
    /// Nota: a layer Perigo nao colide com o Chao na matriz, por isso o chao
    /// e detectado aqui com um teste de sobreposicao.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bala : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer desenho;
        [Tooltip("Tamanho da zona usada para detectar o chao.")]
        [SerializeField] private Vector2 tamanhoDeteccaoChao = new Vector2(0.4f, 0.1f);

        private Rigidbody2D corpo;
        private float direcao = 1f;
        private float velocidade;
        private float fimDaVida;
        private int camadasChao;
        private bool iniciada;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody2D>();
            if (desenho == null) desenho = GetComponentInChildren<SpriteRenderer>();
            camadasChao = LayerMask.GetMask("Chao", "Plataforma");
        }

        /// <summary>Dispara a bala. direcao: 1 para a direita, -1 para a esquerda.</summary>
        public void Iniciar(float direcaoDisparo, float velocidadeBala, float tempoDeVida)
        {
            direcao = direcaoDisparo >= 0f ? 1f : -1f;
            velocidade = velocidadeBala;
            fimDaVida = Time.time + tempoDeVida;
            iniciada = true;

            // O sprite aponta para a direita; o codigo faz o flip.
            if (desenho != null) desenho.flipX = direcao < 0f;
        }

        private void FixedUpdate()
        {
            if (!iniciada) return;

            Vector2 posicao = corpo.position;
            posicao.x += direcao * velocidade * Time.fixedDeltaTime;
            corpo.MovePosition(posicao);

            if (Physics2D.OverlapBox(transform.position, tamanhoDeteccaoChao, 0f, camadasChao) != null)
                Destroy(gameObject);
        }

        private void Update()
        {
            if (iniciada && Time.time >= fimDaVida) Destroy(gameObject);
        }
    }
}
