using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CaminhosDeAngola.Interface
{
    public enum VarianteBotao
    {
        Terra,      // terracota: accao principal
        Asfalto,    // azul do asfalto: accao secundaria
        Inactivo    // cinzento: sem saldo / indisponivel
    }

    /// <summary>
    /// Estilo visual dos menus (pixel art, paleta de Luanda: telhados de
    /// terracota, creme das torres, asfalto azulado e risca amarela).
    /// Os sprites e as fontes estao em Assets/Resources/UI. Cada pixel de arte
    /// ocupa 4 unidades do canvas (os sprites usam PPU 25 com canvas a 100).
    /// Tamanhos de letra aconselhados: Silkscreen 16/24/32, Press Start 2P 24/40.
    /// </summary>
    public static class TemaPixel
    {
        // ---- Cores tiradas das artes do jogo ----
        public static readonly Color Contorno = new Color32(31, 27, 22, 255);
        public static readonly Color Creme = new Color32(243, 230, 196, 255);
        public static readonly Color Amarelo = new Color32(240, 192, 72, 255);
        public static readonly Color Terracota = new Color32(232, 150, 60, 255);
        public static readonly Color TerracotaEscura = new Color32(236, 100, 84, 255);
        public static readonly Color Castanho = new Color32(170, 158, 130, 255);
        public static readonly Color VerdeEscuro = new Color32(110, 205, 130, 255);
        public static readonly Color VerdeClaro = new Color32(150, 200, 130, 255);
        public static readonly Color VermelhoClaro = new Color32(230, 96, 80, 255);
        public static readonly Color FundoEcra = new Color32(12, 10, 18, 215);

        private static Font fonteTitulo;
        private static Font fonteCorpo;

        /// <summary>Press Start 2P: titulos e precos.</summary>
        public static Font FonteTitulo
        {
            get
            {
                if (fonteTitulo == null) fonteTitulo = CarregarFonte("UI/Fontes/PressStart2P-Regular");
                return fonteTitulo;
            }
        }

        /// <summary>Silkscreen: texto corrido.</summary>
        public static Font FonteCorpo
        {
            get
            {
                if (fonteCorpo == null) fonteCorpo = CarregarFonte("UI/Fontes/Silkscreen-Regular");
                return fonteCorpo;
            }
        }

        private static Font CarregarFonte(string caminho)
        {
            Font fonte = Resources.Load<Font>(caminho);
            return fonte != null ? fonte : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        public static Sprite Sprite(string nome)
        {
            return Resources.Load<Sprite>("UI/" + nome);
        }

        // ---------------------------------------------------------------
        // Utilitarios de RectTransform
        // ---------------------------------------------------------------

        public static RectTransform Novo(string nome, Transform pai)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(pai, false);
            return rt;
        }

        public static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        /// <summary>Aplica um sprite em pixel art a uma imagem (esticado pelas margens, sem deformar).</summary>
        public static Image AplicarSprite(Image imagem, string nome, bool ladrilhar = true)
        {
            imagem.sprite = Sprite(nome);
            imagem.type = ladrilhar ? Image.Type.Tiled : Image.Type.Simple;
            imagem.color = Color.white;
            return imagem;
        }

        // ---------------------------------------------------------------
        // Pecas
        // ---------------------------------------------------------------

        /// <summary>Fundo escuro que cobre o ecra e bloqueia cliques no jogo.</summary>
        public static RectTransform CriarFundoEcra(Transform pai, string nome)
        {
            RectTransform rt = Novo(nome, pai);
            Esticar(rt);
            Image imagem = rt.gameObject.AddComponent<Image>();
            imagem.color = FundoEcra;
            return rt;
        }

        /// <summary>Caixa creme com moldura de terracota, ao centro do ecra, que cresce com o conteudo.</summary>
        public static RectTransform CriarPainel(Transform pai, float largura, bool comFaixa)
        {
            RectTransform caixa = Novo("Caixa", pai);
            caixa.anchorMin = caixa.anchorMax = new Vector2(0.5f, 0.5f);
            caixa.pivot = new Vector2(0.5f, 0.5f);
            caixa.anchoredPosition = Vector2.zero;
            caixa.sizeDelta = new Vector2(largura, 100f);
            AplicarSprite(caixa.gameObject.AddComponent<Image>(), "ui_painel");

            VerticalLayoutGroup vertical = caixa.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(44, 44, comFaixa ? 84 : 44, 44);
            vertical.spacing = 12f;
            vertical.childAlignment = TextAnchor.UpperCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;

            ContentSizeFitter ajuste = caixa.gameObject.AddComponent<ContentSizeFitter>();
            ajuste.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return caixa;
        }

        /// <summary>Faixa de telhado com o titulo, a meio do bordo de cima da caixa.</summary>
        public static void CriarFaixa(RectTransform caixa, string titulo, Color cor)
        {
            // Largura = 96 (tampas) + multiplo de 64 (o padrao de telhas repete-se a cada 64), para o desenho emendar.
            float desejada = Mathf.Max(520f, titulo.Length * 40f + 144f);
            float largura = 96f + 64f * Mathf.Ceil((desejada - 96f) / 64f);

            RectTransform faixa = Novo("Faixa", caixa);
            faixa.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            faixa.anchorMin = faixa.anchorMax = new Vector2(0.5f, 1f);
            faixa.pivot = new Vector2(0.5f, 0.5f);
            faixa.anchoredPosition = Vector2.zero;
            faixa.sizeDelta = new Vector2(largura, 96f);
            AplicarSprite(faixa.gameObject.AddComponent<Image>(), "ui_faixa");

            RectTransform rTexto = Novo("Titulo", faixa);
            Esticar(rTexto);
            Text texto = rTexto.gameObject.AddComponent<Text>();
            Configurar(texto, titulo, FonteTitulo, 40, cor, TextAnchor.MiddleCenter);
            texto.horizontalOverflow = HorizontalWrapMode.Overflow;
            AdicionarSombra(texto, 6f);

            faixa.SetAsLastSibling();
        }

        /// <summary>Troco de estrada com risca amarela, para separar blocos.</summary>
        public static RectTransform CriarDivisor(Transform pai)
        {
            RectTransform rt = Novo("Divisor", pai);
            AplicarSprite(rt.gameObject.AddComponent<Image>(), "ui_divisor");
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;
            return rt;
        }

        /// <summary>Placa escura (asfalto) para resumos. Os filhos sao dispostos na vertical.</summary>
        public static RectTransform CriarPlaca(Transform pai, string nome, string sprite = "ui_placa_escura")
        {
            RectTransform rt = Novo(nome, pai);
            AplicarSprite(rt.gameObject.AddComponent<Image>(), sprite);

            VerticalLayoutGroup vertical = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            vertical.padding = new RectOffset(24, 24, 20, 20);
            vertical.spacing = 8f;
            vertical.childAlignment = TextAnchor.MiddleCenter;
            vertical.childControlWidth = true;
            vertical.childControlHeight = true;
            vertical.childForceExpandWidth = true;
            vertical.childForceExpandHeight = false;
            return rt;
        }

        /// <summary>Texto que cresce em altura conforme o conteudo (parte linhas se for preciso).</summary>
        public static Text CriarTexto(string nome, Transform pai, string conteudo, int tamanho, Color cor,
            bool titulo = false, TextAnchor alinhamento = TextAnchor.MiddleCenter, bool sombra = false)
        {
            RectTransform rt = Novo(nome, pai);
            Text texto = rt.gameObject.AddComponent<Text>();
            Configurar(texto, conteudo, titulo ? FonteTitulo : FonteCorpo, tamanho, cor, alinhamento);
            if (sombra) AdicionarSombra(texto, 4f);
            return texto;
        }

        /// <summary>Botao em pixel art. A altura normal e 96 (24 pixels de arte).</summary>
        public static Button CriarBotao(Transform pai, string rotulo, VarianteBotao variante, UnityAction accao, float altura = 96f)
        {
            RectTransform rt = Novo("Botao", pai);
            Image fundo = AplicarSprite(rt.gameObject.AddComponent<Image>(), "ui_botao_terra");

            Button botao = rt.gameObject.AddComponent<Button>();
            botao.targetGraphic = fundo;
            botao.transition = Selectable.Transition.SpriteSwap;
            DefinirVariante(botao, variante);
            if (accao != null) botao.onClick.AddListener(accao);

            LayoutElement le = rt.gameObject.AddComponent<LayoutElement>();
            le.preferredHeight = altura;
            le.minHeight = altura;

            RectTransform rTexto = Novo("Rotulo", rt);
            Esticar(rTexto);
            Text texto = rTexto.gameObject.AddComponent<Text>();
            Configurar(texto, rotulo, FonteCorpo, 24, Creme, TextAnchor.MiddleCenter);
            AdicionarSombra(texto, 4f);
            EstilarRotulo(botao, variante);
            return botao;
        }

        // Rotulo do botao: o dourado leva texto escuro; o resto leva texto claro.
        private static void EstilarRotulo(Button botao, VarianteBotao variante)
        {
            Transform filho = botao.transform.Find("Rotulo");
            if (filho == null) return;
            Text texto = filho.GetComponent<Text>();
            if (texto == null) return;
            Shadow sombra = texto.GetComponent<Shadow>();
            switch (variante)
            {
                case VarianteBotao.Terra:
                    texto.color = Contorno;
                    if (sombra != null) sombra.effectColor = new Color(0f, 0f, 0f, 0f);
                    break;
                case VarianteBotao.Inactivo:
                    texto.color = new Color32(125, 120, 140, 255);
                    if (sombra != null) sombra.effectColor = Contorno;
                    break;
                default:
                    texto.color = Creme;
                    if (sombra != null) sombra.effectColor = Contorno;
                    break;
            }
        }

        /// <summary>Muda o aspecto de um botao (cor e estados ao passar/clicar).</summary>
        public static void DefinirVariante(Button botao, VarianteBotao variante)
        {
            Image fundo = botao.targetGraphic as Image;
            if (fundo == null) return;

            string normal, sobre, premido;
            switch (variante)
            {
                case VarianteBotao.Asfalto:
                    normal = "ui_botao_asfalto"; sobre = "ui_botao_asfalto_sobre"; premido = "ui_botao_asfalto_premido";
                    break;
                case VarianteBotao.Inactivo:
                    normal = sobre = premido = "ui_botao_inactivo";
                    break;
                default:
                    normal = "ui_botao_terra"; sobre = "ui_botao_terra_sobre"; premido = "ui_botao_terra_premido";
                    break;
            }

            fundo.sprite = Sprite(normal);
            SpriteState estados = new SpriteState
            {
                highlightedSprite = Sprite(sobre),
                pressedSprite = Sprite(premido),
                selectedSprite = Sprite(normal),
                disabledSprite = Sprite("ui_botao_inactivo")
            };
            botao.spriteState = estados;
            EstilarRotulo(botao, variante);
        }

        // ---------------------------------------------------------------
        // Internos
        // ---------------------------------------------------------------

        private static void Configurar(Text texto, string conteudo, Font fonte, int tamanho, Color cor, TextAnchor alinhamento)
        {
            texto.font = fonte;
            texto.fontSize = tamanho;
            texto.fontStyle = FontStyle.Normal;
            texto.alignment = alinhamento;
            texto.color = cor;
            texto.raycastTarget = false;
            texto.horizontalOverflow = HorizontalWrapMode.Wrap;
            texto.verticalOverflow = VerticalWrapMode.Overflow;
            texto.text = conteudo;
        }

        /// <summary>Sombra de contorno deslocada, como nos sprites do jogo.</summary>
        public static void AdicionarSombra(Text texto, float distancia)
        {
            Shadow sombra = texto.gameObject.AddComponent<Shadow>();
            sombra.effectColor = Contorno;
            sombra.effectDistance = new Vector2(distancia, -distancia);
            sombra.useGraphicAlpha = false;
        }
    }
}
