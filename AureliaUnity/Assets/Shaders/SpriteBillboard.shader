Shader "Aurelia/Sprite Billboard"
{
 Properties {_BaseMap("Sheet",2D)="white"{} _UV("Frame rect (x,y,w,h)",Vector)=(0,0,1,1) _Tint("Tint",Color)=(1,1,1,1) _Cutoff("Cutoff",Range(0,1))=.4}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _UV;half4 _Tint;float _Cutoff;
 CBUFFER_END
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float fog:TEXCOORD1;float3 w:TEXCOORD2;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.uv=_UV.xy+i.uv*_UV.zw;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(c.a-_Cutoff);
  Light l=GetMainLight();half3 lit=c.rgb*_Tint.rgb*(SampleSH(float3(0,1,0))*1.1+l.color*.62);
  return half4(MixFog(lit,i.fog),1);
 }
 ENDHLSL }
 Pass {Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ColorMask 0 ZWrite On Cull Off HLSLPROGRAM
 #pragma vertex vs
 #pragma fragment fs
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 float4 _UV;half4 _Tint;float _Cutoff;
 CBUFFER_END
 float3 _LightDirection;
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 V vs(A i){V o;float3 w=TransformObjectToWorld(i.p.xyz);float3 n=TransformObjectToWorldNormal(i.n);o.p=TransformWorldToHClip(ApplyShadowBias(w,n,_LightDirection));o.uv=_UV.xy+i.uv*_UV.zw;return o;}
 half4 fs(V i):SV_Target{half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(c.a-_Cutoff);return 0;}
 ENDHLSL }
 }
}
