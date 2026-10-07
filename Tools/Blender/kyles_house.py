"""0.87 Part A: Kyle's house, from Dan's photo of the front (SourceArt/Reference/KylesHouse-front-2026-10-06.jpg) and his
description of the other sides. Stylized low-poly, to the standard of the 0.78 buildings.

House frame (Unity, metres): x = the house's right as it faces out of its front = the viewer's LEFT in the photo (north in the
world), y up, z = forward out of the front (west, towards the street). Origin = the site's transform (the front grade).
  Front (+z): long low ranch: grey lap siding, white trim, charcoal shutters; a front-facing gable wing on the photo's left
    third (+x), a covered porch under the main roof's eave across the rest (four slim white posts, low concrete slab, white
    door with storm door, wall lantern, double-hung windows), wicker chairs and a bench, brick foundation showing.
  Photo's right (-x): red brick chimney outside the end wall (stepped shoulder, metal cap) and a screened porch behind it.
  Photo's left (+x): two garage doors in the walk-out lower level, the driveway arriving there.
  Back (-z): the lower level exposed (walk-out); a raised wooden deck at the main floor with a sliding glass door and
    wooden steps down into the yard.
Run: F:\\blender\\blender.exe --background --factory-startup --python Tools/Blender/kyles_house.py
Writes SourceArt/Blender/KylesHouse.blend, Assets/Scenery/KylesHouse/KylesHouse.fbx (objects '<part>__<slot>') and
Assets/Scenery/KylesHouse/KylesHouse-colliders.txt (the collision shapes in the same frame) for the Unity side
(Tools/Report087/Report087KyleBuild.cs), which colours the model by slot for the Racer/Building shader.
"""
import bpy, math, os, sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import kit
from kit import B, box, hull, cyl

kit.COLORS.update({'siding': (.62, .64, .64), 'trimw': (.93, .93, .90), 'roof': (.33, .35, .37), 'brick': (.53, .30, .23),
                   'shutter': (.18, .19, .21), 'doorw': (.90, .90, .88), 'glass': (.12, .16, .20), 'concrete': (.60, .58, .55),
                   'deck': (.45, .33, .22), 'screen': (.17, .19, .19), 'garage': (.88, .88, .85), 'metal': (.45, .47, .48),
                   'lamp': (.95, .91, .70), 'wicker': (.92, .91, .86), 'cushion': (.55, .55, .50), 'gutter': (.95, .95, .93),
                   'glassdark': (.10, .12, .14), 'deckdark': (.28, .20, .13)})
for ob in list(bpy.data.objects): bpy.data.objects.remove(ob)

ROOT = kit.ROOT
OUT = os.path.join(ROOT, 'Assets', 'Scenery', 'KylesHouse')
# ---------------------------------------------------------------- dimensions
F = .45            # main floor
L = float(os.environ.get('KYLE_LOWER', '-2.45'))  # walk-out lower floor (garage, back yard)
PLATE = 3.2        # wall plate (top of the main-floor walls)
P = .38            # roof pitch (rise per run), ~21 degrees
X0, X1 = -7.6, 7.6  # main body, end walls (x)
Z0, Z1 = -4.6, 4.6  # main body, back and front walls (z)
WX0 = 2.2           # front wing from WX0 to X1, forward to WZ
WZ = 6.4
PX1 = .4            # porch from X0 to PX1, forward to PZ
PZ = 6.5
OH = .38            # eave / rake overhang
RIDGE = PLATE + Z1 * P
G = 'House'
COL = []            # collision shapes: ('box', name, centre, size) / ('prism', name, pts...)


def colbox(name, mn, mx):
    COL.append(('box', name, [(a + b) / 2 for a, b in zip(mn, mx)], [b - a for a, b in zip(mn, mx)]))


def slab(name, mn, mx, slot, bevel=.0):
    c = [(a + b) / 2 for a, b in zip(mn, mx)]; s = [max(.002, b - a) for a, b in zip(mn, mx)]
    return box(name, c, s, G, slot, bevel)


