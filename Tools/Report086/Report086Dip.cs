using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEditor.SceneManagement;using UnityEngine;using Racer;
// 0.86 Part D: Forest Loop Reverse, Granite Saddle approach, the dip at x ~ 532. The trail bends right at the foot of the
// climb (s 55-105) while its outside (left) edge falls away (about -28 % across the trail, then a 40-60 % side slope into
// lower ground); vehicles carried wide by the bend drop onto that slope and lose their speed there (0.85 ATV: 11 m/s).
// Local fill on the outside of that bend only, never a cut: across the trail's left half and out to 7 m from the centre
// line the ground is brought up to the centre-line height rising 4 % outward (no cross-fall; a slight bank), then a 1:1.2
// batter down to the existing hillside. Weight 0 -> 1 over s 52-62 and 1 -> 0 over s 96-106. Centre line, right side,
// the main, gates, the lip, the landing, Fern Gully and the driveway are untouched. Ground mesh asset edited once; normals
// recomputed only where it changed. Tree pieces in the woods batches standing on raised ground are lifted by the fill at
// their base. DIP_DRY=1 lists only.
public static class Report086Dip {
 const float S0=62,S1=72,S2=96,S3=106,Bank=.04f,Lb=7f,Batter=1.2f,Lmax=22f;
 static string V(Vector3 v)=>$"{v.x:F2},{v.y:F2},{v.z:F2}";
 static string P(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
 static bool Skip(Collider c)=>c.isTrigger||c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0||(c.attachedRigidbody&&!c.attachedRigidbody.isKinematic);
 static float Ground(float x,float z,float near){float best=float.NaN,bd=1e9f;foreach(var h in Physics.RaycastAll(new Vector3(x,near+30,z),Vector3.down,80,~0,QueryTriggerInteraction.Ignore)){if(Skip(h.collider))continue;float d=Mathf.Abs(h.point.y-near);if(d<bd){bd=d;best=h.point.y;}}return best;}
 static float W(float s)=>s<=S0||s>=S3?0:s<S1?Mathf.SmoothStep(0,1,(s-S0)/(S1-S0)):s<=S2?1:Mathf.SmoothStep(1,0,(s-S2)/(S3-S2));
 // Fill height at a world point (old height y): >= 0.
 public static RaceRoad Main;
 static float Fill(WoodlandRoute b,Vector3 p){float s=b.Project(p,out _);float w=W(s);if(w<=0)return 0;
  if(Main){float ms=Main.Project(p,out float ml);float clear=Mathf.Abs(ml)-Main.HalfWidth(ms)-4;if(clear<=0)return 0;w*=Mathf.Clamp01(clear/4);}var c=b.At(s,out var f);f.y=0;f.Normalize();var right=Vector3.Cross(Vector3.up,f);
  var d=p-c;d.y=0;float l=-Vector3.Dot(d,right);if(l<=0||l>Lmax)return 0;if(Mathf.Abs(Vector3.Dot(d,f))>2f)return 0; // only points beside this station
  float T=l<=Lb?c.y+Bank*l:c.y+Bank*Lb-Batter*(l-Lb);return w*Mathf.Max(0,T-p.y);}
 public static void Run(){bool dry=Environment.GetEnvironmentVariable("DIP_DRY")=="1";var o=Environment.GetEnvironmentVariable("PROBE_OUT");var sb=new StringBuilder();
  try{var s=EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");Physics.SyncTransforms();var roots=s.GetRootGameObjects();
   var b=roots.SelectMany(g=>g.GetComponentsInChildren<WoodlandRoute>(true)).First(x=>x.title=="Granite Saddle");b.Initialize();Main=roots.SelectMany(g=>g.GetComponentsInChildren<RaceDirector>(true)).First().road;Main.Initialize();
   var zone=new Bounds();zone.SetMinMax(new Vector3(500,0,-240),new Vector3(585,200,-190));
   var mc=roots.SelectMany(g=>g.GetComponentsInChildren<MeshCollider>(true)).First(c=>c.name=="Ground_640_240"&&c.sharedMesh);
   var mesh=mc.sharedMesh;var path=AssetDatabase.GetAssetPath(mesh);var t=mc.transform;var v=mesh.vertices;
   sb.AppendLine($"ground {P(t)} {path} ({mesh.vertexCount} vertices); scenes using the asset: {string.Join(",",AssetDatabase.FindAssets("t:Scene",new[]{"Assets/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(sp=>AssetDatabase.GetDependencies(sp,false).Contains(path)).Select(Path.GetFileNameWithoutExtension))}");
   // Other colliders whose surface lies in the fill area (would be buried or now float above): listed.
   int n=0;float maxFill=0,vol=0;Vector3 maxAt=default;var changed=new bool[v.Length];
   for(int i=0;i<v.Length;i++){var w=t.TransformPoint(v[i]);if(!zone.Contains(w))continue;float f=Fill(b,w);if(f<.005f)continue;changed[i]=true;n++;if(f>maxFill){maxFill=f;maxAt=w;}if(!dry)v[i]=t.InverseTransformPoint(w+Vector3.up*f);}
   // rough volume: 0.5 m grid
   for(float x=zone.min.x;x<zone.max.x;x+=.5f)for(float z=zone.min.z;z<zone.max.z;z+=.5f){float g=Ground(x,z,b.At(b.Project(new Vector3(x,0,z),out _),out _).y);if(float.IsNaN(g))continue;vol+=Fill(b,new Vector3(x,g,z))*.25f;}
   sb.AppendLine($"  vertices raised {n}, largest fill {maxFill:F2} m at {V(maxAt)}, fill volume about {vol:F0} m3");
   for(float st=50;st<=110;st+=5){var c=b.At(st,out var f);f.y=0;f.Normalize();var left=-Vector3.Cross(Vector3.up,f);var row=new StringBuilder();
    foreach(var l in new[]{0f,2f,3.6f,5f,7f,9f,11f,14f}){var p=c+left*l;float g=Ground(p.x,p.z,c.y);row.Append($" l{l:F0}:{g-c.y:+0.00;-0.00}->{g+Fill(b,new Vector3(p.x,g,p.z))-c.y:+0.00;-0.00}");}
    sb.AppendLine($"  s {st:F0} centre {V(c)} w {W(st):F2} (height relative to the centre, before->after):{row}");}
   foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<Collider>(true))){if(c==mc||c.bounds.size.x>80||!c.bounds.Intersects(zone))continue;var bb=c.bounds;var foot=new Vector3(bb.center.x,bb.min.y,bb.center.z);float g=Ground(foot.x,foot.z,foot.y);float f=Fill(b,new Vector3(foot.x,g,foot.z));if(f>.05f)sb.AppendLine($"  collider in the fill: {P(c.transform)} bottom {V(foot)} ground {g:F2} fill {f:F2}");}
   var lifts=new List<(Renderer r,int[] idx,float dy)>();
   foreach(var r in roots.SelectMany(g=>g.GetComponentsInChildren<MeshRenderer>(true))){if(r.GetComponent<MeshCollider>()==mc||!r.bounds.Intersects(zone))continue;var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||!mf.sharedMesh.isReadable)continue;var m=mf.sharedMesh;
    if(r.bounds.size.x<60){var bb=r.bounds;var foot=new Vector3(bb.center.x,bb.min.y,bb.center.z);float g=Ground(foot.x,foot.z,foot.y);float f=Fill(b,new Vector3(foot.x,g,foot.z));if(f>.05f&&Mathf.Abs(foot.y-g)<1.5f)sb.AppendLine($"  object in the fill: {P(r.transform)} bottom {V(foot)} ground {g:F2} fill {f:F2}");continue;}
    // batch: pieces = vertices joined by triangles or shared positions
    var wv=m.vertices.Select(x=>r.transform.TransformPoint(x)).ToArray();var par=Enumerable.Range(0,wv.Length).ToArray();int Find(int x){while(par[x]!=x){par[x]=par[par[x]];x=par[x];}return x;}void U(int a,int c){a=Find(a);c=Find(c);if(a!=c)par[a]=c;}
    var tri=m.triangles;for(int k=0;k<tri.Length;k+=3){U(tri[k],tri[k+1]);U(tri[k],tri[k+2]);}
    var byPos=new Dictionary<Vector3Int,int>();for(int i=0;i<wv.Length;i++){var key=Vector3Int.RoundToInt(wv[i]*200);if(byPos.TryGetValue(key,out var j))U(i,j);else byPos[key]=i;}
    var used=new HashSet<int>(tri);
    foreach(var grp in Enumerable.Range(0,wv.Length).Where(used.Contains).GroupBy(Find)){var idx=grp.ToArray();int low=idx.OrderBy(i=>wv[i].y).First();var bp=wv[low];if(!zone.Contains(bp))continue;
     float g=Ground(bp.x,bp.z,bp.y);if(float.IsNaN(g)||bp.y-g>1.5f||bp.y<g-1.5f)continue;float f=Fill(b,new Vector3(bp.x,g,bp.z));if(f<.05f)continue;
     var ext=new Bounds(wv[idx[0]],Vector3.zero);foreach(var i in idx)ext.Encapsulate(wv[i]);
     sb.AppendLine($"  batch piece {P(r.transform)} [{AssetDatabase.GetAssetPath(m)}] {idx.Length} vertices, base {V(bp)} ground {g:F2}, size {V(ext.size)}: lift {f:F2}");lifts.Add((r,idx,f));}}
   if(!dry){var oldN=mesh.normals;mesh.vertices=v;mesh.RecalculateNormals();var nn=mesh.normals;var near=new HashSet<int>();var tris=mesh.triangles;
    for(int k=0;k<tris.Length;k+=3)if(changed[tris[k]]||changed[tris[k+1]]||changed[tris[k+2]]){near.Add(tris[k]);near.Add(tris[k+1]);near.Add(tris[k+2]);}
    for(int i=0;i<nn.Length;i++)if(!near.Contains(i))nn[i]=oldN[i];mesh.normals=nn;mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);mc.sharedMesh=null;mc.sharedMesh=mesh;
    foreach(var grp in lifts.GroupBy(x=>x.r)){var m=grp.Key.GetComponent<MeshFilter>().sharedMesh;var vv=m.vertices;var tr=grp.Key.transform;foreach(var (r,idx,dy) in grp)foreach(var i in idx)vv[i]=tr.InverseTransformPoint(tr.TransformPoint(vv[i])+Vector3.up*dy);m.vertices=vv;m.RecalculateBounds();EditorUtility.SetDirty(m);}
    EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();sb.AppendLine("saved");}
  }catch(Exception e){sb.AppendLine("ERROR "+e);}
  Directory.CreateDirectory(o);File.WriteAllText(o+"/D-dip.txt",sb.ToString());EditorApplication.Exit(0);}
}
