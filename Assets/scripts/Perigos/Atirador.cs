using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Atirador parado. So dispara quando o jogador esta dentro do alcance, e
    /// dispara com mais frequencia quanto mais perto o jogador estiver. Vira-se
    /// para o jogador (flip do sprite e do FirePoint). Nao e atacavel: o
    /// jogador so foge, salta e desvia.
    /// Frames: parado (fora do alcance), a mirar (dentro do alcance) e disparo
    /// com clarao (cerca de 0,1 s no momento do tiro).
    /// </summary>
    public class Atirador : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private SpriteRenderer desenho;
        [SerializeField] private Sprite spriteParado;
        [SerializeField] private Sprite spriteMira;
        [SerializeField] private Sprite spriteDisparo;
        [Tooltip("Filho vazio na ponta do cano. Posicao local para o atirador virado para a direita.")]
        [SerializeField] private Transform pontoDisparo;

        [Header("Alcance")]
        [Tooltip("Distancia maxima a que o atirador dispara.")]
        [SerializeField, Min(0.1f)] private float distanciaMaxima = 16f;
        [Tooltip("Dentro desta distancia dispara na cadencia maxima.")]
        [SerializeField, Min(0f)] private float distanciaMinima = 6f;

        [Header("Cadencia")]
        [Tooltip("Segundos entre tiros quando o jogador esta no limite do alcance.")]
        [SerializeField, Min(0.05f)] private float recargaLonge = 3f;
        [Tooltip("Segundos entre tiros quando o jogador esta muito perto.")]
        [SerializeField, Min(0.05f)] private float recargaPerto = 0.9f;
        [Tooltip("Quanto da recarga ja esta cumprida quando o jogador entra no alcance (0 a 1). Da um aviso antes do 1.o tiro.")]
        [SerializeField, Range(0f, 0.9f)] private float progressoAoEntrar = 0.4f;
        [Tooltip("Tempo que o frame do clarao fica visivel.")]
        [SerializeField, Min(0f)] private float tempoClarao = 0.1f;

        [Header("Bala")]
        [SerializeField] private Bala prefabBala;
        [SerializeField, Min(0.1f)] private float velocidadeBala = 9f;
        [SerializeField, Min(0.1f)] private float tempoVidaBala = 3f;

        [Header("Referencias (opcionais: se vazias, procura na cena)")]
        [SerializeField] private Transform jogador;

        private float progresso;
        private bool jogadorNoAlcance;
        private float fimClarao;
        private Vector3 pontoDisparoLocal;

        private void Start()
        {
            if (jogador == null)
            {
                GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
                if (encontrado != null) jogador = encontrado.transform;
            }

            if (jogador == null || prefabBala == null || pontoDisparo == null || desenho == null)
            {
                Debug.LogError("Atirador: falta o jogador, o prefab da bala, o ponto de disparo ou o desenho.", this);
                enabled = false;
                return;
            }

            pontoDisparoLocal = pontoDisparo.localPosition;
            desenho.sprite = spriteParado;
            desenho.flipX = true;
        }

        private void Update()
        {
            float distancia = Vector2.Distance(jogador.position, transform.position);
            bool dentro = distancia <= distanciaMaxima;

            if (dentro && !jogadorNoAlcance) progresso = progressoAoEntrar;
            if (!dentro) progresso = 0f;
            jogadorNoAlcance = dentro;

            VirarParaJogador();

            if (!dentro)
            {
                desenho.sprite = Time.time < fimClarao ? spriteDisparo : spriteParado;
                return;
            }

            // Quanto mais perto, menor a recarga (mais tiros). O progresso acumula
            // ao ritmo actual, por isso o jogador a aproximar-se acelera o tiro ja.
            float proximidade = Mathf.InverseLerp(distanciaMaxima, distanciaMinima, distancia);
            float recarga = Mathf.Lerp(recargaLonge, recargaPerto, proximidade);
            progresso += Time.deltaTime / Mathf.Max(0.05f, recarga);

            if (progresso >= 1f)
            {
                Disparar();
                progresso = 0f;
            }

            desenho.sprite = Time.time < fimClarao ? spriteDisparo : spriteMira;
        }

        private void VirarParaJogador()
        {
            bool paraEsquerda = jogador.position.x < transform.position.x;
            desenho.flipX = paraEsquerda;

            Vector3 local = pontoDisparoLocal;
            local.x = paraEsquerda ? -Mathf.Abs(pontoDisparoLocal.x) : Mathf.Abs(pontoDisparoLocal.x);
            pontoDisparo.localPosition = local;
        }

        private void Disparar()
        {
            fimClarao = Time.time + tempoClarao;

            bool paraEsquerda = jogador.position.x < transform.position.x;
            Bala bala = Instantiate(prefabBala, pontoDisparo.position, Quaternion.identity);
            bala.Iniciar(paraEsquerda ? -1f : 1f, velocidadeBala, tempoVidaBala);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, distanciaMaxima);
            Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, distanciaMinima);
        }
    }
}
