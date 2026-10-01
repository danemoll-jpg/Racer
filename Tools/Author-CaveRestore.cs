using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class AuthorCaveRestore {
 const string D="Docs/CaveRestore";
 [Serializable]public class History{public string source;public Rock[] rocks;}
 [Serializable]public class Rock{public string name;public Vector3 position,scale;public Quaternion rotation;}
 [Serializable]public class Line{public Vector3[] points;}
 public static string Main(){var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(Application.isPlaying||scene.isDirty||scene.name!="LakeWoods")throw new Exception("Saved Forest Forward edit mode required");
 var history=Newtonsoft.Json.JsonConvert.DeserializeObject<History>(File.ReadAllText(D+"/history.json"));var rows=new List<string>();
 foreach(var rock in history.rocks){var t=GameObject.Find(rock.name).transform;t.localPosition=rock.position;t.localRotation=rock.rotation;t.localScale=rock.scale;var mf=t.GetComponent<MeshFilter>();if(t.GetComponent<MeshCollider>().sharedMesh!=mf.sharedMesh)throw new Exception("Historical collider mismatch");rows.Add("RESTORED "+rock.name+" exact original transform and mesh/collider from "+history.source);}
 // Reference the original geometric centreline, not the retained right-shifted AI line.
 var temp=new GameObject("Temporary cave geometry reference");var b=temp.AddComponent<WoodlandRoute>();b.points=Newtonsoft.Json.JsonConvert.DeserializeObject<Line>(File.ReadAllText("Docs/FinalTwo/original-cave-line.json")).points;
 var rng=new System.Random(60000);float Rand(float a,float z)=>Mathf.Lerp(a,z,(float)rng.NextDouble());
 var ledges=Object.FindObjectsByType<Transform>().Where(t=>t.name=="Angular cave wall ledge").OrderBy(t=>b.Project(t.position,out _)).ThenBy(t=>t.position.x).ToArray();
 var meshes=history.rocks.Select(r=>GameObject.Find(r.name).GetComponent<MeshFilter>().sharedMesh).ToArray();int removed=0,count=0;
 foreach(var t in ledges){float s=b.Project(t.position,out _);var p=b.At(s,out var f);var side=Vector3.Cross(Vector3.up,Vector3.ProjectOnPlane(f,Vector3.up).normalized);float sign=Mathf.Sign(Vector3.Dot(t.position-p,side));
 // Keep the restored opening and the existing jump/landing envelope free of new hazards.
 if((s>169&&s<255)||(s>282&&s<390)||rng.NextDouble()<.48){Object.DestroyImmediate(t.gameObject);removed++;continue;}
 s=Mathf.Clamp(s+Rand(-2.1f,2.1f),65,473);p=b.At(s,out f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;side=Vector3.Cross(Vector3.up,f);
 t.name="Grounded cave edge boulder "+count;var mesh=meshes[count%meshes.Length];t.GetComponent<MeshFilter>().sharedMesh=mesh;
 float width=Rand(1.0f,1.85f),height=Rand(.85f,1.4f),depth=Rand(1.2f,2.7f);t.localScale=new(width/mesh.bounds.size.x,height/mesh.bounds.size.y,depth/mesh.bounds.size.z);t.rotation=Quaternion.LookRotation(f)*Quaternion.Euler(0,Rand(-24,24),0);t.position=p+side*(Rand(4.25f,4.8f)*sign);
 Physics.SyncTransforms();var r=t.GetComponent<Renderer>();var hits=Physics.RaycastAll(t.position+Vector3.up*2,Vector3.down,35,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).ToArray();if(hits.Length==0)throw new Exception("No floor under "+t.name);t.position+=Vector3.up*(hits[0].point.y-.16f-r.bounds.min.y);
 var col=t.GetComponent<MeshCollider>();if(!col)col=t.gameObject.AddComponent<MeshCollider>();col.sharedMesh=mesh;col.convex=false;col.isTrigger=false;rows.Add($"EDGE {t.name} station={s:F2} side={sign} size={width:F2},{height:F2},{depth:F2} exact visible mesh collision");count++;}
 // Existing fallen stones are tiny clutter, ground each mesh and cap actual visible height.
 int rubble=0;foreach(var t in Object.FindObjectsByType<Transform>().Where(t=>t.name=="Fallen cave stone").ToArray()){var r=t.GetComponent<Renderer>();if(r.bounds.size.y>.22f)t.localScale=Vector3.Scale(t.localScale,new Vector3(1,.22f/r.bounds.size.y,1));var hits=Physics.RaycastAll(t.position+Vector3.up*2,Vector3.down,35,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).ToArray();if(hits.Length>0)t.position+=Vector3.up*(hits[0].point.y-.04f-r.bounds.min.y);rubble++;}
 Object.DestroyImmediate(temp);Physics.SyncTransforms();rows.Add($"Recomposed {ledges.Length} wall ledges into {count} irregular colliding grounded edge boulders; removed {removed} intrusive/repeated pieces; {rubble} tiny rubble meshes grounded. Shell/ceiling/hillside/lighting/floor/route/AI/recovery unchanged.");
 PlayerSettings.bundleVersion="0.60.0-review1";EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();File.WriteAllLines(D+"/authoring.txt",rows);return rows.Last();}
}


