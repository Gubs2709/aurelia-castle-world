Shader "Aurelia/Anime Character"
{
 Properties {[MainColor] _BaseColor("Colour",Color)=(1,1,1,1) [MainTexture] _BaseMap("Surface",2D)="white"{} _Smoothness("Smoothness",Float)=.2 _Metallic("Metal",Float)=0 _EmissionColor("Glow",Color)=(0,0,0,0) _Cull("Cull",Float)=0 _Cutoff("Cutoff",Float)=.5}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull [_Cull]
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor,_EmissionColor;float _Smoothness,_Metallic,_Cull,_Cutoff;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;};struct V {float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
  float3 n=normalize(i.n);Light l=GetMainLight(TransformWorldToShadowCoord(i.w));float d=dot(n,l.direction);float band=lerp(.64,.93,smoothstep(-.12,.12,d));band=lerp(band,1.12,smoothstep(.58,.68,d));
  float3 view=normalize(_WorldSpaceCameraPos-i.w);float spec=pow(saturate(dot(n,normalize(view+l.direction))),60)*_Metallic*.15;
  float rim=pow(1-saturate(dot(n,view)),3)*.035;
  half3 c=_BaseColor.rgb*band*lerp(.86,1,l.shadowAttenuation)+spec*l.color+rim*half3(.55,.72,.8)+_EmissionColor.rgb;
  return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
