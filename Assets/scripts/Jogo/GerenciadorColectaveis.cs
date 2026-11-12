using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Guarda os Zimbos, os itens historicos recolhidos e o estado do
    /// Pensador. A HUD e a nevoa ouvem os eventos para se actualizarem.
    /// Como o nivel recarrega a cena, os valores voltam ao inicio sozinhos.
    /// </summary>
    public class GerenciadorColectaveis : MonoBehaviour
    {
        [Header("Valores iniciais")]
        [SerializeField, Min(0)] private int zimbosIniciais;
        [SerializeField, Min(1)] private int totalItensHistoricos = 7;

        private readonly HashSet<string> itensRecolhidos = new HashSet<string>();

        public int Zimbos { get; private set; }
        public int ItensHistoricos => itensRecolhidos.Count;
        public int TotalItensHistoricos => totalItensHistoricos;
        public bool PensadorRecolhido { get; private set; }

        /// <summary>Disparado sempre que o total de Zimbos muda.</summary>
        public event Action AoMudarZimbos;

        /// <summary>Disparado sempre que um item historico novo e recolhido.</summary>
        public event Action AoMudarItens;

        /// <summary>Disparado quando o jogador apanha O Pensador (a nevoa reage).</summary>
        public event Action AoRecolherPensador;

        private void Awake()
        {
            Reiniciar();
        }

        /// <summary>Repoe os contadores (usado tambem quando o nivel recomeca).</summary>
        public void Reiniciar()
        {
            Zimbos = zimbosIniciais;
            itensRecolhidos.Clear();
            PensadorRecolhido = false;

            AoMudarZimbos?.Invoke();
            AoMudarItens?.Invoke();
        }

        /// <summary>Soma Zimbos ao contador.</summary>
        public void AdicionarZimbo(int quantidade)
        {
            if (quantidade <= 0) return;

            Zimbos += quantidade;
            AoMudarZimbos?.Invoke();
        }

/// <summary>Gasta Zimbos (loja). Devolve falso, sem gastar nada, se nao chegarem.</summary>
        public bool GastarZimbos(int quantidade)
        {
            if (quantidade < 0 || Zimbos < quantidade) return false;

            Zimbos -= quantidade;
            AoMudarZimbos?.Invoke();
            return true;
        }


        /// <summary>
        /// Regista um item historico. Devolve falso se o identificador ja
        /// tinha sido recolhido, para nunca contar o mesmo item duas vezes.
        /// </summary>
        public bool RecolherItem(string identificador)
        {
            if (string.IsNullOrWhiteSpace(identificador)) return false;
            if (!itensRecolhidos.Add(identificador)) return false;

            AoMudarItens?.Invoke();
            return true;
        }

        /// <summary>Verdadeiro se o item historico ja foi recolhido.</summary>
        public bool TemItem(string identificador)
        {
            return !string.IsNullOrWhiteSpace(identificador) && itensRecolhidos.Contains(identificador);
        }

        /// <summary>Marca O Pensador como recolhido e avisa quem estiver a ouvir.</summary>
        public void RecolherPensador()
        {
            if (PensadorRecolhido) return;

            PensadorRecolhido = true;
            AoRecolherPensador?.Invoke();
        }
    }
}