def quad(name, pts, slot, thick=0.0):
    """A flat panel through 4 Unity points (a closed hull with a small thickness along -normal)."""
    import mathutils
    a, b, c, d = [mathutils.Vector(p) for p in pts]
    n = (b - a).cross(d - a).normalized() * (thick or .02)
    pts8 = [tuple(p - n) for p in (a, b, c, d)] + [tuple(p) for p in (a, b, c, d)]
    return hull(name, pts8, G, slot, 0)


# ---------------------------------------------------------------- walls
def siding_wall(name, x0, x1, y0, y1, z0, z1, slot='siding'):
    slab(name, (x0, y0, z0), (x1, y1, z1), slot)
    # lap siding: shallow horizontal courses on the outside faces of the long walls
    if slot == 'siding':
        y = y0 + .2
        while y < y1 - .05:
            if abs(z1 - z0) < abs(x1 - x0):  # a wall running along x: courses on its outer z face
                zf = z1 if (z0 + z1) / 2 > 0 else z0; sgn = 1 if zf == z1 else -1
                slab(name + ' course', (x0, y, zf), (x1, y + .025, zf + sgn * .025), 'siding')
            else:
                xf = x1 if (x0 + x1) / 2 > 0 else x0; sgn = 1 if xf == x1 else -1
                slab(name + ' course', (xf, y, z0), (xf + sgn * .025, y + .025, z1), 'siding')
            y += .2

T = .16  # wall thickness
# main floor walls
siding_wall('front wall', X0, WX0, F, PLATE, Z1 - T, Z1)
siding_wall('back wall', X0, X1, F, PLATE, Z0, Z0 + T)
siding_wall('right end wall', X0, X0 + T, F, PLATE, Z0, Z1)
siding_wall('left end wall', X1 - T, X1, F, PLATE, Z0, WZ)
siding_wall('wing front wall', WX0, X1, F, PLATE, WZ - T, WZ)
siding_wall('wing right wall', WX0, WX0 + T, F, PLATE, Z1, WZ)
# lower level: exposed on the back and the left (siding), brick foundation on the front and right
siding_wall('lower back wall', X0, X1, L, F, Z0, Z0 + T)
siding_wall('lower left wall', X1 - T, X1, L, F, Z0, WZ)
slab('front foundation', (X0, L, Z1 - T), (WX0, F, Z1), 'brick')
slab('wing foundation', (WX0, L, WZ - T), (X1, F, WZ), 'brick')
slab('wing right foundation', (WX0, L, Z1), (WX0 + T, F, WZ), 'brick')
slab('right foundation', (X0, L, Z0), (X0 + T, F, Z1), 'brick')
# a band of brick under the siding on the exposed sides too (as at the bottom left of the photo)
slab('left brick band', (X1, F - .32, Z0 + .3), (X1 + .03, F, WZ - .2), 'brick')
# gable triangles (end walls up to the ridge, wing front gable)
for x, sgn in ((X0, -1), (X1, 1)):
    hull('gable ' + str(sgn), [(x, PLATE, Z0), (x + sgn * .001, PLATE, Z0), (x + sgn * .001, PLATE, Z1), (x, PLATE, Z1),
                               (x, RIDGE - .02, -.01), (x + sgn * .001, RIDGE - .02, -.01), (x + sgn * .001, RIDGE - .02, .01), (x, RIDGE - .02, .01)], G, 'siding', 0)
WR = PLATE + (X1 - WX0) / 2 * P  # wing ridge
WXC = (WX0 + X1) / 2
hull('wing gable', [(WX0, PLATE, WZ - .001), (X1, PLATE, WZ - .001), (X1, PLATE, WZ), (WX0, PLATE, WZ),
                    (WXC - .01, WR - .02, WZ - .001), (WXC + .01, WR - .02, WZ - .001), (WXC + .01, WR - .02, WZ), (WXC - .01, WR - .02, WZ)], G, 'siding', 0)
# corner boards
for (x, z) in ((X0, Z0), (X0, Z1), (X1, Z0), (X1, WZ), (WX0, WZ)):
    slab('corner board', (x - .07, L if (z == Z0 or x == X1) else F, z - .07), (x + .07, PLATE, z + .07), 'trimw')

