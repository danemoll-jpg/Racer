using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Racer.Editor {
// Authored west-to-east slots. Source is the original row index, NOT a prefab variant.
// Moving the whole site preserves its prefab, sign, scale and collision identity.
public static class CR019Commercial {
 public const string Dir = "Docs/CR019";
 public readonly struct Slot {
  public readonly int source; public readonly float x, setback;
  public Slot(int source,float x,float setback){this.source=source;this.x=x;this.setback=setback;}
 }
 public static readonly Slot[] South = {
  new(0,-538,31),new(2,-491,33),new(1,-391,29),new(4,-332,31),new(3,-283,34),
  new(6,-181,30),new(5,-127,33),new(8,-70,29),new(7,44,32),new(10,112,30),new(9,199,34)
 };
 public static readonly Slot[] North = {
  new(1,-512,33),new(0,-430,30),new(3,-375,34),new(2,-270,31),new(5,-218,29),
  new(4,-153,32),new(7,-36,34),new(6,16,30),new(9,82,33),new(8,161,31),new(10,231,34)
 };
 public static Transform[] Shops()=>GameObject.Find(Phase6Review.Root).transform.Cast<Transform>().Where(t=>t.name.Contains("business")).ToArray();
 static void Guard(){if(Application.isPlaying||SceneManager.GetActiveScene().path!=StreetLoopBuilder.ScenePath||SceneManager.GetActiveScene().isDirty)throw new Exception("Open saved StreetLoopGreybox in Edit mode.");Directory.CreateDirectory(Dir);}
 public static Vector3 Road(float x){var route=StreetLoopBuilder.Route();for(int i=0;i<route.Count;i++){var a=route[i];var b=route[(i+1)%route.Count];if(a.z<500||b.z<500||x<Mathf.Min(a.x,b.x)||x>Mathf.Max(a.x,b.x)||Mathf.Abs(a.x-b.x)<.001f)continue;return Vector3.Lerp(a,b,(x-a.x)/(b.x-a.x));}throw new Exception("No main road at "+x);}
 public static string Id(int side,int index)=>"CR019 "+(side<0?"S":"N")+(index+1).ToString("00")+" business";
 public static int PrefabVariant(string name,int legacyIndex)=>name.StartsWith("CR019 ")?(int.Parse(name.Substring(7,2))-1)%4:(legacyIndex/2)%4;
 static Color GroundColor(float d){var road=new Color(.24f,.25f,.26f);var shoulder=new Color(.46f,.45f,.39f);var grass=new Color(.35f,.40f,.30f);return d<6?Color.Lerp(road,shoulder,Mathf.SmoothStep(0,1,Mathf.InverseLerp(4,6,d))):Color.Lerp(shoulder,grass,Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,16,d)));}
 static float Smooth(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
 [Serializable] public class PaintSite {public Vector3 position,scale;public Quaternion rotation;}
 [Serializable] public class PaintSites {public PaintSite[] sites;}
 static PaintSite Describe(Transform t)=>new(){position=t.position,rotation=t.rotation,scale=t.Find("Foundation").localScale};
 static Vector3 Local(PaintSite s,Vector3 p)=>Quaternion.Inverse(s.rotation)*(p-s.position);
 static bool OldPaint(PaintSite s,Vector3 p){var l=Local(s,p);float dz=l.z-s.scale.z*.5f;return Mathf.Abs(l.x)<=s.scale.x*.5f+3&&dz>=-.4f&&dz<=9;}
 [MenuItem("Racer/Phase 8/CR019 Apply authored commercial sites")]
 public static void Apply(){
  Guard();var shops=Shops();if(shops.Length!=22)throw new Exception("Expected 22 commercial sites; inspect before applying.");
  bool first=!shops.Any(t=>t.name.StartsWith("CR019 "));
  if(first){
   var baseline=JsonUtility.ToJson(new PaintSites{sites=shops.Where(t=>t.position.x>-260&&t.position.x<80).Select(Describe).ToArray()},true);
   if(File.Exists(Dir+"/original-frontages.json")&&File.ReadAllText(Dir+"/original-frontages.json")!=baseline)throw new Exception("Baseline does not match scene; refusing to overwrite.");
   var south=shops.Where(t=>t.name.EndsWith("-1")).OrderBy(t=>t.position.x).ToArray();var north=shops.Where(t=>t.name.EndsWith(" 1")).OrderBy(t=>t.position.x).ToArray();
   if(south.Length!=11||north.Length!=11)throw new Exception("Unexpected row count");
   File.WriteAllText(Dir+"/original-frontages.json",baseline);
   for(int i=0;i<11;i++){south[i].name=Id(-1,i);north[i].name=Id(1,i);}
  }
  foreach(int side in new[]{-1,1})foreach(var slot in side<0?South:North){
   var t=shops.Single(s=>s.name==Id(side,slot.source));var road=Road(slot.x);var tangent=(Road(slot.x+.2f)-Road(slot.x-.2f)).normalized;var offset=Vector3.Cross(tangent,Vector3.up).normalized*side;
   var p=road+offset*slot.setback;p.y=Phase6Buildings.Ground(p);t.SetPositionAndRotation(p,Quaternion.LookRotation(-offset));
   var f=t.Find("Foundation");float high=float.NegativeInfinity,low=float.PositiveInfinity;
   foreach(float x in new[]{-.5f,0,.5f})foreach(float z in new[]{-.5f,0,.5f}){float y=Phase6Buildings.Ground(t.TransformPoint(new Vector3(x*f.localScale.x,0,z*f.localScale.z)));high=Mathf.Max(high,y);low=Mathf.Min(low,y);}
   float top=high+.28f;f.localPosition=new(0,(top+low-.10f)*.5f-p.y,0);f.localScale=new(f.localScale.x,top-low+.10f,f.localScale.z);
   var a=t.Find("Phase 6 architecture");a.localPosition=new(0,top-p.y,0);PrefabUtility.RecordPrefabInstancePropertyModifications(a);
   // Existing muted palette; override selected plaster surfaces, never shared prefab assets.
   string tint=(slot.source+ (side>0?2:0))%3==0?"Sage siding":(slot.source+(side>0?1:0))%3==1?"Warm plaster":"Slate blue siding";
   foreach(var r in a.GetComponentsInChildren<MeshRenderer>())if(r.name=="Warm plaster"){r.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Phase6Buildings.Folder+"/"+tint+".mat");PrefabUtility.RecordPrefabInstancePropertyModifications(r);}
   foreach(var old in t.Cast<Transform>().Where(s=>s.name.StartsWith("Entrance step ")).ToArray())Object.DestroyImmediate(old.gameObject);
   for(int i=0;i<3;i++){float z=f.localScale.z*.5f-.15f+i*.6f;float bottom=float.PositiveInfinity;foreach(float x in new[]{-1.55f,1.55f})foreach(float dz in new[]{-.325f,.325f})bottom=Mathf.Min(bottom,Phase6Buildings.Ground(t.TransformPoint(new(x,0,z+dz))));float stepTop=top-i*.14f;if(stepTop<=bottom+.04f)continue;var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Entrance step "+(i+1);g.transform.SetParent(t,false);g.transform.localPosition=new(0,(bottom+stepTop)*.5f-p.y,z);g.transform.localScale=new(3.1f,stepTop-bottom+.1f,.65f);g.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Phase6Buildings.Folder+"/Warm plaster.mat");}
  }
  Physics.SyncTransforms();RefreshFrontage();
  // Remove only trunks intersecting each moved site's footprint or its narrow road access.
  var removed=new List<string>();var woods=GameObject.Find("Woods replacing later subdivisions");
  foreach(var c in woods.GetComponentsInChildren<BoxCollider>())if(shops.Any(t=>{var q=t.InverseTransformPoint(c.bounds.center);var f=t.Find("Foundation").localScale;return Mathf.Abs(q.x)<f.x*.5f+2&&Mathf.Abs(q.z)<f.z*.5f+2 || Mathf.Abs(q.x)<4&&q.z>0&&q.z< Vector3.Distance(t.position,Road(t.position.x))-11;})){removed.Add(c.name+" "+c.transform.position.ToString("F3"));Object.DestroyImmediate(c.gameObject);}
  Phase6Buildings.RefreshVisualBatches();
  // Crown clipping follows moved sites; this refresh retains every surviving trunk transform.
  Phase6Vegetation.Refresh(Dir+"/vegetation-refresh.txt");
  if(first)File.WriteAllLines(Dir+"/local-tree-clearance.txt",removed.Count==0?new[]{"No trunks removed."}:removed);
  Inventory("after");
 }
 // Called by building batching too: deliberate visual refresh always paints CURRENT site transforms.
 public static void RefreshFrontage(){
  if(!File.Exists(Dir+"/original-frontages.json")||!Shops().Any(t=>t.name.StartsWith("CR019 ")))return;
  var old=JsonUtility.FromJson<PaintSites>(File.ReadAllText(Dir+"/original-frontages.json")).sites;
  var current=Shops().Select(Describe).ToArray();var route=StreetLoopBuilder.Route();int count=0;
  // Store the last paint footprint so a subsequent deliberate move also removes its old paving.
  string lastPath=Dir+"/last-frontages.json";var last=File.Exists(lastPath)?JsonUtility.FromJson<PaintSites>(File.ReadAllText(lastPath)).sites:Array.Empty<PaintSite>();
  bool Area(PaintSite s,Vector3 p){var q=Local(s,p);return Mathf.Abs(q.x)<s.scale.x*.5f+3&&q.z> s.scale.z*.5f-.5f&&q.z<40;}
  foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
   var mesh=mf.sharedMesh;var vs=mesh.vertices;var colors=mesh.colors;bool changed=false;
   for(int i=0;i<vs.Length;i++){var p=mf.transform.TransformPoint(vs[i]);if(!old.Any(s=>OldPaint(s,p))&&!last.Any(s=>Area(s,p))&&!current.Any(s=>Area(s,p)))continue;float d=StreetLoopBuilder.Nearest(p,route,out _);if(d<=10)continue;
    var col=GroundColor(d);
    foreach(var s in current){var q=Local(s,p);float dz=q.z-s.scale.z*.5f;float half=s.scale.x*.5f;if(dz<-.4f)continue;float across=Mathf.Abs(q.x);float apron=(1-Smooth(half,half+3,across))*(1-Smooth(5,9,dz));float paving=(1-Smooth(half-1,half+1,across))*Smooth(.3f,1.2f,dz)*(1-Smooth(3.3f,4.2f,dz));float access=(1-Smooth(3,4,across))*(1-Smooth(32,35,q.z))*Smooth(10,12,d);float blend=Mathf.Max(apron,access);col=Color.Lerp(col,Color.Lerp(new Color(.40f,.40f,.34f),new Color(.54f,.53f,.46f),paving),blend);}
    if(colors[i]!=col){colors[i]=col;changed=true;count++;}
   }
   if(changed){mesh.colors=colors;EditorUtility.SetDirty(mesh);}
  }
  File.WriteAllText(lastPath,JsonUtility.ToJson(new PaintSites{sites=current},true));File.WriteAllText(Dir+"/frontage-refresh.txt",$"Changed color entries {count}. Terrain vertices, indices, normals, road marking UVs and colliders unchanged. Flush gravel access stops outside 10m road corridor.\n");
 }
 public static void Inventory(string label){
  Directory.CreateDirectory(Dir);var rows=new List<string>{"id,prefab,x,y,z,foundationWidth,foundationDepth,sign"};
  foreach(var t in Shops().OrderBy(t=>t.position.z).ThenBy(t=>t.position.x)){var a=t.Find("Phase 6 architecture");var f=t.Find("Foundation");rows.Add($"{t.name},{Path.GetFileNameWithoutExtension(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(a.gameObject))},{t.position.x:F3},{t.position.y:F3},{t.position.z:F3},{f.localScale.x:F3},{f.localScale.z:F3},{string.Join(" / ",a.GetComponentsInChildren<TextMesh>().Select(s=>s.text))}");}
  File.WriteAllLines(Dir+"/"+label+"-sites.csv",rows);
 }
 public static void Capture(string label){
  Directory.CreateDirectory(Dir);var cam=Camera.main;var pos=cam.transform.position;var rot=cam.transform.rotation;var ortho=cam.orthographic;var size=cam.orthographicSize;var fov=cam.fieldOfView;var chase=cam.GetComponent<ChaseCamera>();bool ce=chase&&chase.enabled;if(chase)chase.enabled=false;
  void Shot(string name,Vector3 p,Vector3 target,bool top=false,float zoom=100){cam.transform.SetPositionAndRotation(p,Quaternion.LookRotation(target-p,top?Vector3.forward:Vector3.up));cam.orthographic=top;cam.orthographicSize=zoom;cam.fieldOfView=65;var rt=new RenderTexture(1920,1080,24);var old=cam.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(1920,1080,TextureFormat.RGB24,false);try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1920,1080),0,0);tex.Apply();File.WriteAllBytes(Dir+"/"+label+"-"+name+".png",tex.EncodeToPNG());}finally{cam.targetTexture=old;RenderTexture.active=active;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}}
  try{Shot("overhead",new(-155,480,540),new(-155,0,540),true,245);foreach(float x in new[]{-460f,-160f,140f}){Shot("overhead-"+x,new(x,260,540),new(x,0,540),true,110);foreach(int direction in new[]{-1,1}){var p=Road(x);var f=(Road(x+1)-Road(x-1)).normalized*direction;Shot("road-"+x+"-"+direction,p-f*7.5f+Vector3.up*4.25f,p+f*32+Vector3.up*2);}}}finally{cam.transform.SetPositionAndRotation(pos,rot);cam.orthographic=ortho;cam.orthographicSize=size;cam.fieldOfView=fov;if(chase)chase.enabled=ce;}
 }
}
}
