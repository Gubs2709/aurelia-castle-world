Shader "Aurelia/Painted Sky"
{
 Properties { _SkyTint("Sky tint",Color)=(.48,.65,.8,1) _Exposure("Exposure",Float)=1 }
 SubShader
 {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
  Cull Off ZWrite Off
  Pass
  {
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A {float4 positionOS:POSITION;};
   struct V {float4 positionCS:SV_POSITION;float3 dir:TEXCOORD0;};
   float4 _SkyTint;float _Exposure;
   V vert(A i){V o;o.positionCS=TransformObjectToHClip(i.positionOS.xyz);o.dir=i.positionOS.xyz;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y);}
   float fbm(float2 p){float f=0,a=.5;for(int j=0;j<6;j++){f+=noise(p)*a;p=p*2.03+7.1;a*=.5;}return f;}
   half4 frag(V i):SV_Target
   {
    float3 d=normalize(i.dir);float h=saturate(d.y);
    float3 sky=lerp(float3(.60,.77,.82),float3(.10,.36,.64),pow(h,.45));
    sky*=lerp(float3(1,1,1),_SkyTint.rgb*1.6,.28);
    float2 p=d.xz/max(.12,d.y+.16)*2.5;
    float broad=fbm(p*.9);float detail=fbm(p*3.5);
    float density=smoothstep(.45,.66,broad*.76+detail*.24);
    density*=smoothstep(.015,.12,d.y);
    float shade=fbm(p+float2(.12,.08));
    float3 cloud=lerp(float3(.53,.66,.73),float3(1.02,1.0,.91),smoothstep(.35,.65,shade));
    float sun=pow(saturate(dot(d,normalize(float3(-.4,.5,.3)))),150);
    sky=lerp(sky,cloud,density)+sun*float3(1,.83,.55)*.5;
    return half4(sky*_Exposure,1);
   }
   ENDHLSL
  }
 }
}
