using UnityEngine;
using UnityEngine.SceneManagement;
using CaminhosDeAngola.Interface;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Gere as vidas do jogador, o dano, a invulnerabilidade breve depois de
    /// levar dano, o respawn no ultimo checkpoint e o reinicio do nivel
    /// quando as vidas chegam a zero.
    /// </summary>
    public class GerenciadorVidas : MonoBehaviour
    {
        [Header("Vidas")]
        [SerializeField, Min(1)] private int vidasMaximas = 3;
        [Tooltip("Maximo de vidas que o jogador pode ter (vida extra da loja).")]
        [SerializeField, Min(1)] private int vidasLimite = 4;

        [Header("Jogador")]
        [SerializeField] private Transform jogador;
        [SerializeField] private SpriteRenderer desenhoJogador;
        [SerializeField] private Rigidbody2D corpoJogador;

        [Header("Respawn")]
        [Tooltip("Tempo, em segundos, em que o jogador nao pode levar dano depois de reaparecer.")]
        [SerializeField, Min(0f)] private float tempoInvulneravel = 1.5f;
        [Tooltip("Se o jogador descer abaixo desta altura, perde uma vida (queda para o vazio).")]
        [SerializeField] private float limiteQueda = -12f;

        [Header("Camera")]
        [Tooltip("Opcional: faz a camara saltar imediatamente para o respawn, sem arrastar.")]
        [SerializeField] private Unity.Cinemachine.CinemachineCamera cameraVirtual;

        [Header("HUD")]
        [SerializeField] private HUDVidas hud;

        /// <summary>Vidas que restam ao jogador.</summary>
        public int Vidas { get; private set; }

        /// <summary>Numero de vidas com que o nivel comeca.</summary>
        public int VidasMaximas => vidasMaximas;

        /// <summary>Maximo de vidas que se pode acumular (vida extra).</summary>
        public int VidasLimite => Mathf.Max(vidasLimite, vidasMaximas);

        /// <summary>Verdadeiro se ainda cabe uma vida extra.</summary>
        public bool PodeGanharVida => Vidas < VidasLimite;

        /// <summary>Onde o jogador reaparece (ultimo checkpoint ou inicio do nivel).</summary>
        public Vector3 PontoRespawn { get; private set; }

        /// <summary>Verdadeiro enquanto o jogador nao pode levar dano.</summary>
        public bool Invulneravel => Time.time < fimInvulnerabilidade;

        /// <summary>Disparado sempre que o jogador volta ao checkpoint (a nevoa vai usar isto).</summary>
        public event System.Action AoRespawn;

        private float fimInvulnerabilidade;

        private void Awake()
        {
            if (jogador == null)
            {
                GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
                if (encontrado != null) jogador = encontrado.transform;
            }

            if (jogador != null)
            {
                if (desenhoJogador == null) desenhoJogador = jogador.GetComponent<SpriteRenderer>();
                if (corpoJogador == null) corpoJogador = jogador.GetComponent<Rigidbody2D>();
            }
        }

        private void Start()
        {
            if (jogador == null)
            {
                Debug.LogError("GerenciadorVidas: nao encontrei o jogador (tag Player).", this);
                enabled = false;
                return;
            }

            Vidas = vidasMaximas;
            PontoRespawn = jogador.position;
            AtualizarHud();
        }

        private void Update()
        {
            if (jogador == null) return;

            if (jogador.position.y < limiteQueda)
                PerderVida();

            AtualizarPiscar();
        }

        /// <summary>Guarda o ponto onde o jogador deve reaparecer.</summary>
        public void DefinirCheckpoint(Vector3 ponto)
        {
            PontoRespawn = ponto;
        }

        /// <summary>Tira uma vida ao jogador. Ignorado durante a invulnerabilidade.</summary>
        public void PerderVida()
        {
            if (Invulneravel) return;

            Vidas = Mathf.Max(0, Vidas - 1);
            AtualizarHud();

            if (Vidas <= 0)
            {
                RecomecarNivel();
                return;
            }

            Respawn();
        }

/// <summary>Soma uma vida (loja). Devolve falso se ja esta no limite.</summary>
        public bool GanharVida()
        {
            if (!PodeGanharVida) return false;

            Vidas++;
            AtualizarHud();
            return true;
        }


        private void Respawn()
        {
            if (corpoJogador != null)
            {
                corpoJogador.linearVelocity = Vector2.zero;
                corpoJogador.position = PontoRespawn;
            }
            else if (jogador != null)
            {
                jogador.position = PontoRespawn;
            }

            fimInvulnerabilidade = Time.time + tempoInvulneravel;

            // Fazer a camara saltar para o respawn em vez de arrastar pelo nivel todo.
            if (cameraVirtual != null) cameraVirtual.PreviousStateIsValid = false;

            AoRespawn?.Invoke();
        }

        private void RecomecarNivel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void AtualizarPiscar()
        {
            if (desenhoJogador == null) return;

            // Durante a invulnerabilidade o sprite pisca; fora dela fica sempre visivel.
            desenhoJogador.enabled = !Invulneravel || (Mathf.FloorToInt(Time.time * 10f) % 2 == 0);
        }

private void AtualizarHud()
        {
            // Mostra icones a mais enquanto o jogador tiver vidas acima do inicial.
            if (hud != null) hud.AtualizarVidas(Vidas, Mathf.Max(vidasMaximas, Vidas));
        }
    }
}