# ---------------------------------------------------------------- openings
def window(name, centre, normal, w=.95, h=1.45, sill=None, shutters=True, glow=True, grille=True):
    """A double-hung window on a wall face: centre (x, y, z) on the face, normal = the face's outward axis ('+z', '-z',
    '+x', '-x')."""
    x, y, z = centre; ax = normal[1]; sg = 1 if normal[0] == '+' else -1
    def at(u, v, d):  # u along the wall, v up, d out of the wall
        if ax == 'z': return (x + u, y + v, z + sg * d)
        return (x + sg * d, y + v, z - sg * u) if False else (x + sg * d, y + v, z + u)
    def pslab(nm, u0, u1, v0, v1, d0, d1, slot):
        a = at(u0, v0, d0); b = at(u1, v1, d1)
        slab(nm, tuple(min(p, q) for p, q in zip(a, b)), tuple(max(p, q) for p, q in zip(a, b)), slot)
    pslab(name + ' glass', -w / 2, w / 2, -h / 2, h / 2, .0, .03, 'glass' if glow else 'glassdark')
    pslab(name + ' frame top', -w / 2 - .08, w / 2 + .08, h / 2, h / 2 + .1, 0, .07, 'trimw')
    pslab(name + ' frame bottom', -w / 2 - .1, w / 2 + .1, -h / 2 - .1, -h / 2, 0, .1, 'trimw')
    pslab(name + ' frame left', -w / 2 - .08, -w / 2, -h / 2, h / 2, 0, .07, 'trimw')
    pslab(name + ' frame right', w / 2, w / 2 + .08, -h / 2, h / 2, 0, .07, 'trimw')
    pslab(name + ' meeting rail', -w / 2, w / 2, -.025, .025, 0, .05, 'trimw')
    if grille:
        for u in (-w / 6, w / 6):
            pslab(name + ' grille', u - .012, u + .012, -h / 2, h / 2, 0, .04, 'trimw')
        for v in (-h / 4, h / 4):
            pslab(name + ' grille', -w / 2, w / 2, v - .012, v + .012, 0, .04, 'trimw')
    if shutters:
        for u in (-w / 2 - .5, w / 2 + .08):
            pslab(name + ' shutter', u, u + .42, -h / 2 - .02, h / 2 + .02, 0, .05, 'shutter')
            for v in (-h / 3, 0, h / 3):
                pslab(name + ' shutter rail', u + .03, u + .39, v - .015, v + .015, .05, .065, 'shutter')

