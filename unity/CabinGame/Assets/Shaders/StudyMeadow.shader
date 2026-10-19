Shader "CabinHome/Study Meadow"
{
 Properties { _BaseColor("Grass",Color)=(.34,.43,.28,1) _PatchColor("Soft variation",Color)=(.44,.49,.31,1) }
 SubShader {
 Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
 Pass {
 Tags { "LightMode"="UniversalForward" }
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
 #pragma multi_compile_fragment _ _SHADOWS_SOFT
 #pragma multi_compile_fog
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
 CBUFFER_START(UnityPerMaterial)
 float4 _BaseColor,_PatchColor;
 CBUFFER_END
 struct A { float4 p:POSITION; float3 n:NORMAL; };
 struct V { float4 p:SV_POSITION; float3 world:TEXCOORD0; float3 n:TEXCOORD1; float fog:TEXCOORD2; };
 V vert(A v) { V o;o.world=TransformObjectToWorld(v.p.xyz);o.p=TransformWorldToHClip(o.world);o.n=TransformObjectToWorldNormal(v.n);o.fog=ComputeFogFactor(o.p.z);return o; }
 half4 frag(V i):SV_Target {
 float patch=sin(i.world.x*.33+sin(i.world.z*.17)*2)*sin(i.world.z*.29)+.2*sin(i.world.x*1.7+i.world.z);
 float3 base=lerp(_BaseColor.rgb,_PatchColor.rgb,smoothstep(-.6,.8,patch)*.65);
 base=lerp(base,float3(.24,.33,.35),smoothstep(2,24,i.world.y)*.65);
 Light sun=GetMainLight(TransformWorldToShadowCoord(i.world));float3 n=normalize(i.n);
 float3 c=base*(SampleSH(n)+sun.color*saturate(dot(n,sun.direction))*sun.shadowAttenuation);
 return half4(MixFog(c,i.fog),1);
 }
 ENDHLSL
 }
 UsePass "Universal Render Pipeline/Lit/DepthOnly"
 UsePass "Universal Render Pipeline/Lit/ShadowCaster"
 }
}
