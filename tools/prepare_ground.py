"""Retopologize a real Hunyuan terrain surface, retaining sampled material UVs.
Blender --background --python tools/prepare_ground.py -- source.glb
Only exports a candidate if every source surface sample exists.
"""
import bpy,pathlib,sys,json,hashlib,math,statistics
import numpy as np
from mathutils import Vector,Matrix
from mathutils.bvhtree import BVHTree
from mathutils.geometry import barycentric_transform
root=pathlib.Path(__file__).resolve().parents[1];src=pathlib.Path(sys.argv[sys.argv.index('--')+1]);out=root/'docs/candidates';out.mkdir(exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True);bpy.ops.import_scene.gltf(filepath=str(src))
objs=[o for o in bpy.context.scene.objects if o.type=='MESH']
if len(objs)!=1:raise RuntimeError('Expected one ground mesh; inspect before processing multiple meshes')
o=objs[0];bpy.context.view_layer.objects.active=o;bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
size=o.dimensions;thin=min(range(3),key=lambda i:size[i])
if thin!=2:
 o.rotation_euler=(0,math.pi/2,0) if thin==0 else (math.pi/2,0,0);bpy.ops.object.transform_apply(location=False,rotation=True,scale=True)
mesh=o.data;mesh.calc_loop_triangles();verts=[o.matrix_world@v.co for v in mesh.vertices];tris=[tuple(t.vertices) for t in mesh.loop_triangles];tree=BVHTree.FromPolygons(verts,tris,all_triangles=True)
lo=Vector([min(v[i] for v in verts) for i in range(3)]);hi=Vector([max(v[i] for v in verts) for i in range(3)]);span=hi-lo
if span.z/max(span.x,span.y)>.3:raise RuntimeError('Generated object not shallow ground; manual inspection required')
uv=mesh.uv_layers.active
if not uv:raise RuntimeError('Generated model has no UVs')
n=24;samples=[];miss=[]
# Use the inner 80% of the generated patch to avoid rounded generator boundaries.
for j in range(n+1):
 for i in range(n+1):
  x=lo.x+span.x*(.1+.8*i/n);y=lo.y+span.y*(.1+.8*j/n)
  hit,normal,index,dist=tree.ray_cast(Vector((x,y,hi.z+span.z+1)),Vector((0,0,-1)))
  if hit is None:miss.append([i,j]);continue
  tri=mesh.loop_triangles[index];a,b,c=[verts[k] for k in tri.vertices];uvs=[Vector((*uv.data[k].uv,0)) for k in tri.loops]
  mapped=barycentric_transform(hit,a,b,c,*uvs);samples.append((hit.z,(mapped.x,mapped.y)))
report={'source':src.name,'sha256':hashlib.sha256(src.read_bytes()).hexdigest(),'sourceBoundsXYZ':list(span),'grid':n,'sampleCount':len(samples),'missingSamples':miss,'method':'Retopology sampled from real generated mesh; inner 80 percent retained; UVs transferred barycentrically. Top relief constrained to ±15mm, edges leveled to zero for modular use. Not the unmodified Hunyuan original.'}
if miss:
 (root/'docs/ground-validation.json').write_text(json.dumps(report,indent=2));raise RuntimeError('Ground sampling holes; candidate not exported')
median=statistics.median(v[0] for v in samples);vertices=[];uvs=[]
for j in range(n+1):
 for i in range(n+1):
  idx=j*(n+1)+i;z,uvv=samples[idx];edge=min(i,j,n-i,n-j);weight=min(1,edge/3);height=max(-.015,min(.015,(z-median)*4/max(span.x,span.y)))*weight
  vertices.append((-2+4*i/n,-2+4*j/n,height));uvs.append(uvv)
faces=[]
for j in range(n):
 for i in range(n):
  a=j*(n+1)+i;b=a+1;c=a+n+2;d=a+n+1;faces.extend([(a,b,c),(a,c,d)])
# Solid thickness on the rim, using actual source material; no exposed holes.
boundary=list(range(n+1))+[j*(n+1)+n for j in range(1,n+1)]+[n*(n+1)+i for i in range(n-1,-1,-1)]+[j*(n+1) for j in range(n-1,0,-1)]
bottom=[]
for index in boundary:
 bottom.append(len(vertices));v=vertices[index];vertices.append((v[0],v[1],-.12));uvs.append(uvs[index])
