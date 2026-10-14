using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 0.115 (temporary tool; no scene is saved): the ground patch that fills the creek gully under the lake path for the Leviathan Forward race only
// (Dan, 2026-10-14). Along the lake path (its stations 120-230) the gully is filled up to the lower of its two rims (MountainLoop.unity's own
// ground either side, so both ends meet it without a step; Free Roam is flat there); within 6 m of the path at that height with Free Roam's
// ground colours, beyond that its sides ease down to the scene's ground at 35 degrees. Only where it stands above that ground. Saved as
// Assets/Resources/Leviathan/LeviathanForwardGullyFill.asset (LeviathanCourse adds it with a collider when a Leviathan Forward race loads).
public static class Report115Fill {
 const float Core=6f,Slope=.7f,Step=1f;
 static bool Ground(Vector3 at,out float y,out Color c){
  y=0;c=Color.gray;var hits=Physics.RaycastAll(at+Vector3.up*60,Vector3.down,140,~0,QueryTriggerInteraction.Ignore).Where(h=>!h.collider.attachedRigidbody&&h.collider.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)<0&&h.normal.y>.2f).OrderBy(h=>h.distance).ToArray();
  if(hits.Length==0)return false;var h0=hits[0];y=h0.point.y;
  if(h0.collider is MeshCollider mc&&mc.sharedMesh&&mc.sharedMesh.isReadable&&h0.triangleIndex>=0){var m=mc.sharedMesh;var cols=m.colors;var tris=m.triangles;int i=h0.triangleIndex*3;if(cols.Length==m.vertexCount&&i+2<tris.Length){var b=h0.barycentricCoordinate;c=cols[tris[i]]*b.x+cols[tris[i+1]]*b.y+cols[tris[i+2]]*b.z;}}
  return true;}
 public static void Run(){
  var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);var log=new List<string>();
  try{
   EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop.unity");Physics.SyncTransforms();
   var lake=UnityEngine.Object.FindObjectsByType<Racer.RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(r=>r.name=="Lake shore connection").points;
   var s=new float[lake.Length];for(int i=1;i<lake.Length;i++)s[i]=s[i-1]+Vector3.Distance(lake[i-1],lake[i]);
   var path=lake.Where((p,i)=>s[i]>=120&&s[i]<=230).ToArray();
   float x0=path.Min(p=>p.x)-22,x1=path.Max(p=>p.x)+22,z0=path.Min(p=>p.z)-22,z1=path.Max(p=>p.z)+22;int W=Mathf.CeilToInt((x1-x0)/Step)+1,H=Mathf.CeilToInt((z1-z0)/Step)+1;
   int seg=0;float D(Vector3 q){float best=float.MaxValue;for(int i=0;i+1<path.Length;i++){var a=path[i];var b=path[i+1];var ab=new Vector2(b.x-a.x,b.z-a.z);float t=Mathf.Clamp01(Vector2.Dot(new Vector2(q.x-a.x,q.z-a.z),ab)/Mathf.Max(1e-4f,ab.sqrMagnitude));float d=new Vector2(q.x-a.x-ab.x*t,q.z-a.z-ab.y*t).magnitude;if(d<best){best=d;seg=i;}}return best;}
   var ml=new float[W,H];var mlc=new Color[W,H];var okM=new bool[W,H];
   for(int i=0;i<W;i++)for(int j=0;j<H;j++)okM[i,j]=Ground(new Vector3(x0+i*Step,80,z0+j*Step),out ml[i,j],out mlc[i,j]);
   // the path's own ground here, and the fill level along it: each point raised to the lower of the highest ground before and after it
   var pg=new float[path.Length];for(int k=0;k<path.Length;k++){Ground(new Vector3(path[k].x,80,path[k].z),out pg[k],out _);}
   var fill=new float[path.Length];for(int k=0;k<path.Length;k++){float before=pg.Take(k+1).Max(),after=pg.Skip(k).Max();fill[k]=Mathf.Max(pg[k],Mathf.Min(before,after));}
   // only the gully crossing: the path points it fills (more than 5 cm), and 3 either side
   var gully=new bool[path.Length];for(int k=0;k<path.Length;k++)if(fill[k]-pg[k]>.05f)for(int n=Math.Max(0,k-3);n<=Math.Min(path.Length-1,k+3);n++)gully[n]=true;
   EditorSceneManager.OpenScene("Assets/Scenes/FreeRoamWorld.unity");Physics.SyncTransforms();
   var fr=new float[W,H];var frc=new Color[W,H];var okF=new bool[W,H];
   for(int i=0;i<W;i++)for(int j=0;j<H;j++)okF[i,j]=Ground(new Vector3(x0+i*Step,80,z0+j*Step),out fr[i,j],out frc[i,j]);
   var h=new float[W,H];var c=new Color[W,H];var inside=new bool[W,H];int core=0;float deepest=0;
   for(int i=0;i<W;i++)for(int j=0;j<H;j++){
    if(!okM[i,j])continue;var q=new Vector3(x0+i*Step,0,z0+j*Step);float d=D(q);if(!gully[seg])continue;float level=fill[seg];float lift=level-ml[i,j];var col=okF[i,j]?frc[i,j]:mlc[i,j];
    if(d<=Core){h[i,j]=level;c[i,j]=col;}
    else{float span=Mathf.Max(0,lift)/Slope;if(d>Core+span)continue;float t=(d-Core)/Mathf.Max(.01f,span);h[i,j]=Mathf.Lerp(level,ml[i,j],t);c[i,j]=Color.Lerp(col,mlc[i,j],t);}
    if(h[i,j]>ml[i,j]+.03f){inside[i,j]=true;if(d<=Core){core++;deepest=Mathf.Max(deepest,lift);}}}
   // a quad wherever a corner stands above Mountain Loop's ground; its other corners sit just on that ground (they sink into it at the edge)
   var verts=new List<Vector3>();var cols=new List<Color>();var tris=new List<int>();var index=new int[W,H];for(int i=0;i<W;i++)for(int j=0;j<H;j++)index[i,j]=-1;
   int V(int i,int j){if(index[i,j]>=0)return index[i,j];index[i,j]=verts.Count;verts.Add(new Vector3(x0+i*Step,inside[i,j]?h[i,j]:ml[i,j]-.02f,z0+j*Step));cols.Add(inside[i,j]?c[i,j]:mlc[i,j]);return index[i,j];}
   int quads=0;
   for(int i=0;i+1<W;i++)for(int j=0;j+1<H;j++){
    if(!(inside[i,j]||inside[i+1,j]||inside[i,j+1]||inside[i+1,j+1]))continue;if(!(okM[i,j]&&okM[i+1,j]&&okM[i,j+1]&&okM[i+1,j+1]))continue;
    int a=V(i,j),b=V(i+1,j),cc=V(i,j+1),d2=V(i+1,j+1);tris.AddRange(new[]{a,cc,d2,a,d2,b});quads++;}
   var mesh=new Mesh{name="Leviathan Forward gully fill"};mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(verts);mesh.SetColors(cols);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
   Directory.CreateDirectory("Assets/Resources/Leviathan");const string asset="Assets/Resources/Leviathan/LeviathanForwardGullyFill.asset";if(File.Exists(asset))AssetDatabase.DeleteAsset(asset);AssetDatabase.CreateAsset(mesh,asset);AssetDatabase.SaveAssets();
   log.Add($"gully fill: grid {W}x{H} at 1 m over x {x0:F0}-{x1:F0} z {z0:F0}-{z1:F0}; {quads} quads, {verts.Count} vertices; {core} cells on the path (within {Core} m) raised, at most {deepest:F2} m; bounds {mesh.bounds}");
   for(int k=0;k<path.Length;k+=4){var p=path[k];int i=Mathf.RoundToInt((p.x-x0)/Step),j=Mathf.RoundToInt((p.z-z0)/Step);log.Add(FormattableString.Invariant($"  path {p.x:F0},{p.z:F0}: line y {p.y:F2}, Free Roam {fr[i,j]:F2}, Mountain Loop {ml[i,j]:F2}, fill level {fill[k]:F2}, patch {(inside[i,j]?h[i,j].ToString("F2"):"-")}"));}
   File.WriteAllLines(Path.Combine(outDir,"fill.txt"),log);EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Path.Combine(outDir,"fill-error.txt"),e.ToString());EditorApplication.Exit(1);}
 }
}
