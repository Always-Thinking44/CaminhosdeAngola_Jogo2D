using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Um carro ou candongueiro que atravessa a estrada da direita para a
    /// esquerda. Aparece e desaparece com um fade nas pontas do troco, para
    /// nao "estalar" a vista. So fere o jogador quando ja esta bem visivel.
    /// Criado e configurado pelo GeradorTrafego.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class VeiculoTrafego : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer desenho;
        [SerializeField] private Collider2D colisor;

        [Tooltip("Distancia, em unidades, em que o veiculo aparece e desaparece com fade.")]
        [SerializeField, Min(0.1f)] private float distanciaFade = 2.5f;
        [Tooltip("So fere o jogador quando a opacidade passa deste valor (0 a 1).")]
        [SerializeField, Range(0f, 1f)] private float opacidadeParaFerir = 0.8f;

        /// <summary>Largura do sprite em unidades (para o gerador espacar os veiculos).</summary>
        public float Largura => desenho != null && desenho.sprite != null ? desenho.sprite.bounds.size.x : 4f;

        private Rigidbody2D corpo;
        private float velocidade;
        private float xFinal;
        private float xOrigem;
        private bool iniciado;

        [Header("Tampo (parte de cima pisavel)")]
        [Tooltip("Espessura do tampo, em unidades. O tampo e solido e pode ser pisado; so o resto do veiculo fere.")]
        [SerializeField, Min(0.05f)] private float espessuraTampo = 0.2f;
        [Tooltip("Quanto o tampo e mais estreito que o veiculo de cada lado, para nao empurrar o jogador pelas pontas.")]
        [SerializeField, Min(0f)] private float margemLateralTampo = 0.1f;
        [Tooltip("Camada do tampo (tem de colidir com o jogador e contar como chao).")]
        [SerializeField] private string camadaTampo = "Chao";

        private Collider2D tampo;

private void Awake()
        {
            corpo = GetComponent<Rigidbody2D>();
            if (desenho == null) desenho = GetComponentInChildren<SpriteRenderer>();
            if (colisor == null) colisor = GetComponent<Collider2D>();
            CriarTampo();
        }

/// <summary>
        /// Divide o veiculo em duas partes: um tampo solido em cima (pisavel, conta como chao)
        /// e o corpo por baixo, que continua a ferir. Assim o jogador pode ficar em cima.
        /// </summary>
        private void CriarTampo()
        {
            BoxCollider2D caixa = colisor as BoxCollider2D;
            if (caixa == null) return;

            float topo = caixa.offset.y + caixa.size.y * 0.5f;
            float espessura = Mathf.Min(espessuraTampo, caixa.size.y * 0.5f);
            float largura = Mathf.Max(0.1f, caixa.size.x - margemLateralTampo * 2f);

            GameObject obj = new GameObject("Tampo");
            obj.transform.SetParent(transform, false);
            int camada = LayerMask.NameToLayer(camadaTampo);
            if (camada >= 0) obj.layer = camada;

            BoxCollider2D teto = obj.AddComponent<BoxCollider2D>();
            teto.size = new Vector2(largura, espessura);
            teto.offset = new Vector2(caixa.offset.x, topo - espessura * 0.5f);
            tampo = teto;

            // O gatilho que fere perde a faixa de cima.
            caixa.size = new Vector2(caixa.size.x, caixa.size.y - espessura);
            caixa.offset = new Vector2(caixa.offset.x, caixa.offset.y - espessura * 0.5f);
        }


        /// <summary>Arranca o veiculo. Anda para a esquerda ate xFinal e depois desaparece.</summary>
        public void Iniciar(float velocidadeAndar, float xFinalEsquerdo)
        {
            velocidade = velocidadeAndar;
            xFinal = xFinalEsquerdo;
            xOrigem = transform.position.x;
            iniciado = true;

            // Os sprites estao virados para a direita; vao para a esquerda, entao viram-se.
            if (desenho != null) desenho.flipX = true;
            AplicarVisibilidade();
        }

private void FixedUpdate()
        {
            if (!iniciado) return;

            Vector2 antes = corpo.position;
            Vector2 posicao = antes;
            posicao.x -= velocidade * Time.fixedDeltaTime;
            corpo.MovePosition(posicao);

            TransportarJogador(posicao.x - antes.x);
        }

        /// <summary>Se o jogador esta em cima do tampo, leva-o junto com o veiculo.</summary>
        private void TransportarJogador(float deslocamentoX)
        {
            if (tampo == null || !tampo.enabled) return;

            Bounds limites = tampo.bounds;
            Vector2 centro = new Vector2(limites.center.x, limites.max.y + 0.06f);
            Vector2 tamanho = new Vector2(limites.size.x, 0.12f);

            foreach (Collider2D c in Physics2D.OverlapBoxAll(centro, tamanho, 0f))
            {
                if (!c.CompareTag("Player")) continue;

                Rigidbody2D rb = c.attachedRigidbody;
                if (rb == null || rb.linearVelocity.y > 0.5f) return;

                rb.position += new Vector2(deslocamentoX, 0f);
                return;
            }
        }

        private void Update()
        {
            if (!iniciado) return;

            if (transform.position.x <= xFinal)
            {
                Destroy(gameObject);
                return;
            }

            AplicarVisibilidade();
        }

        private void AplicarVisibilidade()
        {
            float x = transform.position.x;
            float entrada = Mathf.Clamp01((xOrigem - x) / distanciaFade);
            float saida = Mathf.Clamp01((x - xFinal) / distanciaFade);
            float opacidade = Mathf.Min(entrada, saida);

            if (desenho != null)
            {
                Color cor = desenho.color;
                cor.a = opacidade;
                desenho.color = cor;
            }

            bool activo = opacidade >= opacidadeParaFerir;
            if (colisor != null) colisor.enabled = activo;
            if (tampo != null) tampo.enabled = activo;
        }
    }
}
