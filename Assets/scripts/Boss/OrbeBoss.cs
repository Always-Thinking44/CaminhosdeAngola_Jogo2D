using UnityEngine;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// Projectil do boss (orbe de Esquecimento). Vai em linha recta, tira uma
    /// vida ao jogador (o PerigoDano, no mesmo objecto, trata disso e destroi-o)
    /// e destroi-se ao tocar no chao ou ao fim do tempo de vida.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class OrbeBoss : MonoBehaviour
    {
        [SerializeField] private float raioDeteccaoChao = 0.15f;

        private Rigidbody2D corpo;
        private Vector2 direcao = Vector2.left;
        private float velocidade;
        private float fimDaVida;
        private int camadasChao;
        private bool iniciado;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody2D>();
            camadasChao = LayerMask.GetMask("Chao", "Plataforma");
        }

        public void Iniciar(Vector2 direcaoDisparo, float velocidadeOrbe, float tempoDeVida, float escala)
        {
            direcao = direcaoDisparo.sqrMagnitude > 0.0001f ? direcaoDisparo.normalized : Vector2.left;
            velocidade = velocidadeOrbe;
            fimDaVida = Time.time + tempoDeVida;
            transform.localScale = Vector3.one * escala;
            iniciado = true;
        }

        private void FixedUpdate()
        {
            if (!iniciado) return;

            corpo.MovePosition(corpo.position + direcao * velocidade * Time.fixedDeltaTime);

            if (Physics2D.OverlapCircle(transform.position, raioDeteccaoChao, camadasChao) != null)
                Destroy(gameObject);
        }

        private void Update()
        {
            if (iniciado)
            {
                transform.Rotate(0f, 0f, 180f * Time.deltaTime);
                if (Time.time >= fimDaVida) Destroy(gameObject);
            }
        }
    }
}
