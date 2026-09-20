using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class ExplorationAuthoring
    {
        public static void PlacementFinish()
        {
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);race=Object.FindAnyObjectByType<RaceDirector>();var collection=race.GetComponent<ExplorationCollection>();var route=collection.routes[0];
                foreach(var site in collection.sites.Skip(12)){float s=route.Project(site.position,out _);var p=route.At(s,out _);p.y=Phase6Buildings.Ground(p)+1;site.position=p;}
                // Clear whole trees along the revised closing leg as well as the new
                // shore road; the initial pass only cleared the shore road itself.
                var woods=GameObject.Find("Woods replacing later subdivisions");
                foreach(var c in woods.GetComponentsInChildren<BoxCollider>().ToArray())
                    if(collection.routes.Any(r=>Near(c.bounds.center,r.points,out _)<9))Object.DestroyImmediate(c.gameObject);
                foreach(var c in woods.GetComponentsInChildren<BoxCollider>())
                    if(c.bounds.center.x>375&&c.bounds.center.z>-210&&c.bounds.center.z<310)c.transform.position+=Vector3.up*(Phase6Buildings.Ground(c.bounds.center)-c.bounds.min.y);
                RebuildMountainTrees(race,new List<string>());
                var home=GameObject.Find("Dan - blue X").transform;
                foreach(var fence in Object.FindObjectsByType<BreakableProp>().Where(p=>p.surface==SmashAudio.Surface.ChainLink||p.name.Contains("fence")||p.name.Contains("crossbuck")))
                    if(Vector3.ProjectOnPlane(fence.transform.position-home.position,Vector3.up).magnitude<85){var p=fence.transform.position;p.y=Phase6Buildings.Ground(p);fence.transform.position=p;}
                var retired=new List<Vector3>();
                foreach(var c in Object.FindObjectsByType<CapsuleCollider>().Where(c=>c.name=="Forest trunk collision").ToArray())
                    if(collection.routes.Any(r=>Near(c.bounds.center,r.points,out _)<8)){retired.Add(c.bounds.center);Object.DestroyImmediate(c.gameObject);}
                if(retired.Count>0)typeof(ReverseReviewRelease).GetMethod("TrimTreeMeshes",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{retired,scene.name+"-shore-final"});
                wood=Mat("Weathered timber",new(.25f,.15f,.075f));leaf=Mat("Mountain foliage",new(.16f,.28f,.105f));
                root=GameObject.Find("CR081 mountain exploration").transform;
                foreach(var batch in root.Cast<Transform>().Where(t=>t.name.StartsWith("Mountain woods")).ToArray())
                {
                    foreach(var mesh in batch.GetComponentsInChildren<MeshFilter>())Object.DestroyImmediate(mesh.gameObject);
                    foreach(var collider in batch.GetComponentsInChildren<BoxCollider>().ToArray())
                    {
                        var p=new Vector3(collider.bounds.center.x,0,collider.bounds.center.z);
                        if(collection.routes.Any(r=>Near(p,r.points,out _)<9)){Object.DestroyImmediate(collider.gameObject);continue;}
                        float h=collider.bounds.size.y/.72f;p.y=Phase6Buildings.Ground(p);collider.transform.position=p+Vector3.up*h*.36f;
                        var crown=new GameObject("Supported mountain crown",typeof(MeshFilter),typeof(MeshRenderer));crown.transform.SetParent(batch);crown.transform.position=p+Vector3.up*h*.36f;crown.transform.localScale=new(3.8f,h*.64f,3.8f);crown.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/IrregularCrown.asset");crown.GetComponent<Renderer>().sharedMaterial=leaf;
                        Part(batch,"Supported trunk visual",p+Vector3.up*h*.36f,new(.55f,h*.72f,.55f),wood);
                    }
                    Combine(batch,scene.name+"-supported-"+batch.name.Replace(' ','-'));
                }
                File.WriteAllLines(Evidence+"/placements-"+scene.name+".csv",collection.sites.Select(s=>$"{s.id},{s.title},{s.position.x:F3},{s.position.y:F3},{s.position.z:F3}"));
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
        }
        public static void Connections()
        {
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);race=Object.FindAnyObjectByType<RaceDirector>();
                root=GameObject.Find("CR081 mountain exploration").transform;
                if(root.Find("Lake shore connection"))throw new Exception("Connections already authored");
                wood=Mat("Weathered timber",new(.25f,.15f,.075f));
                var collection=race.GetComponent<ExplorationCollection>();var mountain=collection.routes[0];
                // Move the closing shore leg clear of the actual water footprint.
                var revised=mountain.points.ToArray();for(int i=0;i<revised.Length;i++)if(revised[i].z>-62&&revised[i].z<41&&revised[i].x<718)revised[i].x=Mathf.Lerp(revised[i].x,718,Smooth(-62,-40,revised[i].z)*(1-Smooth(22,41,revised[i].z)));
                mountain.points=revised;typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(mountain,null);mountain.Initialize();
                var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();
                var home=GameObject.Find("Dan - blue X").transform;var friend=GameObject.Find("Friend across street - blue circle").transform;
                // The legacy house also lives in combined scene meshes. Remove its
                // triangles from copies, preserving every neighboring building.
                foreach(var mf in GameObject.Find("Phase 6 - architectural render batches").GetComponentsInChildren<MeshFilter>())
                {
                    var mesh=Object.Instantiate(mf.sharedMesh);var v=mesh.vertices;var triangles=mesh.triangles;var keep=new List<int>();
                    for(int i=0;i<triangles.Length;i+=3){var p=mf.transform.TransformPoint((v[triangles[i]]+v[triangles[i+1]]+v[triangles[i+2]])/3);var q=home.InverseTransformPoint(p);if(Mathf.Abs(q.x)<18&&Mathf.Abs(q.z)<16&&q.y>-5&&q.y<20)continue;keep.AddRange(new[]{triangles[i],triangles[i+1],triangles[i+2]});}
                    mesh.SetTriangles(keep,0);mesh.RecalculateBounds();mf.sharedMesh=SaveMesh(mesh,scene.name+"-retired-home-"+mf.name);
                }
                var mouth=street.At(street.Project(new Vector3(470,0,-37),out _),out var streetDirection);
                var connection=Curve(new[]{mouth,new Vector3(497,84,-59),new(530,81,-77),new(591,79,-107),new(650,79,-112),new(690,79,-95),new(724,79,-55),new(724,79,20),new(695,79,45),new(654,79,70),new(594,80,72),new(548,82,35),new(526,83,-4),new(537,82,-57),new(530,81,-77),new(497,84,-59),mouth});
                var shore=new GameObject("Lake shore connection").AddComponent<RaceRoad>();shore.transform.SetParent(root);shore.points=connection;shore.forestTrail=true;shore.Initialize();collection.routes=new[]{mountain,shore};
                var house3=GameObject.Find("Original house 3").transform;var entry3=street.At(street.Project(house3.position+house3.forward*14,out _),out _);
                var driveway3=Curve(new[]{entry3,Vector3.Lerp(entry3,house3.position+house3.forward*9,.5f),house3.position+house3.forward*9});
                var downhill=Curve(new[]{home.TransformPoint(new Vector3(20,0,10)),home.TransformPoint(new Vector3(23,-1.5f,-7)),home.TransformPoint(new Vector3(23,-5.5f,-32)),home.TransformPoint(new Vector3(8,-5.5f,-30))});
                var terrain=GameObject.Find("Memory loop - north is +Z").transform;
                foreach(var mf in terrain.GetComponentsInChildren<MeshFilter>())
                {
                    var vertices=mf.sharedMesh.vertices;if(!vertices.Any(v=>v.x>375&&v.x<780&&v.z>-190&&v.z<95))continue;
                    var mesh=Object.Instantiate(mf.sharedMesh);var colors=mesh.colors;bool changed=false;
                    for(int i=0;i<vertices.Length;i++)
                    {
                        var p=mf.transform.TransformPoint(vertices[i]);if(p.x<375||p.x>780||p.z<-190||p.z>95)continue;
                        foreach(var points in new[]{connection,driveway3,downhill,revised})
                        {
                            float d=Near(p,points,out var at);if(d>10)continue;
                            float rs=street.Project(p,out _);var rp=street.At(rs,out _);float rd=Vector3.ProjectOnPlane(p-rp,Vector3.up).magnitude;
                            float width=points==connection||points==revised?5:2.8f;
                            float blend=(1-Smooth(width,10,d))*Smooth(7,12,rd);
                            if(blend<=0)continue;p.y=Mathf.Lerp(p.y,at.y,blend);colors[i]=Color.Lerp(colors[i],new(.43f,.35f,.23f),blend*(1-Smooth(width-1,width+1,d)));changed=true;
                        }
                        vertices[i]=mf.transform.InverseTransformPoint(p);
                    }
                    if(!changed){Object.DestroyImmediate(mesh);continue;}mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=SaveMesh(mesh,scene.name+"-connections-"+mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
                }
                Physics.SyncTransforms();var woods=GameObject.Find("Woods replacing later subdivisions");
                foreach(var c in woods.GetComponentsInChildren<BoxCollider>().ToArray())
                {
                    var p=c.bounds.center;bool clear=new[]{connection,driveway3,downhill}.Any(points=>Near(p,points,out _)<7);
                    if(clear){Object.DestroyImmediate(c.gameObject);continue;}
                    if(p.x>375&&p.x<780&&p.z>-190&&p.z<95)c.transform.position+=Vector3.up*(Phase6Buildings.Ground(p)-c.bounds.min.y);
                }
                RebuildMountainTrees(race,new List<string>());
                var life=race.GetComponent<AmbientLife>();
                Vector3 Ground(Vector3 p){p.y=Phase6Buildings.Ground(p)+.025f;return p;}
                life.football=new[]{Ground(home.TransformPoint(new Vector3(-7,0,11))),Ground(home.TransformPoint(new Vector3(-3,0,12))),Ground(home.TransformPoint(new Vector3(2,0,11)))};
                life.coffee=life.coffee.Select(Ground).ToArray();life.smoking=life.smoking.Select(Ground).ToArray();
                Sign(mouth+Vector3.Cross(Vector3.up,streetDirection).normalized*9,streetDirection,"KYLE'S WOODED DRIVE\nLAKE / MOUNTAIN TRAILS");
                Sign(shore.At(100,out var f)+Vector3.Cross(Vector3.up,f)*8,f,"LAKE SHORE\nBOTH TRAILS RETURN HOME");
                // Stable collectible identities survive the modest shoreline adjustment.
                foreach(var site in collection.sites)site.position=new Vector3(site.position.x,Phase6Buildings.Ground(site.position)+1,site.position.z);
                File.WriteAllText(Evidence+"/connections-"+scene.name+".txt","Terrain-painted Kyle/lake connection, shore return clear of water; descending House 3 approach and Dan's downhill garage access. Existing childhood fence assets retained; no hairpin road restored. Household positions regrounded, exclusive schedule unchanged.\nMouth="+mouth+"; shore="+shore.Length+"m; mountain="+mountain.Length+"m");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
            }
            AssetDatabase.SaveAssets();
        }
    }
}

