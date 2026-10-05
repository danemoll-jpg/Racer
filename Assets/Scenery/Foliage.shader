Shader "Racer/Foliage" {
 // 0.78 Part A: the new trees and bushes (Racer.SceneryTrees draws them instanced). Same lighting as the world's ground
 // and old forest (SurfaceLighting.hlsl, the 0.71-0.73 look, weather and night), plus:
 //  - a gentle wind sway of the leaves (vertex alpha = how much a vertex moves; trunks 0), stronger in rain and storms;
 //  - a patch-coherent leaf tint per tree from its position, as the old forest had (uv0.x = 1 on leaves, 0 on bark);
 //  - snow on the upward faces of the crowns in the Snow look;
 //  - shadows (ShadowCaster), drawn only for the near trees.
 Properties { }
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 float _RacerWind;
 struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
 float3 Origin() { return float3(UNITY_MATRIX_M._m03, UNITY_MATRIX_M._m13, UNITY_MATRIX_M._m23); }
 float3 Sway(float3 world, float weight)
 {
   float3 o = Origin(); float height = length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21));
   float t = _Time.y; float phase = dot(o.xz, float2(.071, .053));
   float gust = .6 + .4 * sin(t * .37 + phase * .5);
   float strength = (.010 + .022 * _RacerWind) * height * gust * weight;
   return world + float3(sin(t * 1.7 + phase + world.y * .25), 0, cos(t * 1.3 + phase * 1.4 + world.x * .2)) * strength;
 }
 ENDHLSL
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
 #pragma multi_compile_fog
 #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 #include "../Track/StreetLoop/SurfaceLighting.hlsl"
 struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float4 color:COLOR; float3 world:TEXCOORD1; float fog:TEXCOORD2; float leaf:TEXCOORD3; float3 origin:TEXCOORD4; };
 V vert(A a)
 {
   UNITY_SETUP_INSTANCE_ID(a);
   V v; float3 w = Sway(TransformObjectToWorld(a.positionOS.xyz), a.color.a);
   v.positionCS = TransformWorldToHClip(w); v.normalWS = TransformObjectToWorldNormal(a.normalOS); v.color = a.color; v.world = w;
   v.fog = ComputeFogFactor(v.positionCS.z); v.leaf = a.uv.x; v.origin = Origin(); return v;
 }
 half4 frag(V v):SV_Target
 {
   // the old forest's patch tint (Phase6Vegetation): broad patches of darker and lighter green, a little per tree
   float patch = RacerNoise((v.origin.xz + float2(913, 771)) / 65);
   float hash = frac(sin(dot(v.origin.xz, float2(12.9898, 78.233))) * 43758.5453);
   half3 tint = lerp(half3(.80, .92, .78), half3(1.12, 1.06, .92), saturate(patch * .8 + hash * .2));
   half3 c = v.color.rgb * lerp(1, tint, v.leaf);
   float3 n = normalize(v.normalWS);
   c = lerp(c, half3(.88, .90, .94), _RacerSnow * smoothstep(.15, .75, n.y) * lerp(.35, .85, v.leaf));
   return half4(MixFog(RacerSurface(c, v.world, n, 1), v.fog), 1);
 }
 ENDHLSL
 }
 Pass { Name "ShadowCaster" Tags { "LightMode"="ShadowCaster" }
 ZWrite On ZTest LEqual ColorMask 0
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
 float3 _LightDirection; float3 _LightPosition;
 float4 vert(A a):SV_POSITION
 {
   UNITY_SETUP_INSTANCE_ID(a);
   float3 w = Sway(TransformObjectToWorld(a.positionOS.xyz), a.color.a); float3 n = TransformObjectToWorldNormal(a.normalOS);
   #if _CASTING_PUNCTUAL_LIGHT_SHADOW
   float3 l = normalize(_LightPosition - w);
   #else
   float3 l = _LightDirection;
   #endif
   float4 p = TransformWorldToHClip(ApplyShadowBias(w, n, l));
   #if UNITY_REVERSED_Z
   p.z = min(p.z, UNITY_NEAR_CLIP_VALUE);
   #else
   p.z = max(p.z, UNITY_NEAR_CLIP_VALUE);
   #endif
   return p;
 }
 half4 frag():SV_Target { return 0; }
 ENDHLSL
 }
 Pass { Name "DepthOnly" Tags { "LightMode"="DepthOnly" }
 ZWrite On ColorMask R
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_instancing
 float4 vert(A a):SV_POSITION { UNITY_SETUP_INSTANCE_ID(a); return TransformWorldToHClip(Sway(TransformObjectToWorld(a.positionOS.xyz), a.color.a)); }
 half4 frag():SV_Target { return 0; }
 ENDHLSL
 }
 }
}
