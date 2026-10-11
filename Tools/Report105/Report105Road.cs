using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105 Part A: the steps on Hwy 92's drivable surface, fixed in the scene meshes (PROBE_PLAY "dry" = report only).
//  1. FreeRoamWorld: the 0.96 crack repair put vertices on the highway seam's edges at the terrain's height (2-4 cm dents every 2 m,
//     a washboard across the whole width, x 230-280); each goes back onto its straight edge, and the terrain vertex it was copied
//     from goes with it (the edge stays closed).
//  2. every scene: the two CR113 seam ribbons end 6-9 cm above the road they meet (x -505, x 161): the last 8 m of each eases
//     down onto that road (5 mm above it).
//  3. StreetLoopReverse: the flush west pavement apron starts 14.5 cm above the road (x -507): its first 9 m ease down onto it.
public static class Report105Road {
 static readonly string[] Scenes={"StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse","DansBackyardForward","DansBackyardReverse","FreeRoamWorld"};
 static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
 sealed class Edit{public Mesh src;public MeshCollider mc;public Vector3[] world;public Vector3[] orig;public int moved;public float maxMove;}
 static Edit Get(Dictionary<MeshCollider,Edit> edits,MeshCollider mc){if(!edits.TryGetValue(mc,out var e)){var v=mc.sharedMesh.vertices;var m=mc.transform.localToWorldMatrix;var w=v.Select(p=>m.MultiplyPoint3x4(p)).ToArray();edits[mc]=e=new Edit{src=mc.sharedMesh,mc=mc,world=w,orig=(Vector3[])w.Clone()};}return e;}
 static float Below(Vector3 p,Func<Collider,bool> skip){float best=float.NaN;foreach(var h in Physics.RaycastAll(p+Vector3.up*.5f,Vector3.down,2.5f,~0,QueryTriggerInteraction.Ignore)){if(skip(h.collider)||!h.collider.name.StartsWith("Ground_"))continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
 static long Key(float v)=>(long)Mathf.Round(v*100);
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");bool dry=Environment.GetEnvironmentVariable("PROBE_PLAY")=="dry";var log=new List<string>{dry?"DRY RUN":"APPLIED"};
  var only=Environment.GetEnvironmentVariable("PROBE_SCENES");var scenes=string.IsNullOrEmpty(only)||only=="all"?Scenes:only.Split(',');
  Directory.CreateDirectory("Assets/Scenery/Report105");
  foreach(var sn in scenes){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();var edits=new Dictionary<MeshCollider,Edit>();
   var all=UnityEngine.Object.FindObjectsByType<MeshCollider>(FindObjectsSortMode.None).Where(c=>c.sharedMesh&&c.enabled&&c.gameObject.activeInHierarchy).ToList();
   var seams=all.Where(c=>c.name=="Ground_CR113 east four lane seam"||c.name=="Ground_CR113 west four lane seam").ToList();
   // 1. the conform dents
   foreach(var mc in seams.Where(c=>c.sharedMesh.name.EndsWith("-conformed"))){
    var e=Get(edits,mc);var w=e.world;int n0=0;for(int j=0;2*j+1<w.Length;j++){var a=w[2*j];var b=w[2*j+1];a.y=b.y=0;if(Mathf.Abs(Vector3.Distance(a,b)-16.4f)>1.5f)break;n0=2*j+2;}
    var fixedKeys=new Dictionary<(long,long),(float from,float to)>();
    for(int i=n0;i<w.Length;i++){var p=w[i];bool done=false;
     for(int j=0;2*j+3<n0&&!done;j++)for(int s=0;s<2&&!done;s++){var a=w[2*j+s];var b=w[2*j+2+s];var ab=new Vector2(b.x-a.x,b.z-a.z);var ap=new Vector2(p.x-a.x,p.z-a.z);float t=Vector2.Dot(ap,ab)/Mathf.Max(1e-6f,ab.sqrMagnitude);if(t<-.001f||t>1.001f)continue;if((ap-ab*t).magnitude>.02f)continue;
      float y=Mathf.Lerp(a.y,b.y,t);if(Mathf.Abs(y-p.y)>.002f){fixedKeys[(Key(p.x),Key(p.z))]=(p.y,y);w[i].y=y;}done=true;}}
    log.Add($"{sn}: {mc.name} ({mc.sharedMesh.name}): ribbon vertices {n0}, inserted {w.Length-n0}, put back on their edge {fixedKeys.Count} (largest {(fixedKeys.Count>0?fixedKeys.Values.Max(v=>Mathf.Abs(v.to-v.from))*100:0):F1} cm)");
    int terr=0;foreach(var tc in all.Where(c=>c!=mc&&c.name.StartsWith("Ground_")&&c.bounds.Intersects(mc.bounds))){Edit te=null;var tw=tc.sharedMesh.vertices;var tm=tc.transform.localToWorldMatrix;
     for(int i=0;i<tw.Length;i++){var p=tm.MultiplyPoint3x4(tw[i]);if(fixedKeys.TryGetValue((Key(p.x),Key(p.z)),out var f)&&Mathf.Abs(p.y-f.from)<.005f){te??=Get(edits,tc);te.world[i].y=f.to;terr++;}}}
    log.Add($"   terrain vertices that met those edge vertices, moved with them: {terr}");
   }
   // 2. the seam ends
   foreach(var mc in seams){var e=Get(edits,mc);var w=e.world;bool east=mc.name.Contains("east");float end=east?w.Min(p=>p.x):w.Max(p=>p.x);if(end<-800||end>800){log.Add($"{sn}: {mc.name} end at x {end:F1} outside the stretch");continue;}
    // the end's two corners (the ribbon's first or last pair; the end is skewed): the ease runs from them
    int n=w.Length;if(mc.sharedMesh.name.EndsWith("-conformed")){n=0;for(int j=0;2*j+1<w.Length;j++){var a=w[2*j];var b=w[2*j+1];a.y=b.y=0;if(Mathf.Abs(Vector3.Distance(a,b)-16.4f)>1.5f)break;n=2*j+2;}}
    bool first=east?(w[0].x+w[1].x)<(w[n-2].x+w[n-1].x):(w[0].x+w[1].x)>(w[n-2].x+w[n-1].x);var cA=first?w[0]:w[n-2];var cB=first?w[1]:w[n-1];
    float Flat(Vector3 a,Vector3 b){a.y=b.y=0;return Vector3.Distance(a,b);}
    int moved=0;float worst=0,step=0;
    float ToEnd(Vector3 p){var a=new Vector2(cA.x,cA.z);var ab=new Vector2(cB.x,cB.z)-a;var q=new Vector2(p.x,p.z)-a;float t=Mathf.Clamp01(Vector2.Dot(q,ab)/ab.sqrMagnitude);return (q-ab*t).magnitude;}
    for(int i=0;i<w.Length;i++){float d=ToEnd(w[i]);if(d>8)continue;float gy=Below(w[i],c=>seams.Contains(c));if(float.IsNaN(gy))continue;float gap=w[i].y-gy;if(d<.5f)step=Mathf.Max(step,gap);if(gap<=.005f||gap>.25f)continue;
     float y=Mathf.Lerp(gy+.005f,w[i].y,Smooth(0,8,d));worst=Mathf.Max(worst,w[i].y-y);w[i].y=y;moved++;}
    log.Add($"{sn}: {mc.name} end at x {end:F1}: was {step*100:F1} cm above the road it meets; {moved} vertices in the last 8 m eased down (most {worst*100:F1} cm)");
   }
   // 3. the Reverse apron
   foreach(var mc in all.Where(c=>c.name=="Reverse flush west pavement apron")){var e=Get(edits,mc);var w=e.world;float west=w.Min(p=>p.x);int moved=0;float step=0;
    for(int i=0;i<w.Length;i++){float d=w[i].x-west;if(d>9)continue;float gy=Below(w[i],c=>c==mc);if(float.IsNaN(gy))continue;float gap=w[i].y-gy;if(d<.5f)step=Mathf.Max(step,gap);if(gap<=.005f)continue;w[i].y=Mathf.Lerp(gy+.005f,w[i].y,Smooth(0,9,d));moved++;}
    float eastX=w.Max(p=>p.x);int movedE=0;float stepE=0;
    for(int i=0;i<w.Length;i++){float d=eastX-w[i].x;if(d>6||w[i].x-west<=9)continue;float gy=Below(w[i],c=>c==mc);if(float.IsNaN(gy))continue;float gap=w[i].y-gy;if(d<.5f)stepE=Mathf.Max(stepE,gap);if(gap<=.005f)continue;w[i].y=Mathf.Lerp(gy+.005f,w[i].y,Smooth(0,6,d));movedE++;}
    log.Add($"{sn}: {mc.name} west edge at x {west:F1}: was {step*100:F1} cm above the road; {moved} vertices in its first 9 m eased down; east edge at x {eastX:F1}: was {stepE*100:F1} cm above; {movedE} vertices in its last 6 m eased down");
   }
   // what rests on the changed surface
   foreach(var e in edits.Values){var changed=Enumerable.Range(0,e.world.Length).Where(i=>Mathf.Abs(e.world[i].y-e.orig[i].y)>.001f).ToList();e.moved=changed.Count;if(changed.Count==0)continue;e.maxMove=changed.Max(i=>Mathf.Abs(e.world[i].y-e.orig[i].y));
    var b=new Bounds(e.world[changed[0]],Vector3.zero);foreach(var i in changed)b.Encapsulate(e.world[i]);b.Expand(new Vector3(1,2,1));
    var near=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&r.bounds.Intersects(b)&&!r.name.StartsWith("Ground_")&&r.gameObject!=e.mc.gameObject&&r.bounds.size.x<200).Select(r=>$"{r.name} {r.bounds.min:F2}-{r.bounds.max:F2}").Take(15).ToList();
    log.Add($"   {e.mc.name}: {e.moved} vertices moved (most {e.maxMove*100:F1} cm) in {b.min:F1}-{b.max:F1}; resting there: {(near.Count==0?"nothing":string.Join("; ",near))}");}
   if(dry)continue;
   // paint and markings lying on the changed surface (no colliders): each vertex follows the surface under it
   var changedCols=edits.Values.Where(x=>x.moved>0).Select(x=>(Collider)x.mc).ToList();var area=new List<Bounds>();
   foreach(var e in edits.Values.Where(x=>x.moved>0)){var b=new Bounds(e.world[0],Vector3.zero);bool any=false;for(int i=0;i<e.world.Length;i++)if(Mathf.Abs(e.world[i].y-e.orig[i].y)>.001f){if(!any){b=new Bounds(e.world[i],Vector3.zero);any=true;}else b.Encapsulate(e.world[i]);}b.Expand(new Vector3(8,1,8));area.Add(b);}
   float Surface(Vector3 p){float best=float.NaN;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+.6f,p.z),Vector3.down,1.5f,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_")&&h.collider.name!="Reverse flush west pavement apron")continue;if(float.IsNaN(best)||h.point.y>best)best=h.point.y;}return best;}
   var paint=new List<(MeshFilter mf,Vector3[] w,float[] y0)>();
   foreach(var mf in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None)){var r=mf.GetComponent<MeshRenderer>();if(!r||!r.enabled||!mf.gameObject.activeInHierarchy||!mf.sharedMesh||mf.GetComponent<Collider>()||mf.name.StartsWith("Ground_"))continue;var rb=r.bounds;if(rb.size.y>.3f||rb.size.x>40||rb.size.z>40||!area.Any(a=>a.Intersects(rb)))continue;
    var m=mf.transform.localToWorldMatrix;var w=mf.sharedMesh.vertices.Select(q=>m.MultiplyPoint3x4(q)).ToArray();var y0=w.Select(q=>Surface(q)).ToArray();if(y0.All(float.IsNaN))continue;if(w.Select((q,i)=>float.IsNaN(y0[i])?99:q.y-y0[i]).Min()>.2f)continue;paint.Add((mf,w,y0));}
   foreach(var e in edits.Values.Where(x=>x.moved>0)){var tmp=UnityEngine.Object.Instantiate(e.mc.sharedMesh);var inv=e.mc.transform.worldToLocalMatrix;tmp.vertices=e.world.Select(p=>inv.MultiplyPoint3x4(p)).ToArray();tmp.RecalculateBounds();e.mc.sharedMesh=tmp;}
   Physics.SyncTransforms();int pieces=0,paintN=0;float pmax=0;
   foreach(var (mf,w,y0) in paint){var nw=(Vector3[])w.Clone();bool ch=false;for(int i=0;i<w.Length;i++){if(float.IsNaN(y0[i]))continue;float y1=Surface(w[i]);if(float.IsNaN(y1))continue;float d=y1-y0[i];if(Mathf.Abs(d)>.002f){nw[i].y+=d;ch=true;pmax=Mathf.Max(pmax,Mathf.Abs(d));}}
    if(!ch)continue;var copy=UnityEngine.Object.Instantiate(mf.sharedMesh);copy.name=sn+"-"+mf.name.Replace("/","-")+"-"+(paintN++)+"-0.105";var inv=mf.transform.worldToLocalMatrix;copy.vertices=nw.Select(p=>inv.MultiplyPoint3x4(p)).ToArray();copy.RecalculateBounds();
    var pp="Assets/Scenery/Report105/"+copy.name+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(pp))AssetDatabase.DeleteAsset(pp);AssetDatabase.CreateAsset(copy,pp);mf.sharedMesh=copy;EditorUtility.SetDirty(mf);pieces++;}
   log.Add($"   paint and markings reseated on the new surface: {pieces} pieces (most {pmax*100:F1} cm)");
   foreach(var e in edits.Values.Where(x=>x.moved>0)){var src=e.src;var copy=UnityEngine.Object.Instantiate(src);copy.name=sn+"-"+e.mc.name+"-0.105";var inv=e.mc.transform.worldToLocalMatrix;copy.vertices=e.world.Select(p=>inv.MultiplyPoint3x4(p)).ToArray();copy.RecalculateBounds();
    var path="Assets/Scenery/Report105/"+copy.name+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(copy,path);
    var mf=e.mc.GetComponent<MeshFilter>();if(mf&&mf.sharedMesh==src)mf.sharedMesh=copy;else if(mf)log.Add($"   NOTE {e.mc.name}: drawn mesh {(mf.sharedMesh?mf.sharedMesh.name:"none")} differs from its collider; collider only");
    e.mc.sharedMesh=null;e.mc.sharedMesh=copy;EditorUtility.SetDirty(e.mc);if(mf)EditorUtility.SetDirty(mf);}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  }
  File.WriteAllLines(Path.Combine(outDir,dry?"road-dry.txt":"road-applied.txt"),log);EditorApplication.Exit(0);}
}
