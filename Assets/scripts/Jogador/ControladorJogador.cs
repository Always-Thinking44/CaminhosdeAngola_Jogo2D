using UnityEngine;
using UnityEngine.InputSystem;

namespace CaminhosDeAngola.Jogador
{
    /// <summary>
    /// Controla o Agostinho Neto: andar/correr, salto variável, coyote time,
    /// jump buffer e agachar. O jogador nunca ataca.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ControladorJogador : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField, Min(0f)] private float velocidade = 6f;
        [SerializeField, Min(0f)] private float aceleracao = 60f;
        [SerializeField, Min(0f)] private float desaceleracao = 80f;

        [Header("Salto")]
        [SerializeField, Min(0f)] private float forcaPulo = 10f;
        [SerializeField, Min(0f)] private float gravidadeSubida = 2.2f;
        [SerializeField, Min(0f)] private float gravidadeQueda = 2.8f;
        [SerializeField, Range(0f, 1f)] private float corteSalto = 0.5f;
        [SerializeField, Min(0f)] private float tempoCoyote = 0.12f;
        [SerializeField, Min(0f)] private float tempoBufferSalto = 0.12f;

        [Header("Agachar")]
        [SerializeField, Range(0.1f, 1f)] private float multiplicadorVelocidadeAgachado = 0.45f;
        [SerializeField, Range(0.3f, 1f)] private float alturaAgachado = 0.7f;

        [Header("Deteccao de chao")]
        [SerializeField] private LayerMask camadasChao;
        [SerializeField, Min(0.01f)] private float raioPes = 0.12f;

        [Header("Degraus")]
        [SerializeField, Min(0f)] private float alturaMaximaDegrau = 0.5f;

        [Header("Accoes (Input System)")]
        [SerializeField] private InputActionAsset accoes;

        [Header("Referencias")]
        [SerializeField] private Animator animador;
        [SerializeField] private SpriteRenderer desenho;

        private Rigidbody2D corpo;
        private BoxCollider2D colisor;

        private InputAction accaoMover;
        private InputAction accaoSaltar;
        private InputAction accaoAgachar;

        private float direccao;
        private float contadorCoyote;
        private float contadorBuffer;
        private bool noChao;
        private bool estaAgachado;

        private Vector2 tamanhoDePe;
        private Vector2 centroDePe;
        private bool alturaAplicada;

        private void Awake()
        {
            corpo = GetComponent<Rigidbody2D>();
            colisor = GetComponent<BoxCollider2D>();
            if (animador == null) animador = GetComponent<Animator>();
            if (desenho == null) desenho = GetComponent<SpriteRenderer>();

            if (colisor != null)
            {
                tamanhoDePe = colisor.size;
                centroDePe = colisor.offset;
            }

            ConfigurarAccoes();
        }

