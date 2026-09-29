using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
public static class YardResetAuthoring {
 const string Folder="Assets/Track/YardReset", Evidence="Docs/YardReset";
 public static readonly string[] Scenes={"StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse"};
 public static readonly Vector3[] Anchors={new(463.6f,80.2f,8),new(422.7f,83.3f,9),new(409.5f,81.9f,9.3f),new(391.9f,79.9f,9.7f),new(331.4f,78.5f,16.2f),new(258.6f,68.2f,8.2f),new(96.1f,38.8f,-108.6f),new(456.4f,81.8f,67.4f)};
 static readonly string[] Labels={"START / FINISH","HILL START","PARKING START","DIRT PATH START","FUTURE DUMP LAUNCH","FUTURE DUMP LANDING","FUTURE BIG GULLY","FOREST RETURN"};
 static Transform home;static float top,parking;static MeshCollider driveway;
 static float Smooth(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
 static float Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,300,p.z),Vector3.down,600,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").OrderBy(h=>Mathf.Abs(h.point.y-p.y)).First().point.y;
 static Mesh Store(Mesh m,string n){string path=Folder+"/"+n+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(path))throw new Exception("Already authored: "+path);AssetDatabase.CreateAsset(m,path);return m;}
 static Vector3 Closest(Vector3 p){var q=home.InverseTransformPoint(p);Vector3 best=q;float dist=float.MaxValue;foreach(var r in new[]{new Vector4(-24.5f,-25,-19.5f,top),new Vector4(-44,-12,-24.5f,-6),new Vector4(-19.5f,-12,-17,-6)}){var c=new Vector3(Mathf.Clamp(q.x,r.x,r.z),q.y,Mathf.Clamp(q.z,r.y,r.w));float d=(c-q).sqrMagnitude;if(d<dist){dist=d;best=c;}}return home.TransformPoint(best);}
 static float Height(float x){if(x<=409.5f)return parking;if(x<=422.7f)return Mathf.Lerp(parking,82.8f,Smooth(409.5f,422.7f,x));if(x<=463.6f)return Mathf.Lerp(82.8f,79.73f,Smooth(422.7f,463.6f,x));return 79.73f;}
 static float Surface(Vector3 p){var q=home.InverseTransformPoint(p);float blend=Smooth(top-5,top,q.z);if(blend<=0)return Height(p.x);var edge=home.TransformPoint(new Vector3(Mathf.Clamp(q.x,-24.47f,-19.53f),q.y,top-.03f));if(!driveway.Raycast(new Ray(new Vector3(edge.x,200,edge.z),Vector3.down),out var hit,400))throw new Exception("No original entrance surface "+edge);return Mathf.Lerp(Height(p.x),hit.point.y,blend);}
 static float Distance(Vector3 p,Bounds b){float dx=Mathf.Max(b.min.x-p.x,0,p.x-b.max.x),dz=Mathf.Max(b.min.z-p.z,0,p.z-b.max.z);return Mathf.Sqrt(dx*dx+dz*dz);}
 public static void Run(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();var report=new List<string>();
 var markerMat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Temporary anchor pink.mat");if(!markerMat){markerMat=new Material(Shader.Find("Universal Render Pipeline/Unlit"));markerMat.color=new Color(1,.2f,.7f);AssetDatabase.CreateAsset(markerMat,Folder+"/Temporary anchor pink.mat");}
 foreach(string name in Scenes){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");home=GameObject.Find("Dan - blue X").transform;
 var concrete=Object.FindObjectsByType<MeshFilter>().Single(m=>m.name=="Ground_Dan beige concrete descent and parking");driveway=concrete.GetComponent<MeshCollider>();var original=concrete.sharedMesh;var cv=original.vertices;top=cv.Max(v=>home.InverseTransformPoint(concrete.transform.TransformPoint(v)).z);
 parking=Ground(Anchors[3]);report.Add(name+" flat parking target="+parking.ToString("F5"));
 // Cache the old pavement profile before swapping its collider. Both terrain and
 // concrete evaluate this one final surface; no road or forest footprint added.
 var surfaceHeights=new Dictionary<Vector2,float>();
 float CachedSurface(Vector3 p){var key=new Vector2(p.x,p.z);if(!surfaceHeights.TryGetValue(key,out float h)){h=Surface(p);surfaceHeights[key]=h;}return h;}
 var buildings=home.GetComponentsInChildren<Collider>().Where(c=>c.name=="Garage walls"||c.name=="Pool house walls"||c.name=="Brick lower rear level"||c.name=="Pool coping").Select(c=>c.bounds).ToArray();
 var fences=Object.FindObjectsByType<MeshFilter>().Where(m=>(m.name=="Grounded wood crossbuck"||m.name=="Grounded roadside chain-link")&&Distance(m.GetComponent<Renderer>().bounds.center,new Bounds(new Vector3(430,80,12),new Vector3(87,0,45)))<3).Select(m=>(m,a:m.transform.Find("Fence endpoint A"),b:m.transform.Find("Fence endpoint B"))).Where(f=>f.a&&f.b).Select(f=>(f.m,f.a,f.b,oldA:Ground(f.a.position),oldB:Ground(f.b.position))).ToArray();
 foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
 if(!mf.GetComponent<Renderer>().bounds.Intersects(new Bounds(new Vector3(430,80,14),new Vector3(94,200,54))))continue;
 var source=mf.sharedMesh;var vertices=source.vertices;var uv=source.uv;var tris=source.triangles;var result=new List<Vector3>();var tex=new List<Vector2>();var indices=new List<int>();int changed=0;
 Vector3 Shape(Vector3 local){var p=mf.transform.TransformPoint(local);var c=Closest(p);float distance=Vector2.Distance(new(p.x,p.z),new(c.x,c.z));if(distance>6||p.x>=470)return local;float blend=1-Smooth(.4f,6,distance);float h=CachedSurface(c)-.035f;foreach(var b in buildings){float bd=Distance(p,b);if(bd<1.5f)blend*=Smooth(.2f,1.5f,bd);}float y=Mathf.Lerp(p.y,h,blend);if(Mathf.Abs(y-p.y)>.001f)changed++;p.y=y;return mf.transform.InverseTransformPoint(p);}
 void Add(Vector3 v,Vector2 t){indices.Add(result.Count);result.Add(Shape(v));tex.Add(t);}
 for(int i=0;i<tris.Length;i+=3){int a=tris[i],b=tris[i+1],c=tris[i+2];var mid=mf.transform.TransformPoint((vertices[a]+vertices[b]+vertices[c])/3);var close=Closest(mid);bool local=Vector2.Distance(new(mid.x,mid.z),new(close.x,close.z))<9&&mid.x<473;int n=local?4:1;for(int j=0;j<n;j++)for(int k=0;k<n-j;k++){
 Vector3 V(int jj,int kk)=>vertices[a]+(vertices[b]-vertices[a])*(jj/(float)n)+(vertices[c]-vertices[a])*(kk/(float)n);
 Vector2 U(int jj,int kk)=>uv.Length==vertices.Length?uv[a]+(uv[b]-uv[a])*(jj/(float)n)+(uv[c]-uv[a])*(kk/(float)n):Vector2.zero;
 Add(V(j,k),U(j,k));Add(V(j+1,k),U(j+1,k));Add(V(j,k+1),U(j,k+1));if(j+k<n-1){Add(V(j+1,k),U(j+1,k));Add(V(j+1,k+1),U(j+1,k+1));Add(V(j,k+1),U(j,k+1));}}}
 if(changed==0)continue;var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(result);mesh.SetUVs(0,tex);mesh.SetTriangles(indices,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mf.sharedMesh=Store(mesh,name+"-"+mf.name);mf.GetComponent<MeshCollider>().sharedMesh=mf.sharedMesh;report.Add("Terrain "+mf.name+": "+changed+" changed samples; building foundation interiors preserved");
 }
 for(int i=0;i<cv.Length;i++){var p=concrete.transform.TransformPoint(cv[i]);p.y=CachedSurface(p);cv[i]=concrete.transform.InverseTransformPoint(p);}var pavement=Object.Instantiate(original);pavement.vertices=cv;pavement.RecalculateNormals();pavement.RecalculateBounds();concrete.sharedMesh=Store(pavement,name+"-concrete");driveway.sharedMesh=pavement;
 // Keep the existing short dirt threshold seated; it is not extended into woods.
 var threshold=Object.FindObjectsByType<MeshFilter>().Single(m=>m.name=="Ground_Dan paved-to-dirt boundary");var dirt=Object.Instantiate(threshold.sharedMesh);var dv=dirt.vertices;Physics.SyncTransforms();for(int i=0;i<dv.Length;i++){var p=threshold.transform.TransformPoint(dv[i]);p.y=Ground(p)+.025f;dv[i]=threshold.transform.InverseTransformPoint(p);}dirt.vertices=dv;dirt.RecalculateNormals();dirt.RecalculateBounds();threshold.sharedMesh=Store(dirt,name+"-existing-threshold");
 int adjusted=0;foreach(var f in fences){float da=Ground(f.a.position)-f.oldA,db=Ground(f.b.position)-f.oldB;if(Mathf.Max(Mathf.Abs(da),Mathf.Abs(db))<.005f)continue;var mesh=Object.Instantiate(f.m.sharedMesh);var v=mesh.vertices;var start=f.a.position;var delta=f.b.position-start;delta.y=0;for(int i=0;i<v.Length;i++){var p=f.m.transform.TransformPoint(v[i]);var q=p-start;q.y=0;p.y+=Mathf.Lerp(da,db,Mathf.Clamp01(Vector3.Dot(q,delta)/delta.sqrMagnitude));v[i]=f.m.transform.InverseTransformPoint(p);}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();f.m.sharedMesh=Store(mesh,name+"-fence-"+adjusted++);f.a.position+=Vector3.up*da;f.b.position+=Vector3.up*db;var box=f.m.GetComponent<BoxCollider>();box.center=mesh.bounds.center;box.size=mesh.bounds.size+Vector3.one*.15f;}
 var mail=GameObject.Find("Mailbox - Dan - blue X").transform;var oldMail=mail.position;var at=new Vector3(466.3f,80,2.0f);at.y=Ground(at);mail.position=at;
 report.Add("Refitted fence sections="+adjusted+"; existing mailbox "+oldMail+" -> "+at+"; south/left looking west into main entrance");
 var root=new GameObject("TEMPORARY Backyard anchor validation - no route").transform;
 for(int i=0;i<Anchors.Length;i++){var atMarker=Anchors[i];atMarker.y=Ground(atMarker);var anchor=new GameObject((i+1)+" - "+Labels[i]).transform;anchor.SetParent(root);anchor.position=atMarker;
 var pole=GameObject.CreatePrimitive(PrimitiveType.Cylinder);pole.name="Non-colliding pink reference pole";pole.transform.SetParent(anchor,false);pole.transform.localPosition=Vector3.up*2;pole.transform.localScale=new(.10f,2,.10f);Object.DestroyImmediate(pole.GetComponent<Collider>());pole.GetComponent<Renderer>().sharedMaterial=markerMat;
 for(int side=0;side<4;side++){var text=new GameObject("Reference label").AddComponent<TextMesh>();text.transform.SetParent(anchor,false);text.transform.localPosition=Vector3.up*4.3f; text.transform.localRotation=Quaternion.Euler(0,side*90,0);text.text=(i+1)+" - "+Labels[i]+"\nX "+atMarker.x.ToString("F1")+" / Z "+atMarker.z.ToString("F1");text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.characterSize=.18f;text.fontSize=64;text.color=new Color(1,.35f,.8f);}
 report.Add("MARKER "+(i+1)+" "+atMarker.ToString("F4"));}
 Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
 }
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");
 // Only the rejected course's assets are deleted. Accepted Kyle mesh copies stay.
 foreach(string path in AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/Track/Backyard/")&&!p.Contains("-Kyle-edge")&&!p.EndsWith(".meta")).OrderByDescending(p=>p.Length).ToArray())AssetDatabase.DeleteAsset(path);
 foreach(string path in new[]{"Assets/Scenes/DansBackyard.unity","Assets/Scenes/DansBackyardReverse.unity"})AssetDatabase.DeleteAsset(path);
 EditorBuildSettings.scenes=EditorBuildSettings.scenes.Where(s=>!s.path.Contains("DansBackyard")).ToArray();PlayerSettings.bundleVersion="0.37.0-review1";AssetDatabase.SaveAssets();
 File.WriteAllLines(Evidence+"/authoring.txt",report);
 }
}}
