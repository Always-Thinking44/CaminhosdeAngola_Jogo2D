using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Visual de fumo/nevoa da frente do Esquecimento. Junta-se ao objecto
    /// Nevoa: monta varias camadas da textura a deslizar a velocidades
    /// diferentes, densas atras e a dissolver-se na frente. A pausa e
    /// automatica (o shader usa o tempo do jogo).
    /// </summary>
    public class NevoaTextura : MonoBehaviour
    {
        [System.Serializable]
        public class Camada
        {
            [Tooltip("Quantas vezes a textura se repete (x = horizontal, y = vertical).")]
            public Vector2 repeticao = Vector2.one;
            [Tooltip("Velocidade de deslize da textura (negativo em x = anda para tras).")]
            public Vector2 velocidade = new Vector2(-0.02f, 0f);
            [Range(0f, 1f)] public float opacidade = 0.5f;
        }

        [Header("Recursos")]
        [SerializeField] private Texture2D textura;
        [SerializeField] private Shader shaderCamada;

        [Header("Forma (acompanhar os valores do script Nevoa)")]
        [SerializeField, Min(1f)] private float largura = 60f;
        [SerializeField, Min(1f)] private float altura = 40f;
        [Tooltip("Parte da largura, de tras para a frente, onde a nevoa comeca a dissolver-se (0 a 1).")]
        [SerializeField, Range(0f, 1f)] private float inicioDegrade = 0.72f;

        [Header("Aspecto")]
        [SerializeField] private Color cor = new Color(0.78f, 0.80f, 0.86f, 1f);
        [SerializeField, Range(0.5f, 3f)] private float contraste = 1.5f;
        [SerializeField] private int ordemDesenho = 102;

        [Header("Camadas")]
        [SerializeField]
        private Camada[] camadas =
        {
            new Camada { repeticao = new Vector2(1.0f, 1.0f), velocidade = new Vector2(-0.015f, 0.000f), opacidade = 0.55f },
            new Camada { repeticao = new Vector2(1.6f, 1.3f), velocidade = new Vector2(-0.030f, 0.004f), opacidade = 0.40f },
            new Camada { repeticao = new Vector2(2.4f, 1.8f), velocidade = new Vector2(-0.050f, -0.003f), opacidade = 0.30f }
        };

        private void Awake()
        {
            if (shaderCamada == null) shaderCamada = Shader.Find("CaminhosDeAngola/NevoaCamada");
            if (shaderCamada == null || textura == null)
            {
                Debug.LogWarning("NevoaTextura: falta a textura ou o shader; sem camadas de fumo.", this);
                return;
            }

            GameObject raiz = new GameObject("Camadas");
            raiz.transform.SetParent(transform, false);

            Mesh malha = CriarMalha();

            for (int i = 0; i < camadas.Length; i++)
            {
                Camada camada = camadas[i];

                GameObject objecto = new GameObject("Camada " + (i + 1));
                objecto.transform.SetParent(raiz.transform, false);

                objecto.AddComponent<MeshFilter>().sharedMesh = malha;

                Material material = new Material(shaderCamada) { name = "NevoaCamada " + (i + 1) };
                material.SetTexture("_MainTex", textura);
                material.SetColor("_Color", cor);
                material.SetFloat("_Opacidade", camada.opacidade);
                material.SetVector("_Tiling", new Vector4(camada.repeticao.x, camada.repeticao.y, 0f, 0f));
                material.SetVector("_Velocidade", new Vector4(camada.velocidade.x, camada.velocidade.y, 0f, 0f));
                material.SetFloat("_InicioDegrade", inicioDegrade);
                material.SetFloat("_Contraste", contraste);

                MeshRenderer desenho = objecto.AddComponent<MeshRenderer>();
                desenho.sharedMaterial = material;
                desenho.sortingOrder = ordemDesenho + i;
                desenho.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                desenho.receiveShadows = false;
            }
        }

        // Rectangulo com a frente da nevoa em x = 0 e o fundo em x = -largura.
        private Mesh CriarMalha()
        {
            Mesh malha = new Mesh { name = "NevoaFaixa" };
            malha.vertices = new[]
            {
                new Vector3(-largura, -altura * 0.5f, 0f),
                new Vector3(0f, -altura * 0.5f, 0f),
                new Vector3(-largura, altura * 0.5f, 0f),
                new Vector3(0f, altura * 0.5f, 0f)
            };
            malha.uv = new[]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0f, 1f), new Vector2(1f, 1f)
            };
            malha.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            malha.RecalculateBounds();
            return malha;
        }
    }
}
