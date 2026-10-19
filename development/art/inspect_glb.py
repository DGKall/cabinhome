"""Render two views of a source GLB without modifying the source."""
import sys
from pathlib import Path
import bpy
from mathutils import Vector
args=sys.argv[sys.argv.index('--')+1:]; source=Path(args[0]).resolve(); out=Path(args[1]).resolve();out.mkdir(parents=True,exist_ok=True)
bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(source))
objects=[o for o in bpy.context.scene.objects if o.type=='MESH']
points=[o.matrix_world @ Vector(c) for o in objects for c in o.bound_box]
lo=Vector(tuple(min(p[i] for p in points) for i in range(3)));hi=Vector(tuple(max(p[i] for p in points) for i in range(3)))
center=(lo+hi)/2;span=max(hi-lo)
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=16;scene.cycles.use_denoising=True
scene.render.resolution_x=720;scene.render.resolution_y=720;scene.render.resolution_percentage=100
scene.world=bpy.data.worlds.new('Review ambient');scene.world.use_nodes=True
scene.world.node_tree.nodes['Background'].inputs[0].default_value=(.55,.62,.72,1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value=.5
scene.view_settings.view_transform='Standard'
bpy.ops.object.light_add(type='AREA',location=center+Vector((1,-1.5,2))*span)
bpy.context.object.data.energy=120*span*span;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=2*span
bpy.ops.object.camera_add();camera=bpy.context.object;camera.data.type='ORTHO';camera.data.ortho_scale=span*1.4;scene.camera=camera
for label,offset in [('front',(1,-1,.6)),('rear',(-1,1,.45))]:
 camera.location=center+Vector(offset)*span*2;camera.rotation_euler=(center-camera.location).to_track_quat('-Z','Y').to_euler()
 scene.render.filepath=str(out/(label+'.png'));bpy.ops.render.render(write_still=True)
print('SOURCE_BOUNDS',list(lo),list(hi),flush=True)
