using UnityEngine;

namespace CaminhosDeAngola.Perigos
{
    /// <summary>
    /// Faz aparecer a moto de tempos a tempos, de repente, na zona do nivel
    /// definida (por defeito, depois da estrada dos carros). So existe uma
    /// moto de cada vez; o intervalo conta a partir do momento em que a moto
    /// anterior desaparece. Todos os valores estao no Inspector.
    /// </summary>
    public class GeradorMoto : MonoBehaviour
    {
        [Header("Moto (prefab)")]
        [SerializeField] private Moto prefabMoto;

        [Header("Zona do nivel")]
        [Tooltip("A moto so aparece quando o jogador esta depois deste X (por defeito, depois da igreja, onde acaba a estrada dos carros).")]
        [SerializeField] private float xInicio = 12f;
        [Tooltip("A moto deixa de aparecer depois deste X.")]
        [SerializeField] private float xFim = 1000f;

        [Header("Ritmo")]
        [Tooltip("Segundos entre uma moto desaparecer e a seguinte aparecer (minimo).")]
        [SerializeField, Min(0.5f)] private float intervaloMinimo = 8f;
        [Tooltip("Segundos entre uma moto desaparecer e a seguinte aparecer (maximo).")]
        [SerializeField, Min(0.5f)] private float intervaloMaximo = 15f;
        [SerializeField, Min(0.1f)] private float velocidadeMinima = 11f;
        [SerializeField, Min(0.1f)] private float velocidadeMaxima = 14f;
        [Tooltip("Segundos de espera quando o jogador entra na zona, antes da primeira moto.")]
        [SerializeField, Min(0f)] private float atrasoInicial = 5f;
        [Tooltip("Quanto fora do ecra, a direita, a moto aparece.")]
        [SerializeField, Min(0f)] private float margemForaDoEcra = 4f;

        [Header("Chao")]
        [Tooltip("De que altura acima do jogador parte o raio que procura o chao no sitio onde a moto aparece.")]
        [SerializeField, Min(1f)] private float alturaSondaChao = 6f;
        [SerializeField, Min(1f)] private float alcanceSondaChao = 20f;

        [Header("Referencias (opcionais: se vazias, procura na cena)")]
        [SerializeField] private Transform jogador;
        [SerializeField] private Camera camaraPrincipal;

        private float espera;
        private bool naZona;
        private Moto motoActual;
        private int camadasChao;

        private void Start()
        {
            if (jogador == null)
            {
                GameObject encontrado = GameObject.FindGameObjectWithTag("Player");
                if (encontrado != null) jogador = encontrado.transform;
            }

            if (camaraPrincipal == null) camaraPrincipal = Camera.main;

            if (jogador == null || prefabMoto == null)
            {
                Debug.LogError("GeradorMoto: falta o jogador (tag Player) ou o prefab da moto.", this);
                enabled = false;
                return;
            }

            camadasChao = LayerMask.GetMask("Chao", "Plataforma");
            espera = atrasoInicial;
        }

        private void Update()
        {
            // Uma moto de cada vez; o intervalo so conta depois de ela desaparecer.
            if (motoActual != null) return;

            bool dentro = jogador.position.x >= xInicio && jogador.position.x <= xFim;
            if (dentro && !naZona) espera = atrasoInicial;
            naZona = dentro;
            if (!dentro) return;

            espera -= Time.deltaTime;
            if (espera > 0f) return;

            if (!Aparecer())
            {
                // Sem chao no sitio onde ia aparecer (vala): tenta daqui a pouco.
                espera = 0.5f;
                return;
            }

            espera = Random.Range(intervaloMinimo, Mathf.Max(intervaloMinimo, intervaloMaximo));
        }

        private bool Aparecer()
        {
            float x = XParaAparecer();
            Vector2 origem = new Vector2(x, jogador.position.y + alturaSondaChao);
            RaycastHit2D chao = Physics2D.Raycast(origem, Vector2.down, alcanceSondaChao, camadasChao);
            if (chao.collider == null) return false;

            motoActual = Instantiate(prefabMoto, new Vector3(x, chao.point.y, 0f), Quaternion.identity, transform);
            motoActual.Iniciar(Random.Range(velocidadeMinima, Mathf.Max(velocidadeMinima, velocidadeMaxima)));
            return true;
        }

        // A direita do ecra, fora da vista do jogador.
        private float XParaAparecer()
        {
            float direitaDoEcra = camaraPrincipal != null
                ? camaraPrincipal.transform.position.x + camaraPrincipal.orthographicSize * camaraPrincipal.aspect
                : jogador.position.x + 12f;

            return direitaDoEcra + margemForaDoEcra;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(xInicio, -6f, 0f), new Vector3(xInicio, 6f, 0f));
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(new Vector3(xFim, -6f, 0f), new Vector3(xFim, 6f, 0f));
        }
    }
}
