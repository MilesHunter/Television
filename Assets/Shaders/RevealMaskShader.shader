Shader "Custom/RevealMaskShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "black" {}
        _RevealColor ("Reveal Color", Color) = (1,1,1,1)
        _HiddenColor ("Hidden Color", Color) = (0,0,0,0)
        _MaskThreshold ("Mask Threshold", Range(0,1)) = 0.1
        _SmoothEdge ("Smooth Edge", Range(0,0.1)) = 0.02
        _RevealIntensity ("Reveal Intensity", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 maskUV : TEXCOORD1;
                UNITY_FOG_COORDS(2)
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float4 _MaskTex_ST;
            fixed4 _RevealColor;
            fixed4 _HiddenColor;
            float _MaskThreshold;
            float _SmoothEdge;
            float _RevealIntensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.maskUV = TRANSFORM_TEX(v.uv, _MaskTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 mainColor = tex2D(_MainTex, i.uv);

                // Sample the mask texture
                fixed4 maskColor = tex2D(_MaskTex, i.maskUV);

                // Use red channel of mask for reveal amount
                float maskValue = maskColor.r;

                // Apply threshold with smooth edges
                float revealAmount = smoothstep(_MaskThreshold - _SmoothEdge,
                                              _MaskThreshold + _SmoothEdge,
                                              maskValue);

                // Blend between hidden and revealed states
                fixed4 finalColor = lerp(_HiddenColor, mainColor * _RevealColor, revealAmount);

                // Apply vertex color and reveal intensity
                finalColor *= i.color;
                finalColor.rgb *= _RevealIntensity;

                // Apply fog
                UNITY_APPLY_FOG(i.fogCoord, finalColor);

                return finalColor;
            }
            ENDCG
        }
    }

    // Fallback for older hardware
    Fallback "Sprites/Default"
}