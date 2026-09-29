using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class DumpCorrection {
 const string Folder="Assets/Track/DumpCorrection",Evidence="Docs/DumpCorrection";
 static readonly Vector3 Origin=new(309.2f,0,15.7f),Axis=new Vector3(-50.6f,0,-7.5f).normalized;
 static Vector3 Side=>Vector3.Cross(Vector3.up,Axis);
 static float Radius(Vector3 p){var q=p-Origin;return Mathf.Sqrt(Mathf.Pow((Vector3.Dot(q,Axis)-25.5f)/24.5f,2)+Mathf.Pow(Vector3.Dot(q,Side)/23,2));}
 static float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 if(Directory.Exists(Folder))throw new Exception("One-shot correction already exists; inspect before applying again");
 var scene=EditorSceneManager.OpenScene(Racer.Editor.BackyardForwardAuthoring.ScenePath);Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();Physics.SyncTransforms();
 var trees=Object.FindObjectsByType<Collider>().Where(c=>c.name.Contains("trunk")||c.name.Contains("Tree")).Where(c=>Radius(c.bounds.center)<1.02f).ToArray();
 var old=trees.ToDictionary(c=>c,c=>(p:c.bounds.center,y:Ground(c.bounds.center)));
 var rows=new List<string>();int samples=0;
 foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
 if(!mf.name.StartsWith("Ground_"))continue;var mesh=mf.sharedMesh;var v=mesh.vertices;var colors=mesh.colors;bool changed=false;
 for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);float r=Radius(p);if(r>=1)continue;float rim=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.38f,1,r));float floor=61.5f;float y=Mathf.Lerp(floor,p.y,rim);if(y>=p.y-.0001f)continue;p.y=y;v[i]=mf.transform.InverseTransformPoint(p);if(colors.Length==v.Length)colors[i]=Color.Lerp(new Color(.29f,.235f,.155f),colors[i],rim*.8f);samples++;changed=true;}
 if(!changed)continue;if(!AssetDatabase.GetAssetPath(mesh).StartsWith("Assets/Track/BackyardForward/"))throw new Exception("Unexpected shared terrain: "+mf.name);
 var copy=Object.Instantiate(mesh);copy.name=mf.name+" dump bowl";copy.vertices=v;copy.colors=colors;copy.RecalculateNormals();copy.RecalculateBounds();AssetDatabase.CreateAsset(copy,Folder+"/"+mf.name+".asset");mf.sharedMesh=copy;mf.GetComponent<MeshCollider>().sharedMesh=copy;rows.Add(mf.name+": one shared rendering/collision mesh");
 }
 Physics.SyncTransforms();var removed=new List<Vector3>();var shifts=new List<(Vector3 p,float dy)>();
 foreach(var tree in trees){var prior=old[tree];float dy=Ground(prior.p)-prior.y;if(Math.Abs(dy)<.015f)continue;if(Radius(prior.p)<.73f){removed.Add(prior.p);Object.DestroyImmediate(tree.gameObject);}else{tree.transform.position+=Vector3.up*dy;shifts.Add((prior.p,dy));}}
 int batch=0;foreach(var mf in GameObject.Find("Woods replacing later subdivisions").GetComponentsInChildren<MeshFilter>()){
 var mesh=mf.sharedMesh;var v=mesh.vertices;var tris=mesh.triangles;var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int n){while(parent[n]!=n){parent[n]=parent[parent[n]];n=parent[n];}return n;}void Union(int a,int b){parent[Find(a)]=Find(b);}
 var weld=new Dictionary<Vector3Int,int>();for(int i=0;i<v.Length;i++){var key=Vector3Int.RoundToInt(v[i]*1000);if(weld.TryGetValue(key,out int other))Union(i,other);else weld[key]=i;}for(int i=0;i<tris.Length;i+=3){Union(tris[i],tris[i+1]);Union(tris[i],tris[i+2]);}
 var groups=new Dictionary<int,Bounds>();for(int i=0;i<v.Length;i++){int id=Find(i);if(!groups.TryGetValue(id,out var b))b=new Bounds(v[i],Vector3.zero);b.Encapsulate(v[i]);groups[id]=b;}
 var cuts=new HashSet<int>();var moves=new Dictionary<int,float>();foreach(var pair in groups){var p=mf.transform.TransformPoint(pair.Value.center);float D(Vector3 t)=>Vector2.Distance(new(t.x,t.z),new(p.x,p.z));if(removed.Any(t=>D(t)<1.8f)){cuts.Add(pair.Key);continue;}var near=shifts.OrderBy(t=>D(t.p)).FirstOrDefault();if(shifts.Count>0&&D(near.p)<1.8f)moves[pair.Key]=near.dy;}
 if(cuts.Count==0&&moves.Count==0)continue;var kept=new List<int>();for(int i=0;i<tris.Length;i+=3)if(!cuts.Contains(Find(tris[i])))kept.AddRange(new[]{tris[i],tris[i+1],tris[i+2]});for(int i=0;i<v.Length;i++)if(moves.TryGetValue(Find(i),out float dy))v[i]+=mf.transform.InverseTransformVector(Vector3.up*dy);var copy=Object.Instantiate(mesh);copy.vertices=v;copy.SetTriangles(kept,0);copy.RecalculateBounds();AssetDatabase.CreateAsset(copy,Folder+"/Affected-trees-"+batch+++".asset");mf.sharedMesh=copy;
 }
 foreach(var t in Object.FindObjectsByType<Transform>().Where(t=>t.name.StartsWith("Old dump ")).ToArray())Object.DestroyImmediate(t.gameObject);
 var root=new GameObject("Old dump - scattered salvage - no collision").transform;root.SetParent(GameObject.Find("Dan's Backyard Forward - terrain trail").transform);
 Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){name=name,color=color};m.SetFloat("_Smoothness",.12f);AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;}
 var rust=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/BackyardForward/Old rust.mat");var wood=Mat("Weathered boards",new(.38f,.31f,.20f));var metal=Mat("Dull discarded metal",new(.38f,.43f,.40f));var faded=Mat("Faded container enamel",new(.27f,.36f,.36f));var black=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/CombinedReview/Tire black.mat");
 var random=new System.Random(39000);float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());int pieces=0;
 void Piece(string name,PrimitiveType type,Vector3 p,Vector3 size,Quaternion rotation,Material material){var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(root);go.transform.SetPositionAndRotation(p,rotation);go.transform.localScale=size;go.GetComponent<Renderer>().sharedMaterial=material;Object.DestroyImmediate(go.GetComponent<Collider>());var verts=go.GetComponent<MeshFilter>().sharedMesh.vertices;float lift=float.NegativeInfinity;foreach(var v in verts){var w=go.transform.TransformPoint(v);lift=Math.Max(lift,Ground(w)-w.y);}go.transform.position+=Vector3.up*(lift-.035f);pieces++;}
 // Irregular separate pockets; the central 8m driving lane and far escape slope remain open.
 for(int i=0;i<23;i++){float a=R(10,40),s=R(5.2f,15.5f)*(i%2==0?1:-1);var p=Origin+Axis*a+Side*s;if(Radius(p)>.81f){i--;continue;}p.y=Ground(p);float yaw=R(0,360);var rot=Quaternion.Euler(R(-12,12),yaw,R(-12,12));switch(i%5){
 case 0:Piece("Discarded rusted drum",PrimitiveType.Cylinder,p,new(.95f,.63f,.95f),Quaternion.Euler(86,yaw,R(-8,8)),rust);Piece("Loose drum lid",PrimitiveType.Cylinder,p+Side*1.2f,new(.92f,.045f,.92f),rot,metal);break;
 case 1:for(int j=0;j<4;j++)Piece("Broken pallet board",PrimitiveType.Cube,p+Side*(j*.37f),new(R(1.5f,2.4f),.10f,.27f),Quaternion.Euler(0,yaw+R(-18,18),R(-5,5)),wood);break;
 case 2:Piece("Dented old container",PrimitiveType.Cube,p,new(R(.8f,1.2f),R(.45f,.9f),R(.6f,1.1f)),rot,faded);Piece("Discarded sheet metal",PrimitiveType.Cube,p+Axis*1.6f,new(1.6f,.055f,1.05f),Quaternion.Euler(6,yaw+42,5),metal);break;
 case 3:for(int j=0;j<3;j++)Piece("Old rubber wheel",PrimitiveType.Cylinder,p+Axis*(j*.73f)+Side*R(-.35f,.35f),new(.85f,.15f,.85f),Quaternion.Euler(R(-15,15),yaw,R(-15,15)),black);break;
 default:Piece("Bent scrap beam",PrimitiveType.Cube,p,new(2.5f,.17f,.24f),rot,rust);Piece("Discarded short timber",PrimitiveType.Cube,p+Side*.8f,new(1.4f,.19f,.35f),Quaternion.Euler(5,yaw+70,3),wood);break;
 }}
 PlayerSettings.bundleVersion="0.39.0-review1";Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
 rows.Add($"Terrain samples={samples}; local tree removals={removed.Count}; locally regrounded trees={shifts.Count}; affected visual batches={batch}; junk pieces={pieces}; junk colliders=0");rows.Add("Ellipse: along 1..50m, lateral +/-23m, center along25.5m; floor Y61.5; smooth sloped perimeter. Exact launch and landing, route, gates, AI/navigation and other geometry preserved.");File.WriteAllLines(Evidence+"/implementation.txt",rows);File.WriteAllText(Evidence+"/tree-adjustments.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{removed,shifts},new Newtonsoft.Json.JsonSerializerSettings{ContractResolver=new Racer.Editor.ForwardJsonResolver()}));return string.Join("\n",rows);
 }
}