        private void ConfigurarAccoes()
        {
            if (accoes == null)
            {
                Debug.LogError("ControladorJogador: falta atribuir o asset de accoes (CaminhosDeAngola.inputactions).", this);
                enabled = false;
                return;
            }

            InputActionMap mapa = accoes.FindActionMap("Jogo", false);
            if (mapa == null)
            {
                Debug.LogError("ControladorJogador: o mapa 'Jogo' nao existe no asset de accoes.", this);
                enabled = false;
                return;
            }

            accaoMover = mapa.FindAction("Mover", false);
            accaoSaltar = mapa.FindAction("Saltar", false);
            accaoAgachar = mapa.FindAction("Agachar", false);

            if (accaoMover == null || accaoSaltar == null || accaoAgachar == null)
            {
                Debug.LogError("ControladorJogador: faltam accoes Mover/Saltar/Agachar no mapa 'Jogo'.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            accaoMover?.Enable();
            accaoSaltar?.Enable();
            accaoAgachar?.Enable();
        }

        private void OnDisable()
        {
            accaoMover?.Disable();
            accaoSaltar?.Disable();
            accaoAgachar?.Disable();
        }

        private void Update()
        {
            if (accaoMover == null) return;

            direccao = accaoMover.ReadValue<float>();

            if (accaoSaltar.WasPressedThisFrame())
                contadorBuffer = tempoBufferSalto;

            // Salto variavel: soltar o botao a subir corta a subida.
            if (accaoSaltar.WasReleasedThisFrame() && corpo.linearVelocity.y > 0f)
                corpo.linearVelocity = new Vector2(corpo.linearVelocity.x, corpo.linearVelocity.y * corteSalto);

            AtualizarAgachamento();
            AtualizarAnimacao();
        }

        private void FixedUpdate()
        {
            AtualizarChao();

            if (noChao) contadorCoyote = tempoCoyote;
            else contadorCoyote -= Time.fixedDeltaTime;

            contadorBuffer -= Time.fixedDeltaTime;

            if (contadorBuffer > 0f && contadorCoyote > 0f && !estaAgachado)
                Saltar();

            TentarSubirDegrau();

            float multiplicador = estaAgachado ? multiplicadorVelocidadeAgachado : 1f;
            float alvo = direccao * velocidade * multiplicador;
            float taxa = Mathf.Abs(alvo) > 0.01f ? aceleracao : desaceleracao;

            corpo.linearVelocity = new Vector2(
                Mathf.MoveTowards(corpo.linearVelocity.x, alvo, taxa * Time.fixedDeltaTime),
                corpo.linearVelocity.y);

            corpo.gravityScale = corpo.linearVelocity.y > 0.1f ? gravidadeSubida : gravidadeQueda;
        }

        private void Saltar()
        {
            corpo.linearVelocity = new Vector2(corpo.linearVelocity.x, forcaPulo);
            contadorBuffer = 0f;
            contadorCoyote = 0f;
            noChao = false;
        }

        private void AtualizarChao()
        {
            if (colisor == null)
            {
                noChao = false;
                return;
            }

            Vector2 pes = new Vector2(colisor.bounds.center.x, colisor.bounds.min.y);
            noChao = Physics2D.OverlapCircle(pes, raioPes, camadasChao) != null;
        }

        /// <summary>
        /// Sobe automaticamente pequenos degraus (ex.: o lancil entre a estrada e o bloco1),
        /// para o jogador nao ficar preso ao andar.
        /// </summary>
        private void TentarSubirDegrau()
        {
            if (!noChao || colisor == null || Mathf.Abs(direccao) < 0.1f || alturaMaximaDegrau <= 0f)
                return;

            float pes = colisor.bounds.min.y;
            float meiaLargura = colisor.bounds.extents.x;
            float frenteX = colisor.bounds.center.x + Mathf.Sign(direccao) * (meiaLargura + 0.08f);

            Vector2 origem = new Vector2(frenteX, pes + alturaMaximaDegrau + 0.05f);
            RaycastHit2D toque = Physics2D.Raycast(origem, Vector2.down, alturaMaximaDegrau + 0.15f, camadasChao);
            if (toque.collider == null) return;

            float subida = toque.point.y - pes;
            if (subida <= 0.03f || subida > alturaMaximaDegrau) return;

            // Verificar se ha espaco livre para o corpo subir.
            Vector2 tamanhoLivre = new Vector2(colisor.bounds.size.x * 0.9f, colisor.bounds.size.y - 0.06f);
            Vector2 centroNovo = new Vector2(colisor.bounds.center.x, pes + subida + 0.03f + tamanhoLivre.y * 0.5f);
            if (Physics2D.OverlapBox(centroNovo, tamanhoLivre, 0f, camadasChao) != null) return;

            transform.position = new Vector3(transform.position.x, transform.position.y + subida + 0.01f, transform.position.z);
        }

        private void AtualizarAgachamento()
        {
            bool querAgachar = accaoAgachar.IsPressed();

            if (querAgachar && noChao)
                estaAgachado = true;
            else if (!querAgachar && estaAgachado && !HaTectoPorCima())
                estaAgachado = false;

            AplicarAlturaColisor();
        }

        private bool HaTectoPorCima()
        {
            if (colisor == null) return false;

            float alturaDePe = tamanhoDePe.y;
            Vector2 centro = new Vector2(
                transform.position.x + centroDePe.x,
                transform.position.y + centroDePe.y - alturaDePe * 0.5f + alturaDePe * 0.9f);

            Vector2 tamanho = new Vector2(tamanhoDePe.x * 0.9f, alturaDePe * 0.2f);
            return Physics2D.OverlapBox(centro, tamanho, 0f, camadasChao) != null;
        }

        private void AplicarAlturaColisor()
        {
            if (colisor == null || alturaAplicada == estaAgachado) return;

            float altura = estaAgachado ? tamanhoDePe.y * alturaAgachado : tamanhoDePe.y;
            float diferenca = tamanhoDePe.y - altura;

            colisor.size = new Vector2(tamanhoDePe.x, altura);
            colisor.offset = new Vector2(centroDePe.x, centroDePe.y - diferenca * 0.5f);
            alturaAplicada = estaAgachado;
        }

        private void AtualizarAnimacao()
        {
            if (animador != null)
            {
                animador.SetBool("isWalking", Mathf.Abs(direccao) > 0.01f && noChao);
                animador.SetBool("isJumping", !noChao);
            }

            if (desenho != null && Mathf.Abs(direccao) > 0.01f)
                desenho.flipX = direccao < 0f;
        }
    }
}