WY = F + 1.45  # window centre height on the main floor
# front, from the photo's left
window('wing front window', (WXC, WY, WZ), '+z')
window('front window by the wing', (1.3, WY, Z1), '+z')
window('porch window left', (-.9, WY, Z1), '+z')
window('porch window right', (-5.1, WY, Z1), '+z')
window('porch window end', (-6.75, WY, Z1), '+z', w=.8)
# front door with storm door, lantern
DX = -3.3
slab('front door', (DX - .48, F, Z1), (DX + .48, F + 2.1, Z1 + .05), 'doorw')
slab('storm door glass', (DX - .38, F + 1.0, Z1 + .05), (DX + .38, F + 1.95, Z1 + .07), 'glassdark')
slab('door frame', (DX - .58, F, Z1), (DX - .48, F + 2.2, Z1 + .08), 'trimw'); slab('door frame', (DX + .48, F, Z1), (DX + .58, F + 2.2, Z1 + .08), 'trimw')
slab('door head', (DX - .58, F + 2.1, Z1), (DX + .58, F + 2.22, Z1 + .08), 'trimw')
slab('door handle', (DX + .3, F + 1.0, Z1 + .07), (DX + .36, F + 1.06, Z1 + .12), 'metal')
slab('lantern', (DX + .85, F + 1.75, Z1 + .02), (DX + 1.05, F + 2.05, Z1 + .2), 'shutter')
slab('lantern glass', (DX + .88, F + 1.79, Z1 + .2), (DX + 1.02, F + 2.0, Z1 + .21), 'lamp')
# sides (the photo's left = +x end, right = -x end)
window('left end window', (X1, WY, 3.2), '+x')
window('left end window back', (X1, WY, -2.6), '+x')
window('right end window', (X0, WY, -3.0), '-x', shutters=False)
# back: windows, the sliding glass door onto the deck
window('back window left', (5.2, WY, Z0), '-z', shutters=False)
window('back window', (2.9, WY, Z0), '-z', shutters=False)
window('back window right', (-6.0, WY, Z0), '-z', shutters=False)
SX0, SX1 = -2.6, -.2
slab('slider glass', (SX0, F, Z0 - .03), (SX1, F + 2.1, Z0), 'glass')
slab('slider frame', (SX0 - .08, F, Z0 - .06), (SX0, F + 2.18, Z0), 'trimw'); slab('slider frame', (SX1, F, Z0 - .06), (SX1 + .08, F + 2.18, Z0), 'trimw')
slab('slider head', (SX0 - .08, F + 2.1, Z0 - .06), (SX1 + .08, F + 2.2, Z0), 'trimw'); slab('slider mid', ((SX0 + SX1) / 2 - .04, F, Z0 - .06), ((SX0 + SX1) / 2 + .04, F + 2.1, Z0), 'trimw')
# lower level: back windows and a door, the two garage doors on the left end
window('lower back window', (4.6, L + 1.5, Z0), '-z', w=1.1, h=1.0, shutters=False, grille=False)
window('lower back window 2', (-5.6, L + 1.5, Z0), '-z', w=1.1, h=1.0, shutters=False, grille=False)
slab('lower back door', (1.2, L, Z0 - .05), (2.1, L + 2.05, Z0), 'doorw')
slab('lower back door frame', (1.1, L, Z0 - .07), (2.2, L + 2.15, Z0 - .04), 'trimw')
GD = [(-4.25, -1.75), (-1.45, 1.05)]
for i, (z0, z1) in enumerate(GD):
    slab('garage door', (X1, L, z0), (X1 + .05, L + 2.25, z1), 'garage')
    for k in range(1, 4):
        slab('garage door panel line', (X1 + .05, L + k * .56 - .015, z0 + .05), (X1 + .065, L + k * .56 + .015, z1 - .05), 'trimw')
    slab('garage door trim', (X1, L + 2.25, z0 - .1), (X1 + .08, L + 2.37, z1 + .1), 'trimw')
    slab('garage door trim', (X1, L, z0 - .1), (X1 + .08, L + 2.37, z0), 'trimw'); slab('garage door trim', (X1, L, z1), (X1 + .08, L + 2.37, z1 + .1), 'trimw')
    slab('garage light', (X1, L + 2.55, (z0 + z1) / 2 - .08), (X1 + .15, L + 2.8, (z0 + z1) / 2 + .08), 'lamp')

# ---------------------------------------------------------------- roof
def roof_plane(name, x0, x1, za, ya, zb, yb, thick=.12):
    """A roof slab between eave/ridge lines along x: (za, ya) and (zb, yb), with fascia."""
    quad(name, [(x0, ya, za), (x1, ya, za), (x1, yb, zb), (x0, yb, zb)], 'roof', thick)

# main: back plane, front plane (over the porch it carries on down to the porch eave)
roof_plane('main roof back', X0 - OH, X1 + OH, 0, RIDGE, Z0 - OH, RIDGE - (Z1 + OH) * P)
roof_plane('main roof front (wing part)', WX0, X1 + OH, Z1 - .2, RIDGE - (Z1 - .2) * P, 0, RIDGE)
roof_plane('main roof front', X0 - OH, WX0, 0, RIDGE, Z1 + OH, RIDGE - (Z1 + OH) * P)
PE = RIDGE - (PZ + OH - 0) * P  # porch eave
roof_plane('porch roof', X0 - OH, PX1, Z1 + OH - .01, RIDGE - (Z1 + OH - .01) * P, PZ + OH, PE)
# the strip of main front roof in front of the wall between the porch and the wing
roof_plane('main roof front by the wing', PX1, WX0, Z1 - .01, RIDGE - (Z1 - .01) * P, Z1 + OH, RIDGE - (Z1 + OH) * P)
# wing gable roof: ridge along z at WXC, from where it meets the main roof to the front overhang
WB = (RIDGE - WR) / P  # z where the wing ridge meets the main roof plane
WE0 = WR - ((X1 - WX0) / 2 + OH) * P  # wing eave height (with the overhang)
for side in (-1, 1):
    xe = WXC + side * ((X1 - WX0) / 2 + OH)
    quad('wing roof', [(WXC, WR, WB - .3), (WXC, WR, WZ + OH), (xe, WE0, WZ + OH), (xe, WE0, WB - .3)], 'roof', .12)
