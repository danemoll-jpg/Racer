using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor {
// Local, repeatable building-only authoring. Never calls the legacy environment rebuild.
public static class Phase6Buildings {
 public const string Folder="Assets/Buildings/Phase6";
 public static readonly Vector3 Dan=new(352,0,105.6f),FormerOne=new(415.8f,0,-1.1f);
 static Material cream,blue,sage,brick,roof,trim,glass,wood,red,teal,ochre;
 static Mesh gable,hip;
 public static float YardDistance(Vector3 p){var a=Dan;var v=FormerOne-a;p.y=0;return (p-(a+v*Mathf.Clamp01(Vector3.Dot(p-a,v)/v.sqrMagnitude))).magnitude;}
 public static bool YardClear(Vector3 p)=>YardDistance(p)<27;
 static Material Mat(string name,Color color){string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;m.SetFloat("_Smoothness",.12f);AssetDatabase.CreateAsset(m,path);}return m;}
 static void Assets(){
  if(!AssetDatabase.IsValidFolder("Assets/Buildings"))AssetDatabase.CreateFolder("Assets","Buildings");if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Buildings","Phase6");
  cream=Mat("Warm plaster",new(.75f,.69f,.56f));blue=Mat("Slate blue siding",new(.40f,.53f,.60f));sage=Mat("Sage siding",new(.52f,.59f,.44f));brick=Mat("Muted brick",new(.53f,.30f,.23f));roof=Mat("Charcoal roof",new(.20f,.24f,.26f));trim=Mat("Ivory trim",new(.88f,.85f,.72f));glass=Mat("Blue glass",new(.16f,.31f,.38f));wood=Mat("Door timber",new(.31f,.22f,.16f));red=Mat("Diner red",new(.64f,.22f,.16f));teal=Mat("Market teal",new(.15f,.39f,.37f));ochre=Mat("Workshop ochre",new(.66f,.46f,.19f));
  gable=RoofMesh(false);hip=RoofMesh(true);
 }
 static Mesh RoofMesh(bool hipped){string path=Folder+(hipped?"/Hip roof.asset":"/Gable roof.asset");var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing)return existing;
  var v=new List<Vector3>();var tris=new List<int>();
  void Face(params Vector3[] p){int n=v.Count;v.AddRange(p);for(int i=1;i<p.Length-1;i++)tris.AddRange(new[]{n,n+i,n+i+1});}
  var a=new Vector3(-.5f,0,-.5f);var b=new Vector3(.5f,0,-.5f);var c=new Vector3(.5f,0,.5f);var d=new Vector3(-.5f,0,.5f);
  var r0=new Vector3(hipped?-.28f:-.5f,1,0);var r1=new Vector3(hipped?.28f:.5f,1,0);
  Face(a,r0,r1,b);Face(d,c,r1,r0);Face(a,d,r0);Face(b,r1,c);Face(a,b,c,d);
  var mesh=new Mesh{name=hipped?"Hip roof":"Gable roof"};mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
 }
 static Transform Cube(Transform t,string name,Vector3 p,Vector3 s,Material m,bool collision=false){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(t,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;if(!collision)Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
 static void Roof(Transform t,Vector3 p,Vector3 s,bool hipped){var g=new GameObject("Pitched roof",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.transform.SetParent(t,false);g.transform.localPosition=p;g.transform.localScale=s;g.GetComponent<MeshFilter>().sharedMesh=hipped?hip:gable;g.GetComponent<Renderer>().sharedMaterial=roof;var c=g.GetComponent<MeshCollider>();c.sharedMesh=hipped?hip:gable;c.convex=true;}
 static void Window(Transform t,float x,float y,float z,float width=1.7f,float height=1.7f){Cube(t,"Window frame",new(x,y,z),new(width+.22f,height+.22f,.15f),trim);Cube(t,"Window glass",new(x,y,z+.10f),new(width,height,.08f),glass);Cube(t,"Window mullion",new(x,y,z+.16f),new(.085f,height,.07f),trim);}
 static void SideWindows(Transform t,float w,float d,float y){foreach(int side in new[]{-1,1}){var g=new GameObject("Side windows").transform;g.SetParent(t,false);g.localPosition=new(side*w*.5f,0,0);g.localRotation=Quaternion.Euler(0,side*90,0);Window(g,-d*.22f,y,.06f);Window(g,d*.22f,y,.06f);}}
 static void Combine(Transform t,string assetName){
  // Reuse combined meshes per material across all instances, keeping collision simple.
  var filters=t.GetComponentsInChildren<MeshFilter>();
  foreach(var group in filters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial)){
   var combine=group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=t.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray();
   var mesh=new Mesh{name=assetName+" "+group.Key.name};mesh.CombineMeshes(combine,true,true);string path=Folder+"/"+mesh.name+".asset";
   var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
   var go=new GameObject(group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(t,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=group.Key;
  }
  foreach(var f in filters){Object.DestroyImmediate(f.GetComponent<Renderer>());Object.DestroyImmediate(f);}
  // Remove empty detail nodes; retain the few dedicated collision nodes.
  foreach(var tr in t.GetComponentsInChildren<Transform>().Reverse().ToArray())if(tr!=t&&tr.childCount==0&&tr.GetComponents<Component>().Length==1)Object.DestroyImmediate(tr.gameObject);
 }
 static GameObject MakePrefab(bool business,int variant){
  string name=(business?"Storefront ":"House ")+variant;string path=Folder+"/"+name+".prefab";var existing=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(existing)return existing;
  var go=new GameObject(name);var t=go.transform;float w=business?24:13,d=business?17:10,h=business?5:(variant==2?4.3f:4.8f);
  var wall=business?(variant==1?brick:cream):new[]{blue,cream,sage,brick}[variant];
  Cube(t,"Wall collision",new(0,h*.5f,0),new(w,h,d),wall,true);
  Cube(t,"Base trim",new(0,.20f,0),new(w+.08f,.4f,d+.08f),business?brick:trim);
  if(!business){
   Roof(t,new(0,h,0),new(w+1,variant==1?2.8f:2.2f,d+1),variant==2);
   Cube(t,"Eave fascia",new(0,h-.08f,0),new(w+.8f,.24f,d+.8f),trim);
   float doorX=variant==1?-2:0;Cube(t,"Entrance surround",new(doorX,1.45f,d*.5f+.06f),new(1.65f,2.9f,.16f),trim);Cube(t,"Entrance door",new(doorX,1.38f,d*.5f+.16f),new(1.3f,2.65f,.12f),wood);Cube(t,"Door glazing",new(doorX,2.05f,d*.5f+.24f),new(.72f,.64f,.06f),glass);
   Window(t,-4.1f,2.65f,d*.5f+.08f);Window(t,3.9f,2.65f,d*.5f+.08f);SideWindows(t,w,d,2.6f);
   var back=new GameObject("Rear windows").transform;back.SetParent(t,false);back.localRotation=Quaternion.Euler(0,180,0);Window(back,-3.2f,2.65f,d*.5f+.08f);Window(back,3.2f,2.65f,d*.5f+.08f);
   if(variant!=2){Roof(t,new(doorX,3.2f,d*.5f+.35f),new(3.2f,.8f,1.9f),false);Cube(t,"Porch lintel",new(doorX,3.15f,d*.5f+.35f),new(3,.2f,1.8f),trim);}
   if(variant==3){Cube(t,"Chimney",new(-3.4f,h+1.9f,-1.4f),new(.95f,3.8f,.95f),brick,true);Cube(t,"Chimney cap",new(-3.4f,h+3.8f,-1.4f),new(1.15f,.18f,1.15f),roof);}
  }else{
   var accent=new[]{teal,red,ochre,blue}[variant];
   if(variant==2)Roof(t,new(0,h,0),new(w+1,2.2f,d+1),false);
   else {Cube(t,"Flat roof collision",new(0,h+.18f,0),new(w+.4f,.36f,d+.4f),roof,true);Cube(t,"Stepped shopfront parapet",new(0,h+.65f,d*.5f),new(variant==1?w*.6f:w,1.3f,.5f),wall,true);}
   Cube(t,"Store fascia",new(0,4.45f,d*.5f+.2f),new(w+.25f,.85f,.5f),accent);
   Cube(t,"Entry frame",new(0,1.65f,d*.5f+.12f),new(2.8f,3.3f,.2f),trim);Cube(t,"Glazed entry",new(0,1.55f,d*.5f+.25f),new(2.4f,3.05f,.10f),glass);Cube(t,"Entry mullion",new(0,1.6f,d*.5f+.32f),new(.12f,3.1f,.06f),trim);
   if(variant==2){foreach(float x in new[]{-7f,7f}){Cube(t,"Workshop bay frame",new(x,1.8f,d*.5f+.10f),new(7,3.6f,.15f),trim);Cube(t,"Workshop shutter",new(x,1.7f,d*.5f+.22f),new(6.6f,3.3f,.12f),roof);for(int i=1;i<6;i++)Cube(t,"Shutter seam",new(x,i*.5f,d*.5f+.30f),new(6.6f,.045f,.035f),trim);}}
   else {Window(t,-7,2.05f,d*.5f+.12f,7.1f,2.5f);Window(t,7,2.05f,d*.5f+.12f,7.1f,2.5f);Cube(t,"Shop canopy",new(0,3.7f,d*.5f+.7f),new(w+.35f,.22f,1.6f),accent);Cube(t,"Canopy valance",new(0,3.48f,d*.5f+1.45f),new(w+.35f,.42f,.12f),accent);}
   SideWindows(t,w,d,2.7f);
   var text=new GameObject("Fictional storefront identity").AddComponent<TextMesh>();text.transform.SetParent(t,false);text.transform.localPosition=new(0,4.45f,d*.5f+.49f);text.transform.localRotation=Quaternion.Euler(0,180,0);text.text=new[]{"CORNER MARKET","HILLTOP DINER","AUTO SERVICE","GENERAL STORE"}[variant];text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.fontSize=64;text.characterSize=.20f;text.color=new Color(.98f,.94f,.81f);
  }
  Combine(t,name);var prefab=PrefabUtility.SaveAsPrefabAsset(go,path);Object.DestroyImmediate(go);return prefab;
 }
 [MenuItem("Racer/Phase 6/Apply focused buildings and CR-012")]
 public static void Build(){
  var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.path!=StreetLoopBuilder.ScenePath||scene.isDirty)throw new Exception("Open saved StreetLoopGreybox outside Play mode");
  var root=GameObject.Find(Phase6Review.Root).transform;var one=root.Find("Original house 1");
  if(one&&Vector2.Distance(new(one.position.x,one.position.z),new(FormerOne.x,FormerOne.z))>.1f)throw new Exception("House 1 identity/location does not match map marker");
  if(!one && root.Cast<Transform>().All(t=>t.Find("Phase 6 architecture")))throw new InvalidOperationException("Phase 6 batch already applied; no repeat yard mutation needed.");
  Assets();var houses=Enumerable.Range(0,4).Select(i=>MakePrefab(false,i)).ToArray();var shops=Enumerable.Range(0,4).Select(i=>MakePrefab(true,i)).ToArray();
  int removed=0;if(one){removed=one.GetComponentsInChildren<Collider>().Length;Object.DestroyImmediate(one.gameObject);}
  int houseIndex=0,shopIndex=0,changed=0;
  foreach(Transform t in root){
   if(t.Find("Phase 6 architecture"))continue;
   var mass=t.Find("House mass");var oldRoof=t.Find("Roof mass");if(!mass||!oldRoof)throw new Exception("Unexpected building layout: "+t.name);
   bool shop=t.name.Contains("business");float w=mass.localScale.x,d=mass.localScale.z,baseY=mass.localPosition.y-mass.localScale.y*.5f;
   int variant=shop?(shopIndex++/2)%4:(houseIndex++)%4;
   if(t.name.StartsWith("Dan"))variant=0;if(t.name=="Original house 2")variant=1;if(t.name=="Original house 3")variant=2;if(t.name.StartsWith("Friend"))variant=3;
   var detail=(GameObject)PrefabUtility.InstantiatePrefab(shop?shops[variant]:houses[variant],t);detail.name="Phase 6 architecture";detail.transform.localPosition=new(0,baseY,0);detail.transform.localScale=new(w/(shop?24:13),1,d/(shop?17:10));PrefabUtility.RecordPrefabInstancePropertyModifications(detail.transform);
   Object.DestroyImmediate(mass.gameObject);Object.DestroyImmediate(oldRoof.gameObject);
   var foundation=t.Find("Foundation");foundation.GetComponent<Renderer>().sharedMaterial=shop?brick:cream;
   // Solid entrance steps meet the existing terrain, never create a floating porch.
   float x=(!shop&&variant==1?-2:0)*w/(shop?24:13);float front=d*.5f;
   for(int i=0;i<3;i++){
    float z=front+.35f+i*.6f;Vector3 sample=t.TransformPoint(new(x,0,z));float ground=Ground(sample);foreach(float sx in new[]{-1f,1f})foreach(float sz in new[]{-.325f,.325f})ground=Mathf.Min(ground,Ground(t.TransformPoint(new Vector3(x+sx*(shop?1.55f:1.05f),0,z+sz))));float top=t.position.y+baseY-i*.24f;if(top<=ground+.06f)continue;
    Cube(t,"Entrance step "+(i+1),new(x,(ground+top)*.5f-t.position.y,z),new(shop?3.1f:2.1f,top-ground+.10f,.65f),cream,true);
   }
   changed++;
  }
  GroundFoundations();var report=ExpandYard();RefreshVisualBatches();Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  File.WriteAllText("Docs/PHASE6_BUILD.txt",$"CR-012 verified map marker (1028,951), world XZ (415.8,-1.1). Removed Original house 1 and {removed} dedicated colliders (foundation/wall/roof); no separate access or prop children existed.\nReplaced {changed} buildings with 4 residential and 4 commercial reusable prefab variants; all surviving site transforms preserved.\n{report}\nArchitecture and fictional shop names are approximations, not historical references. Retained foundation geometry/colliders, replaced wall/roof colliders, added ground-reaching entrance steps.\n");
 }
 public static void RefreshVisualBatches(){
  if(Application.isPlaying)throw new InvalidOperationException("Edit mode only");
  const string batchRoot="Phase 6 - architectural render batches";var oldRoot=GameObject.Find(batchRoot);if(oldRoot)Object.DestroyImmediate(oldRoot);
  var root=new GameObject(batchRoot).transform;var filters=GameObject.Find(Phase6Review.Root).GetComponentsInChildren<MeshFilter>();int tris=0;
  foreach(var group in filters.GroupBy(f=>f.GetComponent<Renderer>().sharedMaterial)){
   var mesh=new Mesh{name="Scene buildings - "+group.Key.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(group.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray(),true,true);tris+=mesh.triangles.Length/3;
   string path=Folder+"/"+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(existing);}else AssetDatabase.CreateAsset(mesh,path);
   var go=new GameObject(group.Key.name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=group.Key;
  }
  foreach(var f in filters){var r=f.GetComponent<Renderer>();r.enabled=false;PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
  File.WriteAllText("Docs/PHASE6_RENDER_BATCHES.txt",$"{filters.Length} building geometry renderers replaced by {root.childCount} shared-material scene batches ({tris} triangles). Original shaders/materials/geometry retained; text renderers remain separate. Prefab renderer instances disabled, collision retained unchanged. To edit a building or prefab, call RefreshVisualBatches afterward. This touches building visuals only.\n");
  EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());EditorSceneManager.SaveScene(SceneManager.GetActiveScene());AssetDatabase.SaveAssets();
 }

 public static void GroundFoundations(){
  var notes=new List<string>();foreach(Transform t in GameObject.Find(Phase6Review.Root).transform){var f=t.Find("Foundation");float gap=0;foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.5f,.5f}){var p=f.TransformPoint(new Vector3(x,-.5f,z));gap=Mathf.Max(gap,p.y-Ground(p));}if(gap<=.02f)continue;float extension=gap+.05f;f.localPosition-=Vector3.up*extension*.5f;f.localScale+=Vector3.up*extension;notes.Add($"{t.name} at {t.position}: foundation bottom extended downward {extension:F3}m; top and building transform unchanged; matching primitive collider extends with it.");}File.WriteAllLines("Docs/PHASE6_FOUNDATIONS.txt",notes);
 }
 public static float Ground(Vector3 p){float y=float.NegativeInfinity;var ray=new Ray(new(p.x,300,p.z),Vector3.down);foreach(var c in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshCollider>())if(c.Raycast(ray,out var hit,600))y=Mathf.Max(y,hit.point.y);if(float.IsNegativeInfinity(y))throw new Exception("No ground at "+p);return y;}
 static string ExpandYard(){int vertices=0,tiles=0,trees=0,blocks=0;var road=StreetLoopBuilder.Route();
  foreach(var f in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
   var mesh=f.sharedMesh;var v=mesh.vertices;var colors=mesh.colors;bool changed=false;
   for(int i=0;i<v.Length;i++){
    float distance=YardDistance(v[i]);if(distance>=34)continue;float rd=StreetLoopBuilder.Nearest(v[i],road,out _);float weight=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(22,34,distance)))*Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,24,rd));if(weight<=0)continue;
    // Original height and normal arrays remain bit-for-bit unchanged: no new seams or road alterations.
    colors[i]=Color.Lerp(colors[i],new Color(.40f,.47f,.30f),weight);vertices++;changed=true;
   }
   if(changed){mesh.colors=colors;EditorUtility.SetDirty(mesh);tiles++;}
  }
  var woods=GameObject.Find("Woods replacing later subdivisions");
  foreach(var c in woods.GetComponentsInChildren<BoxCollider>())if(YardClear(c.transform.position)){Object.DestroyImmediate(c.gameObject);trees++;}
  foreach(var f in woods.GetComponentsInChildren<MeshFilter>()){
   var mesh=f.sharedMesh;var v=mesh.vertices;var normals=mesh.normals;var uv=mesh.uv;var tri=mesh.triangles;if(v.Length%24!=0)throw new Exception("Unexpected forest batch topology");
   bool[] keep=new bool[v.Length/24];bool changed=false;
   for(int b=0;b<keep.Length;b++){var bounds=new Bounds(v[b*24],Vector3.zero);for(int i=1;i<24;i++)bounds.Encapsulate(v[b*24+i]);keep[b]=!YardClear(f.transform.TransformPoint(bounds.center));if(!keep[b]){blocks++;changed=true;}}
   if(!changed)continue;var nv=new List<Vector3>();var nn=new List<Vector3>();var nu=new List<Vector2>();var nt=new List<int>();var map=new int[v.Length];
   for(int i=0;i<v.Length;i++)if(keep[i/24]){map[i]=nv.Count;nv.Add(v[i]);if(normals.Length==v.Length)nn.Add(normals[i]);if(uv.Length==v.Length)nu.Add(uv[i]);}
   for(int i=0;i<tri.Length;i+=3)if(keep[tri[i]/24]){nt.Add(map[tri[i]]);nt.Add(map[tri[i+1]]);nt.Add(map[tri[i+2]]);}mesh.Clear();mesh.SetVertices(nv);mesh.SetTriangles(nt,0);if(nn.Count>0)mesh.SetNormals(nn);if(nu.Count>0)mesh.SetUVs(0,nu);mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);
  }
  return $"Yard: {tiles} terrain tiles, {vertices} color vertices; ZERO terrain height/topology/normal changes. {trees} local tree colliders and {blocks} crown/trunk cube blocks removed within 27m of Dan-to-former-House1 segment. Grass blends to 34m and excludes road/shoulders inside 16m. No shared asset deletion.";
 }
}
}
