Shader "Aurelia/Reference Waterfall"
{
 Properties {_BaseColor("Water",Color)=(.59,.87,.87,1) _Phase("Phase",Float)=0}
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   Cull Off ZWrite Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float fog:TEXCOORD1;};
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float _Phase;
   CBUFFER_END
   V vert(A i){V o;float3 p=i.p.xyz;p.z+=sin(i.uv.y*11+_Time.y*2+_Phase)*.13;o.p=TransformObjectToHClip(p);o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
   half4 frag(V i):SV_Target
   {float2 uv=i.uv;float t=_Time.y;float broad=noise(float2(uv.x*17+_Phase,uv.y*3+t*1.4));float fine=noise(float2(uv.x*85+broad,uv.y*12+t*7));float flow=broad*.65+fine*.35;float edge=smoothstep(0,.07+broad*.05,uv.x)*smoothstep(0,.07+fine*.04,1-uv.x);float foam=pow(1-uv.y,4);half3 col=lerp(half3(.43,.7,.72),half3(.93,.98,.94),saturate(.38+flow*.6+foam*.2));return half4(MixFog(col,i.fog),edge*(.73+flow*.25));}
   ENDHLSL
  }
 }
}
