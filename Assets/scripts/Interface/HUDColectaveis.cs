using CaminhosDeAngola.Jogo;
using UnityEngine;
using UnityEngine.UI;

namespace CaminhosDeAngola.Interface
{
    /// <summary>Mostra os Zimbos e os itens historicos recolhidos no HUD.</summary>
    public class HUDColectaveis : MonoBehaviour
    {
        [Header("Ligacoes")]
        [SerializeField] private GerenciadorColectaveis gerenciador;
        [SerializeField] private Text textoZimbos;
        [SerializeField] private Text textoItens;

        private void Awake()
        {
            if (gerenciador == null)
                gerenciador = FindAnyObjectByType<GerenciadorColectaveis>();
        }

        private void OnEnable()
        {
            if (gerenciador != null)
            {
                gerenciador.AoMudarZimbos += ActualizarZimbos;
                gerenciador.AoMudarItens += ActualizarItens;
            }

            ActualizarTudo();
        }

        private void OnDisable()
        {
            if (gerenciador != null)
            {
                gerenciador.AoMudarZimbos -= ActualizarZimbos;
                gerenciador.AoMudarItens -= ActualizarItens;
            }
        }

        private void ActualizarTudo()
        {
            ActualizarZimbos();
            ActualizarItens();
        }

        private void ActualizarZimbos()
        {
            if (textoZimbos == null || gerenciador == null) return;
            textoZimbos.text = "Zimbos: " + gerenciador.Zimbos;
        }

        private void ActualizarItens()
        {
            if (textoItens == null || gerenciador == null) return;
            textoItens.text = "Itens: " + gerenciador.ItensHistoricos + "/" + gerenciador.TotalItensHistoricos;
        }
    }
}
