Shader "Racer/GreyboxGround" {
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; };
 struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float4 color:COLOR; };
 V vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.normalWS=TransformObjectToWorldNormal(a.normalOS); v.color=a.color; return v; }
 half4 frag(V v):SV_Target { Light l=GetMainLight(); float3 n=normalize(v.normalWS); float3 illumination=clamp(SampleSH(n),.25,.8)+.32*l.color*saturate(dot(n,l.direction)); return half4(v.color.rgb*illumination,1); }
 ENDHLSL
 } }
}
