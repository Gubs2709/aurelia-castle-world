Shader "Aurelia/Reference River"
{
 Properties {_BaseColor("Deep water",Color)=(.035,.2,.19,1) _ShallowColor("Shallow water",Color)=(.24,.64,.53,1) _FlowSpeed("Flow speed (m/s)",Float)=3.2}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"} ZWrite Off
 Pass {Tags {"LightMode"="UniversalForward"}
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
 TEXTURE2D(_RiverReflection);SAMPLER(sampler_RiverReflection);
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float fog:TEXCOORD1;float2 uv:TEXCOORD2;};
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor,_ShallowColor;float _FlowSpeed;
 CBUFFER_END
 float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 float vnoise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
 // River mesh: uv.y runs upstream at 15 m per unit, uv.x spans the 54 m channel. Patterns advect toward decreasing uv.y.
 float flowHeight(float2 q,float t)
 {
  float2 a=float2(q.x*.55,q.y*.15+t*_FlowSpeed*.15);
  float2 b=float2(q.x*1.3+4.1,q.y*.33+t*_FlowSpeed*.33)+vnoise(a*2)*.6;
  return vnoise(a)*.55+vnoise(b)*.3+vnoise(float2(q.x*3.1,q.y*.9+t*_FlowSpeed*.9)+vnoise(b))*.15;
 }
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);float t=_Time.y;float along=i.uv.y*15,across=(i.uv.x-.5)*54;o.w.y+=sin(along*.6+t*_FlowSpeed*.6+across*.2)*.04+sin(across*.45-along*.2+t*1.1)*.03;o.uv=i.uv;o.p=TransformWorldToHClip(o.w);o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target
 {
 float2 uv=GetNormalizedScreenSpaceUV(i.p);float t=_Time.y;
 float2 q=float2((i.uv.x-.5)*54,i.uv.y*15);
 float centre=1-pow(abs(i.uv.x*2-1),2);
 float h=flowHeight(q,t),e=.35;
 float hx=flowHeight(q+float2(e,0),t)-h,hz=flowHeight(q+float2(0,e),t)-h;
 // river direction runs along world z at the mouth and curves; derive the local direction from the mesh uv gradient
 float3 n=normalize(float3(-hx*1.6,1,-hz*1.6));
 float3 view=normalize(_WorldSpaceCameraPos-i.w);float depth=LinearEyeDepth(SampleSceneDepth(uv),_ZBufferParams)+TransformWorldToView(i.w).z;depth=max(0,depth);
 float absorption=1-exp(-depth*.22);half3 transmitted=SampleSceneColor(uv+n.xz*.004*saturate(depth));half3 water=lerp(_ShallowColor.rgb,_BaseColor.rgb,absorption);half3 col=lerp(transmitted,water,.65+absorption*.30);
 float fres=.035+.26*pow(1-saturate(dot(n,view)),5);half3 reflected=SAMPLE_TEXTURE2D(_RiverReflection,sampler_RiverReflection,uv+n.xz*.012).rgb;col=lerp(col,reflected,fres);
 Light l=GetMainLight();col+=pow(saturate(dot(normalize(view+l.direction),n)),160)*l.color*.22;
 // moving bright crests and streaks of foam carried by the current
 float crest=smoothstep(.52,.72,flowHeight(q*float2(1.7,1.0)+float2(3,0),t));
 float streak=smoothstep(.66,.84,vnoise(float2(q.x*.9,q.y*.22+t*_FlowSpeed*.22+hx*3)));
 col=lerp(col,half3(.82,.95,.93),crest*(.04+centre*.06)+streak*centre*.07);
 float bank=smoothstep(.6,1,abs(i.uv.x*2-1));float bankFoam=bank*smoothstep(.35,.7,vnoise(float2(q.x*1.1,q.y*.5+t*_FlowSpeed*.5)));
 float edge=(1-smoothstep(.05,.7,depth));float noise=vnoise(float2(q.x*.9,q.y*.7+t*_FlowSpeed*.7));
 col=lerp(col,half3(.9,.97,.94),saturate(bankFoam*.75+edge*smoothstep(.35,.8,noise)*.65));
 return half4(MixFog(col,i.fog),1);
 }
 ENDHLSL }
 }
}
