Shader "CabinHome/Study Sky"
{
 Properties { _Top("Zenith",Color)=(.4,.5,.7,1) _Horizon("Horizon",Color)=(.8,.7,.6,1) _Cloud("Clouds",Color)=(.9,.8,.7,1) }
 SubShader {
 Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
 Cull Off ZWrite Off
 Pass {
 HLSLPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 struct A { float4 vertex:POSITION; }; struct V { float4 pos:SV_POSITION; float3 dir:TEXCOORD0; };
 float4 _Top,_Horizon,_Cloud;
 V vert(A v) { V o; o.pos=TransformObjectToHClip(v.vertex.xyz); o.dir=v.vertex.xyz; return o; }
 float hash(float2 p) { return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453); }
 float noise(float2 p) { float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(i),hash(i+float2(1,0)),f.x),lerp(hash(i+float2(0,1)),hash(i+1),f.x),f.y); }
 half4 frag(V i):SV_Target {
 float3 d=normalize(i.dir);float h=max(d.y,0);
 float3 c=lerp(_Horizon.rgb,_Top.rgb,smoothstep(0,.65,h));
 float2 p=d.xz/max(.12,d.y+.17)*2.7;
 float n=noise(p)+.35*noise(p*2.2);
 float cloud=smoothstep(.77,1.04,n)*smoothstep(.02,.18,h)*(1-smoothstep(.6,.88,h));
 return half4(lerp(c,_Cloud.rgb,cloud*.65),1);
 }
 ENDHLSL
 }
 }
}
