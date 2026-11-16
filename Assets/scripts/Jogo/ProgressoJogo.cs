using UnityEngine;

namespace CaminhosDeAngola.Jogo
{
    /// <summary>
    /// Progresso do jogador entre niveis (guardado em PlayerPrefs).
    /// Indices: 0 = Luanda, 1 = Malanje, 2 = Huila, 3 = Memoria (boss).
    /// Um nivel so abre depois de concluir o anterior.
    /// </summary>
    public static class ProgressoJogo
    {
        public const string CenaMenu = "MenuPrincipal";
        public const int TotalNiveis = 4;

        private const string Chave = "caminhos.nivel.concluido.";

        /// <summary>Quando verdadeiro, o menu abre logo na escolha de niveis (ex.: depois de concluir um nivel).</summary>
        public static bool AbrirEscolhaDeNiveis;

        public static bool NivelConcluido(int indice)
        {
            return PlayerPrefs.GetInt(Chave + indice, 0) == 1;
        }

        public static void MarcarConcluido(int indice)
        {
            PlayerPrefs.SetInt(Chave + indice, 1);
            PlayerPrefs.Save();
        }

        public static bool NivelDesbloqueado(int indice)
        {
            return indice <= 0 || NivelConcluido(indice - 1);
        }

        public static void DesbloquearTudo()
        {
            for (int i = 0; i < TotalNiveis; i++) PlayerPrefs.SetInt(Chave + i, 1);
            PlayerPrefs.Save();
        }

        public static void Reiniciar()
        {
            for (int i = 0; i < TotalNiveis; i++) PlayerPrefs.DeleteKey(Chave + i);
            PlayerPrefs.Save();
        }
    }
}
