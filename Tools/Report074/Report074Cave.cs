using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
// 0.74 Part C: one cave everywhere - the accepted Forest Loop Forward (LakeWoods) Echo Cave. In a target scene the old cave
// pieces under "CR056 Forest Loop" (the pre-0.57 enclosure, rock buttresses, warm wayfinding lights) are removed and the
// LakeWoods cave is copied in at the same world positions: enclosure (the 0.57 flight chamber), the partial rockfall, edge
// boulders, fallen stones, gravel floor and patches, puddles, ceiling roots, hanging formations, the quiet cave water drips,
// and the 0.59 hillside cover (Ground_Echo wooded hillside, Echo eroded hillside faces). Race routes are not copied (in
// LakeWoods the cave is an optional shortcut; elsewhere it is scenery). The terrain under the cave is compared first.
public static partial class Report074Author {
 static readonly string[] CavePieces={"Echo Cave enclosed rock","Echo Cave partial rockfall","Grounded cave edge boulder","Fallen cave stone","Cave gravel floor patch","Cave natural gravel floor","Shallow cave puddle","Exposed ceiling root","Hanging angular cave formation","Quiet natural cave water"};
 static readonly string[] OldCavePieces={"Echo Cave enclosed rock","Cave rock buttress","Cave warm wayfinding"};
 static readonly string[] HillsidePieces={"Ground_Echo wooded hillside","Echo eroded hillside face"};
 static bool Is(string name,string[] set)=>set.Any(p=>name==p||name.StartsWith(p+" ")||name.StartsWith(p+"(")||System.Text.RegularExpressions.Regex.IsMatch(name,"^"+System.Text.RegularExpressions.Regex.Escape(p)+@"\s*\d+$"));
 static void PartCCave(bool dry){
  var target=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
  if(target.name=="LakeWoods"){Note("Part C LakeWoods: the source (accepted) cave; not changed");return;}
  var troot=target.GetRootGameObjects().FirstOrDefault(g=>g.name=="CR056 Forest Loop");
  if(!troot){Note($"Part C {target.name}: no cave in this scene; not changed");return;}
  var src=EditorSceneManager.OpenScene("Assets/Scenes/LakeWoods.unity",OpenSceneMode.Additive);
  try{
   var sroot=src.GetRootGameObjects().First(g=>g.name=="CR056 Forest Loop");
   var copyChildren=sroot.transform.Cast<Transform>().Where(t=>Is(t.name,CavePieces)).ToList();
   var copyRoots=src.GetRootGameObjects().Where(g=>Is(g.name,HillsidePieces)).Select(g=>g.transform).ToList();
   var oldChildren=troot.transform.Cast<Transform>().Where(t=>Is(t.name,OldCavePieces)||Is(t.name,CavePieces)).ToList();
   var oldRoots=target.GetRootGameObjects().Where(g=>Is(g.name,HillsidePieces)).Select(g=>g.transform).ToList();
   string Count(IEnumerable<Transform> ts)=>string.Join(", ",ts.GroupBy(t=>System.Text.RegularExpressions.Regex.Replace(t.name,@"\s*\d+$","")).Select(g=>$"{g.Key} x{g.Count()}"));
   Note($"Part C {target.name}: remove {oldChildren.Count+oldRoots.Count} old cave pieces ({Count(oldChildren.Concat(oldRoots))})");
   Note($"Part C {target.name}: copy {copyChildren.Count+copyRoots.Count} LakeWoods cave pieces ({Count(copyChildren.Concat(copyRoots))})");
   // routes passing under the cave roof in the target scene (a scene's own race route through the cave is left alone)
   var roof=sroot.transform.Cast<Transform>().First(t=>t.name=="Echo Cave enclosed rock").GetComponent<MeshFilter>();var roofMesh=roof.sharedMesh;
   var tmp=new GameObject("roof probe");tmp.transform.SetPositionAndRotation(roof.transform.position,roof.transform.rotation);tmp.transform.localScale=roof.transform.lossyScale;var mc=tmp.AddComponent<MeshCollider>();mc.sharedMesh=roofMesh;Physics.SyncTransforms();
   foreach(var r in Object.FindObjectsByType<RaceRoad>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==target)){int n=r.points?.Count(p=>mc.Raycast(new Ray(p+Vector3.up*.5f,Vector3.up),out _,40))??0;if(n>0)Note($"Part C {target.name}: race road '{Full(r.transform)}' has {n} points under the cave roof");}
   foreach(var w in Object.FindObjectsByType<WoodlandRoute>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(x=>x.gameObject.scene==target)){int n=w.points?.Count(p=>mc.Raycast(new Ray(p+Vector3.up*.5f,Vector3.up),out _,40))??0;if(n>0)Note($"Part C {target.name}: branch '{w.title}' has {n} points under the cave roof");}
   Object.DestroyImmediate(tmp);
   if(Environment.GetEnvironmentVariable("CAVE_SKIP")?.Split(',').Contains(target.name)==true){Note($"Part C {target.name}: skipped (CAVE_SKIP)");return;}
   if(dry)return;
   foreach(var t in oldChildren.Concat(oldRoots))Object.DestroyImmediate(t.gameObject);
   int made=0;
   foreach(var t in copyChildren){var c=Object.Instantiate(t.gameObject);c.name=t.name;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(c,target);c.transform.SetParent(troot.transform,true);c.transform.SetPositionAndRotation(t.position,t.rotation);made++;}
   foreach(var t in copyRoots){var c=Object.Instantiate(t.gameObject);c.name=t.name;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(c,target);c.transform.SetPositionAndRotation(t.position,t.rotation);made++;}
   Physics.SyncTransforms();Note($"Part C {target.name}: {made} pieces copied");
  }finally{EditorSceneManager.CloseScene(src,true);}
 }
 static string Full(Transform t){var n=t.name;while(t.parent){t=t.parent;n=t.name+"/"+n;}return n;}
}
