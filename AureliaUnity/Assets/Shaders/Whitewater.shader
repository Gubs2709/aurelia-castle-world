Shader "Aurelia/Living Whitewater"
{
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent+20"} Cull Off ZWrite Off
 Pass {Blend SrcAlpha OneMinusSrcAlpha HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float fog:TEXCOORD1;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target {float2 p=(i.uv-.5)*2;float r=length(p),a=atan2(p.y,p.x),t=_Time.y;float edge=1-smoothstep(.7,1,r);float swirl=sin(r*31-t*6+sin(a*9+t)*1.4)*sin(a*17+r*8+t);float froth=smoothstep(.04,.65,swirl)+pow(saturate(1-r),4)*.5;return half4(MixFog(half3(.91,.99,.97),i.fog),saturate(froth)*edge*.9);}
 ENDHLSL }
 }
}
