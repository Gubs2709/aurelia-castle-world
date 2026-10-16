Shader "Aurelia/Reference Masonry"
{
 Properties {_BaseColor("Limestone",Color)=(.8,.84,.8,1)}
 SubShader { Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A{float4 p:POSITION;float3 n:NORMAL;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;};
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;
 CBUFFER_END
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
 float3 n=normalize(i.n);float horizontal=abs(n.x)>abs(n.z)?i.w.z:i.w.x;float row=floor(i.w.y/1.05);float2 p=float2(horizontal/2.4+frac(row*.5),i.w.y/1.05);float2 cell=frac(p);float2 aa=max(fwidth(p),.002);float lineX=1-smoothstep(.005,.005+aa.x, min(cell.x,1-cell.x));float lineY=1-smoothstep(.008,.008+aa.y,min(cell.y,1-cell.y));float mortar=max(lineX,lineY)*saturate(1-length(fwidth(p))*.8);float variation=hash(floor(p));float stain=sin(i.w.y*.12+sin(horizontal*.16)*2)*sin(horizontal*.25)*.035;
 half3 albedo=_BaseColor.rgb*(.94+variation*.09+stain);albedo*=1-mortar*.12;
 Light l=GetMainLight(TransformWorldToShadowCoord(i.w));float ndl=saturate(dot(n,l.direction));half3 lit=albedo*(half3(.29,.38,.43)+l.color*(.28+ndl*.65)*lerp(.54,1,l.shadowAttenuation));return half4(MixFog(lit,i.fog),1);
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
