using UnityEngine;

namespace CaminhosDeAngola.Jogador
{
    /// <summary>
    /// Personagem escolhido no menu (guardado em PlayerPrefs). Lista os
    /// PersonagemDados que estao em Assets/Resources/Personagens, por ordem.
    /// </summary>
    public static class SelecaoPersonagem
    {
        private const string Chave = "CDA_Personagem";
        private static PersonagemDados[] todos;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reiniciar()
        {
            todos = null;
        }

        public static PersonagemDados[] Todos
        {
            get
            {
                if (todos == null)
                {
                    todos = Resources.LoadAll<PersonagemDados>("Personagens");
                    System.Array.Sort(todos, (a, b) => a.Ordem.CompareTo(b.Ordem));
                }
                return todos;
            }
        }

        /// <summary>Posicao do personagem escolhido na lista (0 = Neto por omissao).</summary>
        public static int Indice
        {
            get { return Mathf.Clamp(PlayerPrefs.GetInt(Chave, 0), 0, Mathf.Max(0, Todos.Length - 1)); }
            set
            {
                PlayerPrefs.SetInt(Chave, value);
                PlayerPrefs.Save();
            }
        }

        public static PersonagemDados Actual
        {
            get
            {
                PersonagemDados[] lista = Todos;
                return lista.Length > 0 ? lista[Indice] : null;
            }
        }
    }
}
