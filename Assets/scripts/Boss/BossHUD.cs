using UnityEngine;
using UnityEngine.UI;

namespace CaminhosDeAngola.Boss
{
    /// <summary>
    /// HUD do boss: barra de vida, fase actual e uma instrucao curta
    /// ("chega ao pedestal", "salta no ponto brilhante"). Liga-se ao
    /// GrandeEsquecimento pelos eventos e pelo estado.
    /// </summary>
    public class BossHUD : MonoBehaviour
    {
        [SerializeField] private GrandeEsquecimento boss;
        [SerializeField] private Image barraPreenchida;
        [SerializeField] private Text textoTitulo;
        [SerializeField] private Text textoInstrucao;

        private Transform jogador;
        private PedestalFragmento pedestalActual;
        private int faseDoPedestal = -1;

        private static readonly string[] nomesFase = { "Luanda", "Malanje", "Huila" };

        private void Awake()
        {
            if (boss == null) boss = FindFirstObjectByType<GrandeEsquecimento>();
        }

        private void OnEnable()
        {
            if (boss == null) return;
            boss.AoMudarVida += AoMudarVida;
            boss.AoMudarFase += AoMudarFase;
            AoMudarVida(boss.VidaActual, boss.VidaTotal);
            AoMudarFase(boss.FaseActual);
        }

        private void OnDisable()
        {
            if (boss == null) return;
            boss.AoMudarVida -= AoMudarVida;
            boss.AoMudarFase -= AoMudarFase;
        }

        private void Update()
        {
            if (boss == null || textoInstrucao == null) return;

            switch (boss.EstadoActual)
            {
                case GrandeEsquecimento.Estado.Resistir:
                    textoInstrucao.text = TextoPedestal();
                    break;
                case GrandeEsquecimento.Estado.Atordoado:
                    textoInstrucao.text = "Salta em cima do boss, no brilho!  " + Mathf.CeilToInt(boss.TempoAtordoadoRestante) + " s";
                    break;
                default:
                    textoInstrucao.text = "O Grande Esquecimento foi derrotado!";
                    break;
            }
        }

        private string TextoPedestal()
        {
            if (jogador == null)
            {
                GameObject j = GameObject.FindWithTag("Player");
                if (j != null) jogador = j.transform;
            }

            if (pedestalActual == null || faseDoPedestal != boss.FaseActual)
            {
                faseDoPedestal = boss.FaseActual;
                pedestalActual = null;
                foreach (PedestalFragmento ped in FindObjectsByType<PedestalFragmento>(FindObjectsSortMode.None))
                    if (ped.Fase == boss.FaseActual) pedestalActual = ped;
            }

            if (jogador == null || pedestalActual == null)
                return "Vai ao pedestal dourado e carrega em E";

            float dx = pedestalActual.transform.position.x - jogador.position.x;
            if (Mathf.Abs(dx) <= 1.5f)
                return "Carrega em E para colocar o Fragmento!";

            string seta = dx > 0f ? ">>>" : "<<<";
            string dir = dx > 0f ? "direita" : "esquerda";
            return "Vai ao pedestal dourado (" + Mathf.RoundToInt(Mathf.Abs(dx)) + " m para a " + dir + ")  " + seta;
        }

        private void AoMudarVida(int actual, int total)
        {
            if (barraPreenchida != null) barraPreenchida.fillAmount = total > 0 ? (float)actual / total : 0f;
            AtualizarTitulo();
        }

        private void AoMudarFase(int fase)
        {
            AtualizarTitulo();
        }

        private void AtualizarTitulo()
        {
            if (textoTitulo == null || boss == null) return;
            int i = Mathf.Clamp(boss.FaseActual - 1, 0, nomesFase.Length - 1);
            textoTitulo.text = "O GRANDE ESQUECIMENTO   Fase " + boss.FaseActual + "/3 (" + nomesFase[i] + ")   Vida " + boss.VidaActual + "/" + boss.VidaTotal;
        }
    }
}
