"""0.78 Part A: the tree and bush kit for the world scenery upgrade, generated in Blender from this script.

Run:  blender --background --factory-startup --python Tools/Blender/scenery_trees.py

Every part is built in a NORMALISED space (Unity axes: x right, y up, z forward) so the game can fit it to each existing
tree (Racer.SceneryTrees): the trunk to the trunk collider, the crown to the old crown's bounds.
  Trunk  y 0..1 (ground to the top of the trunk collider), radius 0.5 at the base (diameter 1 = the collider's width)
  Crown  y 0..1 (bottom to top of the old crown), x and z -1..1 (the old crown's radius); it includes the branches
         that reach from the trunk into the leaves (slot bark)
  Bush   the same as a crown, standing on the ground (no trunk)
Objects: "<Variant>_<Part>__<slot>", Part = Trunk / Crown (near) / CrownFar / TrunkFar (simplified, far away);
slot = leaf / bark. Faceted (flat-shaded) low-poly, the style of the 0.73-0.75 vehicles. The game colours the slots
(leaf tint per tree, bark per species) and sways the leaves in its foliage shader.
Output SourceArt/Blender/SceneryTrees.blend and Assets/Resources/Scenery/SceneryTrees.fbx.
"""
import bpy, bmesh, math, random, sys, os
from mathutils import Vector, Matrix, noise
sys.path.insert(0, os.path.dirname(__file__))
import kit
from kit import B

VARIANTS = ('Round', 'Oak', 'Maple', 'Pine', 'Poplar', 'Young', 'Bush', 'Shrub')


def flat(ob):
    for p in ob.data.polygons: p.use_smooth = False


def part(name, bm, slot):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    ob = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(ob)
    ob.data.materials.append(kit.mat('paint' if slot == 'leaf' else 'trim')); flat(ob)
    kit.PARTS.append((ob, name.split('__')[0] if '__' in name else name, slot)); return ob


def lobe(bm, c, r, sub=1, jitter=.16, seed=0, squash=1.0):
    jitter *= .75 if sub >= 2 else 1
    """A faceted leaf clump: an icosphere at c (Unity, normalised space) with radii r, its vertices pushed in and out."""
    rnd = random.Random(seed); geom = bmesh.ops.create_icosphere(bm, subdivisions=sub, radius=1)
    vs = geom['verts']
    for v in vs:
        k = 1 + (rnd.random() - .5) * 2 * jitter
        if v.co.z < -.3: k *= squash  # flatter underside
        v.co = Vector((v.co.x * r[0] * k, v.co.y * r[2] * k, v.co.z * r[1] * k)) + B(*c)
    return vs


def cone(bm, base, top, r, seg=7, seed=0, jag=.18):
    """A pine tier: an open-bottomed jagged cone (Unity coordinates)."""
    rnd = random.Random(seed); ring = []
    for i in range(seg):
        a = i * 2 * math.pi / seg + rnd.random() * .3; rr = r * (1 + (rnd.random() - .5) * jag)
        ring.append(bm.verts.new(B(base[0] + math.cos(a) * rr, base[1] - rnd.random() * .03, base[2] + math.sin(a) * rr)))
    inner = []
    for i in range(seg):
        a = (i + .5) * 2 * math.pi / seg; rr = r * .55
        inner.append(bm.verts.new(B(base[0] + math.cos(a) * rr, base[1] + (top[1] - base[1]) * .12, base[2] + math.sin(a) * rr)))
    tip = bm.verts.new(B(*top))
    for i in range(seg):
        j = (i + 1) % seg
        bm.faces.new((ring[i], ring[j], tip))
        bm.faces.new((ring[j], ring[i], inner[i]))  # underside, so it is solid from below


def branch(bm, p0, p1, r0, r1, seg=5):
    """A tapered bark limb between two Unity points (open ends; they sit inside the trunk / leaves)."""
    a, b = B(*p0), B(*p1); d = (b - a).normalized()
    up = Vector((0, 0, 1)) if abs(d.z) < .9 else Vector((1, 0, 0)); u = d.cross(up).normalized(); w = d.cross(u)
    r0v, r1v = [], []
    for i in range(seg):
        t = i * 2 * math.pi / seg; o = u * math.cos(t) + w * math.sin(t)
        r0v.append(bm.verts.new(a + o * r0)); r1v.append(bm.verts.new(b + o * r1))
    for i in range(seg):
        j = (i + 1) % seg; bm.faces.new((r0v[i], r0v[j], r1v[j], r1v[i]))
    bm.faces.new(list(reversed(r1v)))


