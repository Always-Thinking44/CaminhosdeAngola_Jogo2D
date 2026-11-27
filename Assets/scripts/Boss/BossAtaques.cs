using UnityEngine;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// Ataques do Grande Esquecimento. Enquanto o boss esta protegido (a resistir),
    /// lanca em alternancia: um orbe apontado ao jogador (desvia-te) e uma onda
    /// rasteira ao nivel do chao (salta por cima). Quando esta atordoado nao ataca.
    /// Quanto mais avancada a fase, mais depressa ataca.
    /// </summary>
    public class BossAtaques : MonoBehaviour
    {
        [SerializeField] private GrandeEsquecimento boss;
        [SerializeField] private OrbeBoss prefabOrbe;
        [Tooltip("De onde saem os orbes apontados (a boca do boss).")]
        [SerializeField] private Transform pontoDisparo;

        [Header("Ritmo (segundos entre ataques, por fase)")]
        [SerializeField] private float[] intervaloPorFase = { 2.6f, 2.1f, 1.7f };
        [Tooltip("Pausa antes do primeiro ataque de cada ronda de resistencia.")]
        [SerializeField, Min(0f)] private float atrasoInicial = 2f;

        [Header("Projecteis")]
        [SerializeField, Min(0.5f)] private float velocidadeOrbe = 7f;
        [SerializeField, Min(0.5f)] private float velocidadeOnda = 5.5f;
        [SerializeField, Min(0.5f)] private float tempoVida = 4f;
        [SerializeField, Min(0.1f)] private float escalaOrbe = 0.9f;
        [SerializeField, Min(0.1f)] private float escalaOnda = 1.4f;
        [Tooltip("Altura (y) a que viaja a onda rasteira.")]
        [SerializeField] private float alturaOnda = -1.9f;

        [Header("Alcance")]
        [Tooltip("So ataca se o jogador estiver a menos desta distancia horizontal.")]
        [SerializeField, Min(1f)] private float alcance = 22f;

        private Transform jogador;
        private float proximoAtaque;
        private bool proximoEOnda;

        private void Awake()
        {
            if (boss == null) boss = GetComponent<GrandeEsquecimento>();
            if (pontoDisparo == null) pontoDisparo = transform;
        }

        private void OnEnable()
        {
            if (boss != null)
            {
                boss.AoRecuperar += Reiniciar;
                boss.AoMudarFase += AoMudarFase;
            }
            Reiniciar();
        }

        private void OnDisable()
        {
            if (boss != null)
            {
                boss.AoRecuperar -= Reiniciar;
                boss.AoMudarFase -= AoMudarFase;
            }
        }

        private void AoMudarFase(int fase) { Reiniciar(); }

        private void Reiniciar()
        {
            proximoAtaque = Time.time + atrasoInicial;
        }

        private void Update()
        {
            if (boss == null || prefabOrbe == null) return;
            if (boss.EstadoActual != GrandeEsquecimento.Estado.Resistir) return;
            if (Time.time < proximoAtaque) return;

            if (jogador == null)
            {
                GameObject j = GameObject.FindWithTag("Player");
                if (j == null) return;
                jogador = j.transform;
            }

            float dx = jogador.position.x - boss.transform.position.x;
            if (Mathf.Abs(dx) > alcance) { proximoAtaque = Time.time + 0.5f; return; }

            if (proximoEOnda) LancarOnda(dx); else LancarOrbe();
            proximoEOnda = !proximoEOnda;

            int i = Mathf.Clamp(boss.FaseActual - 1, 0, intervaloPorFase.Length - 1);
            proximoAtaque = Time.time + intervaloPorFase[i];
        }

        private void LancarOrbe()
        {
            Vector3 origem = pontoDisparo.position;
            Vector2 alvo = jogador.position;
            OrbeBoss orbe = Instantiate(prefabOrbe, origem, Quaternion.identity);
            orbe.Iniciar(alvo - (Vector2)origem, velocidadeOrbe, tempoVida, escalaOrbe);
        }

        private void LancarOnda(float dx)
        {
            float lado = dx >= 0f ? 1f : -1f;
            Vector3 origem = new Vector3(boss.transform.position.x + lado * 1.8f, alturaOnda, 0f);
            OrbeBoss onda = Instantiate(prefabOrbe, origem, Quaternion.identity);
            onda.Iniciar(new Vector2(lado, 0f), velocidadeOnda, tempoVida * 2f, escalaOnda);
        }
    }
}
