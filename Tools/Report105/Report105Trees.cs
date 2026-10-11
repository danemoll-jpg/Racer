using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.105 Part C (BUG-003 "tree in road" at (-578.6, -273.1), BUG-004 "trees in path" at (-303.8, -524.5), Free Roam): both stand on the
// same woodland trail, Street Loop Forward's branch through the southwest woods. In the race scene it is a race line (its trees were
// cleared or hidden there); in Free Roam and the other scenes it is only painted ground with edge posts, so nothing kept trees off it.
// Every tree (trunk collider, bark box, or bush) standing inside the trail's width + 1 m, over its whole length, is moved whole
// (collider, bark and crown pieces of the old forest meshes) straight out to beside the trail, onto open ground. PROBE_PLAY "dry".
public static class Report105Trees {
 static readonly string[] Scenes={"FreeRoamWorld","StreetLoopGreybox","StreetLoopReverse","LakeWoods","ForestLoopReverse","MountainLoop","MountainLoopReverse","DansBackyardForward","DansBackyardReverse"};
 static string Path_(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static float Hash(Vector3 p)=>Mathf.Repeat(Mathf.Sin(p.x*12.9898f+p.z*78.233f)*43758.5453f,1);
 // the trail: points and half width, from the race scene's line
 static Vector3[] line;static float half;static string title;
 static (float s,float lat,Vector3 at,Vector3 right) Near(Vector3 p,float maxDy=4){float best=1e9f,bs=0,acc=0,bl=0;Vector3 ba=default,br=default;
  for(int i=0;i+1<line.Length;i++){var a=line[i];var b=line[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);float L=ab.magnitude;var ap=new Vector2(p.x-a.x,p.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(ap,ab)/Mathf.Max(1e-6f,L*L));var q=Vector3.Lerp(a,b,t);var d=new Vector2(p.x-q.x,p.z-q.z);float dd=d.magnitude;
   if(dd<best&&Mathf.Abs(p.y-q.y)<maxDy){best=dd;bs=acc+t*L;ba=q;var f=new Vector3(ab.x,0,ab.y).normalized;br=Vector3.Cross(Vector3.up,f);bl=Vector2.Dot(d,new Vector2(br.x,br.z));}acc+=L;}
  return (bs,best>1e8f?best:(bl<0?-best:best),ba,br);}
 sealed class Tree{public string kind,name;public Vector3 bottom;public float width;public BoxCollider box;public List<(MeshFilter mf,HashSet<int> verts)> parts=new();public Vector3 delta;public bool placed;public string why;public float s,lat;}
 sealed class Comp{public MeshFilter mf;public HashSet<int> verts;public Bounds b;public bool bark;public bool used;}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");bool dry=Environment.GetEnvironmentVariable("PROBE_PLAY")=="dry";var log=new List<string>{dry?"DRY RUN":"APPLIED"};
  var only=Environment.GetEnvironmentVariable("PROBE_SCENES");var scenes=string.IsNullOrEmpty(only)||only=="all"?Scenes:only.Split(',');
  // the line from the race scene
  EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopGreybox.unity");
  var route=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(w=>w.points!=null&&w.points.Length>1).OrderBy(w=>w.points.Min(q=>new Vector2(q.x+578.59f,q.z+273.13f).magnitude)).First();
  line=route.points.ToArray();half=route.halfWidth;title=route.title;float len=0;for(int i=0;i+1<line.Length;i++)len+=Vector3.Distance(line[i],line[i+1]);
  foreach(var pt in new[]{new Vector3(-578.59f,11.5f,-273.13f),new Vector3(-303.76f,25.8f,-524.46f)}){var n=Near(pt);log.Add($"trail: '{title}' ({Path_(route.transform)}), {line.Length} points, {len:F0} m, half width {half:F1} m; Dan's spot {pt.x},{pt.z}: s {n.s:F0}, {Mathf.Abs(n.lat):F1} m from the centre");}
  Directory.CreateDirectory("Assets/Scenery/Report105");
  foreach(var sn in scenes){
   var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+sn+".unity");Physics.SyncTransforms();var trees=new List<Tree>();
   float reach=half+1;
   // trunk colliders
   var boxes=UnityEngine.Object.FindObjectsByType<BoxCollider>(FindObjectsSortMode.None).Where(b=>b.enabled&&b.gameObject.activeInHierarchy&&b.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0).ToList();
   var allTrunks=new List<(Vector3 p,float w)>();
   foreach(var box in boxes){var t=box.transform;var bottom=t.TransformPoint(box.center-Vector3.up*box.size.y*.5f);var size=Vector3.Scale(box.size,t.lossyScale);float w=Mathf.Min(Mathf.Abs(size.x),Mathf.Abs(size.z));allTrunks.Add((bottom,w));
    var n=Near(bottom);if(Mathf.Abs(n.lat)-w*.5f<reach)trees.Add(new Tree{kind="trunk collider",name=Path_(t),bottom=bottom,width=w,box=box,s=n.s,lat=n.lat});}
   // the old forest meshes: connected pieces near the trail
   var comps=new List<Comp>();var area=new Bounds(line[0],Vector3.zero);foreach(var q in line)area.Encapsulate(q);area.Expand(new Vector3(30,60,30));
   foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){if(!r.enabled||!r.gameObject.activeInHierarchy||!Racer.SceneryTrees.IsOldVegetation(r)||!r.bounds.Intersects(area))continue;var mf=r.GetComponent<MeshFilter>();var mesh=mf.sharedMesh;var v=mesh.vertices;var col=mesh.colors;var tri=mesh.triangles;var m=mf.transform.localToWorldMatrix;
    var parent=Enumerable.Range(0,v.Length).ToArray();int Find(int x){while(parent[x]!=x){parent[x]=parent[parent[x]];x=parent[x];}return x;}
    for(int i=0;i<tri.Length;i+=3){int a=Find(tri[i]);parent[Find(tri[i+1])]=a;parent[Find(tri[i+2])]=a;}
    var groups=new Dictionary<int,Comp>();var sumc=new Dictionary<int,(Color c,int n)>();var used=new bool[v.Length];foreach(var t in tri)used[t]=true;
    for(int i=0;i<v.Length;i++){if(!used[i])continue;int root=Find(i);var w=m.MultiplyPoint3x4(v[i]);if(!groups.TryGetValue(root,out var g)){groups[root]=g=new Comp{mf=mf,verts=new HashSet<int>(),b=new Bounds(w,Vector3.zero)};sumc[root]=(Color.clear,0);}g.verts.Add(i);g.b.Encapsulate(w);var sc=sumc[root];sumc[root]=(sc.c+(col.Length==v.Length?col[i]:Color.gray),sc.n+1);}
    foreach(var kv in groups){var c=sumc[kv.Key].c/sumc[kv.Key].n;kv.Value.bark=c.r>c.g*1.05f;var cen=kv.Value.b.center;if(Mathf.Abs(Near(new Vector3(cen.x,kv.Value.b.min.y,cen.z),30).lat)<reach+12)comps.Add(kv.Value);}
   }
   // bark boxes with no collider (merged touching faces) standing in the trail
   var barkComps=comps.Where(c=>c.bark).ToList();var merged=new List<List<Comp>>();
   foreach(var c in barkComps){var hit=merged.Where(g=>g.Any(o=>{var a=o.b;a.Expand(.02f);return a.Intersects(c.b);})).ToList();var ng=new List<Comp>{c};foreach(var g in hit){ng.AddRange(g);merged.Remove(g);}merged.Add(ng);}
   foreach(var g in merged){var b=g[0].b;foreach(var c in g)b.Encapsulate(c.b);var bottom=new Vector3(b.center.x,b.min.y,b.center.z);float w=Mathf.Min(b.size.x,b.size.z);
    var owner=trees.FirstOrDefault(t=>t.box&&new Vector2(t.bottom.x-bottom.x,t.bottom.z-bottom.z).magnitude<Mathf.Max(.4f,b.extents.x));
    if(owner!=null){foreach(var c in g){owner.parts.Add((c.mf,c.verts));c.used=true;}continue;}
    if(allTrunks.Any(t=>new Vector2(t.p.x-bottom.x,t.p.z-bottom.z).magnitude<Mathf.Max(.4f,b.extents.x)))continue; // another trunk's bark, outside the trail
    var n=Near(bottom);if(Mathf.Abs(n.lat)-w*.5f>=reach)continue;
    var tr=new Tree{kind="bark box",name=g[0].mf.name,bottom=bottom,width=w,s=n.s,lat=n.lat};foreach(var c in g){tr.parts.Add((c.mf,c.verts));c.used=true;}trees.Add(tr);}
   // crowns: each to the nearest moving tree whose top reaches it (as the scenery matches them), only if that tree is the nearest trunk of all
   foreach(var c in comps.Where(c=>!c.bark&&!c.used)){var cen=new Vector3(c.b.center.x,c.b.min.y,c.b.center.z);float rr=Mathf.Max(2.5f,c.b.extents.x*.9f);
    Tree best=null;float bd=rr;foreach(var t in trees){float d=new Vector2(t.bottom.x-cen.x,t.bottom.z-cen.z).magnitude;if(d<bd&&t.bottom.y<c.b.max.y){bd=d;best=t;}}
    if(best==null){// a bush standing in the trail
     var n=Near(cen);if(c.b.size.y<3.5f&&Mathf.Abs(n.lat)<reach&&GroundY(cen,out var gy)&&cen.y-gy<1f){var tr=new Tree{kind="bush",name=c.mf.name,bottom=cen,width=Mathf.Max(c.b.size.x,c.b.size.z)*.5f,s=n.s,lat=n.lat};tr.parts.Add((c.mf,c.verts));c.used=true;trees.Add(tr);}continue;}
    if(allTrunks.Any(t=>new Vector2(t.p.x-cen.x,t.p.z-cen.z).magnitude<bd-.05f&&!trees.Any(m=>m.box&&(m.bottom-t.p).sqrMagnitude<.01f)))continue; // nearer to a tree that stays
    best.parts.Add((c.mf,c.verts));c.used=true;}
   // where each goes: straight out to the side it stands on, just past the trail's width + 1 m, onto open ground
   var placedNow=new List<Vector3>();
   foreach(var t in trees.OrderBy(t=>t.s)){
    var n=Near(t.bottom);float side=Mathf.Abs(t.lat)<.3f?(Hash(t.bottom)<.5f?-1:1):Mathf.Sign(t.lat);
    // candidates nearest first: straight out on its own side, then the other side, then a little along the trail (up to 25 m)
    var cands=new List<(float cost,float sd,float extra,float ds)>();
    foreach(float sd in new[]{side,-side})for(float extra=0;extra<=6;extra+=.5f)for(float ds=-25;ds<=25;ds+=2.5f)cands.Add((extra+Mathf.Abs(ds)*.8f+(sd==side?0:1.5f),sd,extra,ds));
    foreach(var cand in cands.OrderBy(c=>c.cost)){if(t.placed)break;float sd=cand.sd,extra=cand.extra+Hash(t.bottom+Vector3.one*cand.ds)*.4f;
     var fwd=Vector3.Cross(n.right,Vector3.up);var along=Near(n.at+fwd*cand.ds+Vector3.up*0);var basePt=along.at+along.right*sd*(reach+t.width*.5f+.6f+extra);
     if(!GroundY(basePt,out var gy,out var normal)){t.why="no ground";continue;}var to=new Vector3(basePt.x,gy,basePt.z);
     if(Mathf.Abs(gy-along.at.y)>3.5f){t.why="ground too far above or below the trail";continue;}
     var nn=Near(to);if(Mathf.Abs(nn.lat)-t.width*.5f<reach){t.why="still in the trail";continue;}
     if(Mathf.Abs(nn.lat)>reach+6.5f){t.why="too far from the trail";continue;}
     if(normal.y<.62f){t.why="too steep";continue;}
     if(Physics.OverlapCapsule(to+Vector3.up*.4f,to+Vector3.up*2.5f,Mathf.Max(.6f,t.width*.5f+.4f),~0,QueryTriggerInteraction.Ignore).Any(c=>!c.name.StartsWith("Ground_")&&c!=t.box&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0)){t.why="something there";continue;}
     if(allTrunks.Any(o=>(o.p-t.bottom).sqrMagnitude>.01f&&new Vector2(o.p.x-to.x,o.p.z-to.z).magnitude<1.2f)||placedNow.Any(o=>new Vector2(o.x-to.x,o.z-to.z).magnitude<1.5f)){t.why="another trunk";continue;}
     if(Racer.SceneryTrees.OnDrivable(to,null,out var what)){t.why="drivable: "+what;continue;}
     t.delta=to-t.bottom;t.placed=true;placedNow.Add(to);}
   }
   log.Add($"===== {sn}: {trees.Count} trees in the trail ({trees.Count(t=>t.kind=="trunk collider")} with trunk colliders, {trees.Count(t=>t.kind=="bark box")} bark boxes, {trees.Count(t=>t.kind=="bush")} bushes); placed beside it {trees.Count(t=>t.placed)}");
   foreach(var t in trees.OrderBy(t=>t.s))log.Add($"  s {t.s,5:F0} {t.kind} '{t.name}' at {t.bottom:F1} {Mathf.Abs(t.lat):F1} m from the centre, width {t.width:F2}, {t.parts.Count} mesh pieces -> {(t.placed?$"moved {new Vector2(t.delta.x,t.delta.z).magnitude:F1} m to {(t.bottom+t.delta):F1} ({Mathf.Abs(Near(t.bottom+t.delta).lat):F1} m from the centre, ground {t.delta.y:+0.00;-0.00} m)":"NOT PLACED: "+t.why)}");
   foreach(var t in trees)log.Add($"    {t.name} at {t.bottom:F1}: {t.parts.Count(pp=>comps.Any(c=>c.verts==pp.verts&&c.bark))} bark + {t.parts.Count(pp=>comps.Any(c=>c.verts==pp.verts&&!c.bark))} crown pieces");
   // crowns left over the trail with no trunk under them any more (the scenery would give such a crown a trunk of its own)
   var staying=allTrunks.Where(o=>!trees.Any(m=>m.placed&&m.box&&(m.bottom-o.p).sqrMagnitude<.01f)).ToList();
   foreach(var c in comps.Where(c=>!c.bark&&!c.used)){var cen=new Vector3(c.b.center.x,c.b.min.y,c.b.center.z);if(Mathf.Abs(Near(cen,30).lat)>reach+2)continue;float rr=Mathf.Max(2.5f,c.b.extents.x*.9f);
    bool had=allTrunks.Any(o=>new Vector2(o.p.x-cen.x,o.p.z-cen.z).magnitude<rr);bool has=staying.Any(o=>new Vector2(o.p.x-cen.x,o.p.z-cen.z).magnitude<rr);if(had&&!has)log.Add($"  CROWN LEFT over the trail without its trunk: {c.mf.name} {c.b.min:F1}-{c.b.max:F1}");}
   if(dry||trees.Count==0)continue;
   // apply: colliders, and the forest meshes (a copy per scene)
   foreach(var t in trees.Where(t=>t.placed&&t.box)){var tr=t.box.transform;if(tr.GetComponents<Component>().Length==2&&tr.childCount==0){tr.position+=t.delta;EditorUtility.SetDirty(tr);}else{t.box.center+=tr.InverseTransformVector(t.delta);EditorUtility.SetDirty(t.box);}}
   foreach(var grp in trees.Where(t=>t.placed).SelectMany(t=>t.parts.Select(p=>(p.mf,p.verts,t.delta))).GroupBy(x=>x.mf)){
    var mf=grp.Key;var src=mf.sharedMesh;var copy=UnityEngine.Object.Instantiate(src);copy.name=src.name.Replace(" (0.105 trail trees)","")+" (0.105 trail trees)";var v=copy.vertices;var inv=mf.transform.worldToLocalMatrix;var m=mf.transform.localToWorldMatrix;
    foreach(var (_,verts,delta) in grp)foreach(var i in verts)v[i]=inv.MultiplyPoint3x4(m.MultiplyPoint3x4(v[i])+delta);copy.vertices=v;copy.RecalculateBounds();
    var path="Assets/Scenery/Report105/"+sn+"-"+mf.name.Replace("/","-")+"-"+Math.Abs(Path_(mf.transform).GetHashCode())+"-trail-trees.asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(path))AssetDatabase.DeleteAsset(path);AssetDatabase.CreateAsset(copy,path);mf.sharedMesh=copy;EditorUtility.SetDirty(mf);}
   EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  }
  File.WriteAllLines(Path.Combine(outDir,dry?"trees-dry.txt":"trees-applied.txt"),log);EditorApplication.Exit(0);}
 static bool GroundY(Vector3 p,out float y)=>GroundY(p,out y,out _);
 static bool GroundY(Vector3 p,out float y,out Vector3 normal){y=0;normal=Vector3.up;float best=float.MinValue;bool any=false;foreach(var h in Physics.RaycastAll(new Vector3(p.x,p.y+40,p.z),Vector3.down,90,~0,QueryTriggerInteraction.Ignore)){if(!h.collider.name.StartsWith("Ground_"))continue;if(h.point.y>best&&h.point.y<p.y+6){best=h.point.y;normal=h.normal;any=true;}}y=best;return any;}
}
