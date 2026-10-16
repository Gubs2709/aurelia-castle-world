Shader "Aurelia/Leaded Glass"
{
 Properties {_BaseColor("Glass tint",Color)=(.18,.31,.34,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float fog:TEXCOORD3;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target {float3 n=normalize(i.n),view=normalize(_WorldSpaceCameraPos-i.w),r=reflect(-view,n);float fres=pow(1-saturate(abs(dot(n,view))),4);float2 grid=float2(i.uv.x*5+i.uv.y*8,i.uv.x*5-i.uv.y*8);float2 f=abs(frac(grid)-.5);float lead=1-smoothstep(.45,.48,max(f.x,f.y));float3 sky=lerp(float3(.18,.35,.4),float3(.5,.69,.8),saturate(r.y*.7+.5));float room=smoothstep(.05,.2,i.uv.y)*(1-smoothstep(.7,.9,i.uv.y));half3 col=lerp(_BaseColor.rgb*(.18+room*.08),sky,.15+fres*.3);col*=lerp(.2,1,lead);Light l=GetMainLight();col+=pow(saturate(dot(reflect(-l.direction,n),view)),96)*.5*l.color;return half4(MixFog(col,i.fog),1);}
 ENDHLSL }
 }
}
