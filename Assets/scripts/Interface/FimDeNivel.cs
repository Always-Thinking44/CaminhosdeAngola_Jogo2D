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
    /// O jogo pára quando o canvas aparece. O aspecto vem do TemaPixel.
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

        private class ItemNivel
        {
            public string identificador;
            public string nome;
            public float x;
        }

        private readonly List<ItemNivel> itens = new List<ItemNivel>();
        private bool concluido;
        private bool mostrado;

        private void Awake()
        {
            if (colectaveis == null) colectaveis = FindAnyObjectByType<GerenciadorColectaveis>();
            if (nevoa == null) nevoa = FindAnyObjectByType<Nevoa>();
            if (loja == null) loja = FindAnyObjectByType<LojaZungueira>();

            GarantirEventSystem();
        }

        private void Start()
        {
            // Guardar os itens do nivel agora, antes de qualquer um ser recolhido.
            foreach (ItemHistorico item in FindObjectsByType<ItemHistorico>(FindObjectsInactive.Include))
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
            TemaPixel.Esticar(raiz);

            RectTransform ecra = TemaPixel.CriarFundoEcra(raiz, "PainelFim");
            RectTransform caixa = TemaPixel.CriarPainel(ecra, 860f, true);

            int total = itens.Count > 0 ? itens.Count : (colectaveis != null ? colectaveis.TotalItensHistoricos : 0);
            int apanhados = colectaveis != null ? colectaveis.ItensHistoricos : 0;

            TemaPixel.CriarFaixa(caixa, sucesso ? "NÍVEL CONCLUÍDO" : "FALHASTE",
                sucesso ? TemaPixel.Amarelo : TemaPixel.Creme);

            TemaPixel.CriarTexto("Subtitulo", caixa,
                sucesso ? "Recolheste todos os itens. O Esquecimento dissipou-se."
                        : "Faltam itens históricos. O Esquecimento levou-os, o nível vai repetir-se.",
                24, TemaPixel.Creme);

            TemaPixel.CriarDivisor(caixa);

            TemaPixel.CriarTexto("Contagem", caixa,
                "Apanhaste " + apanhados + " de " + total + " itens históricos",
                32, TemaPixel.Amarelo);

            RectTransform lista = TemaPixel.Novo("Itens", caixa);
            VerticalLayoutGroup verticalLista = lista.gameObject.AddComponent<VerticalLayoutGroup>();
            verticalLista.spacing = 6f;
            verticalLista.childControlWidth = true;
            verticalLista.childControlHeight = true;
            verticalLista.childForceExpandWidth = true;
            verticalLista.childForceExpandHeight = false;

            foreach (ItemNivel item in itens)
                ConstruirLinha(lista, item, colectaveis != null && colectaveis.TemItem(item.identificador));

            string pensador = colectaveis != null && colectaveis.PensadorRecolhido ? "sim" : "não";
            int zimbos = colectaveis != null ? colectaveis.Zimbos : 0;
            TemaPixel.CriarTexto("Extra", caixa, "Zimbos: " + zimbos + "     O Pensador: " + pensador,
                24, TemaPixel.Castanho);

            TemaPixel.CriarBotao(caixa, sucesso ? textoJogarDeNovo : textoRepetir, VarianteBotao.Terra, Recarregar);
            TemaPixel.CriarTexto("Ajuda", caixa, "Clica no botão ou prime Enter", 24, TemaPixel.Castanho);
        }

        private void ConstruirLinha(RectTransform pai, ItemNivel item, bool apanhado)
        {
            RectTransform rt = TemaPixel.Novo("Item_" + item.identificador, pai);
            TemaPixel.AplicarSprite(rt.gameObject.AddComponent<Image>(), apanhado ? "ui_placa_ok" : "ui_placa_falha");
            rt.gameObject.AddComponent<LayoutElement>().minHeight = 52f;

            HorizontalLayoutGroup horizontal = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            horizontal.padding = new RectOffset(24, 24, 8, 8);
            horizontal.spacing = 20f;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childControlWidth = true;
            horizontal.childControlHeight = true;
            horizontal.childForceExpandWidth = false;
            horizontal.childForceExpandHeight = false;

            // Icone de certo ou errado (10 x 7 pixels de arte).
            RectTransform marca = TemaPixel.Novo("Marca", rt);
            Image icone = TemaPixel.AplicarSprite(marca.gameObject.AddComponent<Image>(),
                apanhado ? "ui_icone_ok" : "ui_icone_falha", false);
            icone.raycastTarget = false;
            LayoutElement leMarca = marca.gameObject.AddComponent<LayoutElement>();
            leMarca.preferredWidth = 40f;
            leMarca.preferredHeight = 28f;

            Text nome = TemaPixel.CriarTexto("Nome", rt, item.nome, 24, TemaPixel.Creme, false, TextAnchor.MiddleLeft, true);
            nome.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;

            Text estado = TemaPixel.CriarTexto("Estado", rt, apanhado ? "Apanhado" : "Em falta", 24,
                apanhado ? TemaPixel.VerdeClaro : TemaPixel.Amarelo, false, TextAnchor.MiddleRight, true);
            estado.gameObject.AddComponent<LayoutElement>().preferredWidth = 190f;
        }

        private void GarantirEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;

            GameObject go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            InputSystemUIInputModule modulo = go.AddComponent<InputSystemUIInputModule>();
            modulo.AssignDefaultActions();
        }
    }
}
