using System;
using UnityEngine;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// O Grande Esquecimento (boss do nivel final, A Sala da Memoria).
    /// 3 fases (Luanda, Malanje, Huila). Em cada fase o jogador resiste ate ao
    /// pedestal; ao colocar o Fragmento o boss fica atordoado (cerca de 10 s) e
    /// o jogador tira-lhe 1 ponto de vida por salto certo no ponto fraco
    /// (3 por fase, 9 no total). So pode ser ferido quando esta atordoado.
    /// Valores a afinar nos testes.
    /// </summary>
    public class GrandeEsquecimento : MonoBehaviour
    {
        public enum Estado { Resistir, Atordoado, Derrotado }

        [Header("Fases")]
        [SerializeField, Min(1)] private int numeroFases = 3;
        [SerializeField, Min(1)] private int vidaPorFase = 3;

        [Header("Atordoamento")]
        [Tooltip("Segundos em que o boss fica atordoado depois de o Fragmento ser colocado.")]
        [SerializeField, Min(1f)] private float duracaoAtordoamento = 10f;

        [Header("Posicao por fase")]
        [Tooltip("Onde o boss fica em cada fase (indice 0 = fase 1). Se vazio, nao se move.")]
        [SerializeField] private Vector3[] posicoesPorFase;

        [Header("Visual (opcional)")]
        [SerializeField] private SpriteRenderer desenho;
        [SerializeField] private GameObject pontoFraco;
        [Tooltip("Collider do corpo: so e solido quando o boss esta atordoado, para o jogador poder passar enquanto resiste e subir para lhe saltar em cima.")]
        [SerializeField] private Collider2D corpoSolido;
        [SerializeField] private Color corNormal = new Color(0.55f, 0.55f, 0.6f, 1f);
        [SerializeField] private Color corAtordoado = new Color(1f, 0.9f, 0.5f, 1f);

        /// <summary>Fase actual, de 1 a numeroFases.</summary>
        public int FaseActual { get; private set; } = 1;
        public int VidaTotal => numeroFases * vidaPorFase;
        public int VidaActual { get; private set; }
        public Estado EstadoActual { get; private set; } = Estado.Resistir;
        public bool EstaAtordoado => EstadoActual == Estado.Atordoado;
        public float TempoAtordoadoRestante => Mathf.Max(0f, fimAtordoamento - Time.time);

        /// <summary>(vida actual, vida total): para a barra de vida do HUD.</summary>
        public event Action<int, int> AoMudarVida;
        /// <summary>Nova fase (1 a 3).</summary>
        public event Action<int> AoMudarFase;
        public event Action AoAtordoar;
        public event Action AoRecuperar;
        public event Action AoSerDerrotado;

        private float fimAtordoamento;
        private int vidaNoInicioDaFase;

        private void Awake()
        {
            VidaActual = VidaTotal;
            vidaNoInicioDaFase = VidaActual;
            AplicarPosicao();
            AplicarVisual();
        }

        private void Start()
        {
            AoMudarVida?.Invoke(VidaActual, VidaTotal);
            AoMudarFase?.Invoke(FaseActual);
        }

        private void Update()
        {
            if (EstadoActual == Estado.Atordoado && Time.time >= fimAtordoamento)
                Recuperar();
        }

        /// <summary>Chamado pelo pedestal quando o jogador coloca o Fragmento.</summary>
        public void Atordoar(float segundosExtra = 0f)
        {
            if (EstadoActual != Estado.Resistir) return;

            EstadoActual = Estado.Atordoado;
            fimAtordoamento = Time.time + duracaoAtordoamento + segundosExtra;
            AplicarVisual();
            AoAtordoar?.Invoke();
        }

        /// <summary>Chamado pelo ponto fraco quando o jogador lhe salta em cima.</summary>
        public bool ReceberSalto()
        {
            if (EstadoActual != Estado.Atordoado) return false;

            VidaActual = Mathf.Max(0, VidaActual - 1);
            AoMudarVida?.Invoke(VidaActual, VidaTotal);

            if (VidaActual <= 0)
            {
                EstadoActual = Estado.Derrotado;
                AplicarVisual();
                AoSerDerrotado?.Invoke();
                return true;
            }

            // Fase concluida quando a vida desce os pontos dessa fase.
            if (vidaNoInicioDaFase - VidaActual >= vidaPorFase)
                ConcluirFase();

            return true;
        }

        /// <summary>O Pensador empurra o boss e prolonga o atordoamento.</summary>
        public void ProlongarAtordoamento(float segundos)
        {
            if (EstadoActual == Estado.Atordoado) fimAtordoamento += segundos;
        }

        private void ConcluirFase()
        {
            EstadoActual = Estado.Resistir;
            FaseActual = Mathf.Min(numeroFases, FaseActual + 1);
            vidaNoInicioDaFase = VidaActual;
            AplicarPosicao();
            AplicarVisual();
            AoRecuperar?.Invoke();
            AoMudarFase?.Invoke(FaseActual);
        }

        private void Recuperar()
        {
            // O tempo acabou: o boss volta a estar protegido. Mantem a vida e a fase,
            // mas o jogador tem de voltar a colocar o Fragmento (pedestal reabre).
            EstadoActual = Estado.Resistir;
            AplicarVisual();
            AoRecuperar?.Invoke();
        }

        private void AplicarPosicao()
        {
            if (posicoesPorFase != null && posicoesPorFase.Length >= FaseActual)
                transform.position = posicoesPorFase[FaseActual - 1];
        }

        private void AplicarVisual()
        {
            if (desenho != null)
                desenho.color = EstadoActual == Estado.Atordoado ? corAtordoado : corNormal;

            if (pontoFraco != null)
                pontoFraco.SetActive(EstadoActual == Estado.Atordoado);

            if (corpoSolido != null)
                corpoSolido.enabled = EstadoActual == Estado.Atordoado;
        }
    }
}
