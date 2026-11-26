using UnityEngine;

namespace CaminhosDeAngola.Jogador
{
    /// <summary>
    /// Ficha de um personagem jogavel (Neto, Njinga, Mandume): nome, traco
    /// proprio, animacoes e os multiplicadores que o distinguem. Os assets
    /// ficam em Assets/Resources/Personagens e sao listados pela
    /// SelecaoPersonagem, por ordem. Valores a afinar nos testes.
    /// </summary>
    [CreateAssetMenu(menuName = "Caminhos de Angola/Personagem", fileName = "Personagem")]
    public class PersonagemDados : ScriptableObject
    {
        [Header("Identidade")]
        [SerializeField] private int ordem;
        [SerializeField] private string nome = "Personagem";
        [SerializeField, TextArea] private string traco = "";

        [Header("Aspecto")]
        [Tooltip("Animator (ou override) com as animacoes deste personagem.")]
        [SerializeField] private RuntimeAnimatorController controlador;
        [Tooltip("Frames do 'parado', usados no ecra de escolha.")]
        [SerializeField] private Sprite[] animacaoParado;

        [Header("Traco proprio (1 = igual ao Neto)")]
        [Tooltip("Mandume corre um pouco mais depressa.")]
        [SerializeField, Min(0.1f)] private float multiplicadorVelocidade = 1f;
        [Tooltip("Njinga salta um pouco mais alto (multiplica a forca do salto).")]
        [SerializeField, Min(0.1f)] private float multiplicadorSalto = 1f;
        [Tooltip("Neto: a nevoa recua mais quando apanha O Pensador.")]
        [SerializeField, Min(0.1f)] private float multiplicadorRecuoNevoa = 1f;

        public int Ordem => ordem;
        public string Nome => nome;
        public string Traco => traco;
        public RuntimeAnimatorController Controlador => controlador;
        public Sprite[] AnimacaoParado => animacaoParado;
        public float MultiplicadorVelocidade => multiplicadorVelocidade;
        public float MultiplicadorSalto => multiplicadorSalto;
        public float MultiplicadorRecuoNevoa => multiplicadorRecuoNevoa;
    }
}
