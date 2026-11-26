using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Bloco suspenso com timing: aparece, fica visivel alguns segundos,
    /// pisca no fim (aviso) e desaparece. Quando desaparece o collider
    /// desliga-se, por isso o jogador cai se ainda estiver em cima.
    /// O ciclo depende so do tempo de jogo (pausa com o jogo) e repete-se:
    /// visivel durante 'tempoVisivel' e oculto durante 'tempoOculto'.
    /// Com 'atraso' diferente em cada bloco forma-se uma onda (um bloco
    /// aparece depois do outro) que o jogador tem de acompanhar.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    public class BlocoSuspenso : MonoBehaviour
    {
        [Header("Ciclo (segundos)")]
        [Tooltip("Tempo que o bloco fica visivel e solido, incluindo o aviso.")]
        [SerializeField, Min(0.5f)] private float tempoVisivel = 3.2f;
        [Tooltip("Tempo que o bloco fica oculto antes de voltar.")]
        [SerializeField, Min(0.1f)] private float tempoOculto = 1.8f;
        [Tooltip("Nos ultimos segundos visiveis o bloco pisca (aviso antes de sumir).")]
        [SerializeField, Min(0f)] private float tempoAviso = 1f;
        [Tooltip("O bloco aparece pela primeira vez neste instante (e depois a cada ciclo). Muda de bloco para bloco para fazer uma onda.")]
        [SerializeField, Min(0f)] private float atraso;

        [Header("Aspecto")]
        [Tooltip("Duracao do fade ao aparecer.")]
        [SerializeField, Min(0f)] private float tempoFade = 0.2f;
        [Tooltip("Opacidade nos instantes escuros do piscar.")]
        [SerializeField, Range(0f, 1f)] private float opacidadePiscar = 0.3f;
        [Tooltip("Duracao de cada meio ciclo do piscar.")]
        [SerializeField, Min(0.02f)] private float periodoPiscar = 0.16f;

        private SpriteRenderer desenho;
        private Collider2D[] colisores;
        private bool solido = true;

        private void Awake()
        {
            desenho = GetComponent<SpriteRenderer>();
            colisores = GetComponents<Collider2D>();
        }

        private void Update()
        {
            float ciclo = tempoVisivel + tempoOculto;
            float fase = Mathf.Repeat(Time.time - atraso, ciclo);

            bool visivel = fase < tempoVisivel;
            DefinirSolido(visivel);

            float opacidade = 0f;
            if (visivel)
            {
                opacidade = tempoFade > 0f ? Mathf.Clamp01(fase / tempoFade) : 1f;

                float inicioAviso = tempoVisivel - tempoAviso;
                if (tempoAviso > 0f && fase >= inicioAviso)
                {
                    bool escuro = Mathf.FloorToInt((fase - inicioAviso) / periodoPiscar) % 2 == 0;
                    if (escuro) opacidade = Mathf.Min(opacidade, opacidadePiscar);
                }
            }

            Color cor = desenho.color;
            cor.a = opacidade;
            desenho.color = cor;
        }

        private void DefinirSolido(bool valor)
        {
            if (solido == valor) return;

            solido = valor;
            for (int i = 0; i < colisores.Length; i++) colisores[i].enabled = valor;
        }
    }
}
