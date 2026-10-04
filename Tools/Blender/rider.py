"""0.75 Part B: the parametric rider, generated in Blender from this script and assembled in Unity from parts.

Run:  blender --background --factory-startup --python Tools/Blender/rider.py

One FBX (Assets/Resources/VehicleModels/Rider.fbx, source SourceArt/Blender/Rider.blend) holds every part in three poses.
Objects are named "<Pose>_<Category>_<Option>_<Body>__<slot>":
  Pose      Moto (Needle 600 vehicle space), Atv (Trail Four vehicle space), Car (relative to the driver's seat point H,
            which each car places; the steering-wheel grips are at H + (+/-0.147, 0.437, 0.396))
  Category  Base (head, face, neck, hands, shoes), Shirt (Tee / Long / Jacket), Pants (Jeans / Shorts),
            Hair (Short / Medium / Long / Ponytail: everything BELOW the hat band), HairTop (the same styles ABOVE the band;
            hidden under a hat, so hair and hat never pass through each other), Hat (FlatCap / Baseball / Beanie / Cowboy)
  Body      Man / Woman, or Any (hair and hats fit both: the head is the same size)
  slot      skin, hair, hat, trim, shirt, pants, shoes, eyes, pupil, mouth (the game colours them per rider)
The head is always upright and faces forward, so hair and hats are the same shapes in every pose. The hat band is a plane
through the head 5.5 cm above its centre, 12 degrees higher at the front; every hat covers the whole head above it.
"""
import bpy, bmesh, math, sys, os
from mathutils import Vector, Matrix
sys.path.insert(0, os.path.dirname(__file__))
import kit
from kit import B, blob, cyl, limb, box, hull, loft, ring

BAND_Y, BAND_TILT = .055, 12.0  # hat band: height above the head centre, tilt (front higher), degrees

# ---------------------------------------------------------------- poses (right side, x >= 0; mirrored for the left)
POSES = {
    # Needle 600 (0.73 rider): seated on the seat, leaning toward the bars, feet on the pegs.
    'Moto': dict(hip=(0, .53, -.30), S=(0, .975, -.10), head=(0, 1.225, -.005), elbow=(.29, .82, .17), hand=(.395, .735, .575),
                 hipj=(.10, .52, -.27), knee=(.205, .42, .07), ankle=(.205, .10, -.13), foot=(0, 0, 1), grip=(1, 0, 0)),
    # Trail Four: more upright, wide bars, feet on the footboards.
    'Atv': dict(hip=(0, .56, -.32), S=(0, 1.00, -.15), head=(0, 1.255, -.08), elbow=(.33, .86, .07), hand=(.42, .755, .36),
                hipj=(.10, .55, -.29), knee=(.27, .52, .03), ankle=(.31, .16, -.10), foot=(0, 0, 1), grip=(1, 0, 0)),
    # Cars (relative to the seat point H): leaning back a little, hands at ten to two, feet on the pedals.
    'Car': dict(hip=(0, 0, 0), S=(0, .50, -.10), head=(0, .74, -.045), elbow=(.25, .30, .13), hand=(.147, .437, .396),
                hipj=(.10, -.01, .02), knee=(.14, .12, .44), ankle=(.13, -.22, .62), foot=(0, .34, .94), grip=(.5, .1, .86)),
}
BODY = {  # Man / Woman: same height class, different build
    'Man': dict(sw=.195, hem=(.172, .118), waist=(.165, .11), chest=(.19, .122), upper=(.198, .11), top=(.178, .088), pelvis=(.17, .12, .15),
                arm=(.056, .047, .044, .034), thigh=(.075, .058), shin=(.056, .044), hand=1.0, shoe=1.0, neck=.048),
    'Woman': dict(sw=.17, hem=(.172, .112), waist=(.138, .097), chest=(.158, .107), upper=(.166, .097), top=(.152, .080), pelvis=(.182, .125, .155),
                  arm=(.050, .042, .039, .031), thigh=(.077, .056), shin=(.053, .041), hand=.9, shoe=.92, neck=.042),
}
SHIRTS, PANTS = ('Tee', 'Long', 'Jacket'), ('Jeans', 'Shorts')
HAIRS, HATS = ('Short', 'Medium', 'Long', 'Ponytail'), ('FlatCap', 'Baseball', 'Beanie', 'Cowboy')


