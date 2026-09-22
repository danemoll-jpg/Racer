using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class SimpleLaurelRamp {
 static readonly Vector3 Start=new Vector3(340,0,-190);
 static readonly Vector3 Forward=new Vector3(1,0,.65f).normalized;
 static readonly Vector3 Right=Vector3.Cross(Vector3.up,Forward);
 const float Begin=-8, Length=26, End=28, HalfWidth=4, Edge=10;
 struct V {public Vector3 p;public Color c;public V(Vector3 p,Color c){this.p=p;this.c=c;}}
 static float Coord(V v,bool along)=>Vector3.Dot(v.p-Start,along?Forward:Right);
 static List<V> Clip(List<V> poly,bool along,float line,bool greater){var result=new List<V>();for(int i=0;i<poly.Count;i++){var a=poly[i];var b=poly[(i+1)%poly.Count];float da=Coord(a,along)-line,db=Coord(b,along)-line;bool ia=greater?da>=0:da<=0,ib=greater?db>=0:db<=0;if(ia)result.Add(a);if(ia!=ib){float t=da/(da-db);result.Add(new V(Vector3.Lerp(a.p,b.p,t),Color.Lerp(a.c,b.c,t)));}}return result;}
 static float Smooth(float t){t=Mathf.Clamp01(t);return t*t*t*(t*(t*6-15)+10);}
 public static string Main(){
 if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Edit mode required");
 EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
 var rows=new List<string>();
 // Restore only terrain cut out beneath the recent shortcut, using the recorded pre-regression assets.
 foreach(var item in Newtonsoft.Json.Linq.JArray.Parse(File.ReadAllText("Docs/SurgicalTracks/baseline-meshes.json"))){var mf=GameObject.Find((string)item["name"]).GetComponent<MeshFilter>();if(!AssetDatabase.GetAssetPath(mf.sharedMesh).Contains("Surgical-shortcut-only"))continue;var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath((string)item["guid"]));if(!mesh)throw new Exception("Missing baseline");mf.sharedMesh=mesh;mf.GetComponent<MeshCollider>().sharedMesh=mesh;rows.Add("Restored "+mf.name+": "+AssetDatabase.GetAssetPath(mesh));}
 var old=GameObject.Find("Laurel separate shortcut ramp");if(old){if(old.GetComponentsInChildren<Racer.JumpRecoveryExclusion>(true).Length!=0)throw new Exception("Recovery component in geometry root");rows.Add("Removed "+old.GetComponentsInChildren<MeshFilter>().Length+" recent shortcut meshes (curved approach, raised terminal strip, narrow sides, arrows)");Object.DestroyImmediate(old);}
 Physics.SyncTransforms();var terrain=GameObject.Find("Ground_560_240").GetComponent<MeshFilter>();var collider=terrain.GetComponent<MeshCollider>();
 collider.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(AssetDatabase.GUIDToAssetPath("d5712e5358ab6234988ecc278e527e41"));terrain.sharedMesh=collider.sharedMesh;Physics.SyncTransforms();if(!collider.Raycast(new Ray(Start+Vector3.up*200,Vector3.down),out var hit,300))throw new Exception("Missing entry ground");float baseY=hit.point.y;
 var source=terrain.sharedMesh;var vs=source.vertices;var cs=source.colors;var ts=source.triangles;var output=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();var weld=new Dictionary<(int,int,int),int>();
 V Shape(V v){float s=Coord(v,true),t=Math.Abs(Coord(v,false));if(s<=Begin||s>=End||t>=Edge)return v;float u=Mathf.Min(s,Length);float target=baseY+.12f*u+.6f*u*u/(2*Length);float weight=Smooth((s-Begin)/5)*(1-Smooth((t-HalfWidth)/(Edge-HalfWidth)));if(s>Length)weight*=1-(s-Length)/(End-Length);v.p.y=Mathf.Lerp(v.p.y,target,weight);return v;}
 void Add(List<V> p,bool shape){if(p.Count<3)return;for(int k=1;k+1<p.Count;k++){var face=new[]{p[0],p[k],p[k+1]};if(Vector3.Cross(face[1].p-face[0].p,face[2].p-face[0].p).sqrMagnitude<1e-12f)continue;var ids=new int[3];for(int j=0;j<3;j++){var v=shape?Shape(face[j]):face[j];var local=terrain.transform.InverseTransformPoint(v.p);var key=(Mathf.RoundToInt(local.x*100000),Mathf.RoundToInt(local.y*100000),Mathf.RoundToInt(local.z*100000));if(!weld.TryGetValue(key,out int index)){index=output.Count;weld[key]=index;output.Add(local);colors.Add(v.c);}ids[j]=index;}if(ids.Distinct().Count()==3)indices.AddRange(ids);}}
 for(int i=0;i<ts.Length;i+=3){var p=new List<V>();for(int j=0;j<3;j++){int ix=ts[i+j];p.Add(new V(terrain.transform.TransformPoint(vs[ix]),cs.Length==vs.Length?cs[ix]:Color.white));}
 if(p.Max(v=>Coord(v,true))<=Begin||p.Min(v=>Coord(v,true))>=End||p.Max(v=>Coord(v,false))<=-Edge||p.Min(v=>Coord(v,false))>=Edge){Add(p,false);continue;}
 // Keep every triangle outside the small local rectangle on its original plane.
 foreach(var cut in new[]{(true,Begin,true),(true,End,false),(false,-Edge,true),(false,Edge,false)}){Add(Clip(p,cut.Item1,cut.Item2,!cut.Item3),false);p=Clip(p,cut.Item1,cut.Item2,cut.Item3);if(p.Count<3)break;}
 if(p.Count<3)continue;
 int a=Mathf.Max((int)(Begin*2),Mathf.FloorToInt(p.Min(v=>Coord(v,true))*2)),b=Mathf.Min((int)(End*2)-1,Mathf.FloorToInt(p.Max(v=>Coord(v,true))*2));
 for(int s=a;s<=b;s++){var strip=Clip(Clip(p,true,s*.5f,true),true,(s+1)*.5f,false);if(strip.Count<3)continue;int lo=Mathf.Max(-20,Mathf.FloorToInt(strip.Min(v=>Coord(v,false))*2)),hi=Mathf.Min(19,Mathf.FloorToInt(strip.Max(v=>Coord(v,false))*2));for(int t=lo;t<=hi;t++)Add(Clip(Clip(strip,false,t*.5f,true),false,(t+1)*.5f,false),true);}
 }
 var shaped=new Mesh{name="Laurel single straight ground ramp",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};shaped.SetVertices(output);shaped.SetColors(colors);shaped.SetTriangles(indices,0);shaped.RecalculateNormals();shaped.RecalculateBounds();const string path="Assets/Track/LocalRecovery/StreetLoopReverse-Laurel simple ground ramp.asset";var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(saved){EditorUtility.CopySerialized(shaped,saved);Object.DestroyImmediate(shaped);shaped=saved;}else AssetDatabase.CreateAsset(shaped,path);terrain.sharedMesh=shaped;collider.sharedMesh=shaped;
 // Route, gate, physics and recovery components are deliberately untouched.
 EditorSceneManager.MarkSceneDirty(terrain.gameObject.scene);EditorSceneManager.SaveScene(terrain.gameObject.scene);AssetDatabase.SaveAssets();Physics.SyncTransforms();
 rows.Add($"One straight ground ramp: start={Start}, axis={Forward}, baseY={baseY:F4}, length={Length}, usable width={HalfWidth*2}, side feather={Edge-HalfWidth}, rise={.12f*Length+.3f*Length:F3}, lip slope=.72; final 2m returns down the near cliff to existing terrain. No geometry across the chasm.");
 File.AppendAllLines("Docs/SimpleLaurel/implementation.txt",rows);return string.Join("\n",rows);
 }
}


