using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Faz aparecer carros e candongueiros no troco de estrada (do inicio do
    /// nivel ate a igreja). Passam sempre da direita para a esquerda. Depois
    /// da igreja o jogador ja nao e incomodado e o gerador pára.
    /// Todos os valores estao no Inspector.
    /// </summary>
    public class GeradorTrafego : MonoBehaviour
    {
        [Header("Veiculos (prefabs)")]
        [SerializeField] private VeiculoTrafego[] veiculos;

        [Header("Troco da estrada")]
        [Tooltip("X onde a estrada comeca: os veiculos desaparecem aqui.")]
        [SerializeField] private float xEsquerda = -9.4f;
        [Tooltip("X onde a estrada acaba (igreja): os veiculos aparecem, no maximo, aqui.")]
        [SerializeField] private float xDireita = 12f;
        [Tooltip("Altura do chao da estrada (base dos veiculos).")]
        [SerializeField] private float alturaEstrada = -2.54f;
        [Tooltip("Quanto fora do ecra, a direita, o veiculo aparece.")]
        [SerializeField, Min(0f)] private float margemForaDoEcra = 2f;

        [Header("Ritmo")]
        [SerializeField, Min(0.1f)] private float intervaloMinimo = 2.5f;
        [SerializeField, Min(0.1f)] private float intervaloMaximo = 5f;
        [SerializeField, Min(0.1f)] private float velocidadeMinima = 3.5f;
        [SerializeField, Min(0.1f)] private float velocidadeMaxima = 6f;
        [Tooltip("Folga minima entre dois veiculos seguidos, para nao se sobreporem.")]
        [SerializeField, Min(0f)] private float folgaEntreVeiculos = 3f;
        [Tooltip("Segundos de espera antes do primeiro veiculo.")]
        [SerializeField, Min(0f)] private float atrasoInicial = 1.5f;

        [Header("Referencias (opcionais: se vazias, procura na cena)")]
        [SerializeField] private Transform jogador;
        [SerializeField] private Camera camaraPrincipal;

        private float espera;
        private VeiculoTrafego ultimo;

        private void Start()
        {
            if (jogador == null)
            {
                GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
                if (encontrado != null) jogador = encontrado.transform;
            }

            if (camaraPrincipal == null) camaraPrincipal = Camera.main;

            if (jogador == null || veiculos == null || veiculos.Length == 0)
            {
                Debug.LogError("GeradorTrafego: falta o jogador (tag Player) ou os prefabs dos veiculos.", this);
                enabled = false;
                return;
            }

            espera = atrasoInicial;
        }

        private void Update()
        {
            // Depois da igreja nao ha mais carros.
            if (jogador.position.x >= xDireita) return;

            espera -= Time.deltaTime;
            if (espera > 0f) return;

            VeiculoTrafego prefab = veiculos[Random.Range(0, veiculos.Length)];
            float xAparecer = PosicaoParaAparecer();

            // Sem espaco no troco, ou o veiculo anterior ainda esta demasiado perto: tenta daqui a pouco.
            bool semEspaco = xAparecer < xEsquerda + 4f;
            bool anteriorPerto = ultimo != null
                && ultimo.transform.position.x > xAparecer - (ultimo.Largura + prefab.Largura) * 0.5f - folgaEntreVeiculos;

            if (semEspaco || anteriorPerto)
            {
                espera = 0.25f;
                return;
            }

            Aparecer(prefab, xAparecer);
            espera = Random.Range(intervaloMinimo, Mathf.Max(intervaloMinimo, intervaloMaximo));
        }

        // A direita do ecra, mas nunca depois do fim da estrada.
        private float PosicaoParaAparecer()
        {
            float direitaDoEcra = camaraPrincipal != null
                ? camaraPrincipal.transform.position.x + camaraPrincipal.orthographicSize * camaraPrincipal.aspect
                : jogador.position.x + 12f;

            return Mathf.Min(direitaDoEcra + margemForaDoEcra, xDireita);
        }

        private void Aparecer(VeiculoTrafego prefab, float x)
        {
            VeiculoTrafego veiculo = Instantiate(prefab, new Vector3(x, alturaEstrada, 0f), Quaternion.identity, transform);
            veiculo.Iniciar(Random.Range(velocidadeMinima, Mathf.Max(velocidadeMinima, velocidadeMaxima)), xEsquerda);
            ultimo = veiculo;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(xEsquerda, alturaEstrada, 0f), new Vector3(xDireita, alturaEstrada, 0f));
            Gizmos.DrawLine(new Vector3(xEsquerda, alturaEstrada, 0f), new Vector3(xEsquerda, alturaEstrada + 2.5f, 0f));
            Gizmos.DrawLine(new Vector3(xDireita, alturaEstrada, 0f), new Vector3(xDireita, alturaEstrada + 2.5f, 0f));
        }
    }
}
