Shader "Aurelia/River"
{
 Properties { _BaseColor("Water",Color)=(.08,.39,.40,1) }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A{float4 positionOS:POSITION;};struct V{float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;float fog:TEXCOORD1;};
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   V vert(A i){V o;o.world=TransformObjectToWorld(i.positionOS.xyz);o.world.y+=sin(o.world.x*.5+_Time.y)*.035;o.positionCS=TransformWorldToHClip(o.world);o.fog=ComputeFogFactor(o.positionCS.z);return o;}
   half4 frag(V i):SV_Target
   {float t=_Time.y;float x=i.world.x,z=i.world.z;float wave=sin(x*1.3+z*2.7+t*1.7)*.15+sin(x*3.7-z*1.1-t)*.08;float3 n=normalize(float3(wave,1,cos(z*3+x+t)*.18));float3 view=normalize(_WorldSpaceCameraPos-i.world);float fres=pow(1-saturate(dot(n,view)),4);Light light=GetMainLight();float spec=pow(saturate(dot(normalize(view+light.direction),n)),170);float foam=smoothstep(.95,1,sin(x*7+sin(z*3+t)*2+t)*sin(z*6-t));half3 col=lerp(_BaseColor.rgb,float3(.53,.74,.78),fres*.8)+spec*light.color*.8+foam*.15;return half4(MixFog(col,i.fog),1);}
   ENDHLSL
  }
 }
}
