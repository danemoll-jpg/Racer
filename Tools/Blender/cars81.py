"""0.81 Part A: four new player cars, generated in Blender from this script with the 0.75 car kit (Tools/Blender/cars.py:
body loft, cabin glass panes, interior and steering wheel; the 0.75 cars themselves are not rebuilt or changed).

Run:  blender --background --factory-startup --python Tools/Blender/cars81.py -- [Roadster|Fastback|Pebble|Skyfin]

Original designs: the era and the body type only (no make, model, badge or one real car's shape).
  Roadster  1960s long-bonnet roadster, top down: long hood with a power bulge, short rounded tail, low framed windscreen,
            two seats behind it (the rider visible from the chest up), wire wheels, faired-in round lamps.
  Fastback  1960s pony / muscle coupe: long hood with a scoop, a roof that slopes all the way to a short ducktail, wide
            stance, horizontal tail-lamp bar, slotted wheels.
  Pebble    1970s compact rear-engined coupe: short and rounded, upright round headlamps on the front wings, no grille, a
            sloping rear window over a louvred engine lid, small bumpers.
  Skyfin    1950s finned cruiser: long and wide, wraparound windscreen, heavy chrome bumpers and grille, a contrasting
            roof and side spear (slot 'cream', the second tone), tall tail fins with bullet lamps, whitewall tyres.
Same conventions as cars.py: wheel centres (+/-track, -0.20, +/-axle), radius 0.33, ground at y = -0.53, driver at the
seat point H (rider pose Car). Groups Body, Steer, WheelFL/FR/RL/RR. Slots paint, cream, metal, chrome, engine, rubber,
lamp, tail, glass, interior.
"""
import bpy, math, sys, os
from mathutils import Vector
sys.path.insert(0, os.path.dirname(__file__))
import kit, cars
from kit import box, hull, cyl, blob, strip, loft, ring, made, place, at
from cars import interp, pane, WY

kit.COLORS['cream'] = (.93, .90, .80)

CARS = {
    'Roadster': dict(style='roadster', track=.84, axle=1.15, H=(-.36, .02, -.55), length=1.95, hw=.90, belt=.40, roof=.72,
                     wind=(.08, -.21), rear=(-1.10, -1.10), bpost=-.6, wagon=False,
                     keys=[(1.95, .78, .26, .03, -.08), (1.88, .85, .33, .04, -.16), (1.70, .88, .36, .04, -.24), (1.0, .90, .38, .03, -.30),
                           (.1, .90, .40, .02, -.33), (-.6, .90, .41, .02, -.33), (-1.4, .89, .42, .03, -.26), (-1.80, .85, .40, .03, -.20),
                           (-1.96, .76, .33, .02, -.10)]),
    'Fastback': dict(style='fastback', track=.95, axle=1.375, H=(-.42, .05, -.20), length=2.30, hw=1.0, belt=.47, roof=1.00,
                     wind=(.55, .05), rear=(-.50, -1.80), bpost=-.40, wagon=False,
                     keys=[(2.30, .92, .33, .02, -.10), (2.22, .98, .41, .04, -.18), (2.0, 1.0, .44, .05, -.26), (1.2, 1.0, .46, .05, -.30),
                           (.55, 1.0, .47, .04, -.30), (0, 1.0, .48, .03, -.30), (-.9, 1.01, .48, .03, -.30), (-1.85, 1.0, .49, .03, -.27),
                           (-2.15, .96, .48, .02, -.18), (-2.31, .90, .44, .01, -.08)]),
    'Pebble': dict(style='compact', track=.78, axle=1.10, H=(-.36, .04, -.05), length=1.85, hw=.83, belt=.43, roof=.98,
                   wind=(.72, .20), rear=(-.85, -1.50), bpost=-.30, wagon=False,  # 0.88: roof to -.85 (was -.40) so it covers the rear seats
                   keys=[(1.85, .70, .22, .06, -.08), (1.76, .78, .32, .08, -.16), (1.55, .82, .38, .08, -.24), (1.0, .83, .41, .06, -.29),
                         (.75, .83, .42, .05, -.32), (0, .83, .43, .04, -.32), (-.8, .83, .43, .05, -.31), (-1.5, .82, .42, .08, -.24),
                         (-1.75, .77, .37, .08, -.16), (-1.86, .68, .28, .06, -.08)]),
    'Skyfin': dict(style='fins', track=1.0, axle=1.45, H=(-.43, .08, -.05), length=2.50, hw=1.04, belt=.50, roof=1.05,
                   wind=(.80, .30), rear=(-.80, -1.30), bpost=-.30, wagon=False,
                   keys=[(2.50, .95, .36, .02, -.08), (2.42, 1.02, .44, .03, -.18), (2.2, 1.04, .47, .04, -.26), (1.3, 1.04, .48, .04, -.30),
                         (.8, 1.04, .49, .03, -.30), (0, 1.04, .50, .03, -.30), (-1.0, 1.04, .50, .03, -.30), (-2.0, 1.04, .51, .03, -.27),
                         (-2.35, 1.02, .50, .02, -.18), (-2.51, .97, .46, .01, -.08)]),
}


