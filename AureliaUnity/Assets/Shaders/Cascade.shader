Shader "Aurelia/Cascade"
{
 Properties {_BaseColor("Water",Color)=(.38,.75,.8,1) _Width("Width m",Float)=10 _Height("Height m",Float)=60 _Seed("Seed",Float)=0}
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent+5"}
  Pass
  {
   Blend SrcAlpha OneMinusSrcAlpha
   Cull Off ZWrite Off
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A{float4 p:POSITION;float2 uv:TEXCOORD0;};struct V{float4 p:SV_POSITION;float2 uv:TEXCOORD0;float3 w:TEXCOORD1;float fog:TEXCOORD2;};
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float _Width,_Height,_Seed;
   CBUFFER_END
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   float vnoise(float2 p){float2 a=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(a),hash(a+float2(1,0)),f.x),lerp(hash(a+float2(0,1)),hash(a+1),f.x),f.y);}
   // uv.x across (0..1), uv.y 0 at the pool and 1 at the lip. Streaks fall toward decreasing uv.y.
   V vert(A i){V o;float3 p=i.p.xyz;float Y=i.uv.y*_Height;float ripple=sin(Y*.35-_Time.y*6+_Seed)*.18+sin(Y*.9-_Time.y*9+i.uv.x*7)*.07;float3 wp=TransformObjectToWorld(p);wp.z-=ripple*(0.4+i.uv.y);o.w=wp;o.p=TransformWorldToHClip(wp);o.uv=i.uv;o.fog=ComputeFogFactor(o.p.z);return o;}
   half4 frag(V i):SV_Target
   {
    float t=_Time.y;float X=i.uv.x*_Width,Y=i.uv.y*_Height;
    float broad=vnoise(float2(X*.55+_Seed,Y*.05+t*3.2));
    float s1=vnoise(float2(X*1.9+_Seed*3,Y*.16+t*7.5));
    float s2=vnoise(float2(X*4.6+9,Y*.32+t*12));
    float s3=vnoise(float2(X*9.5,Y*.7+t*17+s1*2));
    float streak=broad*.28+s1*.34+s2*.26+s3*.12;
    float bright=smoothstep(.34,.82,streak);
    half3 deep=half3(.30,.66,.74),white=half3(.97,1,.99);
    half3 col=lerp(deep,white,bright*.95+.06);
    float lip=smoothstep(_Height-3.5,_Height-.3,Y);col=lerp(col,white,lip*.6);
    float base=1-smoothstep(0,min(10,_Height*.18),Y);float baseFoam=base*(.55+.45*vnoise(float2(X*1.3,t*5+Y*.8)));col=lerp(col,white,saturate(baseFoam));
    float side=smoothstep(0,.07+.05*broad,i.uv.x)*smoothstep(0,.07+.05*s1,1-i.uv.x);
    float alpha=side*saturate(.5+bright*.5+base*.4+lip*.3);
    Light l=GetMainLight();col*=.78+l.color.r*.28;
    return half4(MixFog(col,i.fog),alpha*.96);
   }
   ENDHLSL
  }
 }
}
