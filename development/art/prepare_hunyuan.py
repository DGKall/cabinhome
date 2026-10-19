"""Blender --background --python development/art/prepare_hunyuan.py -- source.glb PRP_001

Reviewed recipes for the first two Hunyuan samples (PRP_001 or VEG_003).
Original downloads are never modified. Outputs are local prototypes pending review.
"""
import hashlib
import json
import sys
from pathlib import Path
import bpy
import bmesh
from mathutils import Vector

ROOT = Path(__file__).resolve().parents[2]
args = sys.argv[sys.argv.index('--') + 1:]
source = Path(args[0]).resolve()
asset_id = args[1] if len(args)>1 else 'PRP_001'
if asset_id not in ('PRP_001','VEG_003'):
    raise RuntimeError('No reviewed recipe for this asset')
is_rock = asset_id == 'PRP_001'
expected = '42cbe4a12db745499a1e125fc4c0225a4262967f7f71823fba1d6a5927bd494f' if is_rock else '54a72627de0db390ee646847835d5a35b3e957e49aee1e1878440cd69969ff27'
sha = hashlib.sha256(source.read_bytes()).hexdigest()
if sha != expected:
    raise RuntimeError('Different source: inspect before reusing sample-specific cleanup')
OUT = ROOT / 'assets-source/exports' / asset_id
OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
if len(objects)!=1:raise RuntimeError('Expected one mesh')
obj=objects[0];bpy.context.view_layer.objects.active=obj;obj.select_set(True)
bpy.ops.object.transform_apply(location=True,rotation=True,scale=True)
obj.data.calc_loop_triangles();original_triangles=len(obj.data.loop_triangles)
original_dimensions=list(obj.dimensions)
material=obj.data.materials[0]
bsdf=next(n for n in material.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
color_image=bsdf.inputs['Base Color'].links[0].from_node.image
image_stats=[{'name':im.name,'size':list(im.size)} for im in bpy.data.images if im.type=='IMAGE']
bm=bmesh.new();bm.from_mesh(obj.data)
bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000001)
if is_rock:
    # Original is a diorama. Cut above its base; keep the main rock component.
    cut=.105
    bmesh.ops.bisect_plane(bm,geom=list(bm.verts)+list(bm.edges)+list(bm.faces),
        plane_co=(0,0,cut),plane_no=(0,0,1),clear_inner=True,dist=.000001)
    remaining=set(bm.verts);components=[]
    while remaining:
        seed=remaining.pop();component={seed};todo=[seed]
        while todo:
            v=todo.pop()
            for e in v.link_edges:
                n=e.other_vert(v)
                if n in remaining:remaining.remove(n);component.add(n);todo.append(n)
        components.append(component)
    keep=max(components,key=len)
    bmesh.ops.delete(bm,geom=[v for v in bm.verts if v not in keep],context='VERTS')
    borders=[e for e in bm.edges if e.is_boundary]
    if borders:bmesh.ops.holes_fill(bm,edges=borders,sides=0)
    bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces))
bm.to_mesh(obj.data);bm.free();obj.data.calc_loop_triangles()
modifier=obj.modifiers.new('Prototype reduction','DECIMATE')
modifier.ratio=min(1.0,(480 if is_rock else 1900)/len(obj.data.loop_triangles))
modifier.use_collapse_triangulate=True
bpy.ops.object.modifier_apply(modifier=modifier.name)
lo=Vector(tuple(min(v.co[i] for v in obj.data.vertices) for i in range(3)))
hi=Vector(tuple(max(v.co[i] for v in obj.data.vertices) for i in range(3)))
center=Vector(((lo.x+hi.x)/2,(lo.y+hi.y)/2,lo.z))
scale=(.6/max(hi.x-lo.x,hi.y-lo.y)) if is_rock else (6/(hi.z-lo.z))
for v in obj.data.vertices:v.co=(v.co-center)*scale
for p in obj.data.polygons:p.use_smooth=not is_rock
obj.name=asset_id;obj.data.name=asset_id+'_Mesh'
color_image.scale(1024,1024);color_image.filepath_raw=str(OUT/(asset_id+'_BaseColor.png'));color_image.file_format='PNG';color_image.save()
mat=bpy.data.materials.new(asset_id+'_Matte');mat.use_nodes=True
shader=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED')
shader.inputs['Roughness'].default_value=.94;shader.inputs['Metallic'].default_value=0
if is_rock:
    # The source painted moss/directional highlights; use a neutral material in-game.
    shader.inputs['Base Color'].default_value=(.24,.28,.33,1)
else:
    tex=mat.node_tree.nodes.new('ShaderNodeTexImage');tex.image=color_image
    mat.node_tree.links.new(tex.outputs['Color'],shader.inputs['Base Color'])
obj.data.materials.clear();obj.data.materials.append(mat)
bpy.context.scene.unit_settings.system='METRIC';bpy.context.scene.unit_settings.scale_length=1
bpy.context.view_layer.update();obj.data.calc_loop_triangles()
bm=bmesh.new();bm.from_mesh(obj.data)
topology={'boundary_edges':sum(e.is_boundary for e in bm.edges),'non_manifold_edges':sum(not e.is_manifold for e in bm.edges),'loose_vertices':sum(not v.link_edges for v in bm.verts),'signed_volume_m3':bm.calc_volume(signed=True)};bm.free()
report={'source':str(source.relative_to(ROOT)),'sha256':sha,'source_triangles':original_triangles,'source_dimensions_m':original_dimensions,'source_images':image_stats,'triangles':len(obj.data.loop_triangles),'vertices':len(obj.data.vertices),'dimensions_blender_xyz_m':list(obj.dimensions),'materials':len(obj.data.materials),'uv_layers':len(obj.data.uv_layers),'topology':topology,'recipe':('Cut source z=0.105; largest component; cap base; max span 0.6m; flat neutral matte' if is_rock else 'Uniform height 6m; smooth normals; 1K base color; matte; no generated normal map'),'publication':'Local prototype; web export terms not yet recorded'}
(OUT/'inspection.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n')
bpy.ops.object.select_all(action='DESELECT');obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=str(OUT/(asset_id+'.fbx')),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,bake_anim=False,add_leaf_bones=False,path_mode='RELATIVE')
bpy.ops.export_scene.gltf(filepath=str(OUT/(asset_id+'.glb')),use_selection=True,export_format='GLB')
bpy.ops.file.pack_all();bpy.ops.wm.save_as_mainfile(filepath=str(OUT/(asset_id+'.blend')))
print('ASSET_PREPARED '+json.dumps(report),flush=True)