def cabin(C):
    G = 'Body'; st = C['style']; belt, roof = C['belt'], C['roof']; zf, zr0 = C['wind']; zr, zd = C['rear']; hw = C['hw']
    if st == 'roadster':
        # a framed windscreen across the cockpit, and a padded rim round the open cockpit. 0.84 (Part L): the top rail was at
        # belt + .30 (0.70 m), 8 cm below the driver's eyes (H + (0, .77, .05)), so in first person it crossed the middle of
        # the view; now it is raked further back and stands at 0.86 m, about 17 degrees above the eye line (the upper part of
        # the view), with the same slim chrome posts and rail.
        xb = hw * .80; top = belt + .46
        pane('Windscreen', [(-xb, belt + .02, zf), (xb, belt + .02, zf), (xb - .04, top, zr0), (-xb + .04, top, zr0)], G)
        for s in (-1, 1): cyl('Screen post', (s * xb, belt, zf + .01), (s * (xb - .04), top + .01, zr0), .022, G, 'chrome', 8)
        cyl('Screen top', (-xb + .04, top + .01, zr0), (xb - .04, top + .01, zr0), .018, G, 'chrome', 8)
        for s in (-1, 1): cyl('Cockpit rim', (s * (hw - .10), belt + .015, zf - .05), (s * (hw - .10), belt + .015, zd + .05), .03, G, 'interior', 8)
        cyl('Cockpit rim rear', (-(hw - .10), belt + .015, zd + .05), (hw - .10, belt + .015, zd + .05), .03, G, 'interior', 8)
        for s in (-1, 1): blob('Headrest fairing', (s * .36, belt + .06, zd - .10), (.16, .10, .30), G, 'paint', 14, 8)
        return
    xb, xr = hw * .93, hw * .78
    hull('Roof', [(-xr, roof - .045, zr - .02), (xr, roof - .045, zr - .02), (xr, roof - .045, zr0 + .02), (-xr, roof - .045, zr0 + .02),
                  (-xr + .03, roof, zr + .01), (xr - .03, roof, zr + .01), (xr - .03, roof, zr0), (-xr + .03, roof, zr0)], G, 'cream' if st == 'fins' else 'paint', .014, 2)
    pane('Windscreen', [(-xb + .03, belt + .01, zf), (xb - .03, belt + .01, zf), (xr - .02, roof - .04, zr0 + .01), (-xr + .02, roof - .04, zr0 + .01)], G)
    pane('Rear glass', [(-xb + .03, belt + .02, zd), (-xr + .02, roof - .04, zr - .01), (xr - .02, roof - .04, zr - .01), (xb - .03, belt + .02, zd)], G)
    for s in (-1, 1):
        cyl('A pillar', (s * (xb - .01), belt, zf + .01), (s * (xr - .005), roof - .03, zr0), .03 if st == 'fins' else .035, G, 'chrome' if st == 'fins' else 'paint', 8)
        cyl('Rear pillar', (s * (xb - .01), belt, zd - .01), (s * (xr - .005), roof - .03, zr), .05 if st == 'fastback' else .04, G, 'cream' if st == 'fins' else 'paint', 8)
        b = C['bpost']
        if st != 'fins': cyl('B pillar', (s * xb, belt, b), (s * xr, roof - .03, b + .03), .04, G, 'engine', 8)
        # side glass: one pane front of the B pillar, one behind (the fastback's rear quarter window is a small triangle)
        posts = [zf, zd] if st == 'fins' else [zf, b, zd]
        for i, (z0, z1) in enumerate(zip(posts, posts[1:])):
            f0 = 0 if i else (zr0 - zf); f1 = 0 if i < len(posts) - 2 else (zr - zd)
            pane('Side glass', [(s * xb, belt + .015, z0 - .03), (s * xr, roof - .05, z0 - .03 + f0 * .96), (s * xr, roof - .05, z1 + .03 + f1 * .96), (s * xb, belt + .015, z1 + .03)], G)
        cyl('Drip rail', (s * xr, roof - .045, zr - .02), (s * xr, roof - .045, zr0 + .02), .012, G, 'chrome', 6)
        cyl('Belt trim', (s * (hw + .002), belt - .005, zf + .05), (s * (hw + .002), belt - .005, zd - .02), .011, G, 'chrome', 6)


