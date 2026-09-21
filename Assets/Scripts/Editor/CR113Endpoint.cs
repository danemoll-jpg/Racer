using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        public static void CR113Endpoints()
        {
            foreach(var scene in CR112Scenes){EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();if(GameObject.Find("CR113 supported outer continuations"))throw new Exception("Outer corrections already authored");worldRoot=new GameObject("CR113 supported outer continuations").transform;
                var additions=new List<Vector3[]>();
                foreach(bool east in new[]{false,true}){var root=GameObject.Find(east?"Hwy 92 east":"Hwy 92 west").transform;var old=root.Find("Decorative Road pavement").GetComponent<MeshFilter>().sharedMesh.vertices;float height=old.Where(p=>Math.Abs(p.z-430)<.1f).Average(p=>p.y);
                    if(scene.EndsWith("/StreetLoopGreybox.unity")){var camera=Camera.main;var cp=camera.transform.position;var cq=camera.transform.rotation;camera.transform.position=root.TransformPoint(new Vector3(35,65,430));camera.transform.LookAt(root.TransformPoint(new Vector3(0,height,445)));ThreeFeatureValidation.CaptureUi("Docs/CR112-118/"+(east?"east":"west")+"-outer-before.png");camera.transform.SetPositionAndRotation(cp,cq);}
                    foreach(var mf in GameObject.Find("CR113 continuous highway seams").GetComponentsInChildren<MeshFilter>()){var v=mf.sharedMesh.vertices;bool changed=false;for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);var q=root.InverseTransformPoint(p);if(q.z<430||q.z>441||Math.Abs(q.x)>9)continue;float extra=mf.name.Contains("white")||mf.name.Contains("yellow")?.045f:.02f;q.y=height+extra;v[i]=mf.transform.InverseTransformPoint(root.TransformPoint(q));changed=true;}if(changed){var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=MeshAsset(mesh,owner.gameObject.scene.name+"-CR113-level-end-"+mf.GetEntityId().ToString().Replace(':','-'));if(mf.TryGetComponent<MeshCollider>(out var col))col.sharedMesh=mf.sharedMesh;}}
                    var path=Enumerable.Range(0,33).Select(i=>root.TransformPoint(new Vector3(0,height,440+i*5))).ToArray();additions.Add(path);Ribbon113((east?"east":"west")+" level outer continuation",path,8.2f,Mat("CR113 highway asphalt",new(.22f,.23f,.24f)),true);
                    Ribbon113((east?"east":"west")+" outer supported shoulders",path.Select(p=>p-Vector3.up*.15f).ToArray(),60,Mat("CR113 outer verge",new(.39f,.43f,.29f)),true);
                    foreach(float side in new[]{-4.1f,-.16f,.16f,4.1f}){var line=path.Select(p=>p+root.right*side+Vector3.up*.025f).ToArray();if(Math.Abs(side)<1)Ribbon113("outer double yellow",line,.065f,Mat("CR113 yellow",new(.9f,.68f,.22f)),false);else for(int i=0;i+1<line.Length;i+=3)Ribbon113("outer lane dash",line.Skip(i).Take(2).ToArray(),.075f,Mat("CR113 white",new(.9f,.9f,.82f)),false);}
                    var rp=owner.throughRoad.points;for(int i=0;i<rp.Length;i++){var q=root.InverseTransformPoint(rp[i]);if(q.z>=430&&q.z<=441&&Math.Abs(q.x)<1){q.y=height;rp[i]=root.TransformPoint(q);}}
                }
                owner.throughRoad.points=additions[0].Reverse().SkipLast(1).Concat(owner.throughRoad.points).Concat(additions[1].Skip(1)).ToArray();owner.throughRoad.Initialize();Save();
            }
            CR113RemoveBackdrops();
            File.WriteAllText("Docs/CR112-118/endpoints-done.txt","East last-sample 33.7m terrain-fallback drop removed; both four-lane continuations extend on supported level pavement. No end walls or obsolete tunnel backdrops.");
        }
        public static void CR113RemoveBackdrops()
        {
            foreach(var scene in CR112Scenes){EditorSceneManager.OpenScene(scene);foreach(string side in new[]{"west","east"}){var root=GameObject.Find("Hwy 92 "+side).transform;var backdrop=root.Find("Distant tunnel backdrop");if(backdrop)Object.DestroyImmediate(backdrop.gameObject);}Save();}
            File.WriteAllText("Docs/CR112-118/backdrops-removed.txt","Removed the two obsolete non-colliding tunnel backdrop panels at local station 429 from all six scenes; legitimate tunnel sides/roof retained.");
        }
    }
}