def V(p): return Vector(p)
def lerp(a, b, t): return tuple(V(a) + (V(b) - V(a)) * t)
def M(p, s): return (p[0] * s, p[1], p[2])  # mirror to side s
def add(a, b): return tuple(V(a) + V(b))


# ---------------------------------------------------------------- body parts
def torso(G, P, body, slot, grow=0.0):
    """Shirt torso: horizontal elliptical rings along the spine from the hem to the collar."""
    hip, S = V(P['hip']), V(P['S'])
    secs = []
    for t, (w, d), fwd in ((.12, body['hem'], 0), (.35, body['waist'], 0), (.70, body['chest'], .012), (.92, body['upper'], .006)):
        c = hip + (S - hip) * t + Vector((0, 0, fwd)); secs.append(ring(tuple(c), w + grow, d + grow, 16, 'y'))
    top = S + Vector((0, .035, .0)); secs.append(ring(tuple(top), body['top'][0] + grow, body['top'][1] + grow, 16, 'y'))
    neck = S + Vector((0, .07, .012)); secs.append(ring(tuple(neck), .066 + grow * .6, .058 + grow * .6, 16, 'y'))
    loft(G + ' torso', secs, G, slot)
    return hip + (S - hip) * .70 + Vector((0, 0, .012))


def bust(G, P, body, slot, chest, grow=0.0):
    d = body['chest'][1] + grow
    for s in (-1, 1): blob(G + ' bust', (s * .055, chest.y - .015, chest.z + d * .62), (.064 + grow * .5, .058 + grow * .3, .055 + grow * .5), G, slot, 12, 8)


def arm_points(P, body, s):
    sh = add(M(P['S'], s), (s * body['sw'], -.02, 0)); el = M(P['elbow'], s); ha = M(P['hand'], s)
    wr = tuple(V(ha) - (V(ha) - V(el)).normalized() * .06)
    return sh, el, wr, ha


def shirt(pose, P, bodyname, kind):
    body = BODY[bodyname]; G = f'{pose}_Shirt_{kind}_{bodyname}'
    grow = .02 if kind == 'Jacket' else .006
    chest = torso(G, P, body, 'shirt', grow)
    if bodyname == 'Woman': bust(G, P, body, 'shirt', chest, grow)
    ua0, ua1, fa0, fa1 = body['arm']
    for s in (-1, 1):
        sh, el, wr, ha = arm_points(P, body, s)
        if kind == 'Tee':
            mid = lerp(sh, el, .48)
            blob(G + ' shoulder', sh, (ua0 + .012,) * 3, G, 'shirt', 12, 8)
            cyl(G + ' sleeve', sh, mid, ua0 + .012, G, 'shirt', 12, r1=ua0 + .014, cap=False)
            cyl(G + ' hem', lerp(sh, el, .44), mid, ua0 + .017, G, 'shirt', 12)
            limb(G + ' upper arm', lerp(sh, el, .4), el, ua0 * .95, ua1, G, 'skin', 10)
            limb(G + ' forearm', el, wr, fa0, fa1, G, 'skin', 10)
        else:
            g = .012 if kind == 'Jacket' else .004
            blob(G + ' shoulder', sh, (ua0 + g + .004,) * 3, G, 'shirt', 12, 8)
            limb(G + ' sleeve', sh, el, ua0 + g, ua1 + g, G, 'shirt', 12)
            limb(G + ' sleeve lower', el, wr, fa0 + g, fa1 + g, G, 'shirt', 12, ends=False)
            cuff = tuple(V(wr) - (V(wr) - V(el)).normalized() * .03)
            cyl(G + ' cuff', cuff, wr, fa1 + g + .006, G, 'trim' if kind == 'Jacket' else 'shirt', 12)
    hip, S = V(P['hip']), V(P['S'])
    if kind == 'Jacket':
        hem = hip + (S - hip) * .12
        loft(G + ' hem band', [ring(tuple(hem), body['hem'][0] + grow + .006, body['hem'][1] + grow + .006, 16, 'y'),
                               ring(tuple(hem + Vector((0, .045, 0))), body['hem'][0] + grow + .004, body['hem'][1] + grow + .004, 16, 'y')], G, 'trim')
        neck = S + Vector((0, .07, .012))
        loft(G + ' collar', [ring(tuple(neck + Vector((0, -.01, 0))), .084, .074, 16, 'y'), ring(tuple(neck + Vector((0, .05, -.006))), .078, .068, 16, 'y')], G, 'shirt')
        # zip down the front, on the surface of each ring
        pts = []
        for t, (w, d), fwd in ((.12, body['hem'], 0), (.35, body['waist'], 0), (.70, body['chest'], .012), (.92, body['upper'], .006)):
            c = hip + (S - hip) * t; dd = d + grow + (.03 if (bodyname == 'Woman' and t == .70) else 0)
            pts.append((0, c.y, c.z + fwd + dd + .002))
        pts.append((0, neck.y + .03, neck.z + .072))
        for a, b in zip(pts, pts[1:]): cyl(G + ' zip', a, b, .006, G, 'trim', 6)
    else:
        neck = S + Vector((0, .07, .012))
        loft(G + ' neckline', [ring(tuple(neck + Vector((0, -.012, 0))), .072, .064, 16, 'y'), ring(tuple(neck + Vector((0, .006, 0))), .07, .062, 16, 'y')], G, 'shirt')


