using System;
using System.Collections.Generic;

namespace CaminhosDeAngola.Sons
{
    /// <summary>
    /// Gera por calculo a musica e os efeitos do jogo, com instrumentos
    /// inspirados na musica angolana: violao de semba, baixo, marimba,
    /// kissange (mbira), ngoma e conga (tambores) e chocalho.
    /// Nao usa UnityEngine, por isso pode correr numa thread de fundo.
    /// </summary>
    public static class SintetizadorAngola
    {
        public const int Taxa = 44100;

        // Parciais (multiplo da frequencia, amplitude, decaimento) de cada instrumento.
        private static readonly double[] ViolaoM = { 1, 2, 3, 4, 5 };
        private static readonly double[] ViolaoA = { 1, 0.5, 0.33, 0.2, 0.12 };
        private static readonly double[] ViolaoD = { 3.2, 4.5, 6.5, 8.5, 10.5 };

        private static readonly double[] BaixoM = { 1, 2, 3 };
        private static readonly double[] BaixoA = { 1, 0.45, 0.15 };
        private static readonly double[] BaixoD = { 2.6, 4.5, 7 };

        private static readonly double[] MarimbaM = { 1, 4, 10 };
        private static readonly double[] MarimbaA = { 1, 0.3, 0.08 };
        private static readonly double[] MarimbaD = { 5, 14, 30 };

        private static readonly double[] KissangeM = { 1, 2.9, 5.2 };
        private static readonly double[] KissangeA = { 1, 0.35, 0.18 };
        private static readonly double[] KissangeD = { 4, 9, 14 };

        // ------------------------------------------------------------------
        // Utilitarios
        // ------------------------------------------------------------------

        private static double Freq(int midi)
        {
            return 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);
        }

        private static void Somar(float[] destino, float[] nota, double inicio, float ganho, bool circular)
        {
            int ini = (int)Math.Round(inicio * Taxa);
            for (int i = 0; i < nota.Length; i++)
            {
                int idx = ini + i;
                if (circular) idx %= destino.Length;
                else if (idx >= destino.Length) break;
                destino[idx] += nota[i] * ganho;
            }
        }

        private static float[] Normalizar(float[] dados, float pico)
        {
            float max = 0f;
            for (int i = 0; i < dados.Length; i++)
            {
                float a = Math.Abs(dados[i]);
                if (a > max) max = a;
            }

            if (max > 0f)
            {
                float escala = pico / max;
                for (int i = 0; i < dados.Length; i++) dados[i] *= escala;
            }

            return dados;
        }

        // ------------------------------------------------------------------
        // Instrumentos
        // ------------------------------------------------------------------

        private static float[] Tom(double freq, double dur, double[] mult, double[] amp, double[] dec)
        {
            int n = (int)(dur * Taxa);
            var o = new float[n];
            int fade = Math.Min(n, (int)(0.015 * Taxa));

            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Taxa;
                double s = 0;
                for (int k = 0; k < mult.Length; k++)
                    s += amp[k] * Math.Sin(2 * Math.PI * freq * mult[k] * t) * Math.Exp(-dec[k] * t);

                double ataque = Math.Min(1.0, t / 0.004);
                double saida = i >= n - fade ? (double)(n - i) / fade : 1.0;
                o[i] = (float)(s * ataque * saida);
            }

