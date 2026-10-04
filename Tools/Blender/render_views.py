"""0.73 Part D: render the Needle 600 source model from front, side, three-quarter and top (Eevee, soft studio light).

Run:  blender --background SourceArt/Blender/Needle600.blend --python Tools/Blender/render_views.py -- <out_dir> <tag>
"""
import bpy, math, sys, os
from mathutils import Vector

out, tag = sys.argv[sys.argv.index('--') + 1], sys.argv[sys.argv.index('--') + 2]
os.makedirs(out, exist_ok=True)
sc = bpy.context.scene
sc.render.engine = 'BLENDER_EEVEE'
sc.eevee.taa_render_samples = 32; sc.eevee.use_gtao = True; sc.eevee.use_soft_shadows = True
sc.render.resolution_x, sc.render.resolution_y = 1000, 760
sc.render.film_transparent = False
world = bpy.data.worlds.new('studio'); sc.world = world; world.use_nodes = True
world.node_tree.nodes['Background'].inputs['Color'].default_value = (.62, .68, .76, 1); world.node_tree.nodes['Background'].inputs['Strength'].default_value = .8
# key + fill sun lights
for name, rot, energy in (('key', (50, 0, 35), 3.2), ('fill', (65, 0, -140), 1.0)):
    l = bpy.data.lights.new(name, 'SUN'); l.energy = energy; l.angle = math.radians(8)
    o = bpy.data.objects.new(name, l); sc.collection.objects.link(o); o.rotation_euler = [math.radians(a) for a in rot]
# ground plane (Blender z = Unity y; wheels touch at Unity y = -0.53)
bpy.ops.mesh.primitive_plane_add(size=8, location=(0, 0, -.53)); g = bpy.context.object
gm = bpy.data.materials.new('ground'); gm.use_nodes = True; gm.node_tree.nodes['Principled BSDF'].inputs['Base Color'].default_value = (.55, .58, .52, 1); g.data.materials.append(gm)
cam = bpy.data.cameras.new('cam'); co = bpy.data.objects.new('cam', cam); sc.collection.objects.link(co); sc.camera = co
target = Vector((0, 0, .25))
# Blender space: +y = vehicle forward, +x = vehicle right, +z = up
views = {'front': Vector((0, 4.2, .45)), 'side': Vector((4.4, 0, .35)), 'three-quarter': Vector((3.0, 3.0, 1.2)), 'top': Vector((0, -.001, 5.0)),
         'rear-three-quarter': Vector((-2.8, -3.0, 1.0)), 'rider-close': Vector((1.3, 1.6, 1.25))}
for name, eye in views.items():
    co.location = eye; t = Vector((0, .25, .95)) if name == 'rider-close' else target
    d = (t - eye); co.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    cam.lens = 70 if name == 'rider-close' else 50
    if name == 'top': cam.type = 'ORTHO'; cam.ortho_scale = 2.8
    else: cam.type = 'PERSP'
    sc.render.filepath = os.path.join(out, f'{tag}-{name}.png'); bpy.ops.render.render(write_still=True)
print('RENDERED', out)
