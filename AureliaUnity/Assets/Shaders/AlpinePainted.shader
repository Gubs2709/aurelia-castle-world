Shader "Aurelia/Alpine Painted"
{
 Properties {_BaseColor("Tint",Color)=(1,1,1,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull Off
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;float4 c:COLOR;};
 struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float4 c:COLOR;float fog:TEXCOORD2;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.c=i.c;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  Light l=GetMainLight();float facing=saturate(dot(normalize(i.n),l.direction)*.5+.5);float band=lerp(facing,floor(facing*5)/5,.35);
  half3 albedo=SRGBToLinear(i.c.rgb)*_BaseColor.rgb;half3 col=albedo*(half3(.44,.55,.68)+l.color*(.22+band*.42));return half4(MixFog(col,i.fog),1);
 }
 ENDHLSL }
 }
}
