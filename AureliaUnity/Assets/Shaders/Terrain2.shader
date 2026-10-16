Shader "Aurelia/Terrain2"
{
 Properties
 {
  _Splat("Splat RGBA (grass, litter, dirt, flagstone)",2D)="red"{}
  _Grass("Grass",2D)="white"{} _GrassN("Grass N",2D)="bump"{}
  _Litter("Leaf litter",2D)="white"{} _LitterN("Litter N",2D)="bump"{}
  _Dirt("Dirt",2D)="white"{} _DirtN("Dirt N",2D)="bump"{}
  _Flag("Flagstone",2D)="white"{} _FlagN("Flag N",2D)="bump"{}
  _Rock("Rock",2D)="white"{} _RockN("Rock N",2D)="bump"{}
  _SplatRect("Splat world rect (x0,z0,sizeX,sizeZ)",Vector)=(-450,-350,900,900)
 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
   #pragma multi_compile_fragment _ _SHADOWS_SOFT
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   TEXTURE2D(_Splat);SAMPLER(sampler_Splat);
   TEXTURE2D(_Grass);SAMPLER(sampler_Grass);TEXTURE2D(_GrassN);SAMPLER(sampler_GrassN);
   TEXTURE2D(_Litter);SAMPLER(sampler_Litter);TEXTURE2D(_LitterN);SAMPLER(sampler_LitterN);
   TEXTURE2D(_Dirt);SAMPLER(sampler_Dirt);TEXTURE2D(_DirtN);SAMPLER(sampler_DirtN);
   TEXTURE2D(_Flag);SAMPLER(sampler_Flag);TEXTURE2D(_FlagN);SAMPLER(sampler_FlagN);
   TEXTURE2D(_Rock);SAMPLER(sampler_Rock);TEXTURE2D(_RockN);SAMPLER(sampler_RockN);
   CBUFFER_START(UnityPerMaterial)
   float4 _SplatRect;
   CBUFFER_END
   struct A{float4 p:POSITION;float3 n:NORMAL;float4 color:COLOR;};
   struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;float4 color:COLOR;};
   V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);o.color=i.color;return o;}
   float3 UnpackN(float4 t){return UnpackNormalScale(t,1.0);}
   half4 frag(V i):SV_Target
   {
    float3 gn=normalize(i.n);float2 xz=i.w.xz;
    float2 suv=(xz-_SplatRect.xy)/_SplatRect.zw;float4 sp=SAMPLE_TEXTURE2D(_Splat,sampler_Splat,suv);
    // macro variation to hide tiling: two scales multiplied
    float2 g1=xz*.22,g2=xz*.031+float2(.37,.61);
    half3 grass=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,g1).rgb*(.55+.9*SAMPLE_TEXTURE2D(_Grass,sampler_Grass,g2).rgb);
    float3 grassN=UnpackN(SAMPLE_TEXTURE2D(_GrassN,sampler_GrassN,g1));
    float2 l1=xz*.17;half3 litter=SAMPLE_TEXTURE2D(_Litter,sampler_Litter,l1).rgb*(.7+.5*SAMPLE_TEXTURE2D(_Litter,sampler_Litter,xz*.029).rgb);float3 litterN=UnpackN(SAMPLE_TEXTURE2D(_LitterN,sampler_LitterN,l1));
    float2 d1=xz*.2;half3 dirt=SAMPLE_TEXTURE2D(_Dirt,sampler_Dirt,d1).rgb*(.8+.4*SAMPLE_TEXTURE2D(_Dirt,sampler_Dirt,xz*.043).rgb);float3 dirtN=UnpackN(SAMPLE_TEXTURE2D(_DirtN,sampler_DirtN,d1));
    float2 f1=xz*.28;half3 flag=SAMPLE_TEXTURE2D(_Flag,sampler_Flag,f1).rgb;float3 flagN=UnpackN(SAMPLE_TEXTURE2D(_FlagN,sampler_FlagN,f1));
    // slope rock, triplanar
    float rockW=smoothstep(.78,.55,gn.y);
    float3 tw=pow(abs(gn),4);tw/=tw.x+tw.y+tw.z;float3 rp=i.w*.11;
    half3 rock=SAMPLE_TEXTURE2D(_Rock,sampler_Rock,rp.zy).rgb*tw.x+SAMPLE_TEXTURE2D(_Rock,sampler_Rock,rp.xz).rgb*tw.y+SAMPLE_TEXTURE2D(_Rock,sampler_Rock,rp.xy).rgb*tw.z;
    float3 rockN=UnpackN(SAMPLE_TEXTURE2D(_RockN,sampler_RockN,rp.xz));
    float gw=max(0,1-sp.g-sp.b-sp.a);float4 w=float4(gw,sp.g,sp.b,sp.a);w/=max(.001,dot(w,1));
    half3 albedo=grass*w.x+litter*w.y+dirt*w.z+flag*w.w;
    float3 nt=grassN*w.x+litterN*w.y+dirtN*w.z+flagN*w.w;
    albedo=lerp(albedo,rock,rockW);nt=lerp(nt,rockN,rockW);
    // subtle sun-bleached / lush variation over large distances
    float macro=SAMPLE_TEXTURE2D(_Grass,sampler_Grass,xz*.006).g;albedo*=lerp(.92,1.1,macro);
    float3 n=normalize(float3(gn.x+nt.x*.7,gn.y,gn.z+nt.y*.7));
    Light l=GetMainLight(TransformWorldToShadowCoord(i.w));
    float ndl=saturate(dot(n,l.direction));float wrap=saturate((dot(n,l.direction)+.35)/1.35);
    half3 gi=SampleSH(n)*1.15;
    half3 lit=albedo*(gi+l.color*(ndl*.85+wrap*.12)*lerp(.38,1,l.shadowAttenuation));
    return half4(MixFog(lit,i.fog),1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/ShadowCaster"
  UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
