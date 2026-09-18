Shader "Racer/MarkedGround" {
 SubShader { Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
 Pass { Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 struct A { float4 positionOS:POSITION; float3 normalOS:NORMAL; float4 color:COLOR; float4 road:TEXCOORD1; };
 struct V { float4 positionCS:SV_POSITION; float3 normalWS:TEXCOORD0; float4 color:COLOR; float4 road:TEXCOORD1; };
 V vert(A a) { V v; v.positionCS=TransformObjectToHClip(a.positionOS.xyz); v.normalWS=TransformObjectToWorldNormal(a.normalOS); v.color=a.color; v.road=a.road; return v; }
 half4 frag(V v):SV_Target {
   float width=max(fwidth(v.road.x),.015);
   float center=1-smoothstep(.085-width,.085+width,abs(v.road.x));
   float period=18; // Sparse three-metre dashes, including neighborhood hills.
   float phase=abs(frac(v.road.y/period)*period-period*.5);
   float longitudinal=max(fwidth(v.road.y),.02);
   float dash=1-smoothstep(1.5-longitudinal,1.5+longitudinal,phase);
   float edge=1-smoothstep(.07-width,.07+width,abs(abs(v.road.x)-3.65));
   float marking=saturate(v.road.z)*center*dash;
   float3 albedo=lerp(v.color.rgb,float3(.73,.60,.27),marking);
   albedo=lerp(albedo,float3(.67,.66,.59),edge*saturate(v.road.w));
   Light l=GetMainLight();
   float3 n=normalize(v.normalWS);
   float3 illumination=clamp(SampleSH(n),.25,.8)+.32*l.color*saturate(dot(n,l.direction));
   return half4(albedo*illumination,1);
 }
 ENDHLSL
 } }
}