for k,index in enumerate(boundary):
 nxt=(k+1)%len(boundary);faces.append((index,bottom[k],bottom[nxt],boundary[nxt]))
faces.append(tuple(reversed(bottom)))
newmesh=bpy.data.meshes.new('Hunyuan ground retopology');newmesh.from_pydata(vertices,[],faces);newmesh.update();layer=newmesh.uv_layers.new(name='Source UV transfer')
for poly in newmesh.polygons:
 for index in poly.loop_indices:layer.data[index].uv=uvs[newmesh.loops[index].vertex_index]
obj=bpy.data.objects.new('ENV_SueloRural_A',newmesh);bpy.context.collection.objects.link(obj)
# Bake the discontinuous source atlas onto coherent top UVs. Directly interpolating
# UVs across different source UV islands creates visible atlas fragments.
material=mesh.materials[0];principled=next(node for node in material.node_tree.nodes if node.type=='BSDF_PRINCIPLED')
base=principled.inputs['Base Color'];source_image=base.links[0].from_node.image
w,h=source_image.size;pixels=np.empty(w*h*4,dtype=np.float32);source_image.pixels.foreach_get(pixels);pixels=pixels.reshape(h,w,4)
resolution=512;baked=np.zeros((resolution,resolution,4),dtype=np.float32);baked[:,:,3]=1
for row in range(resolution):
 for col in range(resolution):
  x=lo.x+span.x*(.1+.8*(col+.5)/resolution);y=lo.y+span.y*(.1+.8*(row+.5)/resolution)
  hit,normal,index,distance=tree.ray_cast(Vector((x,y,hi.z+span.z+1)),Vector((0,0,-1)))
  if hit is None:raise RuntimeError('Missing source surface during material bake')
  tri=mesh.loop_triangles[index];a,b,c=[verts[k] for k in tri.vertices];coordinates=[Vector((*uv.data[k].uv,0)) for k in tri.loops];mapped=barycentric_transform(hit,a,b,c,*coordinates)
  px=(mapped.x%1)*(w-1);py=(mapped.y%1)*(h-1);ix=int(px);iy=int(py);fx=px-ix;fy=py-iy
  baked[row,col]=pixels[iy,ix]*(1-fx)*(1-fy)+pixels[iy,min(ix+1,w-1)]*fx*(1-fy)+pixels[min(iy+1,h-1),ix]*(1-fx)*fy+pixels[min(iy+1,h-1),min(ix+1,w-1)]*fx*fy
image=bpy.data.images.new('ENV_SueloRural_A_baked_diffuse',width=resolution,height=resolution);image.pixels.foreach_set(baked.ravel());image.pack()
mat=bpy.data.materials.new('ENV_SueloRural_A_gameplay');mat.use_nodes=True;bsdf=next(node for node in mat.node_tree.nodes if node.type=='BSDF_PRINCIPLED');bsdf.inputs['Roughness'].default_value=.95;bsdf.inputs['Metallic'].default_value=0
tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=image;mat.node_tree.links.new(tex.outputs['Color'],bsdf.inputs['Base Color']);newmesh.materials.append(mat)
for poly in newmesh.polygons:
 for index in poly.loop_indices:
  v=newmesh.vertices[newmesh.loops[index].vertex_index].co;layer.data[index].uv=((v.x+2)/4,(v.y+2)/4)
report['materialBake']='512 square diffuse sampled per texel from generated source atlas; new coherent top UVs, nonmetallic rough gameplay material, source normal atlas not reused'

bpy.data.objects.remove(o,do_unlink=True)
for im in bpy.data.images:
 if max(im.size)>1024:
  k=1024/max(im.size);im.scale(max(1,round(im.size[0]*k)),max(1,round(im.size[1]*k)))
dst=out/'ENV_SueloRural_A-candidate.glb';bpy.ops.export_scene.gltf(filepath=str(dst),export_format='GLB',export_apply=True)
report.update(candidate=str(dst.relative_to(root)),triangles=sum(len(p.vertices)-2 for p in newmesh.polygons),sizeMeters=[4,4,.135],edgeHeight=0,geometryPrepared=True,unityVerified=False)
(root/'docs/ground-validation.json').write_text(json.dumps(report,indent=2));print(json.dumps(report),flush=True)
