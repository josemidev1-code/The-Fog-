"""Inspect a downloaded Hunyuan doorway without importing unverified geometry into Unity.
Blender --background --python tools/prepare_doorway.py -- /absolute/candidate.glb
Output remains under docs/candidates until Unity import and Play Mode validation.
"""
import bpy, sys, pathlib, json, hashlib
from mathutils import Vector
from mathutils.bvhtree import BVHTree
root=pathlib.Path(__file__).resolve().parents[1]
source=pathlib.Path(sys.argv[sys.argv.index('--')+1])
output=root/'docs/candidates'; output.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(source))
meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
if not meshes: raise RuntimeError('No meshes in downloaded model')
def bounds():
 pts=[o.matrix_world@Vector(p) for o in meshes for p in o.bound_box]
 return Vector([min(v[i] for v in pts) for i in range(3)]),Vector([max(v[i] for v in pts) for i in range(3)])
bpy.context.view_layer.update();lo,hi=bounds()
scale=2.7/(hi.z-lo.z);center=(lo+hi)/2;center.z=lo.z
for o in meshes:
 o.matrix_world.translation=(o.matrix_world.translation-center)*scale
 o.scale*=scale
bpy.context.view_layer.update()
def geometry_checks():
 lo,hi=bounds();vs=[];polys=[]
 for o in meshes:
  start=len(vs);vs.extend(o.matrix_world@v.co for v in o.data.vertices)
  polys.extend(tuple(start+i for i in p.vertices) for p in o.data.polygons)
 tree=BVHTree.FromPolygons(vs,polys)
 depth=hi.y-lo.y;blocked=[]
 for x in [-.23,0,.23]:
  for z in [.15,.5,1,1.5,1.85]:
   hit=tree.ray_cast(Vector((x,lo.y-.2,z)),Vector((0,1,0)),depth+.4)[0]
   if hit is not None:blocked.append([x,z])
 wall_hits=0
 for x in [lo.x+.2,hi.x-.2]:
  for z in [.5,1.5,2.4]:
   if tree.ray_cast(Vector((x,lo.y-.2,z)),Vector((0,1,0)),depth+.4)[0] is not None:wall_hits+=1
 return {'sizeBlenderXYZ':list(hi-lo),'blockedPortalSamples':blocked,'sideWallHits':wall_hits,'portalSampling':'15 rays across central 0.46m width, heights 0.15..1.85m; partial geometry check, not CharacterController traversal','passed':not blocked and wall_hits==6 and depth<.5}
original=geometry_checks();triangles=lambda:sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes)
before=triangles()
for o in meshes:
 bpy.context.view_layer.objects.active=o
 mod=o.modifiers.new('Candidate optimization','DECIMATE');mod.ratio=min(1,16000/max(before,1));mod.use_collapse_triangulate=True
 bpy.ops.object.modifier_apply(modifier=mod.name)
for im in bpy.data.images:
 if max(im.size)>1024:
  k=1024/max(im.size);im.scale(max(1,round(im.size[0]*k)),max(1,round(im.size[1]*k)))
bpy.context.view_layer.update();optimized=geometry_checks()
report={'source':str(source.relative_to(root)) if source.is_relative_to(root) else source.name,'sha256':hashlib.sha256(source.read_bytes()).hexdigest(),'sourceCheck':original,'optimizedCheck':optimized,'trianglesBefore':before,'trianglesAfter':triangles(),'uniformScale':scale,'blenderGeometryPassed':original['passed'] and optimized['passed'],'unityImportVerified':False,'playModeVerified':False,'status':'candidate only; do not integrate until Unity import and physical traversal checks'}
if report['blenderGeometryPassed']:
 dst=output/'INT_Valdara_ParedPuerta_A-candidate.glb';bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=True);report['candidate']=str(dst.relative_to(root))
(root/'docs/doorway-geometry-validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8');print(json.dumps(report),flush=True)
lo,hi=bounds();center=(lo+hi)/2;size=max(hi-lo)
bpy.ops.object.camera_add(location=center+Vector((.5,-2.5,.35))*size);cam=bpy.context.object;cam.rotation_euler=(center-cam.location).to_track_quat('-Z','Y').to_euler();cam.data.type='ORTHO';cam.data.ortho_scale=size*1.4
scene=bpy.context.scene;scene.camera=cam
for direction,power in [((1,-2,3),450),((-2,-1,1),250),((0,2,2),350)]:
 bpy.ops.object.light_add(type='AREA',location=center+Vector(direction)*size);light=bpy.context.object;light.data.energy=power*size*size*.5;light.data.size=size*2;light.rotation_euler=(center-light.location).to_track_quat('-Z','Y').to_euler()
scene.world=bpy.data.worlds.new('Preview');scene.world.color=(.07,.08,.075);scene.render.engine='CYCLES';scene.cycles.samples=16;scene.render.resolution_x=1280;scene.render.resolution_y=720;scene.render.resolution_percentage=100;scene.view_settings.view_transform='Standard';scene.render.filepath=str(root/'docs/doorway-candidate-preview.png');bpy.ops.render.render(write_still=True)
