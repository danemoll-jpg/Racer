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
// Local visual refresh: existing collider transforms are the tree authoring data.
// Never invokes terrain, building, road, shortcut or jump generation.
public static class Phase6Vegetation {
 const string Folder="Assets/Vegetation/Phase6";
 public static bool Reserved(Vector3 p,List<Vector3> road,List<Vector3> shortcut)=>
  Phase6Buildings.YardDistance(p)<28 || StreetLoopBuilder.Nearest(p,road,out _)<StreetLoopRevision.ForestClearance || Phase5Setup.Distance(p,shortcut,out _)<10 || CR014Woodland.SiteReserved(p);
 static Mesh Save(Mesh mesh,string name){string path=Folder+"/"+name+".asset";mesh.name=name;var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){old.Clear();old.indexFormat=mesh.indexFormat;old.vertices=mesh.vertices;old.triangles=mesh.triangles;old.normals=mesh.normals;old.colors=mesh.colors;old.RecalculateBounds();old.UploadMeshData(false);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(old);return old;}AssetDatabase.CreateAsset(mesh,path);return mesh;}
 // Indexed low-poly crowns: share ring vertices to keep forest vertex bandwidth low.
 static Mesh Crown(int variant){
  var v=new List<Vector3>();var t=new List<int>();var c=new List<Color>();
  int sides=6;float[] radius=variant==2?new[]{.35f,.88f,.25f}:new[]{.50f,1f,.45f};
  for(int ring=0;ring<3;ring++)for(int s=0;s<sides;s++){
   float a=s*Mathf.PI*2/sides;float r=radius[ring]*(1+.07f*Mathf.Sin(s*3+variant+ring));
   v.Add(new Vector3(Mathf.Cos(a)*r*.92f+(ring-1)*.035f,ring*.5f,Mathf.Sin(a)*r*.86f));
   c.Add(new Color(.97f,.97f,.97f,1));
  }
  for(int ring=0;ring<2;ring++)for(int s=0;s<sides;s++){int a=ring*sides+s,b=ring*sides+(s+1)%sides;t.AddRange(new[]{a,a+sides,b,b,a+sides,b+sides});}
  v.Add(new Vector3(0,-.04f,0));v.Add(new Vector3(.03f,1.04f,0));c.Add(Color.white);c.Add(Color.white);
  for(int s=0;s<sides;s++)t.AddRange(new[]{18,s,(s+1)%sides,19,12+(s+1)%sides,12+s});
  if(variant==1){var source=v.ToArray();var colors=c.ToArray();var indices=t.ToArray();v.Clear();c.Clear();t.Clear();
   foreach(var lobe in new[]{new Vector4(0,.18f,0,.73f),new Vector4(-.40f,0,.10f,.52f),new Vector4(.38f,.05f,-.10f,.54f)}){
    int offset=v.Count;for(int i=0;i<source.Length;i++){var p=source[i];v.Add(new Vector3(p.x*lobe.w+lobe.x,p.y*lobe.w+lobe.y,p.z*lobe.w+lobe.z));c.Add(colors[i]);}foreach(int i in indices)t.Add(i+offset);
   }
  }
  var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetColors(c);mesh.RecalculateNormals();mesh.RecalculateBounds();return Save(mesh,new[]{"BroadCrown","IrregularCrown","UprightCrown"}[variant]);
 }
 [MenuItem("Racer/Refresh Phase 6 Vegetation Visuals")]
 public static void Refresh()=>Refresh("Docs/VEGETATION_BUILD.txt");
 public static void Refresh(string report){
  var scene=SceneManager.GetActiveScene();if(Application.isPlaying||scene.isDirty||scene.path!=StreetLoopBuilder.ScenePath)throw new InvalidOperationException("Open saved StreetLoopGreybox outside Play mode.");
  var woods=GameObject.Find("Woods replacing later subdivisions").transform;var boxes=woods.GetComponentsInChildren<BoxCollider>();
  if(boxes.Length==0)throw new InvalidOperationException("No authoring trunks; refusing to invent forest placement.");
  Directory.CreateDirectory(Folder);AssetDatabase.Refresh();var shapes=new[]{Crown(0),Crown(1),Crown(2)};
  var primitive=GameObject.CreatePrimitive(PrimitiveType.Cube);var trunk=Object.Instantiate(primitive.GetComponent<MeshFilter>().sharedMesh);Object.DestroyImmediate(primitive);trunk.colors=Enumerable.Repeat(Color.white,trunk.vertexCount).ToArray();trunk=Save(trunk,"Trunk");
  var mat=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Forest.mat");if(!mat){mat=new Material(Shader.Find("Racer/GreyboxGround")){name="Forest muted vertex colors",enableInstancing=true};AssetDatabase.CreateAsset(mat,Folder+"/Forest.mat");}
  var road=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();var sites=GameObject.Find(Phase6Review.Root).transform.Cast<Transform>().ToArray();
  var batches=new Dictionary<Vector2Int,List<CombineInstance>>();var temporary=new List<Mesh>();int[] counts=new int[3];float minRadius=999,maxRadius=0;
  foreach(var box in boxes){
   var bounds=box.bounds;var p=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);float h=bounds.size.y*2;
   float patch=Mathf.PerlinNoise((p.x+913)/65,(p.z+771)/65);float hash=Mathf.Repeat(Mathf.Sin(p.x*12.9898f+p.z*78.233f)*43758.5453f,1);
   int kind=patch<.43f?2:patch>.57f?1:0;counts[kind]++;
   float radius=h*Mathf.Lerp(.27f,.34f,hash);
   // CR-014 mature woodland crowns overlap above generous trunk gaps.
   // Existing accepted trunk placements and their visual dimensions remain exact.
   if(box.name.StartsWith("CR014 trunk "))radius*=1.35f;
   radius=Mathf.Min(radius,Mathf.Max(.1f,Phase6Buildings.YardDistance(p)-27),StreetLoopBuilder.Nearest(p,road,out _)-16,Phase5Setup.Distance(p,cut,out _)-7.6f);
   foreach(var site in sites){var delta=site.position-p;delta.y=0;radius=Mathf.Min(radius,Mathf.Max(.1f,delta.magnitude-16));}
   minRadius=Mathf.Min(minRadius,radius);maxRadius=Mathf.Max(maxRadius,radius);
   var key=new Vector2Int(Mathf.FloorToInt(p.x/160),Mathf.FloorToInt(p.z/160));if(!batches.TryGetValue(key,out var batch))batches[key]=batch=new();
   Color tint=Color.Lerp(new Color(.24f,.34f,.19f),new Color(.39f,.46f,.25f),Mathf.Clamp01(patch*.8f+hash*.2f));
   var crown=Object.Instantiate(shapes[kind]);crown.colors=crown.colors.Select(col=>col*tint).ToArray();temporary.Add(crown);
   float height=h*Mathf.Lerp(.59f,.70f,hash);batch.Add(new CombineInstance{mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.36f,Quaternion.Euler(0,hash*360,0),new Vector3(radius,height,radius))});
   var bark=Object.Instantiate(trunk);bark.colors=Enumerable.Repeat(new Color(.25f,.20f,.145f),bark.vertexCount).ToArray();temporary.Add(bark);
   batch.Add(new CombineInstance{mesh=bark,transform=box.transform.localToWorldMatrix*Matrix4x4.TRS(box.center,Quaternion.identity,box.size)});
  }
  // Replace visible meshes only, never the authored trunks or accepted local objects.
  foreach(var filter in woods.GetComponentsInChildren<MeshFilter>())Object.DestroyImmediate(filter.gameObject);
  foreach(var pair in batches){var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);mesh=Save(mesh,$"Forest_{pair.Key.x}_{pair.Key.y}");var go=new GameObject("Faceted forest batch",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(woods,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=mat;}
  foreach(var mesh in temporary)Object.DestroyImmediate(mesh);
  AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
  File.WriteAllText(report,$"Authored trees {boxes.Length}; crown variants broad/irregular/upright {string.Join("/",counts)}. {batches.Count} spatial 160m combined batches, one shared vertex-color material. Crown radius {minRadius:F2}..{maxRadius:F2}m.\nTrunk vertices exactly match box-collider transforms. No leaf/ground collision or understory. Crown radius limits: yard 27m capsule, road 16m, shortcut 7.6m, building sites 16m.\nPatch-coherent crown variants/color, deterministic individual rotation/height/width variation. No whole-environment rebuild. Three reusable crown meshes + trunk; no duplicate visible sources. Refresh is repeatable from authored colliders.\n");
 }
}
}
