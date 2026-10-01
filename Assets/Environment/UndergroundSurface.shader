Shader "Racer/UndergroundSurface"
{
 Properties { _Color("Surface",Color)=(.3,.32,.3,1) _Natural("Natural rock",Float)=0 }
 SubShader {
 Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass {
 Tags { "LightMode"="SRPDefaultUnlit" }
 Cull Off ZWrite On
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 CBUFFER_START(UnityPerMaterial)
 half4 _Color;float _Natural;
 CBUFFER_END
 struct A {float4 p:POSITION;float3 n:NORMAL;half4 c:COLOR;};
 struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;half4 c:COLOR;};
 V vert(A a){V v;v.p=TransformObjectToHClip(a.p.xyz);v.world=TransformObjectToWorld(a.p.xyz);v.n=TransformObjectToWorldNormal(a.n);v.c=a.c;return v;}
 float hash(float3 p){return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453);}
 float noise(float3 p){float3 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(lerp(hash(i),hash(i+float3(1,0,0)),f.x),lerp(hash(i+float3(0,1,0)),hash(i+float3(1,1,0)),f.x),f.y),lerp(lerp(hash(i+float3(0,0,1)),hash(i+float3(1,0,1)),f.x),lerp(hash(i+float3(0,1,1)),hash(i+float3(1,1,1)),f.x),f.y),f.z);}
 half4 frag(V v):SV_Target{
 float grime=.65+.35*noise(v.world*.8);
 float streak=.78+.22*noise(v.world*float3(3,.25,3));
 float facets=.74+.26*abs(normalize(v.n).y);
 return half4(_Color.rgb*v.c.rgb*lerp(grime*streak,grime*facets,_Natural),1);
 }
 ENDHLSL
 }
 }
}