def pants(pose, P, bodyname, kind):
    body = BODY[bodyname]; G = f'{pose}_Pants_{kind}_{bodyname}'
    px, py, pz = body['pelvis']; blob(G + ' seat', P['hip'], (px, py, pz), G, 'pants', 16, 10)
    t0, t1 = body['thigh']; s0, s1 = body['shin']
    for s in (-1, 1):
        hj, kn, an = M(P['hipj'], s), M(P['knee'], s), M(P['ankle'], s)
        if kind == 'Jeans':
            limb(G + ' thigh', hj, kn, t0, t1 + .002, G, 'pants', 12)
            limb(G + ' shin', kn, an, s0 + .004, s1 + .008, G, 'pants', 12, ends=False)
            hem = tuple(V(an) + (V(an) - V(kn)).normalized() * .01)
            cyl(G + ' hem', lerp(kn, an, .9), hem, s1 + .012, G, 'pants', 12)
        else:
            end = lerp(hj, kn, .66)
            limb(G + ' leg', hj, end, t0 + .004, t1 + .014, G, 'pants', 12, ends=False)
            blob(G + ' hip', hj, (t0 + .004,) * 3, G, 'pants', 12, 8)
            cyl(G + ' hem', lerp(hj, kn, .6), end, t1 + .018, G, 'pants', 12)
            limb(G + ' thigh', lerp(hj, kn, .55), kn, t1 + .006, t1, G, 'skin', 10)
            limb(G + ' shin', kn, an, s0, s1 - .002, G, 'skin', 10)
            cyl(G + ' sock', lerp(kn, an, .88), an, s1 + .002, G, 'eyes', 10)


def shoe(G, an, fwd, scale):
    f = V(fwd).normalized(); up = Vector((0, 1, 0)); up = (up - f * up.dot(f)).normalized(); side = f.cross(up)
    heel = V(an) - f * .065 * scale - up * .085 * scale; L, Wd, H = .27 * scale, .052 * scale, .115 * scale
    def P(a, b, c): return tuple(heel + f * a + side * b + up * c)
    hull(G + ' shoe', [P(0, -Wd, 0), P(0, Wd, 0), P(L, Wd * 1.05, 0), P(L, -Wd * 1.05, 0),
                       P(.01, -Wd * .95, H), P(.01, Wd * .95, H), P(L * .93, Wd, H * .48), P(L * .93, -Wd, H * .48)], G, 'shoes', .022, 2)
    hull(G + ' sole', [P(-.004, -Wd - .003, -.012), P(-.004, Wd + .003, -.012), P(L + .006, Wd * 1.05 + .003, -.012), P(L + .006, -Wd * 1.05 - .003, -.012),
                       P(-.004, -Wd - .003, .014), P(-.004, Wd + .003, .014), P(L + .006, Wd * 1.05 + .003, .014), P(L + .006, -Wd * 1.05 - .003, .014)], G, 'trim', .006, 1)


