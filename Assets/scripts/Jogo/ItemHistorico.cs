using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Item historico de um monumento. O identificador tem de ser unico
    /// para o contador nao somar o mesmo item duas vezes.
    /// </summary>
    public class ItemHistorico : ColectavelBase
    {
        [Header("Item")]
        [SerializeField] private string identificador = "item";
        [SerializeField] private string nomeExibicao = "Item historico";

        public string Identificador => identificador;
        public string NomeExibicao => nomeExibicao;

        protected override Sons.Som SomDeRecolha => Sons.Som.ItemHistorico;

        protected override bool Recolher()
        {
            if (gerenciador == null)
            {
                Debug.LogWarning("ItemHistorico sem GerenciadorColectaveis na cena.", this);
                return false;
            }

            bool novo = gerenciador.RecolherItem(identificador);
            if (novo) Interface.AvisoMonumento.Mostrar(nomeExibicao);
            return novo;
        }
    }
}
