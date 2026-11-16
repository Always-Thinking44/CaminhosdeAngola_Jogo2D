using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using CaminhosDeAngola.Jogo;

namespace CaminhosDeAngola.Interface
{
    /// <summary>
    /// Fim do nivel. Quando o jogador chega a zona final compara o que apanhou
    /// com o que devia ter apanhado (os itens historicos do nivel):
    /// - tudo apanhado: a nevoa dissipa-se e aparece "Nivel concluido";
    /// - falta algum: aparece "Falhaste", com os itens em falta, e o nivel
    ///   repete-se (a cena recarrega, por isso tudo volta ao inicio).
    /// O jogo pára quando o canvas aparece. A UI e criada por este script.
    /// </summary>
    public class FimDeNivel : MonoBehaviour
    {
        [Header("Ligacoes (procuradas sozinhas se ficarem vazias)")]
        [SerializeField] private GerenciadorColectaveis colectaveis;
        [SerializeField] private Nevoa nevoa;
        [SerializeField] private LojaZungueira loja;

        [Header("Fim com sucesso")]
        [Tooltip("Segundos que a nevoa demora a desaparecer antes de aparecer o canvas.")]
        [SerializeField, Min(0f)] private float segundosDissipar = 2f;

        [Header("Textos")]
        [SerializeField] private string textoRepetir = "Repetir nível";
        [SerializeField] private string textoJogarDeNovo = "Jogar de novo";

        [Header("Cores")]
        [SerializeField] private Color corCaixa = new Color(0.13f, 0.1f, 0.07f, 0.97f);
        [SerializeField] private Color corSucesso = new Color(0.5f, 1f, 0.55f);
        [SerializeField] private Color corFalha = new Color(1f, 0.45f, 0.4f);

        private class ItemNivel
        {
            public string identificador;
            public string nome;
            public float x;
        }

        private readonly List<ItemNivel> itens = new List<ItemNivel>();
        private Font fonte;
        private bool concluido;
        private bool mostrado;

        private void Awake()
        {
            if (colectaveis == null) colectaveis = FindAnyObjectByType<GerenciadorColectaveis>();
            if (nevoa == null) nevoa = FindAnyObjectByType<Nevoa>();
            if (loja == null) loja = FindAnyObjectByType<LojaZungueira>();

            fonte = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GarantirEventSystem();
        }

        private void Start()
        {
            // Guardar os itens do nivel agora, antes de qualquer um ser recolhido.
            foreach (ItemHistorico item in FindObjectsByType<ItemHistorico>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                itens.Add(new ItemNivel
                {
                    identificador = item.Identificador,
                    nome = item.NomeExibicao,
                    x = item.transform.position.x
                });
            }
            itens.Sort((a, b) => a.x.CompareTo(b.x));
        }

        private void OnDestroy()
        {
            // Se a cena recarregar, o tempo nao pode ficar parado.
            if (mostrado) Time.timeScale = 1f;
        }

        /// <summary>Chamado pela ZonaFimNivel quando o jogador chega ao fim.</summary>
        public void Concluir()
        {
            if (concluido) return;
            concluido = true;

            if (loja != null && loja.Aberta) loja.Fechar();

            bool completo = TudoApanhado();
            if (completo && nevoa != null && nevoa.gameObject.activeInHierarchy)
                StartCoroutine(DissiparEMostrar());
            else
                Mostrar(completo);
        }

        private IEnumerator DissiparEMostrar()
        {
            nevoa.Dissipar(segundosDissipar);
            yield return new WaitForSecondsRealtime(segundosDissipar + 0.4f);
            Mostrar(true);
        }

        private bool TudoApanhado()
        {
            if (colectaveis == null) return false;

            if (itens.Count == 0)
                return colectaveis.ItensHistoricos >= colectaveis.TotalItensHistoricos;

            foreach (ItemNivel item in itens)
                if (!colectaveis.TemItem(item.identificador)) return false;
            return true;
        }

        private void Update()
        {
            if (!mostrado) return;

            Keyboard teclado = Keyboard.current;
            if (teclado == null) return;

            if (teclado.enterKey.wasPressedThisFrame || teclado.numpadEnterKey.wasPressedThisFrame)
                Recarregar();
        }