slab('ridge cap', (X0 - OH, RIDGE - .02, -.12), (X1 + OH, RIDGE + .07, .12), 'roof')
slab('wing ridge cap', (WXC - .12, WR - .02, WB), (WXC + .12, WR + .07, WZ + OH), 'roof')
# fascia and gutters (white)
def gutter(x0, x1, z, y, out):
    slab('gutter', (x0, y - .2, min(z, z + out * .16)), (x1, y - .02, max(z, z + out * .16)), 'gutter')
gutter(X0 - OH, PX1, PZ + OH, PE, 1)
gutter(PX1, WX0, Z1 + OH, RIDGE - (Z1 + OH) * P, 1)
gutter(X0 - OH, X1 + OH, Z0 - OH, RIDGE - (Z1 + OH) * P, -1)
WE = WE0
for xe in (WX0 - OH, X1 + OH):
    slab('wing gutter', (xe - .08, WE - .2, Z1), (xe + .08, WE - .02, WZ + OH), 'gutter')
for x in (X0 - OH, X1 + OH):  # rake boards on the main gables
    for sgn in (-1, 1):
        zr = sgn * (Z1 + OH)
        quad('rake', [(x - .05, RIDGE + .02, 0), (x + .05, RIDGE + .02, 0), (x + .05, RIDGE - (Z1 + OH) * P, zr), (x - .05, RIDGE - (Z1 + OH) * P, zr)], 'trimw', .16)
for sgn in (-1, 1):  # wing rake
    xe = WXC + sgn * ((X1 - WX0) / 2 + OH)
    quad('wing rake', [(WXC, WR + .02, WZ + OH - .05), (WXC, WR + .02, WZ + OH + .05), (xe, WE, WZ + OH + .05), (xe, WE, WZ + OH - .05)], 'trimw', .16)
# downspouts at the corners, to the ground (bottom at the lower floor on the exposed corners)
for (x, z, y0) in ((X0 - .15, Z0 - .2, L), (X1 + .15, Z0 - .2, L), (X1 + .15, WZ + .1, F - .4), (WX0 - .1, WZ + .1, -.4), (PX1 + .2, PZ + .3, -.3), (X0 - .2, PZ + .3, -.4)):
    slab('downspout', (x - .05, y0, z - .05), (x + .05, PE if z > Z1 else RIDGE - (Z1 + OH) * P, z + .05), 'gutter')

# ---------------------------------------------------------------- front porch
slab('porch slab', (X0, -.6, Z1), (PX1, .2, PZ), 'concrete')
for px in (PX1 - .12, -2.45, -5.05, X0 + .12):
    slab('porch post', (px - .07, .2, PZ - .26), (px + .07, PE - .1, PZ - .12), 'trimw')
    colbox('Porch post', (px - .07, .2, PZ - .26), (px + .07, PE - .1, PZ - .12))
slab('porch beam', (X0, PE - .32, PZ - .3), (PX1, PE - .08, PZ - .08), 'trimw')
# wicker chairs, a bench (no collision)
def chair(x, z, yaw):
    s = math.sin(math.radians(yaw)); c = math.cos(math.radians(yaw))
    box('wicker chair seat', (x, .58, z), (.7, .16, .65), G, 'wicker', 0, rot=(0, yaw, 0))
    box('wicker chair cushion', (x, .69, z), (.6, .08, .55), G, 'cushion', 0, rot=(0, yaw, 0))
    box('wicker chair base', (x, .38, z), (.66, .36, .6), G, 'wicker', 0, rot=(0, yaw, 0))
    box('wicker chair back', (x - s * .3, .95, z - c * .3), (.72, .62, .12), G, 'wicker', 0, rot=(-8, yaw, 0))
    for sg in (-1, 1):
        box('wicker chair arm', (x + c * sg * .36, .78, z - s * sg * .36), (.1, .22, .6), G, 'wicker', 0, rot=(0, yaw, 0))