def trunk(name, seg, flare=.18, taper=.62, bend=0.0, seed=0, far=False):
    """Normalised trunk: y 0..1, base radius .5 tapering to .5 * taper, a slight root flare and lean."""
    bm = bmesh.new(); rnd = random.Random(seed); rows = 2 if far else 5; rings = []
    for k in range(rows + 1):
        t = k / rows; r = .5 * (1 - (1 - taper) * t) * (1 + flare * max(0, .25 - t) / .25)
        off = bend * t * t; ring = []
        for i in range(seg):
            a = i * 2 * math.pi / seg; rr = r * (1 + (rnd.random() - .5) * .08 * (not far))
            ring.append(bm.verts.new(B(math.cos(a) * rr + off, t, math.sin(a) * rr)))
        rings.append(ring)
    for k in range(rows):
        for i in range(seg):
            j = (i + 1) % seg; bm.faces.new((rings[k][i], rings[k][j], rings[k + 1][j], rings[k + 1][i]))
    bm.faces.new(list(reversed(rings[-1])))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return part(name, bm, 'bark')


def finish_crown(name, bm):
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    return part(name, bm, 'leaf')


def fit_unit(objs, ground=False):
    """Scale and move the crown parts so all of them together span x, z -1..1 and y 0..1 (normalised crown space)."""
    pts = [ob.matrix_world @ v.co for ob in objs for v in ob.data.vertices]
    lo = Vector([min(p[i] for p in pts) for i in range(3)]); hi = Vector([max(p[i] for p in pts) for i in range(3)])
    c = (lo + hi) / 2; s = hi - lo  # Blender: x, y (= Unity z), z (= Unity y)
    rxy = max(s.x, s.y) / 2
    M4 = Matrix.Diagonal((1 / rxy, 1 / rxy, 1 / s.z, 1)) @ Matrix.Translation(Vector((-c.x, -c.y, -lo.z)))
    for ob in objs: ob.data.transform(M4)


def crown(variant, far=False):
    """Leaf clumps (and bark branches when near) for one variant, in an unnormalised working space; returns objects."""
    seed = VARIANTS.index(variant) * 101 + (7 if far else 0); rnd = random.Random(seed)
    leaf = bmesh.new(); bark = bmesh.new(); sub = 1 if far else 2
    if variant == 'Round':      # a full rounded broadleaf: a dome of clumps
        lobe(leaf, (0, .62, 0), (.62, .42, .62), sub, .14, seed)
        for i in range(0 if far else 6):
            a = i * math.pi / 3 + .3; lobe(leaf, (math.cos(a) * .55, .45 + rnd.random() * .12, math.sin(a) * .55), (.36, .3, .36), sub, .18, seed + i + 1)
        if far:
            for i in range(3): a = i * 2.1; lobe(leaf, (math.cos(a) * .45, .42, math.sin(a) * .45), (.42, .32, .42), 0, .12, seed + i + 1)
        for i in range(0 if far else 4):
            a = i * math.pi / 2 + .5; branch(bark, (0, .05, 0), (math.cos(a) * .45, .42, math.sin(a) * .45), .05, .02)
    elif variant == 'Oak':      # old, wide and spreading: low flat clumps on thick limbs
        n = 4 if far else 8
        for i in range(n):
            a = i * 2 * math.pi / n + rnd.random() * .4; d = .45 + rnd.random() * .3
            lobe(leaf, (math.cos(a) * d, .40 + rnd.random() * .2, math.sin(a) * d), (.42, .26, .42), sub, .2, seed + i, .7)
        lobe(leaf, (0, .6, 0), (.55, .32, .55), sub, .15, seed + 50)
        for i in range(0 if far else 5):
            a = i * 2 * math.pi / 5 + .2; branch(bark, (0, .0, 0), (math.cos(a) * .62, .38, math.sin(a) * .62), .09, .03)
    elif variant == 'Maple':    # irregular: three big lobes, leaning clumps
        for i, (x, y, z, r) in enumerate(((0, .62, 0, .5), (-.48, .42, .12, .42), (.45, .45, -.15, .44), (.1, .35, .5, .36), (-.12, .4, -.48, .34))):
            if far and i > 2: break
            lobe(leaf, (x, y, z), (r, r * .8, r), sub, .2, seed + i)
        for i in range(0 if far else 3):
            a = i * 2.1 + .4; branch(bark, (0, .05, 0), (math.cos(a) * .4, .45, math.sin(a) * .4), .05, .02)
    elif variant == 'Pine':     # conifer: stacked jagged tiers to a point
        tiers = 3 if far else 5
        for i in range(tiers):
            t = i / tiers; base = .05 + t * .78; r = .95 * (1 - t) + .18
            cone(leaf, (0, base, 0), (0, min(1.0, base + .42), 0), r, 6 if far else 8, seed + i)
    elif variant == 'Poplar':   # tall and narrow
        for i in range(3 if far else 6):
            t = i / (2 if far else 5); w = .40 * (1 - .45 * abs(t - .35)); lobe(leaf, ((rnd.random() - .5) * .14, .18 + t * .64, (rnd.random() - .5) * .14), (w, .30, w), sub, .12, seed + i)
        if not far: branch(bark, (0, .0, 0), (0, .7, 0), .06, .02)
    elif variant == 'Young':    # a young tree: a few small clumps
        for i, (x, y, z) in enumerate(((0, .6, 0), (-.3, .4, .1), (.28, .45, -.1))):
            lobe(leaf, (x, y, z), (.45, .38, .45), sub, .18, seed + i)
        if not far: branch(bark, (0, 0, 0), (-.25, .4, .08), .04, .015); branch(bark, (0, 0, 0), (.24, .42, -.08), .04, .015)
    elif variant == 'Bush':     # a round bush on the ground
        for i, (x, y, z, r) in enumerate(((0, .45, 0, .55), (-.45, .32, .15, .42), (.42, .3, -.1, .44), (.05, .3, .45, .38))):
            if far and i > 1: break
            lobe(leaf, (x, y, z), (r, r * .85, r), sub, .2, seed + i, .6)
    elif variant == 'Shrub':    # a lower, spikier shrub
        for i in range(3 if far else 5):
            a = i * 2 * math.pi / 5; lobe(leaf, (math.cos(a) * .35, .3 + rnd.random() * .15, math.sin(a) * .35), (.32, .4, .32), sub, .22, seed + i, .5)
    tag = 'CrownFar' if far else 'Crown'
    objs = [finish_crown(f'{variant}_{tag}__leaf', leaf)]
    if len(bark.verts): bmesh.ops.recalc_face_normals(bark, faces=bark.faces); objs.append(part(f'{variant}_{tag}__bark', bark, 'bark'))
    else: bark.free()
    fit_unit(objs)
    return objs


