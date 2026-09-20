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
    public static partial class DiscoveryAuthoring
    {
        static Bounds LocalBounds(MeshFilter mesh,Transform target){var vertices=mesh.sharedMesh.vertices;var bounds=new Bounds(target.InverseTransformPoint(mesh.transform.TransformPoint(vertices[0])),Vector3.zero);foreach(var v in vertices)bounds.Encapsulate(target.InverseTransformPoint(mesh.transform.TransformPoint(v)));return bounds;}
        public static void AuditFinish()
        {
            foreach(var scene in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();worldRoot=GameObject.Find("CR091-096 exploration world").transform;var rows=new List<string>();int n=0;
                foreach(var fence in worldRoot.GetComponentsInChildren<BreakableProp>().Where(p=>p.name.StartsWith("Grounded "))){var mf=fence.GetComponent<MeshFilter>();var mesh=Object.Instantiate(mf.sharedMesh);var v=mesh.vertices;var q=mesh.bounds.center*2;q.y=0;var b=fence.transform.TransformPoint(q);float oldA=v.Where(p=>new Vector2(p.x,p.z).magnitude<.12f).Min(p=>p.y)+.05f;float oldB=v.Where(p=>Vector2.Distance(new(p.x,p.z),new(q.x,q.z))<.12f).Min(p=>p.y)+.05f;float da=Phase6Buildings.Ground(fence.transform.position)-(fence.transform.position.y+oldA),db=Phase6Buildings.Ground(b)-(fence.transform.position.y+oldB);
                    for(int i=0;i<v.Length;i++){float t=Mathf.Clamp01(Vector3.Dot(new Vector3(v[i].x,0,v[i].z),q)/Mathf.Max(.001f,q.sqrMagnitude));v[i].y+=Mathf.Lerp(da,db,t);}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=MeshAsset(mesh,owner.gameObject.scene.name+"-regrounded-fence-"+n++);var box=fence.GetComponent<BoxCollider>();box.center=mf.sharedMesh.bounds.center;box.size=mf.sharedMesh.bounds.size+Vector3.one*.15f;rows.Add($"{fence.name}|a={fence.transform.position}|b={b}|adjustmentA={da:F4}|adjustmentB={db:F4}|post feet terrain minus .05m");}
                File.WriteAllLines(Evidence+"/fence-grounding-"+owner.gameObject.scene.name+".txt",rows);rows.Clear();
                foreach(var smash in Object.FindObjectsByType<ActivitySite>().Where(s=>s.kind==ActivitySite.Kind.Smash)){
                    var roadside=worldRoot.GetComponentsInChildren<BreakableProp>().Where(p=>p.name.StartsWith("Grounded ")).Select(p=>{float s=street.Project(p.transform.position,out float d);return (prop:p,station:s,lateral:d);}).Where(p=>p.lateral>9&&p.lateral<13).OrderBy(p=>p.station).ToArray();
                    BreakableProp[] best=null;float bestScore=float.PositiveInfinity;
                    for(int i=0;i<=roadside.Length-14;i++){var run=roadside.Skip(i).Take(14).ToArray();if(Enumerable.Range(1,13).Any(j=>run[j].station-run[j-1].station>3.5f||run[j].station-run[j-1].station<1))continue;var a=run[0].prop.transform.position;var b=run[^1].prop.transform.position;var line=new[]{a,b};float bend=run.Max(p=>Near(p.prop.transform.position,line,out _));float score=bend*100+Vector3.Distance((a+b)*.5f,smash.transform.position)*.1f;if(score>=bestScore)continue;bestScore=score;best=run.Select(p=>p.prop).ToArray();}
                    if(best==null)throw new Exception("No continuous replacement smash run");smash.props=best;smash.transform.position=(best[0].transform.position+best[^1].transform.position)*.5f;smash.forward=Vector3.ProjectOnPlane(best[^1].transform.position-best[0].transform.position,Vector3.up).normalized;
                    var label=Object.FindObjectsByType<TextMesh>().FirstOrDefault(t=>t.text.StartsWith("FENCE LINE SMASH"));var sign=label?label.GetComponentInParent<BreakableProp>():null;if(sign){sign.transform.SetPositionAndRotation(Ground(best[0].transform.position-smash.forward*8),Quaternion.LookRotation(smash.forward));}
                    if(smash.props.Length<smash.gold)throw new Exception("Insufficient replacement fence targets for "+smash.id);
                    File.WriteAllLines(Evidence+"/smash-targets-"+owner.gameObject.scene.name+".txt",smash.props.Select(p=>p.name+" "+p.transform.position));
                }
                var retained=new List<TextMesh>();
                foreach(var label in Object.FindObjectsByType<TextMesh>().Where(t=>t.GetComponent<Renderer>().enabled&&!SceneryText.IsFloating(t))){
                    label.text=System.Text.RegularExpressions.Regex.Replace(label.text,@"(\d+(?:\.\d+)?)(?:\s*-\s*(\d+(?:\.\d+)?))?\s*km/h",m=>{int mph=(int)Math.Round(float.Parse(m.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture)/1.609344f);return m.Groups[2].Success?mph+" - "+Math.Round(float.Parse(m.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture)/1.609344f)+" mph":mph+" mph";});
                    var duplicate=retained.FirstOrDefault(t=>Vector3.Distance(t.transform.position,label.transform.position)<.5f&&t.text==label.text);
                    if(duplicate){label.GetComponent<Renderer>().enabled=false;rows.Add("Duplicate coincident lettering disabled, primary preserved: "+label.text.Replace('\n','/'));}else retained.Add(label);
                }
                foreach(var text in Object.FindObjectsByType<TextMesh>()){
                    if(!text.GetComponent<Renderer>().enabled||SceneryText.IsFloating(text)||!text.transform.parent)continue;var parent=text.transform.parent;var candidates=parent.GetComponentsInChildren<MeshFilter>().Select(m=>(mesh:m,b:LocalBounds(m,text.transform))).Where(v=>v.b.size.x>1&&v.b.size.y>.4f&&v.b.size.z<Mathf.Min(v.b.size.x,v.b.size.y)*1.5f&&Mathf.Abs(v.b.center.z)<3).OrderBy(v=>v.b.center.sqrMagnitude).ToArray();
                    MeshFilter board;Bounds bounds;
                    if(candidates.Length>0){board=candidates[0].mesh;bounds=candidates[0].b;}else{
                        var size=text.GetComponent<Renderer>().localBounds.size;var face=Part(parent,"Mounted fitted backing",parent.InverseTransformPoint(text.transform.TransformPoint(new Vector3(0,0,.09f))),new(Mathf.Max(2,size.x*1.2f),Mathf.Max(1,size.y*1.3f),.12f),Mat("Sign timber",new(.22f,.14f,.07f)));face.rotation=text.transform.rotation;board=face.GetComponent<MeshFilter>();bounds=LocalBounds(board,text.transform);
                    }
                    var support=board.GetComponentInParent<BreakableProp>();
                    if(support&&support.GetComponentsInChildren<TextMesh>().Length<5){
                        var bb=board.sharedMesh.bounds;float lift=0;for(int sample=0;sample<=8;sample++){var foot=board.transform.TransformPoint(new Vector3(Mathf.Lerp(bb.min.x,bb.max.x,sample/8f),bb.min.y,bb.center.z));lift=Mathf.Max(lift,Phase6Buildings.Ground(foot)+.15f-foot.y);}
                        if(lift>0&&(lift<5||support.GetComponent<PhysicalSign>()))support.transform.position+=Vector3.up*lift;
                        foreach(var post in support.GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.IndexOf("post",StringComparison.OrdinalIgnoreCase)>=0&&Vector3.Dot(r.transform.up,Vector3.up)>.95f)){
                            var pb=post.bounds;float ground=Phase6Buildings.Ground(post.transform.position)-.03f;float height=pb.max.y-ground;if(height<.1f||pb.size.y<.01f)continue;post.transform.position+=Vector3.up*((pb.max.y+ground)*.5f-pb.center.y);var scale=post.transform.localScale;scale.y*=height/pb.size.y;post.transform.localScale=scale;
                        }
                    }
                    var rb=text.GetComponent<Renderer>().localBounds;float ratio=Mathf.Min(bounds.size.x*.86f/Mathf.Max(.01f,rb.size.x),bounds.size.y*.78f/Mathf.Max(.01f,rb.size.y));
                    if(ratio<1)text.characterSize*=ratio*.98f;
                    if(!parent.GetComponent<PhysicalSign>())parent.gameObject.AddComponent<PhysicalSign>();
                    rows.Add(text.name+"|"+text.text.Replace('\n','/')+"|backing="+board.name+"|available="+bounds.size+"|scale="+text.characterSize);
                }
                File.WriteAllLines(Evidence+"/all-sign-fit-"+owner.gameObject.scene.name+".txt",rows);
                // Reground all camp props after collectible terrain changes.
                var camp=GameObject.Find("Permanent mountainside camp / two seated guys").transform;
                foreach(Transform child in camp){if(child.name=="Seated guy"){child.position=Ground(child.position);continue;}var renderer=child.GetComponent<Renderer>();if(renderer){float delta=Phase6Buildings.Ground(child.position)-renderer.bounds.min.y;child.position+=Vector3.up*delta;}}
                var corner=owner.GetComponent<ExplorationCollection>().sites.Single(s=>s.id=="woodland-24");var approach=Vector3.ProjectOnPlane(corner.position-corner.access,Vector3.up).normalized;
                var prior=Ground(corner.position);float distance=Vector3.ProjectOnPlane(prior-corner.access,Vector3.up).magnitude;
                var pocket=corner.access+approach*Mathf.Min(16,distance);pocket.y=Mathf.Lerp(corner.access.y,prior.y,Mathf.Min(1,16/Mathf.Max(1,distance)));corner.position=pocket+Vector3.up;
                var corridor=new[]{corner.access-approach*6,pocket+approach*6};
                // A narrow spur was folded by coarse terrain triangles at the ridge edge.
                // Shape a full supported vehicle corridor, including both endpoint aprons.
                Terrain("acorn24-supported-access",p=>{if(Mathf.Abs(p.x-corner.access.x)>55||Mathf.Abs(p.z-corner.access.z)>55)return p;float d=Near(p,corridor,out var at);if(d<18)p.y=Mathf.Lerp(p.y,at.y,1-Smooth(9,18,d));return p;});
                ClearTrees(p=>Near(p,corridor,out _)<9);
                rows.Clear();foreach(var site in owner.GetComponent<ExplorationCollection>().sites){site.position=Ground(site.position)+Vector3.up;site.access=Ground(site.access);rows.Add($"{site.id},{site.title},{site.approach},{site.position.x:F3},{site.position.y:F3},{site.position.z:F3},{site.access.x:F3},{site.access.y:F3},{site.access.z:F3}");}File.WriteAllLines(Evidence+"/final-placements-"+owner.gameObject.scene.name+".csv",rows);
                // Final map is rebaked after the ramp landing refinement.
                street=owner.ambientRoad?owner.ambientRoad:owner.road;
                var old=owner.GetComponent<ExplorationMap>().terrain;string assetPath=AssetDatabase.GetAssetPath(old);if(old){string backupPath=Folder+"/pre-final-map-"+owner.gameObject.scene.name+".asset";if(!AssetDatabase.LoadAssetAtPath<Texture2D>(backupPath))AssetDatabase.CreateAsset(Object.Instantiate(old),backupPath);AssetDatabase.DeleteAsset(assetPath);}Map();Save();
            }
        }
        public static void ProtectBaselineRoutes()
        {
            foreach(var path in ReverseReviewRelease.Scenes){string name=Path.GetFileNameWithoutExtension(path);EditorSceneManager.OpenScene("Assets/ValidationBaseline/CR091/"+name+".unity");
                var originals=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>().ToDictionary(f=>f.name,f=>f.sharedMesh.vertices);
                EditorSceneManager.OpenScene(path);var race=Object.FindAnyObjectByType<RaceDirector>();var routes=new List<(Vector3[] points,float width)>{(race.road.points,race.Forest?10:14)};if(race.ambientRoad)routes.Add((race.ambientRoad.points,14));foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())routes.Add((branch.points,branch.halfWidth+5));foreach(var route in race.GetComponent<ExplorationCollection>().routes.Take(2))routes.Add((route.points,8));int restored=0;float max=0;
                foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
                    if(!originals.TryGetValue(mf.name,out var old))continue;var v=mf.sharedMesh.vertices;if(v.Length!=old.Length)throw new Exception("Baseline topology differs "+mf.name);bool changed=false;
                    for(int i=0;i<v.Length;i++){float delta=Mathf.Abs(v[i].y-old[i].y);if(delta<.0001f)continue;var p=mf.transform.TransformPoint(v[i]);float blend=0;foreach(var route in routes){float d=Near(p,route.points,out _);blend=Mathf.Max(blend,1-Smooth(route.width,route.width+4,d));}if(blend<=0)continue;v[i].y=Mathf.Lerp(v[i].y,old[i].y,blend);changed=true;restored++;max=Mathf.Max(max,delta);}
                    if(!changed)continue;var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=MeshAsset(mesh,name+"-baseline-protected-"+mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
                }
                File.WriteAllText(Evidence+"/baseline-route-protection-"+name+".txt",$"Restored {restored} terrain vertices around original race/ambient/shortcut/mountain routes from a89577e9. Maximum prior change {max:F4}m. Original gate/branch/ramp object geometry unchanged. Optional pockets blend outside protected corridors.");Save();
            }
        }
    }
}