def base(pose, P, bodyname):
    body = BODY[bodyname]; G = f'{pose}_Base_All_{bodyname}'; H = V(P['head']); woman = bodyname == 'Woman'
    def h(x, y, z): return tuple(H + Vector((x, y, z)))
    blob(G + ' head', tuple(H), (.100, .118, .110), G, 'skin', 16, 10)
    blob(G + ' jaw', h(0, -.055, .025), (.072, .05, .07) if woman else (.082, .056, .078), G, 'skin', 14, 8)
    blob(G + ' nose', h(0, -.006, .104), (.013, .019, .017) if woman else (.015, .022, .019), G, 'skin', 10, 6)
    for s in (-1, 1):
        blob(G + ' ear', h(s * .101, -.005, -.01), (.017, .032, .025), G, 'skin', 10, 6)
        blob(G + ' eye', h(s * .038, .028, .092), (.022, .016, .011) if woman else (.021, .015, .011), G, 'eyes', 10, 6)
        blob(G + ' pupil', h(s * .038, .028, .101), (.009, .010, .005), G, 'pupil', 8, 5)
        if woman:
            box(G + ' brow', h(s * .040, .062, .099), (.044, .007, .010), G, 'hair', .003, (0, 0, s * -12))
            box(G + ' lashes', h(s * .058, .036, .095), (.014, .004, .005), G, 'pupil', .0015, (0, 0, s * 30))
        else:
            box(G + ' brow', h(s * .040, .062, .099), (.048, .012, .012), G, 'hair', .004, (0, 0, s * -8))
    blob(G + ' mouth', h(0, -.05, .097), (.027, .012, .013) if woman else (.030, .008, .012), G, 'mouth', 10, 6)
    S = V(P['S']); neck0 = S + Vector((0, .04, .005)); neck1 = H + Vector((0, -.09, -.03))
    cyl(G + ' neck', tuple(neck0), tuple(neck1), body['neck'], G, 'skin', 10)
    k = body['hand']
    for s in (-1, 1):
        sh, el, wr, ha = arm_points(P, body, s); d = (V(ha) - V(wr)).normalized()
        limb(G + ' hand', wr, tuple(V(ha) + d * .02 * k), .034 * k, .04 * k, G, 'skin', 10)
        blob(G + ' fist', tuple(V(ha) + d * .01), (.044 * k, .04 * k, .046 * k), G, 'skin', 12, 8)
        blob(G + ' thumb', tuple(V(ha) + Vector((-s * .02, .03, .0)) * k), (.016 * k, .016 * k, .024 * k), G, 'skin', 8, 6)
        shoe(G, M(P['ankle'], s), P['foot'], body['shoe'])


# ---------------------------------------------------------------- hair and hats (head frame: origin at the head centre)
def band_plane():
    t = math.radians(BAND_TILT); n = Vector((0, -math.sin(t), math.cos(t)))  # Blender space: (x, forward, up)
    return Vector((0, 0, BAND_Y)), n


def shell(name, c, r, G, cuts, solid=.008, flare=0.0):
    """An ellipsoid hair shell around the head, with cut planes removing the face and below the hairline; flare widens it
    below the head centre (a bob / long hair falling away from the face)."""
    bm = bmesh.new(); bmesh.ops.create_uvsphere(bm, u_segments=22, v_segments=14, radius=1)
    for v in bm.verts:
        if flare and v.co.z < 0: k = 1 + flare * (-v.co.z); v.co.x *= k; v.co.y *= k
    bmesh.ops.transform(bm, matrix=Matrix.Translation(B(*c)) @ Matrix.Diagonal((r[0], r[2], r[1], 1)), verts=bm.verts)
    for co, no in cuts:
        bmesh.ops.bisect_plane(bm, geom=bm.verts[:] + bm.edges[:] + bm.faces[:], plane_co=co, plane_no=no, clear_outer=True)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(ob)
    md = ob.modifiers.new('solid', 'SOLIDIFY'); md.thickness = solid; md.offset = -1
    return kit.finish(ob, G, 'hair')


def face_cut(y=.06, z=.075, ang=30, part=0.0):
    """Removes the face side of a hair shell; part > 0 tips the hairline so the hair sweeps lower on the left (a side part)."""
    a = math.radians(ang); return B(0, y, z), Vector((part, math.cos(a), -math.sin(a))).normalized()


