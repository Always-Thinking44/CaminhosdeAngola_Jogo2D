using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Faz do topo de um carro ou candongueiro uma plataforma onde o jogador
    /// pode ficar de pe. O corpo do veiculo continua a ferir (PerigoDano);
    /// so o topo e seguro. Os colisores solidos do topo ficam num filho
    /// chamado "Topo", na layer Plataforma (que o jogador ja trata como chao).
    /// Como o veiculo anda por script, este componente leva o jogador
    /// consigo enquanto ele esta em cima.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class VeiculoPlataforma : MonoBehaviour
    {
        [Tooltip("Filho com os colisores solidos do topo. Se vazio, procura um filho chamado Topo.")]
        [SerializeField] private Transform topo;
        [SerializeField] private SpriteRenderer desenho;
        [Tooltip("O topo so e solido quando o veiculo ja esta bem visivel (sem fade).")]
        [SerializeField, Range(0f, 1f)] private float opacidadeMinima = 0.8f;
        [Tooltip("Altura da zona, por cima do topo, onde se procura o jogador.")]
        [SerializeField, Min(0.02f)] private float alturaDeteccao = 0.12f;

        private Rigidbody2D corpo;
        private Collider2D[] colisoresTopo;
        private ContactFilter2D filtro;
        private readonly Collider2D[] encontrados = new Collider2D[4];
        private Vector2 posicaoAnterior;
        private bool primeiraLeitura = true;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody2D>();
            if (topo == null) topo = transform.Find("Topo");
            if (desenho == null) desenho = GetComponentInChildren<SpriteRenderer>();
            colisoresTopo = topo != null ? topo.GetComponents<Collider2D>() : new Collider2D[0];

            filtro = new ContactFilter2D();
            filtro.useLayerMask = true;
            filtro.layerMask = LayerMask.GetMask("Player");
            filtro.useTriggers = true;
        }

        private void Update()
        {
            bool solido = desenho == null || desenho.color.a >= opacidadeMinima;
            for (int i = 0; i < colisoresTopo.Length; i++)
                if (colisoresTopo[i] != null && colisoresTopo[i].enabled != solido)
                    colisoresTopo[i].enabled = solido;
        }

        private void FixedUpdate()
        {
            // Quanto o veiculo andou desde o ultimo passo de fisica.
            Vector2 actual = corpo.position;
            if (primeiraLeitura)
            {
                posicaoAnterior = actual;
                primeiraLeitura = false;
                return;
            }

            float deslocamentoX = actual.x - posicaoAnterior.x;
            posicaoAnterior = actual;
            if (Mathf.Abs(deslocamentoX) < 0.0001f) return;

            LevarJogador(deslocamentoX);
        }

        private void LevarJogador(float deslocamentoX)
        {
            for (int i = 0; i < colisoresTopo.Length; i++)
            {
                Collider2D topoSolido = colisoresTopo[i];
                if (topoSolido == null || !topoSolido.enabled) continue;

                // Zona fina logo por cima do topo: so apanha quem esta a pisar.
                Bounds limites = topoSolido.bounds;
                Vector2 centro = new Vector2(limites.center.x, limites.max.y + alturaDeteccao * 0.3f);
                Vector2 tamanho = new Vector2(limites.size.x, alturaDeteccao);

                int quantos = Physics2D.OverlapBox(centro, tamanho, 0f, filtro, encontrados);
                for (int j = 0; j < quantos; j++)
                {
                    Collider2D outro = encontrados[j];
                    if (outro == null || !outro.CompareTag("Player")) continue;

                    Rigidbody2D corpoJogador = outro.attachedRigidbody;
                    // Se esta a saltar para cima, nao o arrastar.
                    if (corpoJogador == null || corpoJogador.linearVelocity.y > 0.5f) continue;

                    corpoJogador.position += new Vector2(deslocamentoX, 0f);
                    return;
                }
            }
        }
    }
}
