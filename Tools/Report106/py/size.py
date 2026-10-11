# Design-time sizing only (the first pass); the final table lengths and landings were set from the drives in the built player (Docs/Report106/Player/features.txt).
# 0.106 jump sizing: point-mass flights from a lip (height h, angle th) at speeds v; where they cross the table height and the floor.
import math
g=9.81
def cross(h,th,v,y):  # horizontal distance from the lip where the flight comes down through height y (y<=h+apex)
    a=math.radians(th); vx=v*math.cos(a); vy=v*math.sin(a)
    # h + vy t - g t^2/2 = y
    t=(vy+math.sqrt(vy*vy+2*g*(h-y)))/g
    return vx*t, t, math.degrees(math.atan2(-(vy-g*t),vx))
def jump(name,h,th,vlo,vhi,H=None,phi=None):
    H=h if H is None else H
    R=h/(1-math.cos(math.radians(th))); L=R*math.sin(math.radians(th))
    xk,tk,_=cross(h,th,vlo,H)          # knuckle: where the slow flight comes down to the deck
    x0,t0,ang=cross(h,th,vhi,0)        # the fast flight reaches the floor here
    xm,tm,angm=cross(h,th,(vlo+vhi)/2,H*.5)
    # slope from the knuckle (K,H) to the floor at x0 (+ a little): its angle
    slope=math.degrees(math.atan2(H,x0-xk)) if phi is None else phi
    end=xk+H/math.tan(math.radians(slope))
    print(f"{name}: ramp R={R:.1f} L={L:.1f} lip h={h} th={th}; deck to K={xk:.1f} (air {tk:.2f}s at {vlo}); slope {slope:.1f} deg to {end:.1f}; fast {vhi} m/s down at {x0:.1f} ({t0:.2f}s, {ang:.0f} deg); mid lands at {xm:.1f} {angm:.0f} deg; total {L+end:.1f} m")
jump("K1 small",1.0,20,10,14,0.01)
jump('K2 medium',2.0,25,11,15)
jump('K3 big',3.0,30,12,16)
jump('K4 huge',5.0,34,14,18)
jump('bus gap',3.4,22,15,19,3.4)
jump('car gap',2.0,20,13,17,2.0)
jump('barrel',2.4,24,14,18)
jump('deck launch',3.0,30,13,16,5.0)
