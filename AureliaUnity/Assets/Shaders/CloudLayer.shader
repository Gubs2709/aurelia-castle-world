Shader "Aurelia/Drifting Cloud"
{
 Properties {_BaseMap("Cloud",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1)}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent-100" "RenderType"="Transparent"} Cull Off ZWrite Off
 Pass {Blend SrcAlpha OneMinusSrcAlpha HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;
 CBUFFER_END
 struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float fog:TEXCOORD1;};
 V vert(A i){V o;float3 p=i.p.xyz;p.x+=sin(_Time.y*.04+i.uv.y*5)*.002;o.p=TransformObjectToHClip(p);o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
 half4 frag(V i):SV_Target {half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv)*_BaseColor;float light=dot(c.rgb,half3(.24,.56,.20));half3 painted=lerp(half3(.30,.55,.77),half3(1,.99,.94),smoothstep(.10,.87,light));return half4(MixFog(painted,i.fog*.25),c.a);}
 ENDHLSL }
 }
}