def build():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    shape = {'Round': (.18, .6, 0), 'Oak': (.28, .7, .03), 'Maple': (.2, .6, .04), 'Pine': (.12, .45, 0), 'Poplar': (.12, .55, 0), 'Young': (.1, .65, .05)}
    for v in VARIANTS:
        crown(v); crown(v, True)
        if v in shape:
            fl, tp, bd = shape[v]
            trunk(f'{v}_Trunk__bark', 7, fl, tp, bd, VARIANTS.index(v)); trunk(f'{v}_TrunkFar__bark', 5, fl * .5, tp, 0, 0, True)
    objs = []
    for ob, g, s in kit.PARTS: ob.name = ob.data.name = ob.name  # names already "<Variant>_<Part>__<slot>"
    for ob, g, s in kit.PARTS:
        bpy.context.view_layer.objects.active = ob; bpy.ops.object.select_all(action='DESELECT'); ob.select_set(True)
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True); objs.append(ob)
    kit.PARTS.clear()
    return objs


if __name__ == '__main__':
    objs = build()
    report = [f'{ob.name}: {kit.tris([ob])} triangles' for ob in sorted(objs, key=lambda o: o.name)]
    print('TREES\n' + '\n'.join(report))
    os.makedirs(os.path.join(kit.ROOT, 'Assets', 'Resources', 'Scenery'), exist_ok=True)
    with open(os.path.join(kit.ROOT, 'SourceArt', 'Blender', 'SceneryTrees-check.txt'), 'w') as f: f.write('\n'.join(report) + '\n')
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(kit.ROOT, 'SourceArt', 'Blender', 'SceneryTrees.blend'))
    bpy.ops.export_scene.fbx(filepath=os.path.join(kit.ROOT, 'Assets', 'Resources', 'Scenery', 'SceneryTrees.fbx'), use_selection=False, object_types={'MESH'},
                             axis_forward='-Z', axis_up='Y', bake_space_transform=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                             mesh_smooth_type='FACE', use_mesh_modifiers=True, add_leaf_bones=False, path_mode='STRIP', use_custom_props=False)
