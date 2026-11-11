using System.Collections;
using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Base comum dos checkpoints. Guarda o ponto de respawn no
    /// GerenciadorVidas e mostra um aviso breve quando e activado.
    /// </summary>
    public abstract class CheckpointBase : MonoBehaviour
    {
        [Header("Ligacoes")]
        [SerializeField] protected GerenciadorVidas gerenciador;

        [Tooltip("Ponto exacto onde o jogador reaparece. Se ficar vazio, usa o deslocamento abaixo.")]
        [SerializeField] protected Transform pontoRespawn;
        [SerializeField] protected Vector3 deslocamentoRespawn = new Vector3(0f, 0.7f, 0f);

        [Tooltip("Objecto de texto mostrado quando o checkpoint e activado.")]
        [SerializeField] protected GameObject aviso;
        [SerializeField, Min(0.1f)] protected float tempoAviso = 1.5f;

        /// <summary>Verdadeiro depois de o checkpoint ter sido activado.</summary>
        public bool Activo { get; private set; }

        protected virtual void Awake()
        {
            if (gerenciador == null) gerenciador = FindAnyObjectByType<GerenciadorVidas>();
            if (aviso != null) aviso.SetActive(false);
        }

        /// <summary>Activa o checkpoint uma unica vez e guarda o ponto de respawn.</summary>
        public void Activar()
        {
            if (Activo) return;

            Activo = true;

            Vector3 ponto = pontoRespawn != null
                ? pontoRespawn.position
                : transform.position + deslocamentoRespawn;

            if (gerenciador != null) gerenciador.DefinirCheckpoint(ponto);

            AoActivar();
            MostrarAviso();
        }

        /// <summary>Ponto reservado para as subclasses reagirem a activacao.</summary>
        protected virtual void AoActivar() { }

        /// <summary>Mostra o aviso durante alguns segundos e esconde-o.</summary>
        protected void MostrarAviso(string texto = "Checkpoint")
        {
            if (aviso == null) return;

            TextMesh textoMesh = aviso.GetComponent<TextMesh>();
            if (textoMesh != null) textoMesh.text = texto;

            aviso.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(EsconderAposTempo());
        }

        /// <summary>Mostra o aviso sem contagem (ex.: a dica "Premir E").</summary>
        protected void MostrarAvisoPermanente(string texto)
        {
            if (aviso == null) return;

            TextMesh textoMesh = aviso.GetComponent<TextMesh>();
            if (textoMesh != null) textoMesh.text = texto;

            StopAllCoroutines();
            aviso.SetActive(true);
        }

        /// <summary>Esconde o aviso imediatamente.</summary>
        protected void EsconderAvisoAgora()
        {
            StopAllCoroutines();
            if (aviso != null) aviso.SetActive(false);
        }

        private IEnumerator EsconderAposTempo()
        {
            yield return new WaitForSeconds(tempoAviso);
            if (aviso != null) aviso.SetActive(false);
        }
    }
}
