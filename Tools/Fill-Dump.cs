using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class FillDump
{
 const string Folder="Assets/Track/DumpRefuse", Evidence="Docs/DumpRefuse";
 static readonly Vector3 Origin=new(309.2f,0,15.7f),Axis=new Vector3(-50.6f,0,-7.5f).normalized;
 static Vector3 Side=>Vector3.Cross(Vector3.up,Axis);
 static RaycastHit Ground(Vector3 p)=>Physics.RaycastAll(new Vector3(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First();
 public static string Main()
 {
  if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
  if(Directory.Exists(Folder))throw new Exception("Already authored; inspect before rerunning");
  var scene=EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");Physics.SyncTransforms();
  Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
  Object.DestroyImmediate(GameObject.Find("Old dump - scattered salvage - no collision"));
  var root=new GameObject("Old dump - dense push-through refuse");root.transform.SetParent(GameObject.Find("Dan's Backyard Forward - terrain trail").transform);root.AddComponent<Racer.DumpRefuse>();
  var materials=new List<Material>{
   AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/BackyardForward/Old rust.mat"),
   AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/DumpCorrection/Weathered boards.mat"),
   AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/DumpCorrection/Dull discarded metal.mat"),
   AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/DumpCorrection/Faded container enamel.mat"),
   AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/CombinedReview/Tire black.mat")};
  foreach(var pair in new[]{("Dirty paper and plastic",new Color(.60f,.58f,.47f)),("Wet cardboard",new Color(.36f,.26f,.14f)),("Old blue plastic",new Color(.13f,.24f,.30f))}){var m=new Material(materials[2]){name=pair.Item1,color=pair.Item2};AssetDatabase.CreateAsset(m,Folder+"/"+pair.Item1+".mat");materials.Add(m);}
  var batches=materials.Select(_=>new List<CombineInstance>()).ToArray();
  var primitives=new Dictionary<PrimitiveType,Mesh>();foreach(var type in new[]{PrimitiveType.Cube,PrimitiveType.Cylinder,PrimitiveType.Sphere}){var go=GameObject.CreatePrimitive(type);primitives[type]=go.GetComponent<MeshFilter>().sharedMesh;Object.DestroyImmediate(go);}
  // An inexpensive irregular, faceted refuse shape; reused for bags and broken objects.
  var lump=new Mesh();var lv=new List<Vector3>{new(0,-.45f,0),new(.09f,.55f,-.08f)};for(int j=0;j<8;j++){float a=j*Mathf.PI/4;lv.Add(new Vector3(Mathf.Cos(a)*(.43f+(j%3)*.04f),j%2==0?-.08f:.05f,Mathf.Sin(a)*.48f));}var lt=new List<int>();for(int j=0;j<8;j++){int a=j+2,b=(j+1)%8+2;lt.AddRange(new[]{0,b,a,1,a,b});}lump.SetVertices(lv);lump.SetTriangles(lt,0);lump.RecalculateNormals();
  var random=new System.Random(40000);float R(float a,float b)=>Mathf.Lerp(a,b,(float)random.NextDouble());
  var centers=new List<Vector2>();for(int i=0;i<19;i++){float angle=R(0,Mathf.PI*2),rad=R(.05f,.62f);centers.Add(new Vector2(25.5f+Mathf.Cos(angle)*24.5f*rad,Mathf.Sin(angle)*23*rad));}
  var rows=new List<string>{"piece,kind,x,y,z,groundContactError"};float maxError=0;int triangles=0;
  for(int i=0;i<2100;i++)
  {
   Vector2 at;
   do {if(i<1150){float angle=R(0,Mathf.PI*2),rad=Mathf.Sqrt(R(0,1))*.81f;at=new Vector2(25.5f+Mathf.Cos(angle)*24.5f*rad,Mathf.Sin(angle)*23*rad);}else{var c=centers[random.Next(centers.Count)];at=c+new Vector2(R(-4.3f,4.3f),R(-4.3f,4.3f));}}while(Mathf.Pow((at.x-25.5f)/24.5f,2)+Mathf.Pow(at.y/23,2)>.81f*.81f);
   var p=Origin+Axis*at.x+Side*at.y;var hit=Ground(p);p.y=hit.point.y;
   int kind=random.Next(8),mat;Mesh mesh;Vector3 size;Quaternion rotation=Quaternion.FromToRotation(Vector3.up,hit.normal)*Quaternion.Euler(R(-9,9),R(0,360),R(-9,9));
   switch(kind){
    case 0:mesh=primitives[PrimitiveType.Cube];mat=1;size=new(R(1.4f,3.1f),R(.08f,.19f),R(.2f,.5f));break;
    case 1:mesh=primitives[PrimitiveType.Cylinder];mat=0;size=new(R(.55f,.95f),R(.35f,.65f),R(.55f,.95f));rotation*=Quaternion.Euler(85,0,0);break;
    case 2:mesh=primitives[PrimitiveType.Cube];mat=random.Next(3,5)==3?3:7;size=new(R(.55f,1.25f),R(.35f,.9f),R(.6f,1.2f));break;
    case 3:mesh=primitives[PrimitiveType.Cube];mat=2;size=new(R(1,2),.045f,R(.5f,1.3f));break;
    case 4:mesh=primitives[PrimitiveType.Cylinder];mat=4;size=new(R(.6f,1.0f),.13f,R(.6f,1));break;
    case 5:mesh=lump;mat=5;size=new(R(.8f,1.8f),R(.25f,.65f),R(.7f,1.6f));break;
    case 6:mesh=lump;mat=6;size=new(R(.9f,1.9f),R(.3f,1.1f),R(.7f,1.7f));break;
    default:mesh=lump;mat=4;size=new(R(.8f,1.7f),R(.5f,1.25f),R(.7f,1.6f));break;
   }
   var matrix=Matrix4x4.TRS(p,rotation,size);float lift=float.NegativeInfinity;
   // Seat every piece on the unchanged terrain, including tilted sheet/board corners.
   foreach(var v in mesh.vertices){var w=matrix.MultiplyPoint3x4(v);lift=Math.Max(lift,Ground(w).point.y-w.y);}
   p.y+=lift-.035f;matrix=Matrix4x4.TRS(p,rotation,size);float contact=float.PositiveInfinity;foreach(var v in mesh.vertices){var w=matrix.MultiplyPoint3x4(v);contact=Math.Min(contact,w.y-Ground(w).point.y);}maxError=Math.Max(maxError,Math.Abs(contact+.035f));
   batches[mat].Add(new CombineInstance{mesh=mesh,transform=matrix});triangles+=mesh.triangles.Length/3;rows.Add($"{i},{kind},{p.x:F3},{p.y:F3},{p.z:F3},{contact:F5}");
  }
  for(int i=0;i<batches.Length;i++){var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32,name="Accumulated refuse "+materials[i].name};mesh.CombineMeshes(batches[i].ToArray(),true,true);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/Refuse-"+i+".asset");var go=new GameObject(mesh.name);go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=materials[i];}
  Object.DestroyImmediate(lump);PlayerSettings.bundleVersion="0.40.0-review1";EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
  File.WriteAllLines(Evidence+"/pieces.csv",rows);string report=$"2100 overlapping grounded pieces; 19 irregular concentrations; {batches.Length} renderers; {triangles} triangles; 0 debris colliders/rigidbodies; maximum contact error={maxError:F6}m. Existing terrain, trees, route and jump unchanged.";File.WriteAllText(Evidence+"/implementation.txt",report);return report;
 }
}
