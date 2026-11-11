using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>Moeda do jogo. Soma o valor configurado ao contador de Zimbos.</summary>
    public class Zimbo : ColectavelBase
    {
        [SerializeField, Min(1)] private int valor = 1;

        protected override bool Recolher()
        {
            if (gerenciador == null)
            {
                Debug.LogWarning("Zimbo sem GerenciadorColectaveis na cena.", this);
                return false;
            }

            gerenciador.AdicionarZimbo(valor);
            return true;
        }
    }
}
