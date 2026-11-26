using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Bloco suspenso: aparece, fica visivel alguns segundos, pisca um instante
    /// antes de desaparecer e volta a aparecer, em ciclo. Enquanto esta
    /// escondido nao tem colisao (o jogador cai se ainda estiver em cima) e
    /// mostra-se um "fantasma" muito suave para o jogador ver onde vai voltar.
    ///
    /// O relogio acumula Time.deltaTime, por isso pára com o jogo em pausa
    /// (loja, pausa, fim de nivel). Para criar uma "onda" de blocos, da-se a
    /// cada bloco um desfasamento diferente (campo "desfasamento").
    /// </summary>
    public class BlocoSuspenso : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer desenho;
        [SerializeField] private Collider2D colisor;

        [Header("Tempos (segundos)")]
        [Tooltip("Tempo total em que o bloco existe (inclui o aviso do fim).")]
        [SerializeField, Min(0.5f)] private float tempoVisivel = 3.2f;
        [Tooltip("Tempo em que o bloco esta escondido antes de voltar.")]
        [SerializeField, Min(0.2f)] private float tempoEscondido = 1.8f;
        [Tooltip("No fim do tempo visivel o bloco pisca durante este tempo.")]
        [SerializeField, Min(0f)] private float tempoAviso = 0.8f;
        [Tooltip("Desfasamento no ciclo. Blocos seguidos com +1 s criam uma onda por onde se pode atravessar.")]
        [SerializeField] private float desfasamento;

        [Header("Aspecto")]
        [Tooltip("Opacidade do fantasma quando o bloco esta escondido (0 = invisivel).")]
        [SerializeField, Range(0f, 1f)] private float opacidadeFantasma = 0.18f;
        [Tooltip("Opacidade nos instantes escuros do piscar.")]
        [SerializeField, Range(0f, 1f)] private float opacidadePiscar = 0.3f;
        [SerializeField, Min(0.02f)] private float intervaloPiscar = 0.12f;

        private float relogio;
        private bool? ultimoEstadoColisao;

        /// <summary>Verdadeiro enquanto o bloco existe (tem colisao).</summary>
        public bool Visivel { get; private set; }

        private void Awake()
        {
            if (desenho == null) desenho = GetComponentInChildren<SpriteRenderer>();
            if (colisor == null) colisor = GetComponentInChildren<Collider2D>();
        }

        private void Start()
        {
            Aplicar();
        }

        private void Update()
        {
            relogio += Time.deltaTime;
            Aplicar();
        }

        private void Aplicar()
        {
            float ciclo = tempoVisivel + tempoEscondido;
            float fase = Mathf.Repeat(relogio - desfasamento, ciclo);

            bool existe = fase < tempoVisivel;
            float opacidade;

            if (!existe)
            {
                opacidade = opacidadeFantasma;
            }
            else if (fase >= tempoVisivel - tempoAviso)
            {
                // Aviso: pisca antes de desaparecer.
                bool claro = Mathf.FloorToInt(fase / intervaloPiscar) % 2 == 0;
                opacidade = claro ? 1f : opacidadePiscar;
            }
            else
            {
                opacidade = 1f;
            }

            Visivel = existe;

            if (desenho != null)
            {
                Color cor = desenho.color;
                cor.a = opacidade;
                desenho.color = cor;
            }

            if (colisor != null && ultimoEstadoColisao != existe)
            {
                colisor.enabled = existe;
                ultimoEstadoColisao = existe;
            }
        }
    }
}