def low_cut(a, b):
    """Keep y > a + b z (Unity, head frame): remove everything below a plane that dips toward the back."""
    n = Vector((0, b, -1)).normalized()  # Unity normal (0, -1, b) -> Blender (0, b, -1)
    return B(0, a, 0), n


def hair_parts(style, G):
    if style == 'Short':
        shell(G + ' shell', (0, .02, -.008), (.110, .113, .119), G, [face_cut(.052, .08, 30, .25), low_cut(-.02, .35)], .009)
    elif style == 'Medium':
        shell(G + ' shell', (0, .012, -.012), (.116, .124, .124), G, [face_cut(.045, .075, 36, .3), low_cut(-.095, .2)], .012, .35)
    elif style == 'Long':
        shell(G + ' shell', (0, .012, -.012), (.117, .124, .125), G, [face_cut(.045, .075, 36, -.3), low_cut(-.075, .2)], .012, .3)
        blob(G + ' fall', (0, -.15, -.072), (.108, .17, .055), G, 'hair', 14, 10, (-8, 0, 0))
        for s in (-1, 1): blob(G + ' lock', (s * .1, -.105, -.02), (.032, .13, .05), G, 'hair', 10, 8, (0, 0, s * 4))
    elif style == 'Ponytail':
        shell(G + ' shell', (0, .02, -.008), (.109, .113, .118), G, [face_cut(.06, .078, 28), low_cut(-.03, .35)])
        cyl(G + ' tie', (0, .0, -.112), (0, -.012, -.128), .026, G, 'trim', 10)
        limb(G + ' tail', (0, -.012, -.128), (0, -.10, -.165), .028, .034, G, 'hair', 10)
        limb(G + ' tail end', (0, -.10, -.165), (0, -.20, -.16), .034, .012, G, 'hair', 10)


def place(objs, M4):
    for ob in objs: ob.matrix_world = M4 @ ob.matrix_world


def hair(pose, P):
    H = V(P['head']); co, no = band_plane()
    for style in HAIRS:
        start = len(kit.PARTS); G = f'{pose}_Hair_{style}_Any'
        hair_parts(style, G)
        made = kit.PARTS[start:]; del kit.PARTS[start:]
        # join this style (modifiers applied), then split it at the hat band: Hair = below, HairTop = above
        bpy.ops.object.select_all(action='DESELECT')
        for ob, g, s in made:
            bpy.context.view_layer.objects.active = ob
            for md in list(ob.modifiers): bpy.ops.object.modifier_apply(modifier=md.name)
            ob.select_set(True)
        for slot in sorted({s for _, _, s in made}):
            obs = [ob for ob, g, s in made if s == slot]
            bpy.ops.object.select_all(action='DESELECT')
            for ob in obs: ob.select_set(True)
            bpy.context.view_layer.objects.active = obs[0]
            if len(obs) > 1: bpy.ops.object.join()
            low = bpy.context.view_layer.objects.active; bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
            top = low.copy(); top.data = low.data.copy(); bpy.context.collection.objects.link(top)
            for ob, outer in ((low, True), (top, False)):
                bm = bmesh.new(); bm.from_mesh(ob.data)
                bmesh.ops.bisect_plane(bm, geom=bm.verts[:] + bm.edges[:] + bm.faces[:], plane_co=co, plane_no=no, clear_outer=outer, clear_inner=not outer)
                bm.to_mesh(ob.data); bm.free()
            place([low, top], Matrix.Translation(B(*H)))
            kit.PARTS.append((low, G, slot))
            if len(top.data.polygons): kit.PARTS.append((top, f'{pose}_HairTop_{style}_Any', slot))
            else: bpy.data.objects.remove(top)


