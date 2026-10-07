using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using UnityEditor;using UnityEngine;using Object=UnityEngine.Object;
// 0.87 Part A: Kyle's house model, from Tools/Blender/kyles_house.py's FBX (objects '<group>__<slot>'). The FBX is imported
// readable without materials; its parts are joined into one mesh in the house frame (x right = the photo's left, y up,
// z forward = the front; origin at the site) with each slot's colour in the vertex colours, for the 0.78 Racer/Building
// shader: alpha below 1 marks glass and lamps, which glow at night. Blender's FBX arrives turned half a turn about y; that
// is detected (the garage doors must be at +x) and undone. Also one convex mesh per collision prism of
// KylesHouse-colliders.txt. Writes Assets/Scenery/KylesHouse/KylesHouse-0.87.asset and KylesHouse-collider-<n>.asset.
public static class Report087KyleModel {
 public const string Dir="Assets/Scenery/KylesHouse";
 static readonly Dictionary<string,Color> Slots=new(){
  {"siding",new(.60f,.62f,.62f)},{"trimw",new(.93f,.93f,.90f)},{"roof",new(.30f,.32f,.34f)},{"brick",new(.53f,.30f,.23f)},{"shutter",new(.17f,.18f,.20f)},
  {"doorw",new(.91f,.91f,.89f)},{"glass",new(.13f,.17f,.21f,.35f)},{"glassdark",new(.11f,.13f,.15f)},{"concrete",new(.62f,.60f,.57f)},{"deck",new(.47f,.35f,.23f)},
  {"deckdark",new(.30f,.22f,.14f)},{"screen",new(.18f,.20f,.20f)},{"garage",new(.89f,.89f,.86f)},{"metal",new(.45f,.47f,.48f)},{"lamp",new(.95f,.91f,.70f,.15f)},
  {"wicker",new(.92f,.91f,.86f)},{"cushion",new(.56f,.56f,.51f)},{"gutter",new(.95f,.95f,.93f)}};
 public static Mesh Build(StringBuilder rep){
  string fbx=Dir+"/KylesHouse.fbx";var imp=(ModelImporter)AssetImporter.GetAtPath(fbx);if(imp==null)throw new Exception("no "+fbx);
  bool dirty=false;if(!imp.isReadable){imp.isReadable=true;dirty=true;}if(imp.materialImportMode!=ModelImporterMaterialImportMode.None){imp.materialImportMode=ModelImporterMaterialImportMode.None;dirty=true;}
  if(imp.importAnimation){imp.importAnimation=false;dirty=true;}if(dirty)imp.SaveAndReimport();
  var root=AssetDatabase.LoadAssetAtPath<GameObject>(fbx);var parts=root.GetComponentsInChildren<MeshFilter>(true);
  var v=new List<Vector3>();var n=new List<Vector3>();var c=new List<Color>();var t=new List<int>();var unknown=new HashSet<string>();Vector3 garage=Vector3.zero;int gc=0;
  foreach(var mf in parts){var m=mf.sharedMesh;var slot=mf.name.Contains("__")?mf.name.Substring(mf.name.IndexOf("__")+2):mf.name;if(!Slots.TryGetValue(slot,out var col)){unknown.Add(slot);col=Color.magenta;}
   var tr=mf.transform;var mx=root.transform.worldToLocalMatrix*tr.localToWorldMatrix;int b=v.Count;var mv=m.vertices;var mn=m.normals;
   for(int i=0;i<mv.Length;i++){var p=mx.MultiplyPoint3x4(mv[i]);v.Add(p);n.Add(mx.MultiplyVector(mn.Length==mv.Length?mn[i]:Vector3.up).normalized);c.Add(col);if(slot=="garage"){garage+=p;gc++;}}
   foreach(var i in m.triangles)t.Add(b+i);}
  if(unknown.Count>0)rep.AppendLine("model: slots without a colour: "+string.Join(",",unknown));
  garage/=Mathf.Max(1,gc);bool turned=garage.x<0;
  if(turned){for(int i=0;i<v.Count;i++){v[i]=new Vector3(-v[i].x,v[i].y,-v[i].z);n[i]=new Vector3(-n[i].x,n[i].y,-n[i].z);}}
  var mesh=new Mesh{name="Kyle's house (0.87)",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.RecalculateBounds();
  var path=Dir+"/KylesHouse-0.87.asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior){EditorUtility.CopySerialized(mesh,prior);mesh=prior;}else AssetDatabase.CreateAsset(mesh,path);
  rep.AppendLine($"model: {parts.Length} parts from {fbx}, {v.Count} vertices, {t.Count/3} triangles; FBX turned half a turn: {turned} (undone); bounds {mesh.bounds.min} - {mesh.bounds.max} -> {path}");
  return mesh;}
 // collision shapes: ("box", name, centre, size) or ("prism", name, mesh)
 public static List<(string kind,string name,Vector3 c,Vector3 s,Mesh m)> Colliders(StringBuilder rep){var list=new List<(string,string,Vector3,Vector3,Mesh)>();int k=0;var inv=System.Globalization.CultureInfo.InvariantCulture;
  foreach(var line in File.ReadAllLines(Dir+"/KylesHouse-colliders.txt")){if(line.StartsWith("#")||line.Trim().Length==0)continue;var p=line.Split('\t');var f=p[2].Split(' ').Select(x=>float.Parse(x,inv)).ToArray();
   if(p[0]=="box")list.Add(("box",p[1],new Vector3(f[0],f[1],f[2]),new Vector3(f[3],f[4],f[5]),null));
   else{var pts=new List<Vector3>();for(int i=0;i+2<f.Length;i+=3)pts.Add(new Vector3(f[i],f[i+1],f[i+2]));
    // a convex hull of a few points: every triangle whose plane has all points on one side
    var tri=new List<int>();for(int a=0;a<pts.Count;a++)for(int b=a+1;b<pts.Count;b++)for(int d=b+1;d<pts.Count;d++){var nn=Vector3.Cross(pts[b]-pts[a],pts[d]-pts[a]);if(nn.sqrMagnitude<1e-8f)continue;int pos=0,neg=0;for(int e=0;e<pts.Count;e++){float s=Vector3.Dot(pts[e]-pts[a],nn);if(s>1e-4f)pos++;else if(s<-1e-4f)neg++;}if(pos>0&&neg>0)continue;if(pos>0)tri.AddRange(new[]{a,d,b});else tri.AddRange(new[]{a,b,d});}
    var m=new Mesh{name="Kyle's house "+p[1]};m.SetVertices(pts);m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateBounds();var path=$"{Dir}/KylesHouse-collider-{k++}.asset";var prior=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(prior){EditorUtility.CopySerialized(m,prior);m=prior;}else AssetDatabase.CreateAsset(m,path);
    list.Add(("prism",p[1],Vector3.zero,Vector3.zero,m));}}
  rep.AppendLine($"collision shapes: {list.Count} ({list.Count(x=>x.Item1=="box")} boxes, {list.Count(x=>x.Item1=="prism")} prisms)");return list;}
}
