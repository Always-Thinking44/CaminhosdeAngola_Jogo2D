using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// O Pensador escondido no nivel. Ao ser apanhado avisa o gestor; a
    /// nevoa (Tarefa 5) reage a esse evento e recua.
    /// </summary>
    public class Pensador : ColectavelBase
    {
        protected override Sons.Som SomDeRecolha => Sons.Som.Pensador;

        protected override bool Recolher()
        {
            if (gerenciador == null)
            {
                Debug.LogWarning("Pensador sem GerenciadorColectaveis na cena.", this);
                return false;
            }

            gerenciador.RecolherPensador();
            return true;
        }
    }
}