def dome(name, r, G, slot='hat', solid=.006, tip=0.0):
    """The upper half of an ellipsoid (hat frame: origin on the band, y up), open at the band."""
    bm = bmesh.new(); bmesh.ops.create_uvsphere(bm, u_segments=24, v_segments=16, radius=1)
    if tip:
        for v in bm.verts:
            if v.co.z > .6: v.co.z += tip * (v.co.z - .6) / .4
    bmesh.ops.transform(bm, matrix=Matrix.Diagonal((r[0], r[2], r[1], 1)), verts=bm.verts)
    bmesh.ops.bisect_plane(bm, geom=bm.verts[:] + bm.edges[:] + bm.faces[:], plane_co=(0, 0, 0), plane_no=(0, 0, 1), clear_inner=True)
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free(); ob = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(ob)
    md = ob.modifiers.new('solid', 'SOLIDIFY'); md.thickness = solid; md.offset = 1
    return kit.finish(ob, G, slot)


def brim(name, G, slot, inner, outer, a0, a1, n, lift, thick=.008, y0=0.0):
    """A brim (hat frame): ring sector between the inner ellipse and outer(angle), lifted by lift(x, z)."""
    v, f = [], []
    for i in range(n + 1):
        a = math.radians(a0 + (a1 - a0) * i / n); ca, sa = math.sin(a), math.cos(a)  # angle 0 = straight ahead (+z)
        ox, oz = outer(a)
        for (x, z) in ((inner[0] * ca, inner[1] * sa), (ox * ca, oz * sa)):
            for t in (0, 1): v.append(B(x, y0 + lift(x, z) + t * thick, z))
    for i in range(n):
        a, b = i * 4, (i + 1) * 4
        f += [(a + 0, a + 2, b + 2, b + 0), (a + 1, b + 1, b + 3, a + 3), (a + 2, a + 3, b + 3, b + 2), (a + 0, b + 0, b + 1, a + 1)]
    if a1 - a0 < 360: f += [(0, 1, 3, 2), (n * 4, n * 4 + 2, n * 4 + 3, n * 4 + 1)]
    ob = kit.mesh_ob(name, v, f); kit.outward(ob)
    return kit.finish(ob, G, slot, .003, 1)


def hats(pose, P):
    H = V(P['head'])
    hatframe = Matrix.Translation(B(*H) + B(0, BAND_Y, -.004)) @ Matrix.Rotation(math.radians(BAND_TILT), 4, 'X')
    for kind in HATS:
        start = len(kit.PARTS); G = f'{pose}_Hat_{kind}_Any'
        if kind == 'FlatCap':
            # one smooth lofted shape: snug at the band, swelling forward over the brim, flat top
            secs = [ring((0, -.002, 0), .127, .135, 24, 'y'), ring((0, .026, .012), .136, .152, 24, 'y'), ring((0, .052, .022), .134, .156, 24, 'y'),
                    ring((0, .068, .02), .112, .132, 24, 'y'), ring((0, .074, .016), .06, .07, 24, 'y')]
            loft(G + ' crown', secs, G, 'hat', cap='top')
            brim(G + ' brim', G, 'hat', (.10, .135), lambda a: (.125, .19), -60, 60, 12, lambda x, z: .004 - .3 * x * x, .009)
            blob(G + ' button', (0, .074, .016), (.014, .007, .014), G, 'hat', 8, 5)
        elif kind == 'Baseball':
            dome(G + ' crown', (.126, .098, .135), G)
            blob(G + ' button', (0, .097, .0), (.016, .009, .016), G, 'hat', 8, 5)
            brim(G + ' brim', G, 'hat', (.108, .13), lambda a: (.14, .235), -68, 68, 14, lambda x, z: .004 - .9 * x * x - .05 * max(z - .13, 0), .009)
            loft(G + ' band', [ring((0, -.002, 0), .128, .137, 24, 'y'), ring((0, .016, 0), .128, .137, 24, 'y')], G, 'hat', cap=False)
        elif kind == 'Beanie':
            dome(G + ' crown', (.127, .108, .136), G, tip=.08)
            loft(G + ' cuff', [ring((0, -.004, 0), .133, .142, 24, 'y'), ring((0, .044, 0), .132, .141, 24, 'y')], G, 'hat', cap=False)
        elif kind == 'Cowboy':
            secs = []
            for y, k in ((-.002, 1.0), (.04, .99), (.08, .965), (.115, .93), (.128, .86)):
                secs.append(ring((0, y, 0), .127 * k, .137 * k, 20, 'y'))
            secs.append(ring((0, .118, .004), .07, .09, 20, 'y'))  # the pinched crease on top
            loft(G + ' crown', secs, G, 'hat', cap='top')
            loft(G + ' hatband', [ring((0, .0, 0), .131, .141, 20, 'y'), ring((0, .03, 0), .13, .14, 20, 'y')], G, 'trim', cap=False)
            brim(G + ' brim', G, 'hat', (.125, .135), lambda a: (.205, .222), 0, 360, 32, lambda x, z: 1.7 * x * x - .012 * (abs(z) / .22), .01)
        place([ob for ob, g, s in kit.PARTS[start:]], hatframe)


