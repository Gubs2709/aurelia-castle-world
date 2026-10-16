Shader "Aurelia/Anime Ocean"
{
 Properties {_Deep("Deep sea",Color)=(.025,.19,.3,1) _Shallow("Jade water",Color)=(.08,.58,.58,1) _Crest("Sunlit crests",Color)=(.65,.93,.94,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Deep,_Shallow,_Crest;
 CBUFFER_END
 struct A {float4 p:POSITION;};struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float fog:TEXCOORD1;};
 float wave(float2 p,float t){return sin(p.x*.048+p.y*.031+t*.8)*.15+sin(p.x*.086-p.y*.067-t*1.1)*.085+sin(p.x*.25+p.y*.19+t*1.8)*.025;}
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.w.y+=wave(o.w.xz,_Time.y);o.p=TransformWorldToHClip(o.w);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  float t=_Time.y;float2 p=i.w.xz;float h=wave(p,t);float e=.35;
  float3 n=normalize(float3((h-wave(p+float2(e,0),t))/e,1,(h-wave(p+float2(0,e),t))/e));
  float3 view=normalize(_WorldSpaceCameraPos-i.w);float2 uv=GetNormalizedScreenSpaceUV(i.p);float dep=max(0,LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams)+TransformWorldToView(i.w).z);
  float shallow=exp(-dep*.10);half3 col=lerp(_Deep.rgb,_Shallow.rgb,shallow*.95);
  float fres=.04+.28*pow(1-saturate(dot(n,view)),4);col=lerp(col,half3(.16,.59,.76),fres);
  Light l=GetMainLight();float spec=pow(saturate(dot(n,normalize(l.direction+view))),180);col+=spec*l.color*.12;
  float crest=smoothstep(.86,.99,sin(p.x*.24+p.y*.18+t*1.4)*sin(p.y*.17-p.x*.07-t*.7));
  col=lerp(col,_Crest.rgb,crest*.16);
  float glint=pow(saturate(sin(p.x*2.19+p.y*.93+t*.8)*sin(p.x*.81-p.y*1.73-t*.63)),32);col+=glint*half3(.42,.72,.75)*.35;
  float shore=(1-smoothstep(.1,2.0,dep))*(.6+.4*sin(p.x*1.7+p.y*2.8-t*3));col=lerp(col,_Crest.rgb,shore*.8);
  return half4(MixFog(col,i.fog),.97);
 }
 ENDHLSL }
 }
}