def interior(C):
    start = len(kit.PARTS); c, n = cars.interior(C)
    if C['style'] == 'roadster':  # two seats only: the rear bench would show through the rear deck
        keep = []
        for ob, g, s in kit.PARTS[start:]:
            if ob.name.startswith('Rear seat') or ob.name.startswith('Floor'): bpy.data.objects.remove(ob)
            else: keep.append((ob, g, s))
        del kit.PARTS[start:]; kit.PARTS.extend(keep)
        H = C['H']; x = C['hw'] * .85; z0, z1 = C['rear'][1] + .02, H[2] + .56 + .25
        # 0.88 (BUG-004): top 1.5 cm above the shell's cabin floor (H.y - .30), as in cars.interior
        hull('Floor', [(-x, H[1] - .325, z0), (x, H[1] - .325, z0), (x, H[1] - .325, z1), (-x, H[1] - .325, z1),
                       (-x, H[1] - .285, z0), (x, H[1] - .285, z0), (x, H[1] - .285, z1), (-x, H[1] - .285, z1)], 'Body', 'interior', 0, 1)
    return c, n


def bumper(z, sgn, hw, y0, y1, depth, slot='chrome'):
    hull('Bumper', [(-hw, y0, z - sgn * depth), (hw, y0, z - sgn * depth), (hw * .95, y0 + .01, z + sgn * .04), (-hw * .95, y0 + .01, z + sgn * .04),
                    (-hw, y1, z - sgn * depth), (hw, y1, z - sgn * depth), (hw * .95, y1 - .01, z + sgn * .04), (-hw * .95, y1 - .01, z + sgn * .04)], 'Body', slot, .025, 2)


