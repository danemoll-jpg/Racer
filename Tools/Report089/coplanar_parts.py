"""0.89 Part F (read only): same-facing coplanar overlaps between the named parts of a vehicle model, before they are joined.

Run:  blender --background --factory-startup --python Tools/Report089/coplanar_parts.py -- OUT.txt module:Name [module:Name ...]
      (module = cars, cars81, bikes, traffic, trailfour, mower, needle600; Name = the model, or '-' for a module's only one)

Builds each model with its own script (nothing is saved or exported), takes every part with its modifiers applied in the
game's vehicle space, and reports pairs of parts with triangles facing the same way on the same plane (within 2 mm) whose
areas overlap: the z-fighting candidates, by part name and slot, with the overlapping area and a point on it.
"""
import bpy, sys, os, math, importlib
from collections import defaultdict
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', 'Blender'))
import kit

args = sys.argv[sys.argv.index('--') + 1:]
OUT, JOBS = args[0], args[1:]


OFFSET = {}  # group -> Unity offset applied after the join (needle600 places its wheels on the axles only then)


def triangles(parts):
    dg = bpy.context.evaluated_depsgraph_get(); out = []
    for ob, g, s in parts:
        o = OFFSET.get(g, (0, 0, 0))
        ev = ob.evaluated_get(dg); me = ev.to_mesh(); M = ob.matrix_world
        me.calc_loop_triangles()
        v = [M @ x.co for x in me.vertices]
        for t in me.loop_triangles:
            a, b, c = (v[i] for i in t.vertices)
            a, b, c = ((p.x + o[0], p.z + o[1], p.y + o[2]) for p in (a, b, c))  # Blender -> Unity vehicle space
            n = cross(sub(b, a), sub(c, a)); ln = math.sqrt(dot(n, n))
            if ln < 2e-5: continue
            n = tuple(x / ln for x in n); out.append((a, b, c, n, dot(n, a), f'{ob.name} [{g}/{s}]'))
        ev.to_mesh_clear()
    return out


def sub(a, b): return (a[0] - b[0], a[1] - b[1], a[2] - b[2])
def dot(a, b): return a[0] * b[0] + a[1] * b[1] + a[2] * b[2]
def cross(a, b): return (a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0])


def inside(p, a, b, c, n):
    return dot(cross(sub(b, a), sub(p, a)), n) > 1e-7 and dot(cross(sub(c, b), sub(p, b)), n) > 1e-7 and dot(cross(sub(a, c), sub(p, c)), n) > 1e-7


def scan(parts):
    tris = triangles(parts); buckets = defaultdict(list)
    for i, (a, b, c, n, d, part) in enumerate(tris): buckets[(round(n[0] * 50), round(n[1] * 50), round(n[2] * 50), round(d / .004))].append(i)
    pairs = {}
    for (nx, ny, nz, dd), own in buckets.items():
        cand = [j for e in (-1, 0, 1) for j in buckets.get((nx, ny, nz, dd + e), [])]
        for i in own:
            A = tris[i]
            for j in cand:
                if j <= i: continue
                Bt = tris[j]
                if dot(A[3], Bt[3]) < .9995 or abs(A[4] - Bt[4]) > .002: continue
                lat = [tuple(A[0][k] + (A[1][k] - A[0][k]) * u / 6 + (A[2][k] - A[0][k]) * w / 6 for k in range(3)) for u in range(1, 6) for w in range(1, 6 - u)]
                hit = sum(1 for p in lat if inside(p, Bt[0], Bt[1], Bt[2], Bt[3]))
                if not hit:
                    latb = [tuple(Bt[0][k] + (Bt[1][k] - Bt[0][k]) * u / 6 + (Bt[2][k] - Bt[0][k]) * w / 6 for k in range(3)) for u in range(1, 6) for w in range(1, 6 - u)]
                    if not any(inside(p, A[0], A[1], A[2], A[3]) for p in latb): continue
                    hit = 1
                area = math.sqrt(dot(cross(sub(A[1], A[0]), sub(A[2], A[0])), cross(sub(A[1], A[0]), sub(A[2], A[0])))) * .5 * hit / max(1, len(lat))
                key = ' | '.join(sorted((A[5], Bt[5])))
                at = tuple(round((A[0][k] + A[1][k] + A[2][k]) / 3, 2) for k in range(3))
                cur = pairs.get(key, (0, at, 0)); pairs[key] = (cur[0] + area, cur[1], cur[2] + 1)
    return len(tris), pairs


def build_parts(module, name):
    """Run the model's own build up to the join; returns its parts (object, group, slot)."""
    m = importlib.import_module(module); OFFSET.clear()
    if module == 'needle600':
        OFFSET.update({'WheelFront': (0, -.20, .825), 'WheelRear': (0, -.20, -.825)})
        bpy.ops.wm.read_factory_settings(use_empty=True)
        m.PARTS.clear(); m.bike(); m.rider(); m.wheel('WheelFront', True); m.wheel('WheelRear', False)
        return list(m.PARTS)
    grabbed = []
    def fake_join():
        grabbed.extend(kit.PARTS); kit.PARTS.clear(); return []
    real_join, real_export = kit.join_all, kit.export
    kit.join_all, kit.export = fake_join, (lambda *a, **k: None)
    try:
        if name == '-': m.build()
        else: m.build(name)
    finally:
        kit.join_all, kit.export = real_join, real_export
    return grabbed


lines = []
for job in JOBS:
    module, name = job.split(':')
    parts = build_parts(module, name)
    count, pairs = scan(parts)
    total = sum(p[0] for p in pairs.values())
    label = name if name != '-' else module
    lines.append(f'{label}: {len(parts)} parts, {count} triangles; same-facing coplanar overlaps {sum(p[2] for p in pairs.values())} triangle pairs, about {total * 1e4:.0f} cm2')
    for key, (area, at, n) in sorted(pairs.items(), key=lambda kv: -kv[1][0]):
        lines.append(f'  {key}: {n} pairs, about {area * 1e4:.1f} cm2, e.g. at {at[0]:.2f},{at[1]:.2f},{at[2]:.2f}')
    print('\n'.join(lines[-1 - len(pairs):]))
os.makedirs(os.path.dirname(os.path.abspath(OUT)), exist_ok=True)
with open(OUT, 'w', encoding='utf-8') as f: f.write('\n'.join(lines) + '\n')
