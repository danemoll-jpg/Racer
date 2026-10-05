// 0.79 Part F: paved road pieces and driveways that are separate meshes (Hwy 92's continuous highway, decorative road
// continuations, roadworks, the black driveway) drawn with exactly the ground shader's road lighting (RacerSurface:
// asphalt detail, wet darkening, snow, headlights) in one asphalt colour, so they meet the terrain's painted roads
// without a seam. Used by Scenery: New only.
Shader "Racer/Paved" {
 Properties { _Asphalt("Asphalt colour (raw, like the terrain's vertex colours)",Vector)=(.24,.25,.26,1) }
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
 #include "../Track/StreetLoop/SurfaceLighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _Asphalt;
 CBUFFER_END
 struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
 struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float3 world:TEXCOORD1; float fog:TEXCOORD2; };
 V vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.normalWS=TransformObjectToWorldNormal(a.normalOS); v.world=TransformObjectToWorld(a.positionOS.xyz); v.fog=ComputeFogFactor(v.positionCS.z); return v; }
 half4 frag(V v):SV_Target { return half4(MixFog(RacerSurface(_Asphalt.rgb,v.world,v.normalWS,0),v.fog),1); }
 ENDHLSL
 } }
}
