using System;
using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Guarda os power-ups activos do jogador. Por agora so o Escudo: enquanto
    /// dura, os perigos (carros, moto, bala) nao tiram vida. A nevoa e a queda
    /// para o vazio NAO sao travadas. O escudo perde-se quando o jogador volta
    /// ao checkpoint. O tempo conta em tempo de jogo, por isso pausa na loja.
    /// </summary>
    public class GerenciadorPowerUps : MonoBehaviour
    {
        [Header("Ligacoes")]
        [SerializeField] private GerenciadorVidas vidas;
        [SerializeField] private Transform jogador;

        [Header("Escudo (visual provisorio)")]
        [SerializeField] private Color corEscudo = new Color(0.3f, 0.8f, 1f, 0.45f);
        [SerializeField, Min(0.5f)] private float diametroEscudo = 1.8f;
        [Tooltip("Nos ultimos segundos o escudo pisca para avisar que vai acabar.")]
        [SerializeField, Min(0f)] private float avisoFinalSegundos = 2f;

        private float restante;
        private float duracaoTotal;
        private SpriteRenderer visualEscudo;

        /// <summary>Verdadeiro enquanto o escudo protege o jogador.</summary>
        public bool EscudoActivo => restante > 0f;

        /// <summary>Segundos que faltam ao escudo.</summary>
        public float EscudoRestante => Mathf.Max(0f, restante);

        /// <summary>Duracao total do escudo actual (para a barra do HUD).</summary>
        public float EscudoDuracao => duracaoTotal;

        /// <summary>Disparado quando o escudo liga ou desliga.</summary>
        public event Action AoMudarEscudo;

        private void Awake()
        {
            if (vidas == null) vidas = FindAnyObjectByType<GerenciadorVidas>();
            if (jogador == null)
            {
                GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
                if (encontrado != null) jogador = encontrado.transform;
            }

            CriarVisualEscudo();
        }

        private void Start()
        {
            if (vidas != null) vidas.AoRespawn += RemoverEscudo;
        }

        private void OnDestroy()
        {
            if (vidas != null) vidas.AoRespawn -= RemoverEscudo;
        }

        private void Update()
        {
            if (restante <= 0f) return;

            restante -= Time.deltaTime;
            if (restante <= 0f)
            {
                RemoverEscudo();
                return;
            }

            if (visualEscudo != null)
            {
                bool piscar = restante < avisoFinalSegundos;
                visualEscudo.enabled = !piscar || (Mathf.FloorToInt(Time.time * 8f) % 2 == 0);
            }
        }

        /// <summary>Liga o escudo. Se ja estava ligado, o tempo volta ao maximo.</summary>
        public void ActivarEscudo(float segundos)
        {
            if (segundos <= 0f) return;

            restante = segundos;
            duracaoTotal = segundos;
            if (visualEscudo != null) visualEscudo.enabled = true;
            AoMudarEscudo?.Invoke();
        }

        /// <summary>Desliga o escudo (acabou o tempo ou o jogador voltou ao checkpoint).</summary>
        public void RemoverEscudo()
        {
            if (restante <= 0f && (visualEscudo == null || !visualEscudo.enabled)) return;

            restante = 0f;
            if (visualEscudo != null) visualEscudo.enabled = false;
            AoMudarEscudo?.Invoke();
        }

        /// <summary>Cria um circulo semi-transparente a volta do jogador (placeholder).</summary>
        private void CriarVisualEscudo()
        {
            if (jogador == null) return;

            const int lado = 128;
            Texture2D textura = new Texture2D(lado, lado, TextureFormat.RGBA32, false);
            textura.filterMode = FilterMode.Bilinear;
            float raio = lado * 0.5f;
            for (int y = 0; y < lado; y++)
            {
                for (int x = 0; x < lado; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(raio, raio)) / raio;
                    float miolo = d < 1f ? 0.25f : 0f;
                    float aro = Mathf.Clamp01(1f - Mathf.Abs(d - 0.93f) / 0.07f);
                    textura.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Max(miolo, aro)));
                }
            }
            textura.Apply();

            Sprite sprite = Sprite.Create(textura, new Rect(0, 0, lado, lado), new Vector2(0.5f, 0.5f), lado);

            GameObject objecto = new GameObject("EscudoVisual");
            objecto.transform.SetParent(jogador, false);

            Collider2D colisor = jogador.GetComponent<Collider2D>();
            if (colisor != null)
                objecto.transform.position = colisor.bounds.center;

            objecto.transform.localScale = Vector3.one * diametroEscudo;

            visualEscudo = objecto.AddComponent<SpriteRenderer>();
            visualEscudo.sprite = sprite;
            visualEscudo.color = corEscudo;
            visualEscudo.sortingOrder = 210;
            visualEscudo.enabled = false;
        }
    }
}
