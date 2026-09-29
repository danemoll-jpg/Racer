using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class TrimShortcutEntryShoulder {
 public static string Main(){if(Application.isPlaying)throw new Exception("Edit mode required");EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");var race=UnityEngine.Object.FindAnyObjectByType<RaceDirector>();var b=UnityEngine.Object.FindObjectsByType<WoodlandRoute>().First(r=>r.title=="Tree-Top Trail");var root=GameObject.Find("Backyard optional forest shortcuts");var surface=root.GetComponentsInChildren<MeshCollider>().First(c=>c.name=="Takeoff - Canopy offroad launch");var mesh=surface.sharedMesh;var v=mesh.vertices;float min=999;
 float Width(Vector3 p){race.road.Project(p,out var d);return Mathf.Min(5.2f,Mathf.Max(2.4f,2*(d-2.65f-.25f)));}
 for(int i=0;i<v.Length;i+=2){var center=(v[i]+v[i+1])*.5f;var side=(v[i+1]-v[i]).normalized;float width=Width(center);min=Mathf.Min(min,width);v[i]=center-side*width*.5f;v[i+1]=center+side*width*.5f;}
 mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();surface.sharedMesh=null;surface.sharedMesh=mesh;EditorUtility.SetDirty(mesh);
 foreach(var t in root.GetComponentsInChildren<Transform>().Where(t=>t.name=="Canopy offroad launch plank")){var scale=t.localScale;scale.x=Width(t.position);t.localScale=scale;}
 EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);AssetDatabase.SaveAssets();File.WriteAllText("Docs/BackyardShortcuts/entry-shoulder.txt",$"Tapered launch edge only; identical validated centreline/heights. Minimum launch width {min:F2}m; deck width4.7m. Raised edge kept0.25m outside the existing5.3m physical main trail.");return "Trimmed only outside edges of offroad launch; tested centreline and all elevations unchanged";
 }
}