chair(-1.1, 5.55, 200); chair(-.15, 5.4, 170)
box('porch bench', (-6.3, .62, 5.0), (1.4, .1, .45), G, 'deck', 0)
box('porch bench back', (-6.3, .88, 4.8), (1.4, .4, .06), G, 'deck', 0)
for bx in (-6.9, -5.7):
    box('porch bench leg', (bx, .4, 5.0), (.08, .36, .4), G, 'deck', 0)

# ---------------------------------------------------------------- chimney (outside the photo's right end wall)
CZ0, CZ1 = 1.75, 3.35
slab('chimney base', (X0 - .95, L, CZ0), (X0, 2.3, CZ1), 'brick')
hull('chimney shoulder', [(X0 - .95, 2.3, CZ0), (X0, 2.3, CZ0), (X0, 2.3, CZ1), (X0 - .95, 2.3, CZ1),
                          (X0 - .78, 3.0, CZ0 + .3), (X0, 3.0, CZ0 + .3), (X0, 3.0, CZ1 - .3), (X0 - .78, 3.0, CZ1 - .3)], G, 'brick', 0)
CT = RIDGE + .7
slab('chimney stack', (X0 - .78, 3.0, CZ0 + .3), (X0 + .05, CT, CZ1 - .3), 'brick')
slab('chimney cap', (X0 - .85, CT, CZ0 + .22), (X0 + .12, CT + .1, CZ1 - .22), 'metal')
slab('chimney cap top', (X0 - .62, CT + .1, CZ0 + .45), (X0 - .12, CT + .32, CZ1 - .45), 'metal')
for y in [L + .35 + i * .45 for i in range(int((CT - L) / .45))]:  # mortar courses
    slab('chimney course', (X0 - .97 if y < 2.3 else X0 - .8, y, CZ0 - .01 if y < 2.3 else CZ0 + .29), (X0 - .94 if y < 2.3 else X0 - .77, y + .03, CZ1 + .01 if y < 2.3 else CZ1 - .29), 'concrete')
colbox('Chimney', (X0 - .95, L, CZ0), (X0, CT, CZ1))

# ---------------------------------------------------------------- screened porch (photo's right, behind the chimney)
SP0, SP1 = -4.2, 1.45     # z range
SPX = X0 - 3.8            # outer wall x
SY0 = F                   # floor
SR0, SR1 = PLATE - .1, PLATE - .1 - 3.8 * .22  # its shed roof falls away from the house
slab('screened porch floor', (SPX, SY0 - .2, SP0), (X0, SY0, SP1), 'deck')
slab('screened porch skirt', (SPX, L, SP0), (X0, SY0 - .2, SP1), 'brick')
for (x, z) in ((SPX, SP0), (SPX, SP1), (SPX, (SP0 + SP1) / 2), (X0 - .1, SP0), (X0 - .1, SP1)):
    slab('screened porch post', (x - .07, SY0, z - .07), (x + .07, SR1 + .1, z + .07), 'trimw')
