using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace Racer
{
    // Speculative CCD can produce horizontal internal-edge normals on a smooth heightfield.
    // Use the actual authored supporting face, never remove its collision or add propulsion.
    public static class VehicleSurfaceContacts
    {
        struct Face { public Vector3 Normal, Point; }
        struct Box { public Vector3 Center, Half; }
        static volatile Dictionary<EntityId, Face[]> surfaces = new();
        static volatile Dictionary<EntityId, Box> vehicles = new();
        static bool initialized;
        static UnityEngine.SceneManagement.Scene registeredScene;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            surfaces=new(); vehicles=new(); initialized=false;registeredScene=default;
            Physics.ContactModifyEvent-=Correct;
            Physics.ContactModifyEvent+=Correct;
        }
        public static void Register(BoxCollider box)
        {
            // A course selection loads a different scene without resetting static runtime state.
            // Rebuild the face cache for that scene; old collider entity IDs cannot support it.
            if(!initialized || registeredScene!=box.gameObject.scene)
            {
                var next=new Dictionary<EntityId,Face[]>();
                foreach(var mesh in Object.FindObjectsByType<MeshCollider>())
                {
                    if(mesh.gameObject.scene!=box.gameObject.scene)continue;
                    if(mesh.attachedRigidbody || !mesh.sharedMesh || !mesh.sharedMesh.isReadable) continue;
                    if(!mesh.name.StartsWith("Ground_") && !mesh.name.StartsWith("Takeoff -") && !mesh.name.StartsWith("Landing -") && !mesh.name.StartsWith("Gully supported ramp")) continue;
                    var vertices=mesh.sharedMesh.vertices; var indices=mesh.sharedMesh.triangles;
                    var faces=new Face[indices.Length/3];
                    for(int i=0;i<faces.Length;i++)
                    {
                        var a=mesh.transform.TransformPoint(vertices[indices[i*3]]);
                        var b=mesh.transform.TransformPoint(vertices[indices[i*3+1]]);
                        var c=mesh.transform.TransformPoint(vertices[indices[i*3+2]]);
                        faces[i]=new Face{Normal=Vector3.Cross(b-a,c-a).normalized,Point=a};
                    }
                    next[mesh.GetEntityId()]=faces;
                }
                surfaces=next; initialized=true;registeredScene=box.gameObject.scene;
            }
            var copy=new Dictionary<EntityId,Box>(vehicles);
            copy[box.GetEntityId()]=new Box{Center=Vector3.Scale(box.center,box.transform.lossyScale),Half=Vector3.Scale(box.size*.5f,box.transform.lossyScale)};
            vehicles=copy;
        }
        public static void Unregister(BoxCollider box)
        { var copy=new Dictionary<EntityId,Box>(vehicles); copy.Remove(box.GetEntityId()); vehicles=copy; }
        static void Correct(PhysicsScene scene,NativeArray<ModifiableContactPair> pairs)
        {
            var terrain=surfaces; var bodies=vehicles;
            for(int i=0;i<pairs.Length;i++)
            {
                var pair=pairs[i];
                bool first=bodies.TryGetValue(pair.colliderEntityId,out var box);
                if(!first && !bodies.TryGetValue(pair.otherColliderEntityId,out box)) continue;
                if(!terrain.TryGetValue(first?pair.otherColliderEntityId:pair.colliderEntityId,out var faces)) continue;
                var rotation=first?pair.rotation:pair.otherRotation;
                var center=(first?pair.position:pair.otherPosition)+rotation*box.Center;
                for(int k=0;k<pair.contactCount;k++)
                {
                    uint face=pair.GetFaceIndex(k);
                    if(face>=faces.Length) continue;
                    var surface=faces[face]; var n=surface.Normal;
                    // Sides, undersides, obstacles and vehicle pairs retain their exact solver response.
                    if(n.y<.55f || Vector3.Dot(center-surface.Point,n)<0) continue;
                    var normal=first?n:-n;
                    if(Vector3.Dot(pair.GetNormal(k),normal)>.98f) continue;
                    float radius=Mathf.Abs(Vector3.Dot(rotation*Vector3.right,n))*box.Half.x
                        +Mathf.Abs(Vector3.Dot(rotation*Vector3.up,n))*box.Half.y
                        +Mathf.Abs(Vector3.Dot(rotation*Vector3.forward,n))*box.Half.z;
                    pair.SetNormal(k,normal);
                    pair.SetSeparation(k,Vector3.Dot(center-surface.Point,n)-radius);
                }
            }
        }
    }
}
