using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaminhosDeAngola.Sons
{
    public enum Som { Zimbo, ItemHistorico, Pensador, Dano, Salto, Clique, Zungueira }

    public enum Faixa { Menu, Luanda }

    /// <summary>
    /// Toca a musica de fundo e os efeitos. Cria-se sozinho ao arrancar o
    /// jogo (nao precisa de estar em nenhuma cena) e sobrevive entre cenas.
    /// Os sons sao gerados por calculo (SintetizadorAngola) numa thread de
    /// fundo, por isso nao ha ficheiros de audio para gerir.
    /// Uso: GerenciadorAudio.Tocar(Som.Zimbo);
    /// </summary>
    public class GerenciadorAudio : MonoBehaviour
    {
        private const string ChaveMusica = "volume_musica";
        private const string ChaveEfeitos = "volume_efeitos";
        private const string CenaMenu = "MenuPrincipal";

        public static GerenciadorAudio Instancia { get; private set; }

        private sealed class Resultado
        {
            public float[] menu;
            public float[] luanda;
            public Dictionary<Som, float[]> efeitos;
        }

        private AudioSource fonteMusica;
        private AudioSource fonteEfeitos;
        private Task<Resultado> tarefa;
        private bool pronto;
        private Faixa? faixaAtual;
        private Faixa? faixaPedida;
        private float volumeMusica;
        private float volumeEfeitos;

        private readonly Dictionary<Faixa, AudioClip> musicas = new Dictionary<Faixa, AudioClip>();
        private readonly Dictionary<Som, AudioClip> efeitos = new Dictionary<Som, AudioClip>();

        public float VolumeMusica
        {
            get { return volumeMusica; }
            set
            {
                volumeMusica = Mathf.Clamp01(value);
                if (fonteMusica != null) fonteMusica.volume = volumeMusica;
                PlayerPrefs.SetFloat(ChaveMusica, volumeMusica);
            }
        }

        public float VolumeEfeitos
        {
            get { return volumeEfeitos; }
            set
            {
                volumeEfeitos = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(ChaveEfeitos, volumeEfeitos);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Criar()
        {
            if (Instancia != null) return;

            var go = new GameObject("GerenciadorAudio");
            DontDestroyOnLoad(go);
            go.AddComponent<GerenciadorAudio>();
        }

        /// <summary>Toca um efeito. Seguro de chamar mesmo que o gestor ainda nao esteja pronto.</summary>
        public static void Tocar(Som som)
        {
            if (Instancia != null) Instancia.TocarEfeito(som);
        }

        private void Awake()
        {
            if (Instancia != null && Instancia != this)
            {
                Destroy(gameObject);
                return;
            }

            Instancia = this;

            fonteMusica = gameObject.AddComponent<AudioSource>();
            fonteMusica.loop = true;
            fonteMusica.playOnAwake = false;
            fonteMusica.spatialBlend = 0f;

            fonteEfeitos = gameObject.AddComponent<AudioSource>();
            fonteEfeitos.loop = false;
            fonteEfeitos.playOnAwake = false;
            fonteEfeitos.spatialBlend = 0f;

            volumeMusica = PlayerPrefs.GetFloat(ChaveMusica, 0.5f);
            volumeEfeitos = PlayerPrefs.GetFloat(ChaveEfeitos, 0.8f);
            fonteMusica.volume = volumeMusica;

            string cena = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(cena)) faixaPedida = FaixaDaCena(cena);

            SceneManager.sceneLoaded += AoCarregarCena;
            tarefa = Task.Run(Gerar);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= AoCarregarCena;
            if (Instancia == this) Instancia = null;
        }

        private static Resultado Gerar()
        {
            var r = new Resultado
            {
                menu = SintetizadorAngola.Musica(false),
                luanda = SintetizadorAngola.Musica(true),
                efeitos = new Dictionary<Som, float[]>()
            };

            foreach (Som s in Enum.GetValues(typeof(Som)))
                r.efeitos[s] = SintetizadorAngola.Efeito(s);

            return r;
        }

        private void Update()
        {
            if (pronto || tarefa == null || !tarefa.IsCompleted) return;

            if (tarefa.IsFaulted)
            {
                Debug.LogError("GerenciadorAudio: falha a gerar os sons: " + tarefa.Exception?.GetBaseException());
                tarefa = null;
                return;
            }

            Resultado r = tarefa.Result;
            musicas[Faixa.Menu] = CriarClip("Musica_Menu", r.menu);
            musicas[Faixa.Luanda] = CriarClip("Musica_Luanda", r.luanda);
            foreach (var par in r.efeitos)
                efeitos[par.Key] = CriarClip("Som_" + par.Key, par.Value);

            SubstituirPorFicheiros();
            pronto = true;
            tarefa = null;

            if (faixaPedida.HasValue) TocarMusica(faixaPedida.Value);
        }

        /// <summary>
        /// Se existirem ficheiros em Assets/Resources/Audio com os nomes Musica_Menu,
        /// Musica_Luanda e Som_Zimbo, Som_ItemHistorico, etc., usa-os no lugar dos sons
        /// gerados por codigo. Assim basta trocar um .wav/.mp3/.ogg para mudar o som.
        /// </summary>
        private void SubstituirPorFicheiros()
        {
            foreach (Faixa f in Enum.GetValues(typeof(Faixa)))
            {
                AudioClip c = Resources.Load<AudioClip>("Audio/Musica_" + f);
                if (c != null) musicas[f] = c;
            }

            foreach (Som s in Enum.GetValues(typeof(Som)))
            {
                AudioClip c = Resources.Load<AudioClip>("Audio/Som_" + s);
                if (c != null) efeitos[s] = c;
            }
        }

        private static AudioClip CriarClip(string nome, float[] dados)
        {
            AudioClip clip = AudioClip.Create(nome, dados.Length, 1, SintetizadorAngola.Taxa, false);
            clip.SetData(dados, 0);
            return clip;
        }

        private void AoCarregarCena(Scene cena, LoadSceneMode modo)
        {
            TocarMusica(FaixaDaCena(cena.name));
        }

        private static Faixa FaixaDaCena(string nomeCena)
        {
            return nomeCena == CenaMenu || nomeCena == "Puzzle" ? Faixa.Menu : Faixa.Luanda;
        }

        /// <summary>Muda a musica de fundo. Se a faixa ja estiver a tocar, nao reinicia.</summary>
        public void TocarMusica(Faixa faixa)
        {
            faixaPedida = faixa;
            if (!pronto) return;
            if (faixaAtual == faixa && fonteMusica.isPlaying) return;

            fonteMusica.clip = musicas[faixa];
            fonteMusica.volume = volumeMusica;
            fonteMusica.Play();
            faixaAtual = faixa;
        }

        private void TocarEfeito(Som som)
        {
            if (!pronto) return;

            AudioClip clip;
            if (efeitos.TryGetValue(som, out clip))
                fonteEfeitos.PlayOneShot(clip, volumeEfeitos);
        }
    }
}
