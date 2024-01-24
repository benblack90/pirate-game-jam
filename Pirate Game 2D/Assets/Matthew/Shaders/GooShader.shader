Shader"Unlit/GooShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex("Noise",2D) = "white"{}

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" }
        ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 data = tex2D(_MainTex, i.uv);
    float2 modifiedUV;
    float2 scrollingUV;
    
    float timeModifier = (fmod(_Time, 1) - 0.5f) * 2.0f;
    
    modifiedUV.g = fmod((i.uv.g * 2) + _Time * 0.25f, 1);
    modifiedUV.r = fmod((i.uv.r * 2) + _Time * 0.5f, 1);
    
    scrollingUV.g = abs(fmod((i.uv.g * 2) - _Time * 0.5f, 1));
    scrollingUV.r = abs(fmod((i.uv.r * 2) - _Time * 0.25f, 1));
    
    fixed4 noiseData = tex2D(_NoiseTex, modifiedUV);
    noiseData.a = noiseData.a + tex2D(_NoiseTex, scrollingUV).a;
   
    
    float4 coldTemp = { 0.4, 1, 1, 1 };
    float4 normalTemp = { 0.4, 0, 0.4, 1 };
    float4 hotTemp = { 1, 0.2, 0, 1 };
    float4 colour;
    
    data.y += noiseData.a* 0.1f;
    if (data.y <= 0.5f)
    {
        colour = lerp(coldTemp, normalTemp, (data.y * 2) * (data.y * 2));

    }
    else
    {
        colour = lerp(normalTemp, hotTemp, ((data.y - 0.5f) * 2) * ((data.y - 0.5f) * 2));

    }
    colour.a = ceil(data.x);
    if (data.x >= 2.0f)
        colour.a = 0.0f;
                return colour;
            }
            ENDCG
        }
    }
}
