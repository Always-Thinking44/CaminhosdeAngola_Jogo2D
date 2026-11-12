using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Moto muito rapida que surge de repente e atravessa o ecra da direita
    /// para a esquerda. Tira uma vida ao jogador (o PerigoDano trata disso).
    /// Pode ser saltada: o collider e mais baixo que o sprite.
    /// Como a layer Perigo nao colide com o Chao, a moto segue o terreno com
    /// um raio para baixo (acompanha subidas e descidas; cai nas valas).
    /// Criada e configurada pelo GeradorMoto.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Moto : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer desenho;

        [Header("Terreno")]
        [Tooltip("De que altura acima da moto parte o raio que procura o chao.")]
        [SerializeField, Min(0.1f)] private float alturaSondaChao = 1.5f;
        [Tooltip("Ate onde o raio procura o chao.")]
        [SerializeField, Min(1f)] private float alcanceSondaChao = 6f;
        [Tooltip("Velocidade maxima a que a moto sobe ou desce para acompanhar o chao.")]
        [SerializeField, Min(1f)] private float velocidadeAjusteAltura = 25f;
        [Tooltip("Velocidade de queda quando nao ha chao por baixo (vala).")]
        [SerializeField, Min(0f)] private float velocidadeQueda = 8f;

        [Header("Vida")]
        [Tooltip("Desaparece quando passa esta distancia para la do lado esquerdo do ecra.")]
        [SerializeField, Min(0f)] private float margemSaidaEsquerda = 3f;
        [Tooltip("Desaparece se ficar tao longe a direita do ecra (ex.: o jogador voltou ao checkpoint).")]
        [SerializeField, Min(5f)] private float distanciaMaximaDireita = 20f;
        [SerializeField, Min(1f)] private float tempoMaximoVida = 15f;

        private Rigidbody2D corpo;
        private Camera camara;
        private int camadasChao;
        private float velocidade;
        private float fimDaVida;
        private bool iniciada;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody2D>();
            if (desenho == null) desenho = GetComponentInChildren<SpriteRenderer>();
            camara = Camera.main;
            camadasChao = LayerMask.GetMask("Chao", "Plataforma");
        }

        /// <summary>Arranca a moto. Anda para a esquerda a esta velocidade (unidades por segundo).</summary>
        public void Iniciar(float velocidadeAndar)
        {
            velocidade = velocidadeAndar;
            fimDaVida = Time.time + tempoMaximoVida;
            iniciada = true;

            // O sprite aponta para a direita; a moto vai para a esquerda, entao vira-se.
            if (desenho != null) desenho.flipX = true;
        }

        private void FixedUpdate()
        {
            if (!iniciada) return;

            Vector2 posicao = corpo.position;
            posicao.x -= velocidade * Time.fixedDeltaTime;

            Vector2 origem = new Vector2(posicao.x, posicao.y + alturaSondaChao);
            RaycastHit2D chao = Physics2D.Raycast(origem, Vector2.down, alcanceSondaChao, camadasChao);

            // distance > 0: ignora o caso do raio nascer dentro de um collider.
            if (chao.collider != null && chao.distance > 0.01f)
                posicao.y = Mathf.MoveTowards(posicao.y, chao.point.y, velocidadeAjusteAltura * Time.fixedDeltaTime);
            else
                posicao.y -= velocidadeQueda * Time.fixedDeltaTime;

            corpo.MovePosition(posicao);
        }

        private void Update()
        {
            if (!iniciada) return;

            if (Time.time >= fimDaVida || ForaDoAlcance())
                Destroy(gameObject);
        }

        private bool ForaDoAlcance()
        {
            if (camara == null) camara = Camera.main;
            if (camara == null) return false;

            float meiaLargura = camara.orthographicSize * camara.aspect;
            float esquerda = camara.transform.position.x - meiaLargura - margemSaidaEsquerda;
            float direita = camara.transform.position.x + meiaLargura + distanciaMaximaDireita;
            float x = transform.position.x;
            return x < esquerda || x > direita;
        }
    }
}
