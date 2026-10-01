Shader "Racer/ShallowDrainWater" {
Properties { _Color("Water",Color)=(.075,.16,.17,1) }
SubShader { Tags {"RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"} Pass { Tags {"LightMode"="SRPDefaultUnlit"} Cull Off ZWrite On
HLSLPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
CBUFFER_START(UnityPerMaterial)
half4 _Color;
CBUFFER_END
struct A {float4 p:POSITION;float2 uv:TEXCOORD0;};struct V {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
V vert(A a){V v;v.p=TransformObjectToHClip(a.p.xyz);v.uv=a.uv;return v;}
half4 frag(V v):SV_Target {float ripple=sin(v.uv.y*19+sin(v.uv.x*13)*1.7-_Time.y*2.6);float glint=pow(saturate(ripple),18)*(.035+.025*sin(v.uv.y*1.7));float flow=.88+.12*sin(v.uv.x*23+v.uv.y*3-_Time.y*.8);return half4(_Color.rgb*flow+glint,1);}
ENDHLSL
}}}