def details(C):
    G = 'Body'; st = C['style']; L = C['length']; hw = C['hw']; keys = C['keys']; nose = interp(keys, L - .06); tail = interp(keys, -L + .06)
    belt = C['belt']; zm = C['wind'][0] - .05
    for s in (-1, 1):
        cyl('Exhaust tip', (s * .45, -.22, -L + .25), (s * .45, -.22, -L - .06), .035, G, 'chrome', 10)
        box('Door handle', (s * (hw + .006), belt - .08, C['bpost'] + .12), (.02, .03, .14), G, 'chrome', .006)
        if st != 'roadster':
            box('Mirror arm', (s * (hw + .03), belt + .04, zm), (.08, .03, .05), G, 'chrome', .006)
            blob('Mirror', (s * (hw + .09), belt + .06, zm), (.055, .045, .035), G, 'chrome' if st == 'fins' else 'paint', 10, 6)
    box('Number plate', (0, tail[1] - .26, -L - .02), (.36, .13, .02), G, 'metal', .006)
    if st == 'roadster':
        bumper(L + .02, 1, hw * .55, -.12, -.04, .06); bumper(-L - .02, -1, hw * .6, -.10, -.02, .06)
        for s in (-1, 1):
            box('Overrider', (s * .25, -.06, L + .05), (.05, .14, .05), G, 'chrome', .015)
            # faired-in round headlamps under clear covers
            cyl('Headlamp', (s * hw * .62, nose[1] - .12, L - .14), (s * hw * .62, nose[1] - .10, L - .06), .085, G, 'lamp', 18)
            blob('Lamp cover', (s * hw * .62, nose[1] - .11, L - .12), (.10, .09, .10), G, 'glass', 14, 8)
            box('Tail lamp', (s * hw * .55, tail[1] - .12, -L + .01), (.14, .08, .05), G, 'tail', .02)
            box('Side vent', (s * (hw + .004), .10, 1.0), (.012, .08, .22), G, 'chrome', .004)
        # oval grille mouth and a long power bulge
        blob('Grille', (0, -.02, L - .01), (.30, .12, .05), G, 'engine', 16, 8)
        for i in range(4): box('Grille bar', (0, -.08 + i * .04, L + .03), (.48, .008, .012), G, 'chrome', 0)
        hull('Bonnet bulge', [(-.24, nose[1] + .005, .20), (.24, nose[1] + .005, .20), (.20, nose[1] - .03, L - .35), (-.20, nose[1] - .03, L - .35),
                              (-.18, nose[1] + .06, .25), (.18, nose[1] + .06, .25), (.14, nose[1] + .01, L - .40), (-.14, nose[1] + .01, L - .40)], G, 'paint', .03, 2)
    elif st == 'fastback':
        bumper(L + .02, 1, hw * .95, -.14, -.03, .08); bumper(-L - .02, -1, hw * .95, -.12, -.01, .08)
        box('Grille', (0, nose[1] - .14, L - .005), (hw * 1.5, .16, .06), G, 'engine', .01)
        for s in (-1, 1):
            cyl('Headlamp', (s * hw * .70, nose[1] - .14, L - .02), (s * hw * .70, nose[1] - .14, L + .025), .07, G, 'lamp', 16)
            cyl('Headlamp bezel', (s * hw * .70, nose[1] - .14, L - .03), (s * hw * .70, nose[1] - .14, L + .015), .082, G, 'chrome', 16)
            box('Side scoop', (s * (hw + .006), .20, -.75), (.03, .10, .26), G, 'engine', .01)
            box('Side stripe', (s * (hw + .004), .30, .2), (.008, .045, 3.4), G, 'engine', .002)
        box('Tail lamp bar', (0, tail[1] - .12, -L - .005), (hw * 1.5, .10, .04), G, 'tail', .012)
        box('Tail lamp divider', (0, tail[1] - .12, -L - .03), (.12, .11, .02), G, 'chrome', .004)
        hull('Ducktail', [(-.85, tail[1], -L + .18), (.85, tail[1], -L + .18), (.85, tail[1], -L + .02), (-.85, tail[1], -L + .02),
                          (-.85, tail[1] + .02, -L + .16), (.85, tail[1] + .02, -L + .16), (.85, tail[1] + .07, -L + .0), (-.85, tail[1] + .07, -L + .0)], G, 'paint', .01, 1)
        hull('Hood scoop', [(-.22, nose[1] + .01, .9), (.22, nose[1] + .01, .9), (.22, nose[1], 1.4), (-.22, nose[1], 1.4),
                            (-.18, nose[1] + .09, .95), (.18, nose[1] + .09, .95), (.18, nose[1] + .02, 1.4), (-.18, nose[1] + .02, 1.4)], G, 'paint', .02, 2)
        box('Scoop mouth', (0, nose[1] + .06, .925), (.34, .05, .02), G, 'engine', .006)
    elif st == 'compact':
        bumper(L + .01, 1, hw * .85, -.12, -.05, .05); bumper(-L - .01, -1, hw * .85, -.12, -.05, .05)
        for s in (-1, 1):
            # upright round lamps on top of the front wings
            cyl('Headlamp bezel', (s * hw * .62, nose[1] + .04, L - .30), (s * hw * .62, nose[1] + .02, L - .22), .095, G, 'chrome', 18)
            cyl('Headlamp', (s * hw * .62, nose[1] + .02, L - .22), (s * hw * .62, nose[1] + .015, L - .205), .08, G, 'lamp', 18)
            box('Indicator', (s * hw * .62, -.06, L + .02), (.10, .04, .02), G, 'lamp', .006)
            box('Tail lamp', (s * hw * .55, tail[1] - .10, -L - .005), (.20, .09, .04), G, 'tail', .015)
            box('Air intake', (s * (hw + .004), .30, -1.15), (.012, .12, .30), G, 'engine', .004)
        box('Bonnet crease', (0, nose[1] + .01, 1.2), (.03, .01, .9), G, 'chrome', .003)
        # louvred engine lid behind the rear glass
        for i in range(7): box('Engine louvre', (0, interp(keys, -1.58 - i * .035)[1] + .02, -1.58 - i * .035), (.8, .012, .022), G, 'engine', .003)
    elif st == 'fins':
        bumper(L + .03, 1, hw * 1.0, -.16, .0, .10); bumper(-L - .03, -1, hw * 1.0, -.14, .02, .10)
        for s in (-1, 1): box('Bumper bullet', (s * .40, -.06, L + .10), (.10, .10, .10), G, 'chrome', .04)
        box('Grille', (0, nose[1] - .17, L - .005), (hw * 1.6, .18, .06), G, 'engine', .01)
        for i in range(4): box('Grille bar', (0, nose[1] - .23 + i * .045, L + .03), (hw * 1.55, .012, .015), G, 'chrome', 0)
        for s in (-1, 1):
            cyl('Headlamp hood', (s * hw * .78, nose[1] - .10, L - .08), (s * hw * .78, nose[1] - .10, L + .02), .095, G, 'paint', 18)
            cyl('Headlamp', (s * hw * .78, nose[1] - .10, L + .015), (s * hw * .78, nose[1] - .10, L + .03), .075, G, 'lamp', 18)
            # side spear in the second tone, outlined in chrome
            hull('Side spear', [(s * (hw + .004), .02, .9), (s * (hw + .012), .02, .9), (s * (hw + .012), .02, -2.2), (s * (hw + .004), .02, -2.2),
                                (s * (hw + .004), .22, 1.5), (s * (hw + .012), .22, 1.5), (s * (hw + .012), .30, -2.2), (s * (hw + .004), .30, -2.2)], G, 'cream', .004, 1)
            cyl('Spear trim', (s * (hw + .014), .22, 1.5), (s * (hw + .014), .30, -2.2), .01, G, 'chrome', 6)
            cyl('Spear trim low', (s * (hw + .014), .02, .9), (s * (hw + .014), .02, -2.2), .01, G, 'chrome', 6)
            # tail fin rising to a point over the rear corner, with a bullet lamp
            hull('Tail fin', [(s * (hw - .10), tail[1], -1.4), (s * (hw - .01), tail[1], -1.4), (s * (hw - .01), tail[1], -L + .02), (s * (hw - .10), tail[1], -L + .02),
                              (s * (hw - .07), tail[1] + .02, -1.6), (s * (hw - .03), tail[1] + .02, -1.6), (s * (hw - .03), tail[1] + .32, -L - .02), (s * (hw - .07), tail[1] + .32, -L - .02)], G, 'paint', .015, 2)
            cyl('Bullet lamp', (s * (hw - .05), tail[1] + .16, -L + .08), (s * (hw - .05), tail[1] + .16, -L - .06), .045, G, 'tail', 14, r1=.025)
            box('Tail lamp', (s * hw * .55, tail[1] - .14, -L - .005), (.30, .07, .04), G, 'tail', .012)
        box('Hood ornament', (0, nose[1] + .04, L - .25), (.04, .06, .20), G, 'chrome', .01)


