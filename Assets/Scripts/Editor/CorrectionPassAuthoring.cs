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
    // Correction-only authoring. Never regenerates roads, ramp profiles or properties.
    public static class CorrectionPassAuthoring
    {
        const string Folder="Assets/Track/CorrectionPass";
        public static void Author()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
            Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
            foreach(var path in ReverseReviewRelease.Scenes)
            {
                var scene=EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;road.Initialize();
                var report=new List<string>();
                var start=race.gates.First(g=>g.name.Contains("START FINISH"));
                if(race.reverseCourse)
                {
                    var previous=GameObject.Find("CR088-089 physical direction cues");if(previous)Object.DestroyImmediate(previous);
                    var superseded=GameObject.Find("CR075 direction signs");if(superseded)superseded.SetActive(false);
                    var root=new GameObject("CR088-089 physical direction cues").transform;
                    if(!race.Forest)
                    {
                        // Retire the crossing on the launch. Keep one beyond its landing zone.
                        var launch=race.gates.FirstOrDefault(g=>Mathf.Abs(road.Project(g.transform.position,out _)-1524.109f)<5);
                        var landing=race.gates.FirstOrDefault(g=>Mathf.Abs(road.Project(g.transform.position,out _)-1594.104f)<5);
                        if(launch){race.gates=race.gates.Where(g=>g!=launch).ToArray();report.Add("Retired launch gate "+launch.name+" @ "+launch.transform.position);Object.DestroyImmediate(launch.gameObject);}
                        if(landing){PlaceGate(landing,road,1690);report.Add("Post-landing gate station 1690; no required crossing on Trickum launch");}
                        RebuildTrees(race,report);
                        // Pine Ridge is a forward exit, never an advertised reverse shortcut.
                        float exit=road.Project(new Vector3(-624.83f,7.47f,-204.56f),out _);
                        for(int i=0;i<3;i++){var p=road.At(exit-28+i*12,out var f);Sign(root,p+Vector3.Cross(Vector3.up,f).normalized*10,f,i==0?"MAIN ROUTE\nSTRAIGHT":"^",new Color(.19f,.28f,.15f));}
                        var oldExit=new Vector3(-611,7.5f,-205);road.At(exit,out var exitDirection);Sign(root,oldExit,exitDirection,"EXIT ONLY\nNO REVERSE ROUTE",new Color(.32f,.23f,.15f));
                    }
                    else
                    {
                        // Use the supported main-route lead-in immediately before the split.
                        // The grid faces its main bend; both routes remain available first lap.
                        var first=Object.FindObjectsByType<WoodlandRoute>().OrderBy(b=>b.entryRoad).First();
                        PlaceGate(start,road,first.entryRoad-38);
                        for(int i=0;i<5;i++)
                        {
                            float s=first.entryRoad-15+i*14;var p=road.At(s,out var f);
                            Sign(root,p+Vector3.Cross(Vector3.up,f).normalized*(road.HalfWidth(s)+1.5f),f,i==0?"MAIN COURSE\nFOLLOW CURVE":"^",new Color(.34f,.25f,.10f));
                        }
                        report.Add("Forest Reverse start moved to supported station "+(first.entryRoad-38)+"; grid 32m before it, facing main route; physical main-course boards follow bend");
                    }
                    foreach(var b in Object.FindObjectsByType<WoodlandRoute>())
                    {
                        var p=road.At(b.entryRoad-25,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;
                        var mouth=b.At(Mathf.Max(12,b.entryInset),out _);float side=Vector3.Dot(mouth-p,right)>0?1:-1;
                        Sign(root,p+right*side*(road.HalfWidth(b.entryRoad)+2),f,"OPTIONAL\n"+(side>0?">  SHORTCUT":"SHORTCUT  <"),new Color(.16f,.26f,.17f));
                        var entrance=b.At(Mathf.Max(10,b.entryInset),out var direction);
                        Sign(root,entrance+Vector3.Cross(Vector3.up,direction).normalized*8,direction,"REVERSE\nSHORTCUT",new Color(.16f,.26f,.17f));
                    }
                    float origin=road.Project(start.transform.position,out _);
                    race.gates=race.gates.OrderBy(g=>g==start?-1:road.Relative(road.Project(g.transform.position,out _),origin)).ToArray();
                    var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;
                    spawn.position=road.At(origin-32,out var heading)+Vector3.up*.7f;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(heading,Vector3.up));
                    race.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
                    report.Add("Grid "+spawn.position+"; timing/visible START FINISH index 0 @ "+start.transform.position+"; origin="+origin);
                }
                for(int i=0;i<race.gates.Length;i++)
                {
                    var g=race.gates[i];g.name=race.courseName+(i==0?" START FINISH":" CP "+i);
                    // Inner faces of the posts coincide with the accepted opening.
                    foreach(Transform t in g.transform)
                    {
                        var scale=t.localScale;var p=t.localPosition;
                        if(scale.y>2&&scale.x<.5f){p.x=Mathf.Sign(p.x)*(g.halfWidth+scale.x*.5f);t.localPosition=p;}
                        else if(scale.x>5&&scale.y<.5f&&p.y>1){scale.x=g.halfWidth*2+.5f;t.localScale=scale;}
                    }
                    report.Add("GATE "+i+" station="+road.Project(g.transform.position,out _)+" opening="+g.halfWidth*2+" tolerance="+RaceGate.EdgeTolerance);
                }
                foreach(var b in Object.FindObjectsByType<WoodlandRoute>())
                {
                    b.bypassedGates=race.gates.Select((g,i)=>(s:road.Relative(road.Project(g.transform.position,out _),b.entryRoad),i))
                        .Where(x=>x.i>0&&x.s>0&&x.s<road.Relative(b.exitRoad,b.entryRoad)).Select(x=>x.i).ToArray();
                    report.Add("BRANCH "+b.title+" bypass="+string.Join(",",b.bypassedGates));
                }
                race.courseId=race.Forest?(race.reverseCourse?"forest-reverse-v3-corrections":"lake-v5-corrections"):(race.reverseCourse?"street-reverse-v3-corrections":"street-v12-corrections");
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);File.WriteAllLines("Docs/CR082-089/author-"+scene.name+".txt",report);
                if(race.reverseCourse)
                {
                    var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;
                    LivingWorldValidation.Capture("Docs/CR082-089/after-"+scene.name+"-grid.png",spawn.position-spawn.forward*6+Vector3.up*3,spawn.position+spawn.forward*28+Vector3.up);
                    foreach(var b in Object.FindObjectsByType<WoodlandRoute>())LivingWorldValidation.Capture("Docs/CR082-089/after-"+scene.name+"-"+b.title.Replace(' ','-')+".png",road.At(b.entryRoad-35,out _)+Vector3.up*2.6f,road.At(b.entryRoad+20,out _)+Vector3.up*1.5f);
                }
            }
            AssetDatabase.SaveAssets();
        }
        static void PlaceGate(RaceGate gate,RaceRoad road,float station){var p=road.At(station,out var f);gate.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));}
        static Vector3 Ground(Vector3 p)
        {
            var hits=Physics.RaycastAll(p+Vector3.up*70,Vector3.down,150,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).ToArray();if(hits.Length>0)p.y=hits[0].point.y;return p;
        }
        static void Sign(Transform parent,Vector3 p,Vector3 forward,string words,Color color)
        {
            var matPath=Folder+"/"+(color.g>.25f?"DirectionGreen":"DirectionAmber")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};AssetDatabase.CreateAsset(mat,matPath);}
            var sign=new GameObject(words).transform;sign.SetParent(parent);sign.SetPositionAndRotation(Ground(p),Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));sign.gameObject.AddComponent<PhysicalSign>();
            void Box(Vector3 at,Vector3 size){var b=GameObject.CreatePrimitive(PrimitiveType.Cube);b.transform.SetParent(sign,false);b.transform.localPosition=at;b.transform.localScale=size;b.GetComponent<Renderer>().sharedMaterial=mat;Object.DestroyImmediate(b.GetComponent<Collider>());}
            Box(new(0,2.5f,0),new(6,2,.18f));foreach(float x in new[]{-2f,2f})Box(new(x,1.3f,.12f),new(.15f,2.6f,.15f));
            var text=new GameObject("Mounted lettering").AddComponent<TextMesh>();text.transform.SetParent(sign,false);text.transform.localPosition=new(0,2.5f,-.11f);text.text=words;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");text.fontSize=64;text.characterSize=.24f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=Color.white;text.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Environment/Phase8/Depth tested world lettering 0.mat");
            var size=text.GetComponent<Renderer>().localBounds.size;text.characterSize*=Mathf.Min(5.4f/Mathf.Max(.01f,size.x),1.6f/Mathf.Max(.01f,size.y));
            sign.localScale=Vector3.one*.65f;
        }
        static void RebuildTrees(RaceDirector race,List<string> report)
        {
            var woods=GameObject.Find("Woods replacing later subdivisions").transform;
            var branches=Object.FindObjectsByType<WoodlandRoute>();int removed=0;
            foreach(var box in woods.GetComponentsInChildren<BoxCollider>())
            {
                bool blocked=branches.Any(b=>{float s=b.Project(box.bounds.center,out float d);return s<150&&d<17;});
                if(blocked){Object.DestroyImmediate(box.gameObject);removed++;}
            }
            var boxes=woods.GetComponentsInChildren<BoxCollider>();if(boxes.Length==0)throw new Exception("Missing authored tree colliders");
            var shapes=new[]{"BroadCrown","IrregularCrown","UprightCrown"}.Select(n=>AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/"+n+".asset")).ToArray();
            var trunk=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/Trunk.asset");var batches=new Dictionary<Vector2Int,List<CombineInstance>>();var temps=new List<Mesh>();
            var originalRoad=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();var sites=GameObject.Find(Phase6Review.Root).transform.Cast<Transform>().ToArray();
            foreach(var box in boxes)
            {
                var bounds=box.bounds;var p=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);float h=bounds.size.y*2;
                float patch=Mathf.PerlinNoise((p.x+913)/65,(p.z+771)/65),hash=Mathf.Repeat(Mathf.Sin(p.x*12.9898f+p.z*78.233f)*43758.5453f,1);
                int kind=patch<.43f?2:patch>.57f?1:0;float radius=h*Mathf.Lerp(.27f,.34f,hash);
                if(box.name.StartsWith("CR014 trunk ")||box.name.StartsWith("CR016 trunk "))radius*=1.35f;
                radius=Mathf.Min(radius,Mathf.Max(.1f,Phase6Buildings.YardDistance(p)-27),StreetLoopBuilder.Nearest(p,originalRoad,out _)-16,Phase5Setup.Distance(p,cut,out _)-7.6f);
                if(CompactYard.OldDistance(p)<40)radius=Mathf.Min(radius,Mathf.Max(.1f,CompactYard.AccessDistance(p)-3.5f));
                foreach(var site in sites)radius=Mathf.Min(radius,Mathf.Max(.1f,Vector3.ProjectOnPlane(site.position-p,Vector3.up).magnitude-16));
                foreach(var b in branches){b.Project(p,out var d);radius=Mathf.Min(radius,Mathf.Max(.1f,d-10));}
                radius=Mathf.Max(.1f,radius);
                var key=new Vector2Int(Mathf.FloorToInt(p.x/160),Mathf.FloorToInt(p.z/160));if(!batches.TryGetValue(key,out var batch))batches[key]=batch=new();
                var crown=Object.Instantiate(shapes[kind]);var tint=Color.Lerp(new(.24f,.34f,.19f),new(.39f,.46f,.25f),Mathf.Clamp01(patch*.8f+hash*.2f));crown.colors=crown.colors.Select(c=>c*tint).ToArray();temps.Add(crown);
                batch.Add(new(){mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.36f,Quaternion.Euler(0,hash*360,0),new(radius,h*Mathf.Lerp(.59f,.70f,hash),radius))});
                var bark=Object.Instantiate(trunk);bark.colors=Enumerable.Repeat(new Color(.25f,.20f,.145f),bark.vertexCount).ToArray();temps.Add(bark);batch.Add(new(){mesh=bark,transform=box.transform.localToWorldMatrix*Matrix4x4.TRS(box.center,Quaternion.identity,box.size)});
            }
            foreach(var filter in woods.GetComponentsInChildren<MeshFilter>()){if(filter.GetComponent<Collider>())throw new Exception("Refusing to remove collider-owned tree mesh");Object.DestroyImmediate(filter.gameObject);}
            foreach(var pair in batches)
            {
                var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);string path=Folder+"/StreetReverseTrees-"+pair.Key.x+"-"+pair.Key.y+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
                var go=new GameObject("Complete authored trees",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(woods,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");
            }
            foreach(var mesh in temps)Object.DestroyImmediate(mesh);
            report.Add("Rebuilt coherent complete crowns/trunks from "+boxes.Length+" remaining authored trees into "+batches.Count+" reverse-only batches; removed "+removed+" whole entrance trees. Replaces orphan lobe fragments from earlier centroid trimming. No tree LOD/billboard generators on this root.");
        }
    }
}
