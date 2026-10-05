Shader "Racer/MarkedGround" {
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fog
 #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "SurfaceLighting.hlsl"
 struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; float4 road:TEXCOORD1; };
 struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float4 color:COLOR; float4 road:TEXCOORD1; float3 world:TEXCOORD2; float fog:TEXCOORD3; };
 V vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.normalWS=TransformObjectToWorldNormal(a.normalOS); v.color=a.color; v.road=a.road; v.world=TransformObjectToWorld(a.positionOS.xyz); v.fog=ComputeFogFactor(v.positionCS.z); return v; }
 half4 frag(V v):SV_Target {
   float width=max(fwidth(v.road.x),.015);
   float center=1-smoothstep(.085-width,.085+width,abs(v.road.x));
   float period=18; // Sparse three-metre dashes, including neighborhood hills.
   float phase=abs(frac(v.road.y/period)*period-period*.5);
   float longitudinal=max(fwidth(v.road.y),.02);
   float dash=1-smoothstep(1.5-longitudinal,1.5+longitudinal,phase);
   float edge=1-smoothstep(.07-width,.07+width,abs(abs(v.road.x)-3.65));
   float marking=saturate(v.road.z)*center*dash;
   float3 albedo=v.color.rgb;
   if(_RacerScenery>.5)
   {
     // 0.78 New scenery: darker, smoother wheel tracks in each lane and a slightly crumbled edge; crisper, brighter paint.
     float x=abs(v.road.x);
     float tracks=exp(-pow((x-1.05)/.34,2))+exp(-pow((x-2.55)/.34,2));
     albedo*=1-tracks*.07*saturate(v.road.w+v.road.z);
     albedo*=1+(RacerNoise(v.world.xz*3.7)-.5)*.10*smoothstep(3.2,3.9,x);
     albedo=lerp(albedo,float3(.86,.70,.30),marking);
     albedo=lerp(albedo,float3(.84,.83,.78),edge*saturate(v.road.w));
   }
   else
   {
     albedo=lerp(albedo,float3(.73,.60,.27),marking);
     albedo=lerp(albedo,float3(.67,.66,.59),edge*saturate(v.road.w));
   }
   return half4(MixFog(RacerSurface(albedo,v.world,v.normalWS,0),v.fog),1);
 }
 ENDHLSL
 } }
}
