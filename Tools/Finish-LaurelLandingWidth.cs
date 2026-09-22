using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;
public static class FinishLaurelLandingWidth
{
 public static string Main(){EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");var road=Object.FindAnyObjectByType<RaceDirector>().road;
 Vector3 Map(Vector3 p){if(p.x<485||p.x>525||p.z< -115||p.z> -55)return p;float s=road.Project(p,out _);if(s<3943||s>3993)return p;var at=road.At(s,out var f);if(Math.Abs(p.y-at.y)>12)return p;var right=Vector3.Cross(Vector3.up,f).normalized;float side=Vector3.Dot(p-at,right);if(side<=0||side>=20)return p;
 float along=Mathf.SmoothStep(0,1,Mathf.Clamp01((s-3943)/12))*(1-Mathf.SmoothStep(0,1,Mathf.Clamp01((s-3980)/13)));
 float lateral=side<=7?Mathf.SmoothStep(0,1,side/7):1-Mathf.SmoothStep(0,1,(side-7)/13);
 return p+right*(3.5f*along*lateral);}
 int changed=0;
 foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_")&&m.TryGetComponent<MeshCollider>(out _))){var v=mf.sharedMesh.vertices;bool touched=false;for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);var q=Map(p);if((p-q).sqrMagnitude<.00000001f)continue;v[i]=mf.transform.InverseTransformPoint(q);touched=true;}if(!touched)continue;
 var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();string path=AssetDatabase.GetAssetPath(mf.sharedMesh);
 if(path.StartsWith("Assets/Track/LocalRecovery/")){EditorUtility.CopySerialized(mesh,mf.sharedMesh);Object.DestroyImmediate(mesh);EditorUtility.SetDirty(mf.sharedMesh);}else{AssetDatabase.CreateAsset(mesh,"Assets/Track/LocalRecovery/StreetLoopReverse-landing-width-"+mf.name+".asset");mf.sharedMesh=mesh;}
 var collider=mf.GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=mf.sharedMesh;changed++;}
 Physics.SyncTransforms();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
 string report=$"Expanded only the main-road landing's right shoulder by up to 3.5m, tapering within stations 3943–3993. Road, earth and adjacent terrain share the same continuous displacement; no new overlapping collider or elevated slab. Meshes affected={changed}. Straight ramp geometry unchanged.";File.WriteAllText("Docs/LocalRecovery/landing-width.txt",report);return report;}
}