def wheel(group, pos, left, st):
    s0 = len(kit.PARTS)
    kit.tyre(group, .255, .105, .075, 28, 8, knobs=0, squared=.86)
    if st == 'fins':  # whitewall: a cream band on the outer sidewall
        s1 = len(kit.PARTS); kit.tyre(group, .235, .008, .036, 28, 6, slot='cream')
        for ob in made(s1): ob.location.x += .113
    cyl(group + ' rim', (-.07, 0, 0), (.07, 0, 0), .19, group, 'metal', 24, cap=False)
    face = 'chrome' if st in ('roadster', 'fins', 'fastback') else 'metal'
    cyl(group + ' rim face', (.06, 0, 0), (.075, 0, 0), .195 if st != 'fins' else .15, group, face, 24)
    if st == 'roadster':  # wire wheel: many thin spokes and a knock-off spinner
        for i in range(24):
            a = i * 2 * math.pi / 24; x0 = .02 if i % 2 else .075
            cyl(group + ' wire', (x0, math.cos(a) * .05, math.sin(a) * .05), (.06, math.cos(a + .3) * .185, math.sin(a + .3) * .185), .004, group, 'chrome', 4, smooth=False)
        for k in range(2): box(group + ' spinner', (.11, 0, 0), (.02, .16, .03), group, 'chrome', .006, (90 * k, 0, 0))
    else:
        n = 5
        for i in range(n):
            a = i * 2 * math.pi / n
            if st == 'fastback': box(group + ' slot', (.077, math.cos(a + .6) * .13, math.sin(a + .6) * .13), (.012, .045, .08), group, 'engine', .006, (math.degrees(a + .6) - 90, 0, 0))
            elif st == 'compact': cyl(group + ' hole', (.074, math.cos(a) * .12, math.sin(a) * .12), (.08, math.cos(a) * .12, math.sin(a) * .12), .03, group, 'engine', 10)
            cyl(group + ' nut', (.093, math.cos(a) * .035, math.sin(a) * .035), (.104, math.cos(a) * .035, math.sin(a) * .035), .008, group, 'metal', 6)
        cyl(group + ' hub cap', (.07, 0, 0), (.10, 0, 0), .07 if st != 'fins' else .11, group, 'chrome', 16, r1=.03 if st == 'fins' else None)
    place(made(s0), at(pos, left))


def build(name):
    C = CARS[name]; bpy.ops.wm.read_factory_settings(use_empty=True)
    cars.body(C); cabin(C); details(C); c, n = interior(C)
    T, A = C['track'], C['axle']
    pos = {'WheelFL': (-T, WY, A), 'WheelFR': (T, WY, A), 'WheelRL': (-T, WY, -A), 'WheelRR': (T, WY, -A)}
    for g, p in pos.items(): wheel(g, p, p[0] < 0, C['style'])
    out = kit.join_all()
    for ob in out:
        g = ob.name.split('__')[0]
        if g in pos: kit.set_origin(ob, pos[g])
        if g == 'Steer': kit.set_origin(ob, c)
    print(name, 'steering wheel centre', c, 'column', n)
    kit.export(name)


if __name__ == '__main__':
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    for nm in (args or list(CARS)): build(nm)
