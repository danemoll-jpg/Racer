"""0.87 Part A: Workbench previews of SourceArt/Blender/KylesHouse.blend (photo angle, left, back, right). Output dir after --."""
import bpy, math, os, sys
from mathutils import Vector
out = sys.argv[sys.argv.index('--') + 1]
bpy.ops.wm.open_mainfile(filepath=os.path.join(os.path.dirname(__file__), '..', '..', 'SourceArt', 'Blender', 'KylesHouse.blend'))
sc = bpy.context.scene; sc.render.engine = 'BLENDER_WORKBENCH'; sc.display.shading.light = 'STUDIO'; sc.display.shading.color_type = 'MATERIAL'
sc.render.resolution_x, sc.render.resolution_y = 1400, 900
cam = bpy.data.objects.new('cam', bpy.data.cameras.new('cam')); sc.collection.objects.link(cam); sc.camera = cam; cam.data.lens = 28
def B(x, y, z): return Vector((x, z, y))
for name, eye, look in [('photo', (-9, 2.2, 20), (-1, 1.5, 0)), ('left', (24, 3, 4), (5, -.5, 0)), ('back', (6, 4, -22), (-1, 0, -3)), ('right', (-25, 3, -3), (-6, 0, -1)), ('top', (0, 40, 1), (0, 0, 0))]:
    cam.location = B(*eye); d = B(*look) - B(*eye); cam.rotation_euler = d.to_track_quat('-Z', 'Y').to_euler()
    sc.render.filepath = os.path.join(out, 'kyle-preview-' + name + '.png'); bpy.ops.render.render(write_still=True)
