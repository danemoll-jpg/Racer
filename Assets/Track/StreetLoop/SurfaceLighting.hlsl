#ifndef RACER_SURFACE_LIGHTING_INCLUDED
#define RACER_SURFACE_LIGHTING_INCLUDED
// Continuous world-space detail: identical at tile edges, filtered before it becomes subpixel.
float RacerHash(float2 p) {p=frac(p*float2(.1031,.1030));p+=dot(p,p.yx+33.33);return frac((p.x+p.y)*p.x);}
float RacerNoise(float2 p)
{
    float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(RacerHash(i),RacerHash(i+float2(1,0)),f.x),lerp(RacerHash(i+float2(0,1)),RacerHash(i+1),f.x),f.y);
}
half3 RacerSurface(half3 color,float3 world,float3 normal,float vegetation)
{
    Light light=GetMainLight();
    if(vegetation<.5)
    {
        float road=smoothstep(.005,.017,color.b-color.r);
        float2 uv=world.xz*3;
        float filter=1-smoothstep(.3,1.2,max(length(ddx(uv)),length(ddy(uv))));
        float grain=(RacerNoise(uv)-.5)*.07*filter;
        color*=1-road*(.10-grain);
        light.shadowAttenuation=lerp(MainLightRealtimeShadow(TransformWorldToShadowCoord(world)),1,GetMainLightShadowFade(world));
    }
    float3 n=normalize(normal);
    float3 ambient=clamp(SampleSH(n),.25,.8);
    float sun=lerp(.32,.43,vegetation)*saturate(dot(n,light.direction));
    float shade=lerp(1,light.shadowAttenuation,.70*(1-vegetation));
    float underside=lerp(1,lerp(.86,1.03,saturate(n.y*.5+.5)),vegetation);
    return color*(ambient+sun*light.color)*shade*underside;
}
#endif
