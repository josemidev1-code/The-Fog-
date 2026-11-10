"""Read-only geometry inspection. Run with Blender --background --python tools/inspect_house.py."""
import bpy, pathlib, json, hashlib, bmesh
from mathutils import Vector
root=pathlib.Path(__file__).resolve().parents[1]
p=root/'Assets/Models/Optimized/ENV_CasaDaniel.glb'
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(p))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
points=[o.matrix_world@Vector(corner) for o in objects for corner in o.bound_box]
lo=Vector([min(v[i] for v in points) for i in range(3)]); hi=Vector([max(v[i] for v in points) for i in range(3)])
report={'asset':str(p.relative_to(root)),'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'blenderBoundsXYZ':{'min':list(lo),'max':list(hi),'size':list(hi-lo)},'meshes':[],'textures':[{'name':i.name,'size':list(i.size)} for i in bpy.data.images if i.type=='IMAGE'],'interpretation':'Geometry inventory only; topology is not proof of a playable interior. Door separation, accessible openings and collision traversal remain unverified in Unity.'}
for obj in objects:
 bm=bmesh.new();bm.from_mesh(obj.data)
 report['meshes'].append({'name':obj.name,'vertices':len(bm.verts),'triangles':sum(len(f.verts)-2 for f in bm.faces),'boundaryEdges':sum(e.is_boundary for e in bm.edges),'nonManifoldEdges':sum(not e.is_manifold for e in bm.edges),'materials':[m.name for m in obj.data.materials]});bm.free()
(root/'docs/casa-daniel-geometry-audit.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps(report),flush=True)
