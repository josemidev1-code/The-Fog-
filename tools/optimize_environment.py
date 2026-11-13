import bpy,pathlib,json,hashlib
root=pathlib.Path(__file__).resolve().parents[1];out=[]
for src in pathlib.Path('C:/Users/josem/Desktop/TheFog').glob('*.glb'):
 dst=root/'Assets/Models/Optimized'/src.name
 if dst.exists():continue
 bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(src))
 meshes=[o for o in bpy.context.scene.objects if o.type=='MESH'];before=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes);target=40000 if src.name.startswith('ENV_Casa') else 16000
 for o in meshes:
  bpy.context.view_layer.objects.active=o
  m=o.modifiers.new('Environment copy','DECIMATE');m.ratio=min(1,target/before);m.use_collapse_triangulate=True;bpy.ops.object.modifier_apply(modifier=m.name)
 for im in bpy.data.images:
  if max(im.size)>1024:
   k=1024/max(im.size);im.scale(max(1,round(im.size[0]*k)),max(1,round(im.size[1]*k)))
 bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=True)
 out.append(dict(source=src.name,sourceHash=hashlib.sha256(src.read_bytes()).hexdigest(),before=before,after=sum(sum(len(p.vertices)-2 for p in o.data.polygons) for o in meshes),status='optimized copy; artistic review pending'))
 (root/'docs/environment-optimization.json').write_text(json.dumps(out,indent=2),encoding='utf8')
