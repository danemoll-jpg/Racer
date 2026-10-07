"""0.88 Part D: the acorn reward, the Turf Rocket riding mower (a lawn tractor), generated in Blender from this script.

Run:  blender --background --factory-startup --python Tools/Blender/mower.py

Unity vehicle space as the other vehicles (x right, y up, z forward; ground at y = -0.53). Physics wheelbase 1.6 (axles at
z = +/-0.8). Small front wheels (radius 0.22) and large rear tyres (radius 0.38), both on the ground. The rider uses the cars'
pose (Tools/Blender/rider.py, pose Car) at the seat point H, with the steering wheel at H + (0, 0.36, 0.36) tilted 25 degrees,
exactly as in the cars, so the hands sit on its rim and follow it (0.85). The driver sits on the centre line: feet on the
footboards either side of the steering pedestal, the bonnet ahead of the toes. Output SourceArt/Blender/TurfRocket.blend and
Assets/Resources/VehicleModels/TurfRocket.fbx. Groups: Body, Steer, WheelFL / WheelFR / WheelRL / WheelRR. Slots: paint
(body colour), metal, chrome, engine, rubber, lamp, tail, interior. No make, badge or logo.
"""
import bpy, bmesh, math, sys, os
from mathutils import Vector
sys.path.insert(0, os.path.dirname(__file__))
import kit
from kit import B, box, hull, cyl, blob, strip, made, place, at

GROUND = -.53
H = Vector((0, .22, -.52))                      # seat point (the Car pose's hip)
AXLE = .80
RR, RF = .38, .22                               # rear / front wheel radius
TR, TF = .50, .40                               # rear / front track (wheel centres, half)


def wheel(group, pos, left, R, half, knobs):
    s0 = len(kit.PARTS)
    ry = .085 if R > .3 else .055
    kit.tyre(group, R - ry, half, ry, 28, 8, knobs=knobs, squared=.93, knob=(.8, .016, .05))
    rim = (R - ry) * .72
    cyl(group + ' rim', (-half * .8, 0, 0), (half * .8, 0, 0), rim, group, 'metal', 22, cap=False)
    cyl(group + ' dish', (.0, 0, 0), (half * .78, 0, 0), rim * .96, group, 'paint', 20, r1=rim * .55)
    cyl(group + ' hub', (half * .6, 0, 0), (half * .95, 0, 0), rim * .32, group, 'chrome', 12)
    for i in range(4):
        a = i * math.pi / 2 + math.pi / 4
        cyl(group + ' nut', (half * .9, math.cos(a) * rim * .2, math.sin(a) * rim * .2), (half * 1.02, math.cos(a) * rim * .2, math.sin(a) * rim * .2), .009, group, 'metal', 6)
    place(made(s0), at(pos, left))


