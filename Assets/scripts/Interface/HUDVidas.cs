using UnityEngine;
using UnityEngine.UI;

namespace CaminhosDeAngola.Interface
{
    /// <summary>
    /// Mostra as vidas no canto superior esquerdo: um icone por vida, que fica
    /// escuro quando a vida e perdida.
    /// </summary>
    public class HUDVidas : MonoBehaviour
    {
        [SerializeField] private Image[] icones;

        [SerializeField] private Color corCheia = new Color(0.86f, 0.16f, 0.16f, 1f);
        [SerializeField] private Color corVazia = new Color(0.22f, 0.22f, 0.22f, 0.55f);

        /// <summary>Actualiza os icones. Chamado pelo GerenciadorVidas.</summary>
        public void AtualizarVidas(int vidas, int maximo)
        {
            if (icones == null) return;

            for (int i = 0; i < icones.Length; i++)
            {
                if (icones[i] == null) continue;

                bool visivel = i < maximo;
                icones[i].enabled = visivel;
                if (visivel) icones[i].color = i < vidas ? corCheia : corVazia;
            }
        }
    }
}
