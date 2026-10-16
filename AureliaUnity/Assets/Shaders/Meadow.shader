Shader "Aurelia/Meadow"
{
    Properties { _BaseColor("Tint", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float4 color:COLOR; float fog:TEXCOORD2; };
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            V vert(A i) { V o; o.world=TransformObjectToWorld(i.positionOS.xyz); o.positionCS=TransformWorldToHClip(o.world); o.normal=TransformObjectToWorldNormal(i.normalOS); o.color=i.color; o.fog=ComputeFogFactor(o.positionCS.z); return o; }
            half4 frag(V i):SV_Target
            {
                Light l=GetMainLight(TransformWorldToShadowCoord(i.world));
                float diff=saturate(dot(normalize(i.normal),l.direction))*.75+.25;
                float noise=frac(sin(dot(floor(i.world.xz*14),float2(12.9898,78.233)))*43758.54);
                half3 albedo=SRGBToLinear(i.color.rgb)*_BaseColor.rgb*(.96+noise*.08);
                half3 col=albedo*(SampleSH(i.normal)*.38+l.color*diff*.56*lerp(.42,1,l.shadowAttenuation));
                return half4(MixFog(col,i.fog),1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
