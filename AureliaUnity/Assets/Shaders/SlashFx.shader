Shader "Aurelia/Slash Fx"
{
 Properties {_BaseMap("Slash",2D)="white"{} _Tint("Tint",Color)=(.8,.95,1,1) _Fade("Fade",Range(0,1))=1}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+10" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha One
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint;float _Fade;
 CBUFFER_END
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;return o;}
 half4 frag(V i):SV_Target{half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);return half4(c.rgb*_Tint.rgb*1.6,c.a*_Fade);}
 ENDHLSL }
 }
}