            return o;
        }

        private static float[] Violao(int midi, double dur) { return Tom(Freq(midi), dur, ViolaoM, ViolaoA, ViolaoD); }
        private static float[] Baixo(int midi, double dur) { return Tom(Freq(midi), dur, BaixoM, BaixoA, BaixoD); }
        private static float[] Marimba(int midi, double dur) { return Tom(Freq(midi), dur, MarimbaM, MarimbaA, MarimbaD); }
        private static float[] Kissange(int midi, double dur) { return Tom(Freq(midi), dur, KissangeM, KissangeA, KissangeD); }

        /// <summary>Tambor (ngoma / conga): seno que desce de f0 para f1 com um estalo de ruido.</summary>
        private static float[] Tambor(double f0, double f1, double dur, double dec, double ruido, int seed)
        {
            int n = (int)(dur * Taxa);
            var o = new float[n];
            var rnd = new Random(seed);
            double fase = 0;
            int fade = Math.Min(n, (int)(0.01 * Taxa));

            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Taxa;
                double f = f1 + (f0 - f1) * Math.Exp(-t * 30);
                fase += 2 * Math.PI * f / Taxa;
                double s = Math.Sin(fase) * Math.Exp(-dec * t);
                double r = (rnd.NextDouble() * 2 - 1) * ruido * Math.Exp(-t * 60);
                double saida = i >= n - fade ? (double)(n - i) / fade : 1.0;
                o[i] = (float)((s + r) * saida);
            }

            return o;
        }

        /// <summary>Chocalho (shaker): ruido de agudos com envolvente curta.</summary>
        private static float[] Chocalho(double dur, double dec, int seed)
        {
            int n = (int)(dur * Taxa);
            var o = new float[n];
            var rnd = new Random(seed);
            double ant = 0;

            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Taxa;
                double x = rnd.NextDouble() * 2 - 1;
                double y = x - ant;
                ant = x;
                double env = Math.Min(1.0, t / 0.003) * Math.Exp(-dec * t);
                o[i] = (float)(y * 0.5 * env);
            }

            return o;
        }

        private static float[] Deslize(double f0, double f1, double dur, double dec)
        {
            int n = (int)(dur * Taxa);
            var o = new float[n];
            double fase = 0;

            for (int i = 0; i < n; i++)
            {
                double t = (double)i / Taxa;
                double f = f0 + (f1 - f0) * (t / dur);
                fase += 2 * Math.PI * f / Taxa;
                o[i] = (float)(Math.Sin(fase) * Math.Min(1.0, t / 0.003) * Math.Exp(-dec * t));
            }

            return o;
        }

        // ------------------------------------------------------------------
        // Musica (loop de 8 compassos, sem emenda)
        // ------------------------------------------------------------------

        /// <summary>
        /// Loop em la menor com sabor a semba (progressao Am - Dm - E - Am).
        /// completa = true: versao de jogo, com percussao. false: versao calma do menu.
        /// </summary>
        public static float[] Musica(bool completa)
        {
            double bpm = completa ? 104.0 : 90.0;
            double passo = 60.0 / bpm / 4.0; // semicolcheia
            const int compassos = 8;
            double total = compassos * 16 * passo;
            var mix = new float[(int)Math.Round(total * Taxa)];

            int[] graus = { 0, 1, 2, 0, 0, 1, 2, 0 };
            int[] baixos = { 45, 50, 40 };
            int[][] acordes =
            {
                new[] { 57, 60, 64, 69 },
                new[] { 57, 62, 65, 69 },
                new[] { 56, 59, 64, 68 }
            };

            // Padrao do violao de semba (arpejo sincopado) na versao completa.
            int[] gPassos = { 0, 3, 6, 8, 10, 12, 14 };
            int[] gIdx = { 0, 2, 1, 3, 2, 1, 2 };
            float[] gGanho = { 0.34f, 0.24f, 0.26f, 0.28f, 0.24f, 0.26f, 0.24f };

            (int c, int s, int m)[] melodia =
            {
                (0, 0, 76), (0, 6, 74), (0, 8, 72), (0, 12, 74),
                (1, 0, 69), (1, 6, 72), (1, 8, 74), (1, 12, 72),
                (2, 0, 71), (2, 4, 74), (2, 8, 76), (2, 12, 71),
                (3, 0, 69), (3, 6, 72), (3, 10, 76),
                (4, 0, 76), (4, 6, 79), (4, 8, 76), (4, 12, 74),
                (5, 0, 72), (5, 6, 74), (5, 8, 77), (5, 12, 74),
                (6, 0, 71), (6, 6, 74), (6, 8, 76), (6, 12, 80),
                (7, 0, 81), (7, 8, 76)
            };

            var violoes = new Dictionary<int, float[]>();
            var baixosCache = new Dictionary<string, float[]>();
            var marimbas = new Dictionary<int, float[]>();

            float[] V(int m)
            {
                if (!violoes.TryGetValue(m, out var x)) { x = Violao(m, 0.8); violoes[m] = x; }
                return x;
            }

            float[] B(int m, double d)
            {
                string chave = m + "_" + d;
                if (!baixosCache.TryGetValue(chave, out var x)) { x = Baixo(m, d); baixosCache[chave] = x; }
                return x;
            }

            float[] M(int m)
            {
                if (!marimbas.TryGetValue(m, out var x)) { x = Marimba(m, 1.1); marimbas[m] = x; }
                return x;
            }

            float[] ngoma = Tambor(130, 62, 0.45, 8, 0.15, 1);
            float[] congaAberta = Tambor(330, 220, 0.30, 12, 0.2, 2);
            float[] congaSlap = Tambor(520, 380, 0.12, 28, 0.5, 3);
            float[] chocalho = Chocalho(0.07, 45, 4);

            for (int c = 0; c < compassos; c++)
            {
                int g = graus[c];
                int[] ac = acordes[g];
                int baixo = baixos[g];

                for (int s = 0; s < 16; s++)
                {
                    double t = (c * 16 + s) * passo;

                    // Violao
                    if (completa)
                    {
                        int k = Array.IndexOf(gPassos, s);
                        if (k >= 0) Somar(mix, V(ac[gIdx[k]]), t, gGanho[k], true);
                    }
                    else if (s % 4 == 0)
                    {
                        Somar(mix, V(ac[s / 4]), t, 0.26f, true);
                    }

                    // Baixo
                    if (completa)
                    {
                        if (s == 0) Somar(mix, B(baixo, 0.7), t, 0.55f, true);
                        else if (s == 6) Somar(mix, B(baixo, 0.4), t, 0.40f, true);
                        else if (s == 10) Somar(mix, B(baixo + 7, 0.4), t, 0.40f, true);
                    }
                    else
                    {
                        if (s == 0) Somar(mix, B(baixo, 1.0), t, 0.40f, true);
                        else if (s == 8) Somar(mix, B(baixo + 7, 0.9), t, 0.30f, true);
                    }

                    // Percussao
                    if (completa)
                    {
                        if (s == 0) Somar(mix, ngoma, t, 0.50f, true);
                        else if (s == 8) Somar(mix, ngoma, t, 0.35f, true);
                        else if (s == 11) Somar(mix, ngoma, t, 0.28f, true);

                        if (s == 6 || s == 14) Somar(mix, congaAberta, t, 0.26f, true);
                        if (s == 3 || s == 10) Somar(mix, congaSlap, t, 0.16f, true);
                        if (s == 15) Somar(mix, congaSlap, t, 0.12f, true);

                        if (s % 2 == 0) Somar(mix, chocalho, t, s % 4 == 2 ? 0.16f : 0.09f, true);
                    }
                    else if (s % 4 == 2)
                    {
                        Somar(mix, chocalho, t, 0.06f, true);
                    }
                }
            }

            // Melodia de marimba
            float ganhoMelodia = completa ? 0.26f : 0.22f;
            foreach (var nota in melodia)
                Somar(mix, M(nota.m), (nota.c * 16 + nota.s) * passo, ganhoMelodia, true);

            return Normalizar(mix, 0.85f);
        }

        // ------------------------------------------------------------------
        // Efeitos
        // ------------------------------------------------------------------

        public static float[] Efeito(Som som)
        {
            switch (som)
            {
                case Som.Zimbo:
                {
                    var o = new float[(int)(0.5 * Taxa)];
                    Somar(o, Kissange(83, 0.4), 0.0, 0.6f, false);
                    Somar(o, Kissange(90, 0.4), 0.07, 0.6f, false);
                    return Normalizar(o, 0.8f);
                }

                case Som.ItemHistorico:
                {
                    var o = new float[(int)(1.9 * Taxa)];
                    int[] notas = { 69, 72, 76, 81 };
                    for (int i = 0; i < notas.Length; i++)
                        Somar(o, Marimba(notas[i], 1.0), i * 0.11, 0.5f, false);
                    Somar(o, Tambor(130, 62, 0.45, 8, 0.15, 7), 0.0, 0.6f, false);
                    Somar(o, Kissange(93, 1.2), 0.45, 0.35f, false);
                    return Normalizar(o, 0.85f);
                }

                case Som.Pensador:
                {
                    var o = new float[(int)(2.0 * Taxa)];
                    int[] acorde = { 57, 64, 69, 72 };
                    for (int i = 0; i < acorde.Length; i++)
                        Somar(o, Violao(acorde[i], 1.4), i * 0.035, 0.35f, false);
                    Somar(o, Marimba(76, 1.3), 0.30, 0.35f, false);
                    Somar(o, Kissange(88, 1.3), 0.55, 0.30f, false);
                    return Normalizar(o, 0.8f);
                }

                case Som.Dano:
                    return Normalizar(Tambor(160, 45, 0.4, 9, 0.5, 11), 0.9f);

                case Som.Salto:
                    return Normalizar(Deslize(260, 520, 0.13, 14), 0.4f);

                case Som.Clique:
                    return Normalizar(Marimba(76, 0.2), 0.6f);

                case Som.Zungueira:
                {
                    var o = new float[(int)(0.8 * Taxa)];
                    Somar(o, Kissange(76, 0.5), 0.00, 0.6f, false);
                    Somar(o, Kissange(83, 0.5), 0.09, 0.6f, false);
                    Somar(o, Kissange(88, 0.5), 0.18, 0.6f, false);
                    return Normalizar(o, 0.8f);
                }

                default:
                    return new float[1];
            }
        }
    }
}
