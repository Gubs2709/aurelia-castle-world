Shader "Aurelia/Lake"
{
 Properties {_Shallow("Shallow",Color)=(.36,.88,.78,1) _Deep("Deep",Color)=(.07,.5,.66,1) _Sky("Sky reflection",Color)=(.62,.82,.98,1) _Speed("Speed",Float)=.5}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha
 Pass {Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
 struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float fog:TEXCOORD1;};
 CBUFFER_START(UnityPerMaterial)
 half4 _Shallow,_Deep,_Sky;float _Speed;
 CBUFFER_END
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float vn(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);float t=_Time.y*_Speed;o.w.y+=sin(o.w.x*.35+t*1.6)*.05+sin(o.w.z*.5-t*1.3)*.04;o.p=TransformWorldToHClip(o.w);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  float2 uv=GetNormalizedScreenSpaceUV(i.p);float t=_Time.y*_Speed;float2 q=i.w.xz;
  float h0=vn(q*.45+float2(t*.4,t*.25)),h1=vn(q*1.3+float2(-t*.6,t*.5)),h2=vn(q*3.4+float2(t,-t*.8));
  float e=.25;float hx=(vn((q+float2(e,0))*.45+float2(t*.4,t*.25))-h0)*.9+(vn((q+float2(e,0))*1.3+float2(-t*.6,t*.5))-h1)*.5;
  float hz=(vn((q+float2(0,e))*.45+float2(t*.4,t*.25))-h0)*.9+(vn((q+float2(0,e))*1.3+float2(-t*.6,t*.5))-h1)*.5;
  float3 n=normalize(float3(-hx*1.4,1,-hz*1.4));
  float3 view=normalize(_WorldSpaceCameraPos-i.w);
  float depth=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams)+TransformWorldToView(i.w).z;depth=max(0,depth);
  float k=1-exp(-depth*.28);
  half3 under=SampleSceneColor(uv+n.xz*.015*saturate(depth));
  half3 water=lerp(_Shallow.rgb,_Deep.rgb,saturate(k*1.15));
  half3 col=lerp(under,water,.35+k*.6);
  float fres=.06+.62*pow(1-saturate(dot(n,view)),4);
  half3 skyc=lerp(_Sky.rgb,half3(.9,.95,1),pow(saturate(n.y),8)*.3);
  col=lerp(col,skyc,fres);
  Light l=GetMainLight();
  float spec=pow(saturate(dot(normalize(view+l.direction),n)),90);col+=spec*l.color*.55;
  float sp=smoothstep(.93,1,vn(q*5+float2(t*2,-t*1.5)))*smoothstep(.2,.8,h1);col+=sp*half3(1,1,.92)*1.2;  // sparkles
  float foam=(1-smoothstep(.0,.7,depth))*smoothstep(.3,.8,h2+h1*.4);col=lerp(col,half3(.95,1,.98),foam*.85);
  float shore=1-smoothstep(.0,.25,depth);
  return half4(MixFog(col,i.fog),saturate(.78+k*.2-shore*.35));
 }
 ENDHLSL }
 }
}
