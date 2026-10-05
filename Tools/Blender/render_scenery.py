"""0.78 Part A: render a scenery kit sheet (Eevee, soft daylight) from a SourceArt/Blender/Scenery*.blend.

Run:  blender --background SourceArt/Blender/SceneryTrees.blend --python Tools/Blender/render_scenery.py -- <out_dir> trees
Each tree variant is assembled the way the game fits it to an old tree (trunk 5 m tall and 0.5 m wide, crown 6 m tall
and 3 m in radius from 3.4 m up; bushes 1.4 m tall, 1.3 m radius) and coloured like the game (leaf green, bark brown).
Near and far versions stand side by side; one image per variant plus a group view.
"""
import bpy, math, sys, os
from mathutils import Vector, Matrix

a = sys.argv[sys.argv.index('--') + 1:]
out, kind = a[0], a[1]; os.makedirs(out, exist_ok=True)
sc = bpy.context.scene
LEAF = {'Round': (.20, .33, .13), 'Oak': (.17, .29, .11), 'Maple': (.27, .36, .14), 'Pine': (.10, .22, .12), 'Poplar': (.26, .38, .15), 'Young': (.30, .43, .17),
        'Bush': (.18, .30, .12), 'Shrub': (.22, .32, .14)}
BARK = (.20, .15, .10)


def material(name, c):
    m = bpy.data.materials.get('R ' + name) or bpy.data.materials.new('R ' + name); m.use_nodes = True
    b = m.node_tree.nodes['Principled BSDF']; b.inputs['Base Color'].default_value = (*c, 1); b.inputs['Roughness'].default_value = .8
    return m


objs = {o.name: o for o in sc.objects if o.type == 'MESH'}
variants = sorted({n.split('_')[0] for n in objs})
for o in objs.values(): o.hide_render = True
placed = []


def place(name, M4, mat):
    o = objs.get(name)
    if not o: return
    c = o.copy(); c.data = o.data; sc.collection.objects.link(c); c.matrix_world = M4; c.hide_render = False
    c.material_slots[0].link = 'OBJECT'; c.material_slots[0].material = mat; placed.append(c)


def tree(v, x, far):
    leaf = material(v, LEAF.get(v, (.2, .32, .13))); bark = material('bark', BARK); bush = v in ('Bush', 'Shrub')
    T = '_TrunkFar' if far else '_Trunk'; C = '_CrownFar' if far else '_Crown'
    if bush: crown = Matrix.Translation((x, 0, 0)) @ Matrix.Diagonal((1.3, 1.3, 1.4, 1))
    else:
        place(v + T + '__bark', Matrix.Translation((x, 0, 0)) @ Matrix.Diagonal((.5, .5, 5, 1)), bark)
        crown = Matrix.Translation((x, 0, 3.4)) @ Matrix.Diagonal((3, 3, 6, 1))
    place(v + C + '__leaf', crown, leaf); place(v + C + '__bark', crown, bark)


sc.render.engine = 'BLENDER_EEVEE'; sc.eevee.taa_render_samples = 32; sc.eevee.use_gtao = True; sc.eevee.use_soft_shadows = True
sc.render.resolution_x, sc.render.resolution_y = 900, 700
world = bpy.data.worlds.new('sky'); sc.world = world; world.use_nodes = True
world.node_tree.nodes['Background'].inputs['Color'].default_value = (.55, .68, .85, 1); world.node_tree.nodes['Background'].inputs['Strength'].default_value = .9
l = bpy.data.lights.new('sun', 'SUN'); l.energy = 3.5; l.angle = math.radians(6); s = bpy.data.objects.new('sun', l); sc.collection.objects.link(s); s.rotation_euler = (math.radians(50), 0, math.radians(30))
bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, 0)); g = bpy.context.object; g.data.materials.append(material('ground', (.30, .38, .22)))
cam = bpy.data.cameras.new('cam'); co = bpy.data.objects.new('cam', cam); sc.collection.objects.link(co); sc.camera = co; cam.lens = 50


def shoot(name, eye, target):
    co.location = Vector(eye); co.rotation_euler = (Vector(target) - Vector(eye)).to_track_quat('-Z', 'Y').to_euler()
    sc.render.filepath = os.path.join(out, name + '.png'); bpy.ops.render.render(write_still=True)


for v in variants:
    bush = v in ('Bush', 'Shrub'); gap = 1.8 if bush else 4
    tree(v, -gap, False); tree(v, gap, True)
    shoot(f'{kind}-{v}', (0, -24 if not bush else -9, 5 if not bush else 2.2), (0, 0, 4.6 if not bush else .7))
    for c in placed: bpy.data.objects.remove(c)
    placed.clear()
# group: a little grove of near versions
for i, v in enumerate(variants): tree(v, (i - len(variants) / 2) * 6.5, False)
shoot(f'{kind}-group', (0, -38, 9), (0, 0, 4))
print('RENDERED', out)