slab('screened porch knee wall', (SPX - .05, SY0, SP0), (SPX + .08, SY0 + .5, SP1), 'siding')
slab('screened porch knee wall', (SPX, SY0, SP0 - .05), (X0, SY0 + .5, SP0 + .08), 'siding')
slab('screened porch knee wall', (SPX, SY0, SP1 - .08), (X0, SY0 + .5, SP1 + .05), 'siding')
slab('screen', (SPX - .02, SY0 + .5, SP0), (SPX + .02, SR1, SP1), 'screen')
slab('screen', (SPX, SY0 + .5, SP0 - .02), (X0, SR1, SP0 + .02), 'screen')
slab('screen', (SPX, SY0 + .5, SP1 - .02), (X0, SR1, SP1 + .02), 'screen')
# its door is on the front face, opening onto the front terrace (three steps down to the terrace, at the front grade)
slab('screen door', (-10.2, SY0 + .05, SP1 - .02), (-9.3, SY0 + 2.05, SP1 + .08), 'trimw')
slab('screen door screen', (-10.1, SY0 + .25, SP1 + .07), (-9.4, SY0 + 1.95, SP1 + .09), 'screen')
slab('screen door handle', (-9.5, SY0 + 1.0, SP1 + .08), (-9.44, SY0 + 1.06, SP1 + .13), 'metal')
for i in range(3):
    slab('screened porch step', (-10.35, SY0 - .15 * (i + 1) - .12, SP1 + .3 * i), (-9.15, SY0 - .15 * (i + 1), SP1 + .3 * (i + 1)), 'concrete')
colbox('Screened porch', (SPX, L, SP0), (X0, SR0, SP1))

# ---------------------------------------------------------------- back deck, stairs down to the yard
DX0, DX1, DZ = -4.9, 1.9, Z0 - 3.8
slab('deck boards', (DX0, F - .06, DZ), (DX1, F, Z0), 'deck')
slab('deck joists', (DX0, F - .3, DZ), (DX1, F - .06, Z0), 'deck')
for k in range(int((DX1 - DX0) / .3)):
    slab('deck board gap', (DX0 + .3 * k + .14, F - .001, DZ), (DX0 + .3 * k + .16, F + .002, Z0), 'deckdark')
posts = [(x, DZ + .1) for x in (DX0 + .1, (DX0 + DX1) / 2, DX1 - .1)] + [(DX0 + .1, Z0 - .2), (DX1 - .1, Z0 - .2)]
for (x, z) in posts:
    slab('deck post', (x - .09, L - .2, z - .09), (x + .09, F - .3, z + .09), 'deck')
    colbox('Deck post', (x - .09, L - .2, z - .09), (x + .09, F - .3, z + .09))
# railing on the three open sides, an opening at the stairs (at the -x end of the back edge)
STW = 1.1
rails = [((DX0 + STW, DZ), (DX1, DZ)), ((DX1, DZ), (DX1, Z0)), ((DX0, DZ + STW + .1), (DX0, Z0))]
for (a, b) in rails:
    mn = (min(a[0], b[0]) - .05, F, min(a[1], b[1]) - .05); mx = (max(a[0], b[0]) + .05, F + .95, max(a[1], b[1]) + .05)
    slab('deck top rail', (mn[0] - .02, F + .9, mn[2] - .02), (mx[0] + .02, F + .98, mx[2] + .02), 'deck')
    slab('deck bottom rail', (mn[0], F + .1, mn[2]), (mx[0], F + .16, mx[2]), 'deck')
    n = int(max(abs(b[0] - a[0]), abs(b[1] - a[1])) / .45)
    for k in range(n + 1):
        t = k / max(1, n); x = a[0] + (b[0] - a[0]) * t; z = a[1] + (b[1] - a[1]) * t
        slab('deck baluster', (x - .025, F, z - .025), (x + .025, F + .9, z + .025), 'deck')
    colbox('Deck railing', (mn[0], F, mn[2]), (mx[0], F + .98, mx[2]))
colbox('Deck', (DX0, F - .3, DZ), (DX1, F, Z0))
# stairs: from the deck's back corner down to the yard along -x
NST = max(4, int(round((F - L) / .18))); RISE = (F - L) / NST; RUN = .27
for i in range(NST):
    x1 = DX0 - i * RUN; x0 = x1 - RUN
    slab('deck stair tread', (x0, F - RISE * (i + 1) - .05, DZ), (x1, F - RISE * (i + 1), DZ + STW), 'deck')
SX = DX0 - NST * RUN
for z in (DZ, DZ + STW):
    quad('deck stair stringer', [(DX0, F - .02, z), (DX0, F - .32, z), (SX, L - .02, z), (SX, L + .26, z)], 'deck', .06)
    slab('deck stair rail post', (SX - .05, L, z - .04), (SX + .05, L + 1.0, z + .04), 'deck')
    quad('deck stair rail', [(DX0, F + .95, z - .03), (DX0, F + .9, z - .03), (SX, L + .92, z - .03), (SX, L + .97, z - .03)], 'deck', .06)
