using UnityEngine;
using UnityEngine.SceneManagement;
using CaminhosDeAngola.Jogo;

namespace CaminhosDeAngola.Jogador
{
    /// <summary>
    /// Aplica o personagem escolhido no menu ao jogador de cada cena de jogo
    /// (animacoes, velocidade, salto e recuo da nevoa). Cria-se sozinho, por
    /// isso nao e preciso pôr nada nas cenas. Nas cenas sem
    /// ControladorJogador (menu, puzzle) nao faz nada.
    /// </summary>
    public class AplicadorPersonagem : MonoBehaviour
    {
        private static AplicadorPersonagem instancia;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Criar()
        {
            if (instancia != null) return;
            GameObject go = new GameObject("AplicadorPersonagem");
            instancia = go.AddComponent<AplicadorPersonagem>();
            DontDestroyOnLoad(go);
            Aplicar();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += AoCarregarCena;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
        }

        private void OnDestroy()
        {
            if (instancia == this) instancia = null;
        }

        private static void AoCarregarCena(Scene cena, LoadSceneMode modo)
        {
            Aplicar();
        }

        public static void Aplicar()
        {
            ControladorJogador jogador = FindAnyObjectByType<ControladorJogador>();
            if (jogador == null) return;

            PersonagemDados dados = SelecaoPersonagem.Actual;
            if (dados == null) return;

            Animator animador = jogador.GetComponent<Animator>();
            if (animador != null && dados.Controlador != null)
                animador.runtimeAnimatorController = dados.Controlador;

            jogador.AplicarModificadores(dados.MultiplicadorVelocidade, dados.MultiplicadorSalto);

            Nevoa nevoa = FindAnyObjectByType<Nevoa>(FindObjectsInactive.Include);
            if (nevoa != null) nevoa.MultiplicadorRecuo = dados.MultiplicadorRecuoNevoa;
        }
    }
}
