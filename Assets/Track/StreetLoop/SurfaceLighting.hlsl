#ifndef RACER_SURFACE_LIGHTING_INCLUDED
#define RACER_SURFACE_LIGHTING_INCLUDED
// Continuous world-space detail: identical at tile edges, filtered before it becomes subpixel.
float RacerHash(float2 p) {p=frac(p*float2(.1031,.1030));p+=dot(p,p.yx+33.33);return frac((p.x+p.y)*p.x);}
float RacerNoise(float2 p)
{
    float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);
    return lerp(lerp(RacerHash(i),RacerHash(i+float2(1,0)),f.x),lerp(RacerHash(i+float2(0,1)),RacerHash(i+1),f.x),f.y);
}
// 0.71 world look (Racer.WorldLook sets these globals; all 0 = the authored look, so "-lookOff" is unchanged).
float _RacerLook,_RacerSunBoost,_RacerAmbientScale,_RacerShadowLift,_RacerRoadSheen,_RacerGroundVariation;
// 0.72 weather: wet surfaces (darker, glossier road) and snow cover (0 = clear, the 0.71 look).
float _RacerWet,_RacerSnow;
half3 RacerSurface(half3 color,float3 world,float3 normal,float vegetation)
{
    Light light=GetMainLight();
    float road=0;
    if(vegetation<.5)
    {
        road=smoothstep(.005,.017,color.b-color.r);
        float2 uv=world.xz*3;
        float filter=1-smoothstep(.3,1.2,max(length(ddx(uv)),length(ddy(uv))));
        float grain=(RacerNoise(uv)-.5)*.07*filter;
        color*=1-road*(.10-grain);
        light.shadowAttenuation=lerp(MainLightRealtimeShadow(TransformWorldToShadowCoord(world)),1,GetMainLightShadowFade(world));
        // Look: grass and dirt read as different surfaces - broad, soft patchiness on grass, finer grain on dirt.
        float look=_RacerLook*_RacerGroundVariation*(1-road);
        float grass=saturate((color.g-max(color.r,color.b))*14);
        float dirt=saturate((color.r-color.g)*12);
        float patch=RacerNoise(world.xz*.09)*.65+RacerNoise(world.xz*.31)*.35-.5;
        float2 fine=world.xz*5;float fineFilter=1-smoothstep(.3,1.2,max(length(ddx(fine)),length(ddy(fine))));
        color*=1+look*(grass*patch*.16+dirt*(RacerNoise(fine)-.5)*.10*fineFilter);
        // 0.72 wet: the road darkens (its sheen rises through _RacerRoadSheen); open ground darkens slightly.
        color*=1-_RacerWet*(road*.32+(1-road)*.12);
        // 0.72 snow: open ground and gentle slopes turn white; paved road stays dark and dirt trails keep a tracked,
        // darker snow, so roads and trails stay readable against the ground.
        float flat=saturate((normalize(normal).y-.45)*2.5);
        float cover=_RacerSnow*flat*(1-road)*(1-dirt*.8)*(.85+.15*patch);
        color=lerp(color,half3(.86,.88,.92),cover);
        color*=1-_RacerSnow*road*.08;
    }
    else
    {
        // 0.72 snow on vegetation: upward-facing foliage takes a light dusting.
        color=lerp(color,half3(.84,.87,.90),_RacerSnow*saturate(normalize(normal).y)*.45);
    }
    float3 n=normalize(normal);
    float3 ambient=clamp(SampleSH(n),.25,.8)*lerp(1,_RacerAmbientScale,_RacerLook);
    float sun=lerp(.32,.43,vegetation)*(1+_RacerLook*_RacerSunBoost)*saturate(dot(n,light.direction));
    float shade=lerp(1,light.shadowAttenuation,.70*(1-vegetation)*(1-_RacerLook*_RacerShadowLift));
    float underside=lerp(1,lerp(.86,1.03,saturate(n.y*.5+.5)),vegetation);
    half3 result=color*(ambient+sun*light.color)*shade*underside;
    // Look: a faint sun sheen on the paved road only.
    float3 viewDir=normalize(GetWorldSpaceViewDir(world));
    float spec=pow(saturate(dot(n,normalize(light.direction+viewDir))),48)*road*_RacerRoadSheen*_RacerLook*light.shadowAttenuation;
    result+=spec*light.color;
    // 0.72 vehicle headlights (and any other local light): diffuse, plus a wet-road glint.
    #if defined(_ADDITIONAL_LIGHTS)
    uint lights=GetAdditionalLightsCount();
    for(uint li=0;li<lights;li++)
    {
        Light l=GetAdditionalLight(li,world);
        float atten=l.distanceAttenuation;
        result+=color*l.color*(atten*saturate(dot(n,l.direction))*.85);
        result+=l.color*(atten*pow(saturate(dot(n,normalize(l.direction+viewDir))),24)*road*(.08+_RacerWet*.6));
    }
    #endif
    return result;
}
#endif