def body():
    G = 'Body'
    # ---- chassis, axles, deck
    for s in (-1, 1):
        cyl('Frame rail', (s * .22, -.24, 1.02), (s * .22, -.24, -1.0), .03, G, 'engine', 8)
    cyl('Front axle', (-TF + .04, GROUND + RF, AXLE), (TF - .04, GROUND + RF, AXLE), .035, G, 'engine', 8)
    box('Front axle beam', (0, -.25, AXLE), (.18, .12, .10), G, 'engine', .02)
    cyl('Rear axle', (-TR + .06, GROUND + RR, -AXLE), (TR - .06, GROUND + RR, -AXLE), .045, G, 'engine', 8)
    box('Transaxle', (0, GROUND + RR, -AXLE + .02), (.34, .22, .26), G, 'engine', .03)
    # mowing deck slung under the middle, its side discharge chute out to the right
    hull('Mowing deck', [(-.56, -.43, -.42), (.56, -.43, -.42), (.56, -.43, .44), (-.56, -.43, .44),
                         (-.54, -.33, -.40), (.54, -.33, -.40), (.54, -.33, .42), (-.54, -.33, .42)], G, 'engine', .05, 2)
    for z in (-.16, .18): cyl('Deck spindle', (0, -.33, z), (0, -.28, z), .07, G, 'metal', 12)
    for s in (-1, 1): cyl('Deck roller', (s * .5, -.45, .46), (s * .38, -.45, .46), .03, G, 'metal', 8)
    hull('Discharge chute', [(.55, -.41, -.06), (.55, -.41, .26), (.78, -.42, .22), (.78, -.42, -.02),
                             (.55, -.31, -.06), (.55, -.31, .26), (.78, -.35, .22), (.78, -.35, -.02)], G, 'paint', .03, 2)
    box('Chute deflector lip', (.79, -.385, .10), (.03, .08, .26), G, 'engine', .01)
    for s in (-1, 1): cyl('Deck hanger', (s * .30, -.33, .30), (s * .22, -.20, .40), .015, G, 'metal', 6)
    # ---- bonnet (hood) with grille and headlights, the engine under it
    hull('Bonnet', [(-.31, -.14, .30), (.31, -.14, .30), (.27, -.14, 1.10), (-.27, -.14, 1.10),
                    (-.29, .34, .30), (.29, .34, .30), (.24, .22, 1.06), (-.24, .22, 1.06)], G, 'paint', .07, 3)
    box('Grille', (0, .03, 1.115), (.40, .24, .03), G, 'engine', .012)
    for i in range(5): box('Grille bar', (0, -.06 + i * .045, 1.13), (.38, .012, .012), G, 'chrome', .003)
    for s in (-1, 1):
        cyl('Headlight bezel', (s * .17, .19, 1.06), (s * .17, .19, 1.10), .058, G, 'chrome', 16)
        cyl('Headlight', (s * .17, .19, 1.095), (s * .17, .19, 1.11), .046, G, 'lamp', 16)
        for k in range(3): box('Bonnet vent', (s * .295, .10, .55 + k * .1), (.012, .05, .07), G, 'engine', .004)
    box('Front bumper bar', (0, -.12, 1.17), (.66, .06, .05), G, 'engine', .015)
    for s in (-1, 1): cyl('Bumper stay', (s * .25, -.14, 1.06), (s * .25, -.14, 1.15), .02, G, 'engine', 6)
    # ---- the driver's area: steering pedestal, footboards, the seat on its spring, rear fenders and deck
    hull('Steering pedestal', [(-.07, -.10, -.05), (.07, -.10, -.05), (.07, -.10, .30), (-.07, -.10, .30),
                               (-.06, .34, .10), (.06, .34, .10), (.07, .34, .30), (-.07, .34, .30)], G, 'paint', .03, 2)
    box('Dash pod', (0, .36, .19), (.20, .07, .16), G, 'interior', .025)
    for s in (-1, 1):
        hull('Footboard', [(s * .08, -.16, -.34), (s * .44, -.16, -.34), (s * .44, -.16, .30), (s * .08, -.16, .30),
                           (s * .08, -.09, -.34), (s * .44, -.09, -.34), (s * .44, -.09, .30), (s * .08, -.09, .30)], G, 'engine', .015, 1)
        box('Footboard mat', (s * .26, -.085, -.02), (.32, .012, .58), G, 'rubber', .004)
    # rear body: the tub under the seat and the two fenders over the big tyres
    hull('Rear tub', [(-.34, -.20, -1.06), (.34, -.20, -1.06), (.34, -.20, -.34), (-.34, -.20, -.34),
                      (-.34, .02, -1.04), (.34, .02, -1.04), (.34, .02, -.36), (-.34, .02, -.36)], G, 'paint', .04, 2)
    for s in (-1, 1):
        prof = [(-AXLE + math.cos(math.radians(a)) * (RR + .07), GROUND + RR + math.sin(math.radians(a)) * (RR + .07)) for a in range(10, 171, 16)]
        strip('Rear fender', prof, .27, .02, s * TR, G, 'paint')
        box('Fender deck', (s * .40, .045, -AXLE + .02), (.16, .03, .66), G, 'paint', .01)
        box('Reflector', (s * .45, -.02, -1.06), (.08, .05, .015), G, 'tail', .004)
    # seat: pan on a coil spring, cushion and a high back
    cyl('Seat spring', (0, .03, H.z), (0, H.y - .13, H.z), .075, G, 'chrome', 12)
    for k in range(3): cyl('Spring coil', (0, .05 + k * .035, H.z), (0, .065 + k * .035, H.z), .088, G, 'metal', 12)  # 0.89: clear of the seat pan's faces
    box('Seat pan', (0, H.y - .13, H.z + .02), (.50, .03, .44), G, 'engine', .01)
    hull('Seat cushion', [(-.25, H.y - .12, H.z - .22), (.25, H.y - .12, H.z - .22), (.25, H.y - .12, H.z + .23), (-.25, H.y - .12, H.z + .23),
                          (-.25, H.y - .02, H.z - .22), (.25, H.y - .02, H.z - .22), (.25, H.y - .03, H.z + .23), (-.25, H.y - .03, H.z + .23)], G, 'interior', .04, 2)
    hull('Seat back', [(-.25, H.y - .06, H.z - .30), (.25, H.y - .06, H.z - .30), (.25, H.y - .06, H.z - .17), (-.25, H.y - .06, H.z - .17),
                       (-.23, H.y + .46, H.z - .42), (.23, H.y + .46, H.z - .42), (.23, H.y + .46, H.z - .30), (-.23, H.y + .46, H.z - .30)], G, 'interior', .045, 2)
    for s in (-1, 1): cyl('Seat bolster', (s * .26, H.y - .02, H.z - .20), (s * .26, H.y + .10, H.z + .12), .035, G, 'interior', 8)
    # rear tow hitch
    box('Hitch plate', (0, -.22, -1.10), (.16, .05, .14), G, 'metal', .01)
    cyl('Hitch pin', (0, -.16, -1.13), (0, -.30, -1.13), .014, G, 'chrome', 8)
    # ---- steering column (fixed) and wheel (Steer: origin on the wheel centre, turns about the column), as in the cars
    t = math.radians(25); c = H + Vector((0, .36, .36)); u = Vector((0, math.cos(t), math.sin(t))); n = Vector((0, -math.sin(t), math.cos(t)))
    cyl('Steering column', tuple(c + n * .02), tuple(c + n * .32), .024, G, 'interior', 10)
    S = 'Steer'; bm = bmesh.new()
    seg, side, R, r = 28, 6, .175, .016
    vs = []
    for i in range(seg):
        a = i * 2 * math.pi / seg; row = []
        for j in range(side):
            b = j * 2 * math.pi / side; rr = R + r * math.cos(b)
            p = c + Vector((1, 0, 0)) * math.cos(a) * rr + u * math.sin(a) * rr + n * r * math.sin(b)
            row.append(bm.verts.new(B(*p)))
        vs.append(row)
    for i in range(seg):
        for j in range(side): bm.faces.new((vs[i][j], vs[(i + 1) % seg][j], vs[(i + 1) % seg][(j + 1) % side], vs[i][(j + 1) % side]))
    me = bpy.data.meshes.new('Steering rim'); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new('Steering rim', me); bpy.context.collection.objects.link(ob); kit.outward(ob)
    kit.finish(ob, S, 'rubber')
    cyl('Steering hub', tuple(c - n * .005), tuple(c + n * .05), .045, S, 'paint', 12)
    for ang in (90, 210, 330):
        a = math.radians(ang); cyl('Steering spoke', tuple(c), tuple(c + Vector((1, 0, 0)) * math.cos(a) * R + u * math.sin(a) * R), .012, S, 'metal', 6)
    return tuple(c)


def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    c = body()
    pos = {'WheelFL': (-TF, GROUND + RF, AXLE), 'WheelFR': (TF, GROUND + RF, AXLE), 'WheelRL': (-TR, GROUND + RR, -AXLE), 'WheelRR': (TR, GROUND + RR, -AXLE)}
    for g, p in pos.items():
        front = g.startswith('WheelF')
        wheel(g, p, p[0] < 0, RF if front else RR, .065 if front else .115, 0 if front else 22)
    out = kit.join_all()
    for ob in out:
        g = ob.name.split('__')[0]
        if g in pos: kit.set_origin(ob, pos[g])
        if g == 'Steer': kit.set_origin(ob, c)
    print('TurfRocket steering wheel centre', c)
    kit.export('TurfRocket')


if __name__ == '__main__':
    build()
