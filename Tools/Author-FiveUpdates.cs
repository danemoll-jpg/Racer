using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class AuthorFiveUpdates {
 const string Folder="Assets/Track/FiveUpdates";
 static readonly string[] Scenes=Racer.Editor.ReverseReviewRelease.Scenes.Concat(new[]{"Assets/Scenes/MountainLoop.unity","Assets/Scenes/MountainLoopReverse.unity"}).ToArray();
 static Material Mat(string name,Color color){string p=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};AssetDatabase.CreateAsset(m,p);}return m;}
 static float Ground(Vector3 p){var h=Physics.RaycastAll(new Vector3(p.x,400,p.z),Vector3.down,800,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(h.Length==0)throw new Exception("Missing ground "+p);return h[0].point.y;}
 static GameObject MeshObject(string name,Vector3[] v,int[] triangles,Material mat,Transform parent,string asset,bool collider){var m=new Mesh{vertices=v,triangles=triangles};m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,Folder+"/"+asset+".asset");var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent);go.GetComponent<MeshFilter>().sharedMesh=m;go.GetComponent<Renderer>().sharedMaterial=mat;if(collider)go.AddComponent<MeshCollider>().sharedMesh=m;return go;}
 static void Sign(Transform parent,Vector3 at,Vector3 f,string text,Color color){typeof(Racer.Editor.CorrectionPassAuthoring).GetMethod("Sign",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{parent,at,f,text,color});}
 public static string Forest(){
 Guard();Directory.CreateDirectory(Folder);AssetDatabase.Refresh();EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");
 var route=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.name=="House 3 Detour");var label=GameObject.Find("House 3 Detour OPTIONAL sign");if(!label)throw new Exception("Expected floating label missing");
 var root=label.transform.parent;var at=label.transform.position;var f=label.transform.forward;Object.DestroyImmediate(label);route.title="McFadden Cut";EditorUtility.SetDirty(route);
 Sign(root,at,f,"McFADDEN CUT\nOPTIONAL",new Color(.32f,.23f,.15f));Save();return "Mounted existing timber sign; display title McFadden Cut; stable object ID and geometry retained";
 }
 public static string Exit(){
 Guard();Directory.CreateDirectory(Folder);AssetDatabase.Refresh();
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");var b=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Pine Ridge");
 float lip=b.Length-49;var p=b.At(lip,out var dir);dir=Vector3.ProjectOnPlane(dir,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,dir);var verts=new List<Vector3>();var tri=new List<int>();
 // Low, gently rounded approach ending flat; one metre road-facing step, no launch slope.
 const float width=13,run=10,height=1;
 for(int i=0;i<=40;i++){float s=-run+i*run/40;float t=(s+run)/run;float rise=height*Mathf.SmoothStep(0,1,t);foreach(float x in new[]{-width/2,width/2}){var q=p+dir*s+side*x;q.y=Ground(q)+rise+.025f;verts.Add(q);}if(i<40){int n=i*2;tri.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}
 // Solid front face and tapered side faces prevent wheel access from the wrong side.
 int top=verts.Count-2;var left=verts[top];var right=verts[top+1];var bl=left;bl.y=Ground(bl)-.15f;var br=right;br.y=Ground(br)-.15f;int k=verts.Count;verts.Add(bl);verts.Add(br);tri.AddRange(new[]{top,k,top+1,top+1,k,k+1});
 for(int i=0;i<40;i++)foreach(int edge in new[]{0,1}){var a=verts[i*2+edge];var z=verts[(i+1)*2+edge];var ba=a;ba.y=Ground(ba)-.15f;var bz=z;bz.y=Ground(bz)-.15f;int n=verts.Count;verts.AddRange(new[]{a,z,ba,bz});tri.AddRange(edge==0?new[]{n,n+2,n+1,n+1,n+2,n+3}:new[]{n,n+1,n+2,n+1,n+3,n+2});}
 foreach(var scene in new[]{Scenes[0],Scenes[2]}){
 EditorSceneManager.OpenScene(scene);if(GameObject.Find("Pine Ridge one-way exit"))throw new Exception("Already authored");var root=new GameObject("Pine Ridge one-way exit").transform;
 MeshObject("Ground_Pine Ridge low exit lip",verts.ToArray(),tri.ToArray(),Mat("Exit concrete",new(.48f,.44f,.34f)),root,Path.GetFileNameWithoutExtension(scene)+"-exit",true);
 Sign(root,p+side*8,-dir,"NO ENTRY",new Color(.48f,.07f,.04f));Save();
 }
 File.WriteAllText("Docs/FiveUpdates/exit-geometry.txt",$"Pine Ridge screenshot matched by western straight / right-side house. Lip station {lip:F3}; position {p:F3}; forward {dir:F5}; width {width}m; run {run}m; raised edge {height}m with zero terminal slope. Main road, route points, checkpoints and original ramp unchanged.\n");return "Small one-way Pine Ridge exit authored in Street Forward/Reverse";
 }
 public static string Property(){
 Guard();Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var rows=new List<string>();
 foreach(var scene in Scenes){EditorSceneManager.OpenScene(scene);if(GameObject.Find("Dan beige concrete driveway"))throw new Exception("Already authored");var home=GameObject.Find("Dan - blue X").transform;var root=new GameObject("Dan beige concrete driveway").transform;
 var race=Object.FindAnyObjectByType<RaceDirector>();var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();
 Vector3 World(float x,float z){var q=home.TransformPoint(new Vector3(x,-3,z));q.y=Ground(q)+.035f;return q;}
 var gate=World(20,10);var station=street.Project(gate,out _);var rp=street.At(station,out _);var toward=(gate-rp);toward.y=0;toward.Normalize();var start=rp+toward*(street.HalfWidth(station)+.2f);
 // Follow the existing southern gate/descent, then the immediate rear property apron.
 // Stop just beyond the kennel/pool-house line; a short dirt threshold is planning context only.
 var line=new[]{start,World(20,10),World(23,-7),World(23,-16),World(17,-25),World(-20,-25),World(-23,-21),World(-23,-11),World(-23,-27)};
 var v=new List<Vector3>();var ts=new List<int>();
 void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=v.Count;foreach(var q0 in new[]{a,b,c,d}){var q=q0;q.y=Ground(q)+.04f;v.Add(q);}ts.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}
 // Small ground-following quads avoid changing terrain elevations or building positions.
 void Strip(Vector3 a,Vector3 b,float width){var d=Vector3.ProjectOnPlane(b-a,Vector3.up);var side=Vector3.Cross(Vector3.up,d).normalized*width*.5f;int count=Mathf.CeilToInt(d.magnitude*2);int across=Mathf.CeilToInt(width*2);for(int i=0;i<count;i++){var p=Vector3.Lerp(a,b,i/(float)count);var q=Vector3.Lerp(a,b,(i+1)/(float)count);for(int j=0;j<across;j++){var l=side*Mathf.Lerp(-1,1,j/(float)across);var r=side*Mathf.Lerp(-1,1,(j+1)/(float)across);Quad(p+l,p+r,q+l,q+r);}}}
 for(int i=1;i<line.Length;i++)Strip(line[i-1],line[i],5);
 // Parking below the descent and paved access in front of the kennel doors.
 foreach(var area in new[]{new Rect(13,-24,13,10),new Rect(-43,-12,23,6)})for(float x=area.xMin;x<area.xMax;x++)for(float z=area.yMin;z<area.yMax;z++)Quad(World(x,z),World(x+1,z),World(x,z+1),World(x+1,z+1));
 MeshObject("Ground_Beige concrete property surface",v.ToArray(),ts.ToArray(),Mat("Beige concrete",new(.67f,.59f,.44f)),root,Path.GetFileNameWithoutExtension(scene)+"-concrete",true);
 // End at the property boundary: no trail extension, clearing, gates or race metadata.
 v.Clear();ts.Clear();Strip(World(-23,-27.05f),World(-23,-29),4.6f);
 MeshObject("Ground_Dirt path threshold",v.ToArray(),ts.ToArray(),Mat("Dirt threshold",new(.43f,.35f,.23f)),root,Path.GetFileNameWithoutExtension(scene)+"-dirt",false);
 Save();rows.Add(Path.GetFileNameWithoutExtension(scene)+": first southern gate, existing descent, bottom parking, kennel/pool-house access paved; dirt threshold just behind z=-27; no trail constructed.");}
 File.WriteAllLines("Docs/FiveUpdates/property-authoring.txt",rows);return "Local paving and dirt threshold applied across shared-world scenes";
 }
 static void Guard(){if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");}
 static void Save(){EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
}