COL.append(('prism', 'Deck stairs', [(DX0, F, DZ), (DX0, F, DZ + STW), (SX, L, DZ + STW), (SX, L, DZ), (DX0, L, DZ), (DX0, L, DZ + STW)]))

# ---------------------------------------------------------------- the main collision shapes
colbox('Wall collision', (X0, L, Z0), (X1, PLATE, Z1))
colbox('Wing wall collision', (WX0, L, Z1), (X1, PLATE, WZ))
colbox('Porch floor', (X0, -.6, Z1), (PX1, .2, PZ))
# roofs as prisms (6 points: two triangles, eave-ridge-eave at each end)
COL.append(('prism', 'Pitched roof', [(X0 - OH, RIDGE - (Z1 + OH) * P, Z0 - OH), (X0 - OH, RIDGE, 0), (X0 - OH, RIDGE - (Z1 + OH) * P, Z1 + OH),
                                      (X1 + OH, RIDGE - (Z1 + OH) * P, Z0 - OH), (X1 + OH, RIDGE, 0), (X1 + OH, RIDGE - (Z1 + OH) * P, Z1 + OH)]))
COL.append(('prism', 'Pitched roof', [(WXC - (X1 - WX0) / 2 - OH, WE, WZ + OH), (WXC, WR, WZ + OH), (WXC + (X1 - WX0) / 2 + OH, WE, WZ + OH),
                                      (WXC - (X1 - WX0) / 2 - OH, WE, WB), (WXC, WR, WB), (WXC + (X1 - WX0) / 2 + OH, WE, WB)]))
COL.append(('prism', 'Porch roof', [(X0 - OH, RIDGE - (Z1 + OH) * P, Z1 + OH), (X0 - OH, PE, PZ + OH), (X0 - OH, PE - .15, PZ + OH),
                                    (PX1, RIDGE - (Z1 + OH) * P, Z1 + OH), (PX1, PE, PZ + OH), (PX1, PE - .15, PZ + OH)]))

# ---------------------------------------------------------------- export
os.makedirs(OUT, exist_ok=True)
kit.COLORS.update({'glassdark': (.10, .12, .14), 'deckdark': (.28, .20, .13)})
for ob, g, s in kit.PARTS: pass
objs = kit.join_all()
os.makedirs(os.path.join(ROOT, 'SourceArt', 'Blender'), exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT, 'SourceArt', 'Blender', 'KylesHouse.blend'))
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT, 'KylesHouse.fbx'), use_selection=False, object_types={'MESH'},
                         axis_forward='-Z', axis_up='Y', bake_space_transform=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_UNITS',
                         mesh_smooth_type='FACE', use_mesh_modifiers=True, add_leaf_bones=False, path_mode='STRIP', use_custom_props=False)
with open(os.path.join(OUT, 'KylesHouse-colliders.txt'), 'w') as f:
    f.write('# Kyle\'s house collision shapes, house frame (x right = the photo\'s left, y up, z forward = front). box name cx cy cz sx sy sz | prism name x y z ...\n')
    f.write(f'# lower floor {L} main floor {F} plate {PLATE} ridge {RIDGE:.3f} main body x {X0}..{X1} z {Z0}..{Z1} wing x {WX0}..{X1} z ..{WZ} porch x {X0}..{PX1} z ..{PZ} deck x {DX0}..{DX1} z {DZ}..{Z0} stairs to x {SX:.2f} screened porch x {SPX}..{X0} z {SP0}..{SP1} garage doors z {GD}\n')
    for c in COL:
        if c[0] == 'box': f.write('box\t%s\t%s\n' % (c[1], ' '.join('%.3f' % v for v in list(c[2]) + list(c[3]))))
        else: f.write('prism\t%s\t%s\n' % (c[1], ' '.join('%.3f %.3f %.3f' % p for p in c[2])))
print('KYLESHOUSE triangles', kit.tris(), {ob.name: kit.tris([ob]) for ob in objs})
