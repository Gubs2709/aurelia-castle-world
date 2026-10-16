Shader "Aurelia/Natural Foliage"
{
 Properties {_BaseMap("Leaf branch",2D)="white"{} _BaseColor("Tint",Color)=(1,1,1,1) _Cutoff("Cutoff",Range(0,1))=.45 _Wind("Wind",Float)=1 _Lift("Translucency",Float)=.35 _HueShift("Hue shift",Float)=0 _Saturation("Saturation",Float)=1}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" "RenderType"="TransparentCutout"} Cull Off
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #pragma multi_compile_instancing
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;float _Cutoff,_Wind,_Lift,_HueShift,_Saturation;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;float4 color:COLOR;UNITY_VERTEX_INPUT_INSTANCE_ID};
 struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float fog:TEXCOORD3;float4 color:COLOR;};
 V vert(A i){UNITY_SETUP_INSTANCE_ID(i);V o;o.w=TransformObjectToWorld(i.p.xyz);float sway=i.color.a;float t=_Time.y;o.w.x+=sin(t*1.3+o.w.x*.31+o.w.z*.27)*.07*sway*_Wind;o.w.z+=cos(t*1.1+o.w.z*.29+o.w.y*.2)*.05*sway*_Wind;o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);o.color=i.color;return o;}
 half4 frag(V i,bool front:SV_IsFrontFace):SV_Target
 {
  half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(c.a-_Cutoff);
  float3 n=normalize(i.n);Light l=GetMainLight(TransformWorldToShadowCoord(i.w));
  float ndl=dot(n,l.direction);float diffuse=saturate(ndl*.5+.5);diffuse*=diffuse;
  float3 view=normalize(_WorldSpaceCameraPos-i.w);float trans=pow(saturate(dot(view,-l.direction)),2.5)*_Lift;
  half3 albedo=c.rgb;
  float hi=max(albedo.r,max(albedo.g,albedo.b)),lo=min(albedo.r,min(albedo.g,albedo.b)),delta=hi-lo;
  float hue=0;if(delta>.0001){if(hi==albedo.r)hue=(albedo.g-albedo.b)/delta;else if(hi==albedo.g)hue=2+(albedo.b-albedo.r)/delta;else hue=4+(albedo.r-albedo.g)/delta;hue=frac(hue/6+_HueShift);}
  float sat=(hi>.0001?delta/hi:0)*_Saturation;float3 k=saturate(abs(frac(hue+float3(0,2.0/3,1.0/3))*6-3)-1);albedo=hi*lerp(float3(1,1,1),k,sat);
  albedo*=_BaseColor.rgb*i.color.rgb;
  half3 gi=SampleSH(n)*1.1;
  half3 lit=albedo*(gi+l.color*(diffuse*.95+trans)*lerp(.55,1,l.shadowAttenuation));
  return half4(MixFog(lit,i.fog),1);
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 Pass {Name "ShadowCaster" Tags {"LightMode"="ShadowCaster"} ColorMask 0 ZWrite On Cull Off HLSLPROGRAM
 #pragma vertex vs
 #pragma fragment fs
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;float _Cutoff,_Wind,_Lift,_HueShift,_Saturation;
 CBUFFER_END
 float3 _LightDirection;
 struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
 V vs(A i){V o;float3 w=TransformObjectToWorld(i.p.xyz);float3 n=TransformObjectToWorldNormal(i.n);o.p=TransformWorldToHClip(ApplyShadowBias(w,n,_LightDirection));o.uv=i.uv;return o;}
 half4 fs(V i):SV_Target{half4 c=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,i.uv);clip(c.a-_Cutoff);return 0;}
 ENDHLSL }
 }
}
