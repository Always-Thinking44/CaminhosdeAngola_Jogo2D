Shader "CaminhosDeAngola/NevoaCamada"
{
    Properties
    {
        _MainTex ("Textura", 2D) = "white" {}
        _Color ("Cor", Color) = (0.78, 0.80, 0.86, 1)
        _Opacidade ("Opacidade", Range(0, 1)) = 0.6
        _Tiling ("Repeticao (xy)", Vector) = (1, 1, 0, 0)
        _Velocidade ("Velocidade (xy)", Vector) = (-0.02, 0, 0, 0)
        _InicioDegrade ("Inicio do degrade na frente", Range(0, 1)) = 0.75
        _Contraste ("Contraste", Range(0.5, 3)) = 1.5
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            float _Opacidade;
            float4 _Tiling;
            float4 _Velocidade;
            float _InicioDegrade;
            float _Contraste;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // u: 0 = atras da nevoa, 1 = frente da nevoa.
                float2 uv = i.uv * _Tiling.xy + _Velocidade.xy * _Time.y;
                float lum = tex2D(_MainTex, uv).r;
                float densidade = saturate((lum - 0.5) * _Contraste + 0.5);

                // A frente dissolve-se; as pontas de cima e de baixo tambem.
                float frente = 1.0 - smoothstep(_InicioDegrade, 1.0, i.uv.x);
                float bordas = smoothstep(0.0, 0.12, i.uv.y) * smoothstep(1.0, 0.88, i.uv.y);

                fixed3 cor = _Color.rgb * lerp(0.55, 1.05, lum);
                float alfa = _Opacidade * lerp(0.45, 1.0, densidade) * frente * bordas;
                return fixed4(cor, alfa);
            }
            ENDCG
        }
    }
}
