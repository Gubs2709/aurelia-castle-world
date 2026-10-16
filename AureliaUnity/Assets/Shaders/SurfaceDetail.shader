Shader "Aurelia/Surface Detail"
{
 Properties {[MainTexture] _BaseMap("Surface",2D)="white"{} [MainColor] _BaseColor("Tint",Color)=(1,1,1,1) _Scale("Metres tiling",Float)=.22 _Relief("Surface relief",Float)=.06 _Smoothness("Smoothness",Range(0,1))=.1 _TextureStrength("Texture strength",Range(0,1))=1 _Terrain("Terrain blend",Float)=0 _VertexTint("Vertex tint",Float)=0 _Cull("Culling",Float)=2}
 SubShader {Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"} Cull [_Cull]
 Pass {Tags {"LightMode"="UniversalForward"} HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
 CBUFFER_START(UnityPerMaterial)
 half4 _BaseColor;float _Scale,_Relief,_Smoothness,_TextureStrength,_Terrain,_VertexTint;
 CBUFFER_END
 struct A{float4 p:POSITION;float3 n:NORMAL;float4 color:COLOR;};struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float fog:TEXCOORD2;float4 color:COLOR;};
 V vert(A i){V o;o.w=TransformObjectToWorld(i.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(i.n);o.fog=ComputeFogFactor(o.p.z);o.color=i.color;return o;}
 half4 frag(V i):SV_Target
 {
 float3 n=normalize(i.n),w=pow(abs(n),6);w/=w.x+w.y+w.z;float3 p=i.w*_Scale;
 half3 a=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.zy).rgb*w.x+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xy).rgb*w.z+SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,p.xz).rgb*w.y;
 float h=dot(a,float3(.3,.5,.2));float3 dx=ddx(i.w),dy=ddy(i.w),r1=cross(dy,n),r2=cross(n,dx);float det=dot(dx,r1);float3 gradient=(r1*ddx(h)+r2*ddy(h))*sign(det)/max(abs(det),.0001);n=normalize(n-gradient*_Relief);
 Light l=GetMainLight(TransformWorldToShadowCoord(i.w));float diffuse=saturate(dot(n,l.direction));float3 view=normalize(_WorldSpaceCameraPos-i.w);float spec=pow(saturate(dot(normalize(view+l.direction),n)),80)*_Smoothness;
 half3 albedo=lerp(half3(1,1,1),a,_TextureStrength)*_BaseColor.rgb;
 if(_VertexTint>.5){float lum=dot(albedo,float3(.3,.5,.2));albedo=lerp(albedo,SRGBToLinear(i.color.rgb)*(lum*lerp(2.9,1.7,step(.35,i.color.a))+.1),step(i.color.a,.5));}
 if(_Terrain>.5){float cliff=1-smoothstep(.22,.65,normalize(i.n).y);albedo=lerp(SRGBToLinear(i.color.rgb)*(.9+a.g*.2),albedo,cliff);albedo=lerp(albedo,SRGBToLinear(i.color.rgb)*(.85+a.g*.3),saturate((1-i.color.a)*1.05));}
 half3 lit=albedo*(half3(.27,.34,.37)+l.color*(.23+diffuse*.65)*lerp(.46,1,l.shadowAttenuation))+spec*l.color;return half4(MixFog(lit,i.fog),1);
 }
 ENDHLSL }
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 }
}
