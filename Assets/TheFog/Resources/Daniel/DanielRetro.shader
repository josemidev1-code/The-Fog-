Shader "Hidden/TheFog/DanielRetro"
{
    Properties { _MainTex ("Scene", 2D) = "white" {} }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            half4 frag(v2f_img input) : SV_Target
            {
                float height = min(360.0, _MainTex_TexelSize.w);
                float2 grid = float2(round(height * _MainTex_TexelSize.z / _MainTex_TexelSize.w), height);
                float2 pixel = floor(saturate(input.uv) * grid);
                half3 color = tex2D(_MainTex, (pixel + 0.5) / grid).rgb;
                float dither = (fmod(pixel.x + 2.0 * fmod(pixel.y, 2.0), 4.0) - 1.5) / 128.0;
                color = floor(max(color + dither, 0) * 39.0 + 0.5) / 39.0;
                return half4(color, 1);
            }
            ENDCG
        }
    }
}