        private void Recarregar()
        {
            Time.timeScale = 1f;
            mostrado = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // ---------------------------------------------------------------
        // Canvas de resultado
        // ---------------------------------------------------------------

        private void Mostrar(bool sucesso)
        {
            if (mostrado) return;
            mostrado = true;
            Time.timeScale = 0f;

            RectTransform raiz = (RectTransform)transform;
            Esticar(raiz);

            RectTransform painel = Novo("PainelFim", raiz);
            Esticar(painel);
            painel.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            RectTransform caixa = Novo("Caixa", painel);
            caixa.anchorMin = caixa.anchorMax = new Vector2(0.5f, 0.5f);
            caixa.pivot = new Vector2(0.5f, 0.5f);
            caixa.anchoredPosition = Vector2.zero;
            caixa.sizeDelta = new Vector2(860f, 100f);
            caixa.gameObject.AddComponent<Image>().color = corCaixa;

            VerticalLayoutGroup vertical = caixa.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(32, 32, 26, 26);
            vertical.spacing = 10f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;

            ContentSizeFitter ajuste = caixa.gameObject.AddComponent<ContentSizeFitter>();
            ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            int total = itens.Count > 0 ? itens.Count : (colectaveis != null ? colectaveis.TotalItensHistoricos : 0);
            int apanhados = colectaveis != null ? colectaveis.ItensHistoricos : 0;

            NovoTexto("Titulo", caixa, sucesso ? "NÍVEL CONCLUÍDO" : "FALHASTE", 46,
                TextAnchor.MiddleCenter, sucesso ? corSucesso : corFalha, 62f, FontStyle.Bold);

            NovoTexto("Subtitulo", caixa,
                sucesso ? "Recolheste todos os itens. O Esquecimento dissipou-se."
                        : "Faltam itens históricos. O Esquecimento levou-os, o nível vai repetir-se.",
                24, TextAnchor.MiddleCenter, Color.white, 36f, FontStyle.Normal);

            NovoTexto("Contagem", caixa, "Apanhaste " + apanhados + " de " + total + " itens históricos", 28,
                TextAnchor.MiddleCenter, new Color(1f, 0.82f, 0.4f), 44f, FontStyle.Bold);

            foreach (ItemNivel item in itens)
                ConstruirLinha(caixa, item, colectaveis != null && colectaveis.TemItem(item.identificador));

            string pensador = colectaveis != null && colectaveis.PensadorRecolhido ? "sim" : "não";
            int zimbos = colectaveis != null ? colectaveis.Zimbos : 0;
            NovoTexto("Extra", caixa, "Zimbos: " + zimbos + "     |     O Pensador: " + pensador, 22,
                TextAnchor.MiddleCenter, new Color(0.75f, 0.75f, 0.75f), 36f, FontStyle.Normal);

            ConstruirBotao(caixa, sucesso ? textoJogarDeNovo : textoRepetir);
            NovoTexto("Ajuda", caixa, "Clica no botão ou prime Enter", 20,
                TextAnchor.MiddleCenter, new Color(0.65f, 0.65f, 0.65f), 28f, FontStyle.Normal);
        }

        private void ConstruirLinha(RectTransform pai, ItemNivel item, bool apanhado)
        {
            RectTransform rt = Novo("Item_" + item.identificador, pai);
            rt.gameObject.AddComponent<Image>().color = apanhado
                ? new Color(0.14f, 0.26f, 0.15f, 1f)
                : new Color(0.30f, 0.14f, 0.12f, 1f);

            LayoutElement altura = rt.gameObject.AddComponent<LayoutElement>();
            altura.preferredHeight = 44f;

            HorizontalLayoutGroup horizontal = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(16, 16, 4, 4);
            horizontal.spacing = 14f;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            RectTransform marca = Novo("Marca", rt);
            Image imgMarca = marca.gameObject.AddComponent<Image>();
            imgMarca.color = apanhado ? corSucesso : corFalha;
            LayoutElement leMarca = marca.gameObject.AddComponent<LayoutElement>();
            leMarca.preferredWidth = 22f;
            leMarca.preferredHeight = 22f;

            Text nome = NovoTextoSimples("Nome", rt, item.nome, 24, TextAnchor.MiddleLeft, Color.white, FontStyle.Normal);
            nome.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            Text estado = NovoTextoSimples("Estado", rt, apanhado ? "Apanhado" : "Em falta", 22,
                TextAnchor.MiddleRight, apanhado ? corSucesso : corFalha, FontStyle.Bold);
            estado.gameObject.AddComponent<LayoutElement>().preferredWidth = 150f;
        }

        private void ConstruirBotao(RectTransform pai, string rotulo)
        {
            RectTransform rt = Novo("Botao", pai);
            Image fundo = rt.gameObject.AddComponent<Image>();
            fundo.color = new Color(0.55f, 0.36f, 0.14f, 1f);

            Button botao = rt.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            botao.onClick.AddListener(Recarregar);

            LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;

            RectTransform rTexto = Novo("Rotulo", rt);
            Esticar(rTexto);
            Text texto = rTexto.gameObject.AddComponent<Text>();
            texto.font = fonte;
            texto.fontSize = 30;
            texto.fontStyle = FontStyle.Bold;
            texto.alignment = TextAnchor.MiddleCenter;
            texto.color = Color.white;
            texto.raycastTarget = false;
            texto.text = rotulo;
        }

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }

        private Text NovoTexto(string nome, RectTransform pai, string conteudo, int tamanho,
            TextAnchor alinhamento, Color cor, float altura, FontStyle estilo)
        {
            Text texto = NovoTextoSimples(nome, pai, conteudo, tamanho, alinhamento, cor, estilo);
            texto.gameObject.AddComponent<LayoutElement>().preferredHeight = altura;
            return texto;
        }

        private Text NovoTextoSimples(string nome, Transform pai, string conteudo, int tamanho,
            TextAnchor alinhamento, Color cor, FontStyle estilo)
        {
            RectTransform rt = Novo(nome, pai);
            Text texto = rt.gameObject.AddComponent<Text>();
            texto.font = fonte;
            texto.fontSize = tamanho;
            texto.fontStyle = estilo;
            texto.alignment = alinhamento;
            texto.color = cor;
            texto.raycastTarget = false;
            texto.text = conteudo;
            return texto;
        }

        private static RectTransform Novo(string nome, Transform pai)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(pai, false);
            return rt;
        }

        private static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
