Shader "Aurelia/RockTri"
{
 Properties {_Rock("Rock",2D)="white"{} _RockN("Rock N",2D)="bump"{} _Tint("Tint",Color)=(1,1,1,1) _Scale("Tiles per metre",Float)=.22 _Moss("Moss on top",Range(0,1))=.6 _MossColor("Moss",Color)=(.30,.46,.18,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_Rock);SAMPLER(sampler_Rock);TEXTURE2D(_RockN);SAMPLER(sampler_RockN);
 CBUFFER_START(UnityPerMaterial)
 half4 _Tint,_MossColor;float _Scale,_Moss;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;float4 color:COLOR;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;float4 color:COLOR;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);o.color=i.color;return o;}
 half4 frag(V i):SV_Target
 {
  float3 gn=normalize(i.n);float3 tw=pow(abs(gn),5);tw/=tw.x+tw.y+tw.z;float3 p=i.w*_Scale;
  half3 a=SAMPLE_TEXTURE2D(_Rock,sampler_Rock,p.zy).rgb*tw.x+SAMPLE_TEXTURE2D(_Rock,sampler_Rock,p.xz).rgb*tw.y+SAMPLE_TEXTURE2D(_Rock,sampler_Rock,p.xy).rgb*tw.z;
  float3 nx=UnpackNormalScale(SAMPLE_TEXTURE2D(_RockN,sampler_RockN,p.zy),1.2),ny=UnpackNormalScale(SAMPLE_TEXTURE2D(_RockN,sampler_RockN,p.xz),1.2),nz=UnpackNormalScale(SAMPLE_TEXTURE2D(_RockN,sampler_RockN,p.xy),1.2);
  float3 bump=float3(nx.z*0+ny.x*tw.y+nz.x*tw.z+0,0,ny.y*tw.y+nx.x*tw.x)*0.8;
  float3 n=normalize(gn+float3(bump.x,0,bump.z)*.7+float3(0,0,0));
  float mossMask=saturate((gn.y-.35)*2.2)*_Moss*(.5+.8*a.g);half3 albedo=lerp(a*_Tint.rgb,_MossColor.rgb*(.7+a.g*.7),saturate(mossMask));
  albedo*=lerp(.78,1.08,i.color.r);
  Light l=GetMainLight(TransformWorldToShadowCoord(i.w));float ndl=saturate(dot(n,l.direction));
  half3 lit=albedo*(SampleSH(n)*1.1+l.color*(ndl*.9+.06)*lerp(.4,1,l.shadowAttenuation));return half4(MixFog(lit,i.fog),1);
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
