using System.Collections.Generic;
using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// O Esquecimento. Uma frente de nevoa que segue o jogador por tras, a
    /// velocidade constante. Se o apanha, o jogador perde 1 vida e volta ao
    /// ultimo checkpoint (a nevoa e reposta atras dele). Recua quando o
    /// jogador apanha O Pensador. A pausa e automatica: usa Time.deltaTime,
    /// por isso para quando Time.timeScale for 0.
    /// </summary>
    public class Nevoa : MonoBehaviour
    {
        [Header("Referencias (opcionais: se vazias, procura na cena)")]
        [SerializeField] private Transform jogador;
        [SerializeField] private GerenciadorVidas gerenciadorVidas;
        [SerializeField] private GerenciadorColectaveis gerenciadorColectaveis;

        [Header("Movimento")]
        [Tooltip("Velocidade constante da nevoa, em unidades por segundo.")]
        [SerializeField, Min(0f)] private float velocidade = 1.5f;
        [Tooltip("Segundos parada no inicio do nivel e depois de cada respawn.")]
        [SerializeField, Min(0f)] private float atrasoAoArrancar = 2f;
        [Tooltip("Distancia atras do jogador no inicio do nivel.")]
        [SerializeField, Min(0f)] private float distanciaInicial = 15f;
        [Tooltip("Distancia atras do ponto de respawn quando o jogador volta ao checkpoint (por decidir).")]
        [SerializeField, Min(0f)] private float distanciaReposicao = 15f;
        [Tooltip("Margem extra: o jogador so e apanhado quando passa a frente da nevoa por esta distancia.")]
        [SerializeField] private float folgaApanhar = 0f;

        [Header("Recuo com O Pensador")]
        [Tooltip("Quanto a nevoa recua quando o jogador apanha O Pensador.")]
        [SerializeField, Min(0f)] private float distanciaRecuo = 8f;
        [Tooltip("Velocidade do recuo, em unidades por segundo (para ser suave).")]
        [SerializeField, Min(0.1f)] private float velocidadeRecuo = 6f;

        [Header("Visual")]
        [SerializeField, Min(1f)] private float largura = 60f;
        [Tooltip("Parte da largura, junto a frente, onde a nevoa vai de transparente a opaca.")]
        [SerializeField, Min(0.5f)] private float larguraDegrade = 14f;
        [SerializeField, Min(1f)] private float altura = 40f;
        [SerializeField] private Color cor = new Color(0.82f, 0.84f, 0.88f, 1f);
        [SerializeField, Range(0f, 1f)] private float opacidadeMaxima = 0.75f;
        [SerializeField] private int ordemDesenho = 100;
        [SerializeField] private bool usarParticulas = true;
        [SerializeField, Min(0f)] private float particulasPorSegundo = 6f;

        /// <summary>Multiplica o recuo ao apanhar O Pensador (traco do Agostinho Neto).</summary>
        public float MultiplicadorRecuo { get; set; } = 1f;

        /// <summary>Posicao X da frente da nevoa.</summary>
        public float Frente { get; private set; }

        private float tempoParaArrancar;
        private float recuoRestante;
        private Camera camaraPrincipal;

        // Dissipacao no fim do nivel (jogador apanhou tudo).
        private class Desvanecimento
        {
            public SpriteRenderer sprite;
            public Color corInicial;
            public Material material;
            public bool usaOpacidade;
            public float valorInicial;
            public Color materialCorInicial;
        }

        private readonly List<Desvanecimento> desvanecer = new List<Desvanecimento>();
        private bool dissipando;
        private float duracaoDissipar;
        private float tempoDissipar;

        /// <summary>Verdadeiro quando a nevoa ja desapareceu por completo.</summary>
        public bool Dissipada { get; private set; }

        private void Awake()
        {
            if (jogador == null)
            {
                GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
                if (encontrado != null) jogador = encontrado.transform;
            }

            if (gerenciadorVidas == null) gerenciadorVidas = FindFirstObjectByType<GerenciadorVidas>();
            if (gerenciadorColectaveis == null) gerenciadorColectaveis = FindFirstObjectByType<GerenciadorColectaveis>();

            camaraPrincipal = Camera.main;
            CriarVisual();
        }

        private void OnEnable()
        {
            if (gerenciadorVidas != null) gerenciadorVidas.AoRespawn += Repor;
            if (gerenciadorColectaveis != null) gerenciadorColectaveis.AoRecolherPensador += Recuar;
        }

        private void OnDisable()
        {
            if (gerenciadorVidas != null) gerenciadorVidas.AoRespawn -= Repor;
            if (gerenciadorColectaveis != null) gerenciadorColectaveis.AoRecolherPensador -= Recuar;
        }

        private void Start()
        {
            if (jogador == null)
            {
                Debug.LogError("Nevoa: nao encontrei o jogador (tag Player).", this);
                enabled = false;
                return;
            }

            Frente = jogador.position.x - distanciaInicial;
            tempoParaArrancar = atrasoAoArrancar;
            AplicarPosicao();
        }

        private void Update()
        {
            if (dissipando)
            {
                AvancarDissipacao();
                return;
            }

            float dt = Time.deltaTime;

            if (tempoParaArrancar > 0f)
                tempoParaArrancar -= dt;
            else
                Frente += velocidade * dt;

            if (recuoRestante > 0f)
            {
                float passo = Mathf.Min(recuoRestante, velocidadeRecuo * dt);
                Frente -= passo;
                recuoRestante -= passo;
            }

            VerificarApanhado();
        }

        private void LateUpdate()
        {
            AplicarPosicao();
        }

        private void VerificarApanhado()
        {
            if (gerenciadorVidas == null || gerenciadorVidas.Invulneravel) return;

            if (jogador.position.x < Frente - folgaApanhar)
                gerenciadorVidas.PerderVida();
        }

        // Chamado quando o jogador volta ao checkpoint: a nevoa fica atras dele.
        private void Repor()
        {
            if (dissipando || Dissipada) return;

            Frente = gerenciadorVidas.PontoRespawn.x - distanciaReposicao;
            recuoRestante = 0f;
            tempoParaArrancar = atrasoAoArrancar;
            AplicarPosicao();
        }

        /// <summary>
        /// Fim do nivel com tudo recolhido: a nevoa para, deixa de apanhar o jogador
        /// e esvanece-se em alguns segundos. Depois desactiva-se.
        /// </summary>
        public void Dissipar(float segundos)
        {
            if (dissipando || Dissipada) return;

            dissipando = true;
            duracaoDissipar = Mathf.Max(0.01f, segundos);
            tempoDissipar = 0f;
            desvanecer.Clear();

            foreach (Renderer r in GetComponentsInChildren<Renderer>())
            {
                SpriteRenderer sprite = r as SpriteRenderer;
                if (sprite != null)
                {
                    desvanecer.Add(new Desvanecimento { sprite = sprite, corInicial = sprite.color });
                    continue;
                }

                Material material = r.sharedMaterial;
                if (material == null) continue;

                if (material.HasProperty("_Opacidade"))
                    desvanecer.Add(new Desvanecimento { material = material, usaOpacidade = true, valorInicial = material.GetFloat("_Opacidade") });
                else if (material.HasProperty("_Color"))
                    desvanecer.Add(new Desvanecimento { material = material, materialCorInicial = material.GetColor("_Color") });
            }

            foreach (ParticleSystem sistema in GetComponentsInChildren<ParticleSystem>())
                sistema.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void AvancarDissipacao()
        {
            tempoDissipar += Time.unscaledDeltaTime;
            float restante = 1f - Mathf.Clamp01(tempoDissipar / duracaoDissipar);

            foreach (Desvanecimento d in desvanecer)
            {
                if (d.sprite != null)
                {
                    Color c = d.corInicial;
                    c.a *= restante;
                    d.sprite.color = c;
                }
                else if (d.material != null)
                {
                    if (d.usaOpacidade)
                    {
                        d.material.SetFloat("_Opacidade", d.valorInicial * restante);
                    }
                    else
                    {
                        Color c = d.materialCorInicial;
                        c.a *= restante;
                        d.material.SetColor("_Color", c);
                    }
                }
            }

            if (restante <= 0f)
            {
                dissipando = false;
                Dissipada = true;
                gameObject.SetActive(false);
            }
        }

        // Chamado quando o jogador apanha O Pensador.
        private void Recuar()
        {
            recuoRestante += distanciaRecuo * MultiplicadorRecuo;
        }

        private void AplicarPosicao()
        {
            if (camaraPrincipal == null) camaraPrincipal = Camera.main;

            float y = camaraPrincipal != null
                ? camaraPrincipal.transform.position.y
                : (jogador != null ? jogador.position.y : 0f);

            transform.position = new Vector3(Frente, y, 0f);
        }

        // ---------------------------------------------------------------
        // Visual: faixa com degrade (opaca atras, transparente na frente)
        // e particulas lentas. Tudo gerado por codigo, sem arte.
        // ---------------------------------------------------------------

        private void CriarVisual()
        {
            const int larguraTextura = 256;
            const int alturaTextura = 4;

            Texture2D textura = new Texture2D(larguraTextura, alturaTextura, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "NevoaDegrade"
            };

            float inicioDegrade = 1f - Mathf.Clamp01(larguraDegrade / largura);
            Color32[] pixeis = new Color32[larguraTextura * alturaTextura];

            for (int x = 0; x < larguraTextura; x++)
            {
                float u = x / (float)(larguraTextura - 1);
                float alfa = u <= inicioDegrade
                    ? 1f
                    : 1f - Mathf.InverseLerp(inicioDegrade, 1f, u);
                alfa = Mathf.SmoothStep(0f, 1f, alfa) * opacidadeMaxima;

                Color32 pixel = new Color(cor.r, cor.g, cor.b, alfa);
                for (int y = 0; y < alturaTextura; y++)
                    pixeis[y * larguraTextura + x] = pixel;
            }

            textura.SetPixels32(pixeis);
            textura.Apply();

            float pixeisPorUnidade = larguraTextura / largura;
            Sprite sprite = Sprite.Create(
                textura,
                new Rect(0, 0, larguraTextura, alturaTextura),
                new Vector2(1f, 0.5f),   // pivot na frente da nevoa
                pixeisPorUnidade);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(transform, false);
            visual.transform.localScale = new Vector3(1f, altura / (alturaTextura / pixeisPorUnidade), 1f);

            SpriteRenderer desenho = visual.AddComponent<SpriteRenderer>();
            desenho.sprite = sprite;
            desenho.sortingOrder = ordemDesenho;

            if (usarParticulas) CriarParticulas();
        }

        private void CriarParticulas()
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                Debug.LogWarning("Nevoa: shader Sprites/Default nao encontrado; sem particulas.", this);
                return;
            }

            GameObject objecto = new GameObject("Particulas");
            objecto.transform.SetParent(transform, false);
            objecto.transform.localPosition = new Vector3(-larguraDegrade * 0.5f, 0f, 0f);

            ParticleSystem sistema = objecto.AddComponent<ParticleSystem>();
            sistema.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule principal = sistema.main;
            principal.loop = true;
            principal.simulationSpace = ParticleSystemSimulationSpace.Local;
            principal.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
            principal.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
            principal.startSize = new ParticleSystem.MinMaxCurve(3f, 6f);
            principal.startColor = new Color(cor.r, cor.g, cor.b, 0.35f);
            principal.maxParticles = 200;

            ParticleSystem.EmissionModule emissao = sistema.emission;
            emissao.rateOverTime = particulasPorSegundo;

            ParticleSystem.ShapeModule forma = sistema.shape;
            forma.shapeType = ParticleSystemShapeType.Box;
            forma.scale = new Vector3(larguraDegrade, altura * 0.5f, 1f);

            ParticleSystem.ColorOverLifetimeModule corAoLongoDaVida = sistema.colorOverLifetime;
            corAoLongoDaVida.enabled = true;
            Gradient degrade = new Gradient();
            degrade.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.3f), new GradientAlphaKey(0f, 1f) });
            corAoLongoDaVida.color = degrade;

            ParticleSystemRenderer desenho = objecto.GetComponent<ParticleSystemRenderer>();
            desenho.sortingOrder = ordemDesenho + 1;
            desenho.material = new Material(shader) { mainTexture = CriarTexturaCirculo() };

            sistema.Play();
        }

        private static Texture2D CriarTexturaCirculo()
        {
            const int tamanho = 64;
            Texture2D textura = new Texture2D(tamanho, tamanho, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                name = "NevoaParticula"
            };

            Color32[] pixeis = new Color32[tamanho * tamanho];
            float centro = (tamanho - 1) * 0.5f;

            for (int y = 0; y < tamanho; y++)
            {
                for (int x = 0; x < tamanho; x++)
                {
                    float distancia = Mathf.Sqrt((x - centro) * (x - centro) + (y - centro) * (y - centro)) / centro;
                    float alfa = Mathf.Clamp01(1f - distancia);
                    alfa *= alfa;
                    pixeis[y * tamanho + x] = new Color(1f, 1f, 1f, alfa);
                }
            }

            textura.SetPixels32(pixeis);
            textura.Apply();
            return textura;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.8f, 0.8f, 1f, 0.9f);
            float x = Application.isPlaying ? Frente : transform.position.x;
            Gizmos.DrawLine(new Vector3(x, transform.position.y - altura * 0.5f, 0f),
                            new Vector3(x, transform.position.y + altura * 0.5f, 0f));
        }
    }
}
