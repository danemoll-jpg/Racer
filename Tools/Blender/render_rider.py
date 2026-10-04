"""0.75 Part B: render rider combinations from SourceArt/Blender/Rider.blend (Eevee, soft studio light).

Run:  blender --background SourceArt/Blender/Rider.blend --python Tools/Blender/render_rider.py -- <out_dir> [pose] [set]
Sets: bodies, hair, hats, hathair, shirts, pants, poses. Each image shows one look (same activation rules as the game).
"""
import bpy, math, sys, os
from mathutils import Vector

argv = sys.argv[sys.argv.index('--') + 1:]
out = argv[0]; POSE = argv[1] if len(argv) > 1 else 'Moto'; SET = argv[2] if len(argv) > 2 else 'all'
os.makedirs(out, exist_ok=True)
DEFAULT = dict(body='Man', hair='Short', hat='FlatCap', shirt='Tee', pants='Jeans', skin=(.78, .52, .36), haircol=(.18, .065, .028),
               hatcol=(.018, .021, .025), shirtcol=(.12, .36, .95), pantscol=(.10, .21, .48))
SETS = {
    'bodies': [dict(body='Man'), dict(body='Woman', hair='Long', hat='None', haircol=(.45, .13, .05), skin=(.91, .69, .53))],
    'hair': [dict(hat='None', hair=h, body=b) for b in ('Man', 'Woman') for h in ('Short', 'Medium', 'Long', 'Ponytail', 'Bald')],
    'hats': [dict(hat=h, hatcol=c) for h, c in (('None', None), ('FlatCap', (.018, .021, .025)), ('Baseball', (.9, .13, .09)), ('Beanie', (.96, .67, .08)), ('Cowboy', (.6, .19, .8)))],
    'hathair': [dict(hat=h, hair=r, body='Woman' if r in ('Long', 'Ponytail') else 'Man', hatcol=(.88, .9, .92)) for h in ('FlatCap', 'Baseball', 'Beanie', 'Cowboy') for r in ('Short', 'Medium', 'Long', 'Ponytail', 'Bald')],
    'shirts': [dict(shirt=s, body=b, shirtcol=c) for b in ('Man', 'Woman') for s, c in (('Tee', (.12, .36, .95)), ('Long', (.9, .13, .09)), ('Jacket', (.12, .64, .65)))],
    'pants': [dict(pants=p, body=b, pantscol=c) for b in ('Man', 'Woman') for p, c in (('Jeans', (.10, .21, .48)), ('Shorts', (.96, .67, .08)))],
}
# Blender space eye/target per pose: full body, head front three-quarter, head rear three-quarter (head close-ups for hair and hats)
HEAD = {'Moto': (0, -.005, 1.225), 'Atv': (0, -.08, 1.255), 'Car': (0, -.045, .74)}
FULL = {'Moto': ((1.9, 2.0, 1.2), (0, .05, .75)), 'Atv': ((1.9, 1.9, 1.2), (0, -.05, .78)), 'Car': ((1.7, 1.9, .55), (0, .15, .25))}
def head_views(pose):
    h = Vector(HEAD[pose]); return [(h + Vector((.42, .55, .12)), h), (h + Vector((.5, -.55, .14)), h)]


def active(name, L):
    head, slot = name.split('__'); pose, cat, opt, body = head.split('_')
    if pose != POSE or (body != 'Any' and body != L['body']): return False
    if cat == 'Base': return True
    if cat == 'Shirt': return opt == L['shirt']
    if cat == 'Pants': return opt == L['pants']
    if cat == 'Hair': return opt == L['hair']
    if cat == 'HairTop': return opt == L['hair'] and L['hat'] == 'None'
    if cat == 'Hat': return opt == L['hat']
    return False


def colour(slot, c):
    m = bpy.data.materials[slot]; b = m.node_tree.nodes['Principled BSDF']; b.inputs['Base Color'].default_value = (c[0], c[1], c[2], 1)


sc = bpy.context.scene
sc.render.engine = 'BLENDER_EEVEE'; sc.eevee.taa_render_samples = 32; sc.eevee.use_gtao = True; sc.eevee.use_soft_shadows = True
sc.render.resolution_x, sc.render.resolution_y = 520, 560
world = bpy.data.worlds.new('studio'); sc.world = world; world.use_nodes = True
world.node_tree.nodes['Background'].inputs['Color'].default_value = (.62, .68, .76, 1); world.node_tree.nodes['Background'].inputs['Strength'].default_value = .8
for name, rot, energy in (('key', (50, 0, 35), 3.2), ('fill', (65, 0, -140), 1.0)):
    l = bpy.data.lights.new(name, 'SUN'); l.energy = energy; l.angle = math.radians(8)
    o = bpy.data.objects.new(name, l); sc.collection.objects.link(o); o.rotation_euler = [math.radians(a) for a in rot]
cam = bpy.data.cameras.new('cam'); co = bpy.data.objects.new('cam', cam); sc.collection.objects.link(co); sc.camera = co; cam.lens = 60
meshes = [o for o in bpy.data.objects if o.type == 'MESH']
for setname, looks in SETS.items():
    if SET != 'all' and SET != setname: continue
    for i, delta in enumerate(looks):
        L = dict(DEFAULT); L.update({k: v for k, v in delta.items() if v is not None})
        if L['hair'] == 'Bald': L['hair'] = 'Bald'
        for o in meshes: o.hide_render = not active(o.name, L)
        colour('skin', L['skin']); colour('hair', L['haircol']); colour('hat', L['hatcol']); colour('shirt', L['shirtcol']); colour('pants', L['pantscol'])
        label = '-'.join(str(delta[k]) for k in delta if k in ('body', 'hair', 'hat', 'shirt', 'pants'))
        views = head_views(POSE) if setname in ('hair', 'hats', 'hathair') else [FULL[POSE]]
        for k, (eye, target) in enumerate(views):
            co.location = Vector(eye); co.rotation_euler = (Vector(target) - Vector(eye)).to_track_quat('-Z', 'Y').to_euler()
            sc.render.filepath = os.path.join(out, f'{POSE}-{setname}-{i:02d}{"ab"[k]}-{label}.png'); bpy.ops.render.render(write_still=True)
print('RENDERED', out)
