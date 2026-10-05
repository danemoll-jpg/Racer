Shader "Racer/Building" {
 // 0.78 Part A: the new buildings, rocks and props (Racer.SceneryBuildings and friends). Vertex colour albedo, the world's
 // light (sun with shadows, sky ambient, vehicle headlights, fog), snow on upward faces (roofs, ledges) in the Snow look,
 // and lit windows at night: vertex alpha is a window's glow (0 = dark), scaled by _RacerWindowGlow (0 by day).
 Properties { }
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 HLSLINCLUDE
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
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
 float _RacerLook,_RacerSunBoost,_RacerAmbientScale,_RacerSnow,_RacerWet,_RacerWindowGlow;
 struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float4 color:COLOR; float3 world:TEXCOORD1; float fog:TEXCOORD2; };
 V vert(A a)
 {
   UNITY_SETUP_INSTANCE_ID(a);
   V v; v.world = TransformObjectToWorld(a.positionOS.xyz); v.positionCS = TransformWorldToHClip(v.world);
   v.normalWS = TransformObjectToWorldNormal(a.normalOS); v.color = a.color; v.fog = ComputeFogFactor(v.positionCS.z); return v;
 }
 half4 frag(V v):SV_Target
 {
   float3 n = normalize(v.normalWS); half3 c = v.color.rgb;
   float glow = v.color.a < .99 ? (1 - v.color.a) : 0;  // alpha 1 = ordinary surface; below 1 = a window, lit by night
   c *= 1 - _RacerWet * .10 * saturate(n.y);
   c = lerp(c, half3(.88, .90, .94), _RacerSnow * smoothstep(.35, .8, n.y));
   Light light = GetMainLight(TransformWorldToShadowCoord(v.world));
   float shadow = lerp(light.shadowAttenuation, 1, GetMainLightShadowFade(v.world));
   float3 ambient = clamp(SampleSH(n), .25, .8) * lerp(1, _RacerAmbientScale, _RacerLook);
   float sun = .45 * (1 + _RacerLook * _RacerSunBoost) * saturate(dot(n, light.direction));
   half3 result = c * (ambient + sun * light.color * lerp(.35, 1, shadow));
   #if defined(_ADDITIONAL_LIGHTS)
   uint count = GetAdditionalLightsCount();
   for (uint i = 0; i < count; i++) { Light l = GetAdditionalLight(i, v.world); result += c * l.color * (l.distanceAttenuation * saturate(dot(n, l.direction)) * .85); }
   #endif
   result += half3(1.0, .74, .38) * glow * _RacerWindowGlow * 1.6;
   return half4(MixFog(result, v.fog), 1);
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
   float3 w = TransformObjectToWorld(a.positionOS.xyz); float3 n = TransformObjectToWorldNormal(a.normalOS);
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
 float4 vert(A a):SV_POSITION { UNITY_SETUP_INSTANCE_ID(a); return TransformObjectToHClip(a.positionOS.xyz); }
 half4 frag():SV_Target { return 0; }
 ENDHLSL
 }
 }
}