# ---------------------------------------------------------------- build, check, export
def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    for pose, P in POSES.items():
        for bodyname in BODY:
            base(pose, P, bodyname)
            for k in SHIRTS: shirt(pose, P, bodyname, k)
            for k in PANTS: pants(pose, P, bodyname, k)
        hair(pose, P); hats(pose, P)
    return kit.join_all()


def covered(H, p, bvh):
    """The hat covers p: a hat surface lies on the ray from the head centre through p, beyond p."""
    d = p - H; u = d.normalized(); o, travelled = H.copy(), 0.0
    for _ in range(12):
        hit = bvh.ray_cast(o, u)
        if hit[0] is None: return False
        travelled += hit[3]
        if travelled > d.length + .001: return True
        o = hit[0] + u * 1e-5; travelled += 1e-5
    return False


def check(objs):
    """Hair and hats never pass through each other: (1) every HairTop vertex is above the band and every Hair vertex
    below it; (2) with each hat, every head-skin point above the band lies under the hat (nothing pokes through)."""
    from mathutils.bvhtree import BVHTree
    co, no = band_plane(); out = []
    by = {ob.name: ob for ob in objs}
    for pose, P in POSES.items():
        H = B(*P['head'])
        def side(v): return (v - H - co).dot(no)
        for st in HAIRS:
            lo, hi = by.get(f'{pose}_Hair_{st}_Any__hair'), by.get(f'{pose}_HairTop_{st}_Any__hair')
            bad = sum(1 for v in lo.data.vertices if side(v.co) > 1e-4) + (sum(1 for v in hi.data.vertices if side(v.co) < -1e-4) if hi else 0)
            out.append(f'{pose} hair {st}: split errors {bad}')
        for kind in HATS:
            hat = [o for n, o in by.items() if n.startswith(f'{pose}_Hat_{kind}_Any__') and not n.endswith('trim')]
            bm = bmesh.new()
            for o in hat: bm.from_mesh(o.data)
            bvh = BVHTree.FromBMesh(bm); bm.free()
            # sample the head skin above the band (head shells of both bodies) and the below-band hair near the band
            pts = []
            for b in BODY:
                head = by[f'{pose}_Base_All_{b}__skin']
                pts += [v.co for v in head.data.vertices if side(v.co) > .002 and (v.co - H).length < .14]
            outside = sum(1 for p in pts if not covered(H, p, bvh))
            out.append(f'{pose} hat {kind}: head points above band {len(pts)}, outside the hat {outside}')
    return out


def counts(objs):
    """Triangles of each combination's worst case per pose (base + heaviest shirt/pants/hair top+rest/hat)."""
    t = {ob.name: kit.tris([ob]) for ob in objs}
    def cat(pose, c, o, b): return sum(v for n, v in t.items() if n.startswith(f'{pose}_{c}_{o}_{b}__'))
    out = []
    for pose in POSES:
        for b in BODY:
            worst = cat(pose, 'Base', 'All', b) + max(cat(pose, 'Shirt', k, b) for k in SHIRTS) + max(cat(pose, 'Pants', k, b) for k in PANTS) \
                + max(cat(pose, 'Hair', k, 'Any') + cat(pose, 'HairTop', k, 'Any') for k in HAIRS) + max(cat(pose, 'Hat', k, 'Any') for k in HATS)
            out.append(f'{pose} {b}: worst-case rider triangles {worst}')
    return out


if __name__ == '__main__':
    objs = build()
    report = check(objs) + counts(objs)
    print('RIDER CHECK\n' + '\n'.join(report))
    with open(os.path.join(kit.ROOT, 'SourceArt', 'Blender', 'Rider-check.txt'), 'w') as f: f.write('\n'.join(report) + '\n')
    kit.export('Rider')
