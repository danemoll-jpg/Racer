"""0.75 Part A: the Trail Four ATV, generated in Blender from this script (rider: Tools/Blender/rider.py, pose Atv).

Run:  blender --background --factory-startup --python Tools/Blender/trailfour.py

Fitted to the existing vehicle (not the other way round): wheel centres (+/-0.725, -0.20, +/-0.825), wheel radius 0.33,
ground at y = -0.53; rider seated at hips (0, 0.56, -0.32), hands on the grips at (+/-0.42, 0.755, 0.36), feet on the
footboards. Output SourceArt/Blender/TrailFour.blend and Assets/Resources/VehicleModels/TrailFour.fbx.
Groups: Body (fixed), Front (handlebars: steer about the column axis through FRONT_PIVOT along FRONT_AXIS),
WheelFL / WheelFR / WheelRL / WheelRR (origin on the axle, axle along x). Slots: paint (body colour), metal, engine,
rubber, lamp, tail.
"""
import bpy, math, sys, os
from mathutils import Vector
sys.path.insert(0, os.path.dirname(__file__))
import kit
from kit import B, box, hull, cyl, blob, strip, tube, limb, loft, ring, made, place, at

TRACK, AXLE, WY = .725, .825, -.20
FRONT_PIVOT, FRONT_TOP = (0, .10, .62), (0, .70, .34)   # steering column, bottom and top (Unity)


def wheel(group, pos, left):
    s0 = len(kit.PARTS)
    kit.tyre(group, .25, .13, .08, 24, 8, knobs=20, squared=.92, knob=(.85, .014, .045))
    cyl(group + ' rim', (-.09, 0, 0), (.09, 0, 0), .165, group, 'metal', 20, cap=False)
    cyl(group + ' rim lip', (.085, 0, 0), (.10, 0, 0), .172, group, 'metal', 20)
    cyl(group + ' dish', (.02, 0, 0), (.088, 0, 0), .15, group, 'engine', 16, r1=.11)
    cyl(group + ' hub', (.06, 0, 0), (.11, 0, 0), .055, group, 'metal', 12)
    for i in range(4):
        a = i * math.pi / 2 + math.pi / 4
        cyl(group + ' nut', (.105, math.cos(a) * .035, math.sin(a) * .035), (.122, math.cos(a) * .035, math.sin(a) * .035), .009, group, 'metal', 6)
    place(made(s0), at(pos, left))


