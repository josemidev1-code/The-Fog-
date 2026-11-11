Shader "Hidden/TheFog/DanielRetro"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                float height = min(360.0, _BlitTexture_TexelSize.w);
                float2 grid = float2(round(height * _BlitTexture_TexelSize.z / _BlitTexture_TexelSize.w), height);
                float2 pixel = floor(saturate(input.texcoord) * grid);
                half3 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_PointClamp, (pixel + 0.5) / grid).rgb;
                float dither = (fmod(pixel.x + 2.0 * fmod(pixel.y, 2.0), 4.0) - 1.5) / 128.0;
                color = floor(max(color + dither, 0) * 39.0 + 0.5) / 39.0;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
