using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.74 Part B: the Backyard Reverse storm-drain tunnel in Free Roam everywhere. Dan's Backyard Forward and Reverse have it
// already (open in Free Roam since 0.54). In the six other course scenes the Backyard Reverse version is copied in as Free
// Roam content (FreeRoamOnly): the culvert, its floor, mouths, lights, channels, inlets, the 0.57 storm flow and washed
// debris, the gully takeoff and the west-bank landing, and the "Storm Drain atmosphere" (rat encounter, litter, sediment;
// not the LOGGING RIDGE sign). The other courses have no gully there and the tunnel mouths are closed, so the terrain tiles
// of the area take the Backyard Reverse shape in Free Roam only (inside the tunnel's area, plus 14 m), batched trees on the
// changed ground are re-grounded on it (Free Roam meshes), trunks and props follow; in races the scene is exactly as before.
// Route components (the shortcut, its guidance, recovery exclusions, the Backyard gate logic) are not copied.
public static partial class Report074Author {
 static readonly string[] DrainPieces={"Long storm culvert walls and ceiling","Ground_Culvert seamless floor","Ground_Gully continuous curved takeoff","Ground_Wide west bank landing","Supported culvert mouth pier","Culvert mouth lintel","Buried gully mouth lining","Buried gully mouth crown lining","Culvert vertical seam","Culvert overhead expansion joint","Low maintenance tunnel light","Wet edge channel","Side stormwater inlet","Edge sediment","Shallow drain ripple footprint","Continuous shallow storm flow","Visible washed paper and leaves","Visible washed debris accumulation"};
 const string TunnelRoot="Storm drain tunnel (Free Roam)";
 static void PartBTunnel(bool dry){
  var target=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(target.name.StartsWith("DansBackyard")){Note($"Part B {target.name}: the tunnel is part of this scene (open in Free Roam); not changed");return;}
  foreach(var old in target.GetRootGameObjects().Where(g=>g.name==TunnelRoot))Object.DestroyImmediate(old);
  var src=EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity",OpenSceneMode.Additive);
  try{
   var sroot=src.GetRootGameObjects().First(g=>g.name=="Reverse optional forest shortcuts");var atm=src.GetRootGameObjects().First(g=>g.name=="Storm Drain atmosphere");
   var pieces=sroot.transform.Cast<Transform>().Where(t=>Is(t.name,DrainPieces)).ToList();
   var atmPieces=atm.transform.Cast<Transform>().Where(t=>t.name.IndexOf("logging",StringComparison.OrdinalIgnoreCase)<0).ToList();
   var rs=pieces.Concat(atmPieces).SelectMany(t=>t.GetComponentsInChildren<Renderer>(true)).ToArray();var area=rs[0].bounds;foreach(var r in rs)area.Encapsulate(r.bounds);
   var R=new Rect(area.min.x-14,area.min.z-14,area.size.x+28,area.size.z+28);
   // the changed ground: within 14 m of a tunnel piece (not the whole box), and never within 20 m of this scene's own routes
   var near=rs.Select(r=>{var b=r.bounds;return new Rect(b.min.x-14,b.min.z-14,b.size.x+28,b.size.z+28);}).ToArray();
   var own=Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==target&&x.points!=null).SelectMany(x=>x.points).Concat(Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==target&&x.points!=null).SelectMany(x=>x.points)).Where(q=>R.Contains(new Vector2(q.x,q.z))||new Vector2(q.x-R.center.x,q.z-R.center.y).magnitude<R.width).Select(q=>new Vector2(q.x,q.z)).ToArray();
   Note($"Part B {target.name}: {pieces.Count} tunnel pieces + {atmPieces.Count} atmosphere pieces from Dan's Backyard Reverse; area x {R.xMin:F0}..{R.xMax:F0}, z {R.yMin:F0}..{R.yMax:F0}; {own.Length} route points of this scene nearby kept clear");
   bool InR(Vector3 w){var p2=new Vector2(w.x,w.z);if(!R.Contains(p2))return false;bool any=false;foreach(var n in near)if(n.Contains(p2)){any=true;break;}if(!any)return false;foreach(var q in own)if((q-p2).sqrMagnitude<400)return false;return true;}
   // ---- terrain tiles: Free Roam shapes ----
   // The Backyard tiles are much finer than the other scenes' tiles, so a Free Roam tile is a hybrid: the Backyard Reverse
   // triangles where its ground differs from this scene's (the gully, the tunnel mouths) and that region is connected to
   // the tunnel, this scene's own triangles everywhere else. The region's edge lies where both grounds agree (within 5 cm).
   var rx=new System.Text.RegularExpressions.Regex(@"^Ground_-?\d+_-?\d+$");
   var tTiles=target.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).Where(m=>rx.IsMatch(m.name)&&m.GetComponent<MeshCollider>()&&m.sharedMesh).ToList();
   var sTiles=src.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).Where(m=>rx.IsMatch(m.name)&&m.GetComponent<MeshCollider>()&&m.sharedMesh).ToList();
   float TopOn(IEnumerable<MeshFilter> tiles,float x,float z){float best=float.NaN;foreach(var t in tiles){var c=t.GetComponent<MeshCollider>();if(x<c.bounds.min.x-.01f||x>c.bounds.max.x+.01f||z<c.bounds.min.z-.01f||z>c.bounds.max.z+.01f)continue;if(c.Raycast(new Ray(new Vector3(x,700,z),Vector3.down),out var h,1400)&&(float.IsNaN(best)||h.point.y>best))best=h.point.y;}return best;}
   int GX=Mathf.CeilToInt(R.width),GZ=Mathf.CeilToInt(R.height);var diff=new bool[GX,GZ];var keep=new bool[GX,GZ];int nd=0;
   for(int i=0;i<GX;i++)for(int j=0;j<GZ;j++){float x=R.xMin+i+.5f,z=R.yMin+j+.5f;var p2=new Vector3(x,0,z);if(!InR(p2))continue;float a=TopOn(tTiles,x,z),b=TopOn(sTiles,x,z);if(float.IsNaN(a))continue;if(float.IsNaN(b)||Mathf.Abs(a-b)>.05f){diff[i,j]=true;nd++;}}
   // connected to the tunnel: flood from the cells under the tunnel pieces
   var fill=new Queue<(int,int)>();foreach(var r in rs){var b=r.bounds;for(float x=b.min.x;x<=b.max.x;x+=1)for(float z=b.min.z;z<=b.max.z;z+=1){int i=Mathf.FloorToInt(x-R.xMin),j=Mathf.FloorToInt(z-R.yMin);if(i>=0&&j>=0&&i<GX&&j<GZ&&diff[i,j]&&!keep[i,j]){keep[i,j]=true;fill.Enqueue((i,j));}}}
   while(fill.Count>0){var (i,j)=fill.Dequeue();foreach(var (di,dj) in new[]{(1,0),(-1,0),(0,1),(0,-1)}){int a=i+di,b=j+dj;if(a>=0&&b>=0&&a<GX&&b<GZ&&diff[a,b]&&!keep[a,b]){keep[a,b]=true;fill.Enqueue((a,b));}}}
   // one cell of margin so the hybrid edge sits on agreeing ground
   var region=(bool[,])keep.Clone();for(int i=0;i<GX;i++)for(int j=0;j<GZ;j++)if(keep[i,j])for(int di=-1;di<=1;di++)for(int dj=-1;dj<=1;dj++){int a=i+di,b=j+dj;if(a>=0&&b>=0&&a<GX&&b<GZ&&InR(new Vector3(R.xMin+a+.5f,0,R.yMin+b+.5f)))region[a,b]=true;}
   bool InRegion(Vector3 w){int i=Mathf.FloorToInt(w.x-R.xMin),j=Mathf.FloorToInt(w.z-R.yMin);return i>=0&&j>=0&&i<GX&&j<GZ&&region[i,j];}
   int cells=0;foreach(var v in region)if(v)cells++;
   Note($"Part B {target.name}: ground differs from Backyard Reverse on {nd} m² of the area; {cells} m² connected to the tunnel take the Backyard Reverse shape in Free Roam");
   var swapMF=new List<MeshFilter>();var swapMesh=new List<Mesh>();var probes=new List<GameObject>();
   foreach(var tt in tTiles){var b=tt.GetComponent<MeshCollider>().bounds;if(!R.Overlaps(new Rect(b.min.x,b.min.z,b.size.x,b.size.z)))continue;
    var st=sTiles.FirstOrDefault(m=>m.name==tt.name);if(!st)continue;
    var tm=tt.sharedMesh;var sm=st.sharedMesh;var tv=tm.vertices;var tt3=tm.triangles;var sv=sm.vertices;var st3=sm.triangles;
    var verts=new List<Vector3>();var cols=new List<Color>();var tris=new List<int>();var tc=tm.colors;var sc=sm.colors;bool colors=tc.Length==tv.Length&&sc.Length==sv.Length;
    var mapT=new Dictionary<int,int>();var mapS=new Dictionary<int,int>();int kept=0,taken=0;
    int AddT(int i){if(!mapT.TryGetValue(i,out var n)){n=verts.Count;verts.Add(tv[i]);if(colors)cols.Add(tc[i]);mapT[i]=n;}return n;}
    int AddS(int i){if(!mapS.TryGetValue(i,out var n)){n=verts.Count;verts.Add(tt.transform.InverseTransformPoint(st.transform.TransformPoint(sv[i])));if(colors)cols.Add(sc[i]);mapS[i]=n;}return n;}
    for(int k=0;k<tt3.Length;k+=3){var c=tt.transform.TransformPoint((tv[tt3[k]]+tv[tt3[k+1]]+tv[tt3[k+2]])/3);if(InRegion(c))continue;tris.Add(AddT(tt3[k]));tris.Add(AddT(tt3[k+1]));tris.Add(AddT(tt3[k+2]));kept++;}
    for(int k=0;k<st3.Length;k+=3){var c=st.transform.TransformPoint((sv[st3[k]]+sv[st3[k+1]]+sv[st3[k+2]])/3);if(!InRegion(c))continue;tris.Add(AddS(st3[k]));tris.Add(AddS(st3[k+1]));tris.Add(AddS(st3[k+2]));taken++;}
    if(taken==0)continue;
    var roam=new Mesh{name=tm.name+" (Free Roam)",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};roam.SetVertices(verts);if(colors)roam.SetColors(cols);roam.SetTriangles(tris,0);roam.RecalculateNormals();roam.RecalculateBounds();
    Note($"Part B {target.name}: tile {tt.name}: Free Roam shape - {kept} of its own triangles kept, {taken} Backyard Reverse triangles in the tunnel region");
    if(!dry)roam=Store("freeroam-"+tt.name,roam);
    swapMF.Add(tt);swapMesh.Add(roam);
    var pg=new GameObject("probe "+tt.name);pg.transform.SetPositionAndRotation(tt.transform.position,tt.transform.rotation);pg.transform.localScale=tt.transform.lossyScale;pg.AddComponent<MeshCollider>().sharedMesh=roam;pg.layer=30;probes.Add(pg);}
   Physics.SyncTransforms();
   float Roam(Vector3 p){foreach(var g in probes){if(g.GetComponent<MeshCollider>().Raycast(new Ray(new Vector3(p.x,600,p.z),Vector3.down),out var h,1200))return h.point.y;}return float.NaN;}
   var tileMF=swapMF.ToList();
   float Race(Vector3 p){foreach(var tt in tileMF){if(tt.GetComponent<MeshCollider>().Raycast(new Ray(new Vector3(p.x,600,p.z),Vector3.down),out var h,1200))return h.point.y;}return float.NaN;}
   // ---- trees on changed ground: batched pieces re-grounded (Free Roam meshes), trunks follow; trees standing in the
   // tunnel's driving line (within 6 m of the Backyard branch line, outside the bore) are put out of sight in Free Roam ----
   var line3=sroot.GetComponentsInChildren<WoodlandRoute>(true).First(w=>w.title.StartsWith("Storm")).points;var line=line3.Select(p=>new Vector2(p.x,p.z)).ToArray();
   // a tree whose base is at the level of the line (not on the hill above the bore)
   bool OnLine(Vector3 w,float baseY){var p2=new Vector2(w.x,w.z);for(int k=0;k+1<line.Length;k++)if(SegDist(p2,line[k],line[k+1])<6&&Mathf.Abs(baseY-line3[k].y)<5)return true;return false;}
   var movedT=new List<Transform>();var movedP=new List<Vector3>();int treeMoves=0,hidden=0;
   var batched=target.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<MeshFilter>(true)).Where(m=>m.sharedMesh&&!m.GetComponent<Collider>()&&m.GetComponent<Renderer>()&&m.sharedMesh.isReadable&&(Path(m.transform).Contains("tree")||Path(m.transform).Contains("Tree")||Path(m.transform).Contains("canopy")||Path(m.transform).Contains("woodland")||Path(m.transform).Contains("Woods")||Path(m.transform).Contains("foliage")||Path(m.transform).Contains("Forest detail"))).Where(m=>{var b=m.GetComponent<Renderer>().bounds;return R.Overlaps(new Rect(b.min.x,b.min.z,b.size.x,b.size.z));}).ToList();
   var trunks=target.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>!c.isTrigger&&c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0&&c.bounds.size.x<3&&(InR(c.bounds.center)||OnLine(c.bounds.center,c.bounds.min.y))).ToList();
   foreach(var mf in batched){var m=mf.sharedMesh;var t=m.triangles;var par=Enumerable.Range(0,m.vertexCount).ToArray();int F(int a){while(par[a]!=a){par[a]=par[par[a]];a=par[a];}return a;}
    for(int i=0;i<t.Length;i+=3){int a=F(t[i]),b2=F(t[i+1]);par[b2]=a;int c=F(t[i+2]);par[c]=F(a);}
    var w=m.vertices.Select(v=>mf.transform.TransformPoint(v)).ToArray();var lv=m.vertices;bool any=false;
    foreach(var g in new HashSet<int>(t).GroupBy(F)){var vs=g.ToArray();var c=Vector3.zero;float mn=1e9f;foreach(int v in vs){c+=w[v];mn=Mathf.Min(mn,w[v].y);}c/=vs.Length;bool on=OnLine(c,mn);if(!InR(c)&&!on)continue;
     float a0=Race(c),a1=Roam(c);if(float.IsNaN(a0)&&!on)continue;float dy=on||float.IsNaN(a1)?-200:a1-a0;if(Mathf.Abs(dy)<.25f)continue;if(dy<-100)hidden++;
     foreach(int v in vs){var q=w[v];q.y+=dy;lv[v]=mf.transform.InverseTransformPoint(q);}any=true;treeMoves++;}
    if(any){var roam=Object.Instantiate(m);roam.name=m.name+" (Free Roam)";roam.vertices=lv;roam.RecalculateBounds();if(!dry)roam=Store("freeroam-trees-"+batched.IndexOf(mf),roam);swapMF.Add(mf);swapMesh.Add(roam);}}
   foreach(var c in trunks){var b=c.bounds;float a0=Race(b.center),a1=Roam(b.center);bool on=OnLine(b.center,b.min.y);if(float.IsNaN(a0)&&!on)continue;float dy=on||float.IsNaN(a1)?-200:a1-a0;if(Mathf.Abs(dy)<.25f)continue;movedT.Add(c.transform);movedP.Add(c.transform.position+Vector3.up*dy);}
   // other things standing on changed ground (props, fences, signs, rocks) follow it
   int props=0;
   foreach(var r in target.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Renderer>(true))){var b=r.bounds;if(!InR(b.center)||r.name.StartsWith("Ground")||b.size.x>12||b.size.z>12||swapMF.Any(s=>s.transform==r.transform)||trunks.Any(c=>c.transform==r.transform)||r is ParticleSystemRenderer)continue;
    float a0=Race(b.center),a1=Roam(b.center);if(float.IsNaN(a0)||b.min.y>a0+1.5f||b.min.y<a0-1.5f)continue;float dy=float.IsNaN(a1)?-200:a1-a0;if(Mathf.Abs(dy)<.25f)continue;movedT.Add(r.transform);movedP.Add(r.transform.position+Vector3.up*dy);props++;}
   foreach(var g in probes)Object.DestroyImmediate(g);
   Note($"Part B {target.name}: Free Roam terrain on {swapMF.Count(m=>rx.IsMatch(m.name))} tiles; {treeMoves} batched tree pieces re-grounded ({hidden} in the tunnel line or where the ground opens: put out of sight), {movedT.Count-props} trunks and {props} other objects follow the Free Roam ground");
   // the routes of this scene inside the area (must be none: then no race is affected; Free Roam only anyway)
   foreach(var road in Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==target))if(road.points!=null&&road.points.Any(p=>InR(p)))Note($"Part B {target.name}: route '{Full(road.transform)}' passes through the area ({road.points.Count(p=>InR(p))} points)");
   foreach(var wr in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==target))if(wr.points!=null&&wr.points.Any(p=>InR(p)))Note($"Part B {target.name}: branch '{wr.title}' passes through the area ({wr.points.Count(p=>InR(p))} points)");
   if(dry)return;
   // ---- the content ----
   var root=new GameObject(TunnelRoot);UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,target);var content=new GameObject("Tunnel content");content.transform.SetParent(root.transform,false);
   foreach(var t in pieces.Concat(atmPieces)){var c=Object.Instantiate(t.gameObject);c.name=t.name;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(c,target);c.transform.SetParent(content.transform,true);c.transform.SetPositionAndRotation(t.position,t.rotation);
    foreach(var comp in c.GetComponentsInChildren<Component>(true).Where(x=>x is WoodlandRoute||x is ReverseShortcutGuidance||x is JumpRecoveryExclusion||x is ReverseShortcutWorldState).ToArray())Object.DestroyImmediate(comp);}
   content.SetActive(false);
   var fro=root.AddComponent<FreeRoamOnly>();fro.content=content;fro.meshes=swapMF.ToArray();fro.roamMeshes=swapMesh.ToArray();fro.moved=movedT.ToArray();fro.roamPositions=movedP.ToArray();
   // swapped renderers cannot be statically batched (their mesh changes at run time)
   foreach(var mf in swapMF){var f=GameObjectUtility.GetStaticEditorFlags(mf.gameObject);GameObjectUtility.SetStaticEditorFlags(mf.gameObject,f&~StaticEditorFlags.BatchingStatic);}
   foreach(var t in movedT){var f=GameObjectUtility.GetStaticEditorFlags(t.gameObject);GameObjectUtility.SetStaticEditorFlags(t.gameObject,f&~StaticEditorFlags.BatchingStatic);}
   Note($"Part B {target.name}: '{TunnelRoot}' added ({content.GetComponentsInChildren<Renderer>(true).Length} renderers, {content.GetComponentsInChildren<Light>(true).Length} lights, {content.GetComponentsInChildren<UndergroundLife>(true).Length} rat encounter, {content.GetComponentsInChildren<ShallowWater>(true).Length} water volumes); shown only in Free Roam");
  }finally{EditorSceneManager.CloseScene(src,true);}
 }
 // Exploration map landmark at the tunnel's upper entrance (fast travel there in Free Roam, as for the other landmarks).
 static void TunnelLandmark(bool dry){var map=Object.FindAnyObjectByType<ExplorationMap>();if(!map){Note($"Part B {Scene}: no exploration map");return;}
  if(map.destinations.Any(d=>d.id=="storm-drain")){Note($"Part B {Scene}: landmark already present");return;}
  var list=map.destinations.ToList();var pos=new Vector3(182.5f,0,83.5f);if(Physics.Raycast(new Vector3(pos.x,300,pos.z),Vector3.down,out var h,600,~0,QueryTriggerInteraction.Ignore))pos.y=h.point.y+.2f;
  list.Add(new ExplorationMap.Destination{id="storm-drain",title="Storm drain tunnel",position=pos,yaw=239});Note($"Part B {Scene}: landmark 'Storm drain tunnel' at {pos:F1}");
  if(!dry){map.destinations=list.ToArray();EditorUtility.SetDirty(map);}}
}