def atv():
    G = 'Body'
    # ---- frame and running gear (fixed) ----
    for s in (-1, 1):
        cyl('Lower rail', (s * .2, -.2, .95), (s * .2, -.2, -.95), .028, G, 'engine', 8)
        cyl('Upper rail', (s * .22, .22, .70), (s * .2, .26, -.95), .024, G, 'engine', 8)
        cyl('Front upright', (s * .2, -.2, .80), (s * .22, .22, .70), .024, G, 'engine', 8)
        cyl('Rear upright', (s * .2, -.2, -.85), (s * .2, .26, -.85), .024, G, 'engine', 8)
        # front double wishbones and shock
        for y0, y1 in ((-.17, -.24), (-.02, -.12)):
            cyl('A-arm', (s * .22, y0, .70), (s * .58, y1, .825), .018, G, 'engine', 8)
            cyl('A-arm', (s * .22, y0, .96), (s * .58, y1, .825), .018, G, 'engine', 8)
        cyl('Knuckle', (s * .585, -.26, .825), (s * .585, -.10, .825), .03, G, 'metal', 10)
        cyl('Shock spring', (s * .28, .24, .80), (s * .52, -.08, .82), .04, G, 'metal', 10)
        cyl('Shock body', (s * .27, .26, .80), (s * .40, .08, .81), .028, G, 'engine', 10)
        # footboards with raised edges
        hull('Footboard', [(s * .24, .015, -.42), (s * .54, .015, -.42), (s * .54, .015, .14), (s * .24, .015, .14),
                           (s * .24, .055, -.42), (s * .54, .055, -.42), (s * .54, .055, .14), (s * .24, .055, .14)], G, 'engine', .01, 1)
        box('Footboard lip', (s * .535, .08, -.14), (.03, .06, .56), G, 'engine', .008)
        for z in (-.33, -.20, -.07, .06): box('Footboard grip', (s * .39, .06, z), (.26, .012, .03), G, 'rubber', 0)
    cyl('Rear axle', (-.60, -.2, -.825), (.60, -.2, -.825), .035, G, 'metal', 10)
    hull('Swing arm', [(-.16, -.25, -.20), (.16, -.25, -.20), (.10, -.25, -.80), (-.10, -.25, -.80),
                       (-.16, -.08, -.20), (.16, -.08, -.20), (.10, -.14, -.80), (-.10, -.14, -.80)], G, 'engine', .02, 1)
    box('Diff', (0, -.2, -.825), (.18, .16, .16), G, 'engine', .02)
    # ---- engine ----
    hull('Crankcase', [(-.17, -.24, -.28), (.17, -.24, -.28), (.17, -.22, .22), (-.17, -.22, .22), (-.18, .10, -.30), (.18, .10, -.30), (.18, .12, .22), (-.18, .12, .22)], G, 'engine', .03)
    hull('Cylinder', [(-.11, .08, .02), (.11, .08, .02), (.11, .08, .24), (-.11, .08, .24), (-.10, .30, .08), (.10, .30, .08), (.10, .30, .30), (-.10, .30, .30)], G, 'engine', .015, 1)
    for i in range(4):
        y = .13 + i * .04
        hull('Cooling fin', [(-.14, y, .0 + i * .015), (.14, y, .0 + i * .015), (.14, y, .27 + i * .015), (-.14, y, .27 + i * .015),
                             (-.14, y + .012, .0 + i * .015), (.14, y + .012, .0 + i * .015), (.14, y + .012, .27 + i * .015), (-.14, y + .012, .27 + i * .015)], G, 'metal', 0, 1)
    blob('Clutch cover', (.18, -.06, -.02), (.035, .12, .14), G, 'metal', 14, 8)
    blob('Pull starter', (-.18, -.04, .02), (.035, .1, .1), G, 'engine', 14, 8)
    tube('Exhaust', [(.08, .26, .30), (.20, .20, .38), (.30, .08, .20), (.32, .10, -.20), (.32, .22, -.55)], .028, G, 'metal')
    cyl('Muffler', (.32, .22, -.55), (.32, .27, -1.02), .065, G, 'metal', 14)
    cyl('Muffler tip', (.32, .27, -1.02), (.32, .275, -1.06), .03, G, 'engine', 10)
    # ---- bodywork (paint): front and rear fenders as flat-topped wings over the wheels, centre nose and tail ----
    for s in (-1, 1):
        strip('Front fender', [(.36, .10), (.46, .24), (.60, .305), (.82, .32), (1.02, .30), (1.14, .22), (1.20, .10)], .40, .022, s * .69, G, 'paint')
        strip('Rear fender', [(-.34, .10), (-.44, .25), (-.60, .325), (-.84, .34), (-1.04, .32), (-1.16, .24), (-1.22, .12)], .40, .022, s * .69, G, 'paint')
        # inner skirts joining the wings to the centre body
        hull('Front skirt', [(s * .30, .12, .44), (s * .50, .12, .44), (s * .50, .12, 1.10), (s * .30, .12, 1.10),
                             (s * .30, .31, .56), (s * .50, .31, .58), (s * .50, .30, 1.0), (s * .30, .31, 1.0)], G, 'paint', .015, 1)
        hull('Rear skirt', [(s * .30, .12, -.40), (s * .50, .12, -.40), (s * .50, .12, -1.14), (s * .30, .12, -1.14),
                            (s * .30, .335, -.56), (s * .50, .33, -.58), (s * .50, .32, -1.04), (s * .30, .33, -1.04)], G, 'paint', .015, 1)
        # side panels below the seat
        hull('Side panel', [(s * .20, .10, -.40), (s * .30, .10, -.40), (s * .30, .14, .10), (s * .20, .14, .10),
                            (s * .19, .36, -.44), (s * .28, .36, -.44), (s * .28, .30, .08), (s * .19, .30, .08)], G, 'paint', .015, 1)
    hull('Nose', [(-.31, .02, .56), (.31, .02, .56), (.27, .04, 1.17), (-.27, .04, 1.17), (-.30, .32, .52), (.30, .32, .52), (.25, .30, 1.05), (-.25, .30, 1.05)], G, 'paint', .035, 2)
    hull('Tail', [(-.31, .05, -.40), (.31, .05, -.40), (.29, .06, -1.18), (-.29, .06, -1.18), (-.30, .34, -.46), (.30, .34, -.46), (.28, .33, -1.10), (-.28, .33, -1.10)], G, 'paint', .035, 2)
    # fuel tank / console between the bars and the seat
    hull('Tank', [(-.19, .25, -.04), (.19, .25, -.04), (.20, .25, .46), (-.20, .25, .46), (-.13, .49, -.02), (.13, .49, -.02), (.15, .43, .40), (-.15, .43, .40)], G, 'paint', .04, 3)
    cyl('Fuel cap', (0, .48, .12), (0, .51, .125), .04, G, 'metal', 12)
    # seat
    hull('Seat', [(-.15, .33, -.76), (.15, .33, -.76), (.14, .34, .02), (-.14, .34, .02), (-.15, .455, -.74), (.15, .455, -.74), (.12, .445, .0), (-.12, .445, .0)], G, 'rubber', .04, 3)
    # lamps: two headlamps in the nose, a tail lamp each side of the tail
    for s in (-1, 1):
        hull('Lamp housing', [(s * .09, .17, 1.13), (s * .25, .17, 1.12), (s * .25, .17, 1.17), (s * .09, .17, 1.18),
                              (s * .09, .27, 1.10), (s * .24, .27, 1.09), (s * .24, .27, 1.14), (s * .09, .27, 1.15)], G, 'engine', .01, 1)
        hull('Headlamp', [(s * .10, .18, 1.17), (s * .24, .18, 1.16), (s * .24, .18, 1.185), (s * .10, .18, 1.195),
                          (s * .10, .26, 1.145), (s * .23, .26, 1.135), (s * .23, .26, 1.16), (s * .10, .26, 1.17)], G, 'lamp', .008, 1)
        box('Tail lamp', (s * .20, .25, -1.185), (.12, .06, .03), G, 'tail', .008)
    box('Grille', (0, .12, 1.175), (.30, .09, .03), G, 'engine', .01)
    # racks (tube) and bumpers
    for z0, z1, y, name in ((.62, 1.06, .38, 'Front rack'), (-.70, -1.12, .41, 'Rear rack')):
        for s in (-1, 1):
            cyl(name, (s * .44, y, z0), (s * .44, y, z1), .016, G, 'metal', 8)
            cyl(name + ' leg', (s * .44, y, z0), (s * .36, y - .10, z0), .014, G, 'metal', 6)
            cyl(name + ' leg', (s * .44, y, z1), (s * .36, y - .12, z1), .014, G, 'metal', 6)
        for z in (z0, (z0 + z1) / 2, z1): cyl(name + ' bar', (-.44, y, z), (.44, y, z), .016, G, 'metal', 8)
    for s in (-1, 1):
        cyl('Front bumper', (s * .24, -.02, 1.16), (s * .22, .18, 1.25), .024, G, 'engine', 8)
        cyl('Rear bumper', (s * .24, .0, -1.16), (s * .22, .14, -1.25), .024, G, 'engine', 8)
    cyl('Front bumper bar', (-.24, .18, 1.25), (.24, .18, 1.25), .024, G, 'engine', 8)
    cyl('Front bumper low', (-.26, -.02, 1.16), (.26, -.02, 1.16), .024, G, 'engine', 8)
    cyl('Rear bumper bar', (-.24, .14, -1.25), (.24, .14, -1.25), .024, G, 'engine', 8)
    box('Hitch', (0, -.02, -1.24), (.06, .05, .10), G, 'metal', .008)
    # ---- handlebars (steer) ----
    F = 'Front'
    cyl('Steering column', FRONT_PIVOT, FRONT_TOP, .024, F, 'metal', 10)
    box('Bar clamp', (0, .715, .345), (.10, .045, .06), F, 'engine', .01)
    cyl('Handlebar', (-.36, .735, .355), (.36, .735, .355), .015, F, 'metal', 10)
    for s in (-1, 1):
        cyl('Grip', (s * .355, .737, .356), (s * .50, .745, .36), .022, F, 'rubber', 10)
        box('Lever', (s * .40, .74, .41), (.12, .012, .02), F, 'metal', .004, (0, s * 15, 0))
        box('Hand guard', (s * .47, .75, .41), (.06, .07, .09), F, 'paint', .015)
    cyl('Bar pad', (-.10, .755, .356), (.10, .755, .356), .028, F, 'engine', 10)
    hull('Bar pod', [(-.12, .62, .40), (.12, .62, .40), (.10, .62, .52), (-.10, .62, .52), (-.11, .72, .38), (.11, .72, .38), (.09, .70, .50), (-.09, .70, .50)], F, 'paint', .02, 2)
    box('Pod lamp', (0, .665, .522), (.12, .045, .02), F, 'lamp', .006)


def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    atv()
    pos = {'WheelFL': (-TRACK, WY, AXLE), 'WheelFR': (TRACK, WY, AXLE), 'WheelRL': (-TRACK, WY, -AXLE), 'WheelRR': (TRACK, WY, -AXLE)}
    for g, p in pos.items(): wheel(g, p, p[0] < 0)
    out = kit.join_all()
    for ob in out:
        g = ob.name.split('__')[0]
        if g in pos: kit.set_origin(ob, pos[g])
    return out


if __name__ == '__main__':
    build(); kit.export('TrailFour')
