import bpy
import numpy as np

# make mesh
vertices = []
edges = []
faces = []

u, v = np.mgrid[0:2*np.pi:20j, -30:np.pi:20j]
m = 0.2
r = 2*m + v*v/8*m
x = r * np.cos(u)
y = r * np.sin(u)
z = -v



scale = 5

en = 0
hs = x.shape[1]
ws = x.shape[0]

for i in range(0,ws,1):
    for j in range(0,hs,1):
        vertices.append([scale*x[i,j],scale*y[i,j],scale*z[i,j]])        
      

for j in range(0,ws,1):
   for n in range(j*hs,j*hs+hs-2,1):
       if n < (x.size - hs - 1):
           edges.append([n,n+1])
           edges.append([n,n+hs])
           edges.append([n+1,n+hs])
           faces.append([n,n+1,n+hs])
           faces.append([n+1,n+hs+1,n+hs])
       print(n)

#  faces.append([n,n+1,n+hs])
#  faces.append([n+1,n+hs+1,n+hs])
    
print(faces)
        
    
new_mesh = bpy.data.meshes.new('new_mesh')
new_mesh.from_pydata(vertices, edges, faces)
new_mesh.update()
# make object from mesh
new_object = bpy.data.objects.new('new_object', new_mesh)
# make collection
new_collection = bpy.data.collections.new('new_collection')
bpy.context.scene.collection.children.link(new_collection)
# add object to scene collection
new_collection.objects.link(new_object)