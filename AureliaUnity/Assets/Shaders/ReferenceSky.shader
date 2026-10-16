Shader "Aurelia/Reference Sky"
{
 Properties {_MainTex("Painted sky",2D)="white"{}}
 SubShader { Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"} Cull Off ZWrite Off
 Pass { HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
 struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float3 d:TEXCOORD0;};
 V vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.d=i.p.xyz;return o;}
 half4 frag(V i):SV_Target {
 float2 screenUV=GetNormalizedScreenSpaceUV(i.p);
 #if UNITY_REVERSED_Z
 float depth=0;
 #else
 float depth=1;
 #endif
 float3 d=normalize(i.d);
 float elevation=saturate(d.y);half3 horizon=half3(.20,.61,.90),zenith=half3(.015,.19,.62);half3 sky=lerp(horizon,zenith,pow(elevation,.55));return half4(sky,1);}
 ENDHLSL }
 }
}
