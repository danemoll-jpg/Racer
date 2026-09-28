using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class VerifyPropertyCorrections {
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");var rows=new List<string>();
 void Check(bool ok,string message){rows.Add((ok?"PASS ":"FAIL ")+message);File.WriteAllLines("Docs/PropertyCorrections/geometry-checks.txt",rows);if(!ok)throw new Exception(message);}
 var paths=new[]{"StreetLoopGreybox","LakeWoods","StreetLoopReverse","ForestLoopReverse","MountainLoop","MountainLoopReverse"}.Select(n=>"Assets/Scenes/"+n+".unity").ToArray();
 foreach(var path in paths){var scene=EditorSceneManager.OpenScene(path);var home=GameObject.Find("Dan - blue X").transform;var race=Object.FindAnyObjectByType<RaceDirector>();
 var asphalt=GameObject.Find("Ground_Black asphalt driveway and terminal parking").GetComponent<MeshFilter>();var mesh=asphalt.sharedMesh;var v=mesh.vertices;var tri=mesh.triangles;var keep=new List<int>();for(int i=0;i<tri.Length;i+=3){var q=home.InverseTransformPoint((v[tri[i]]+v[tri[i+1]]+v[tri[i+2]])/3);if(q.x>=13)keep.AddRange(new[]{tri[i],tri[i+1],tri[i+2]});}mesh.triangles=keep.ToArray();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);asphalt.GetComponent<MeshCollider>().sharedMesh=null;asphalt.GetComponent<MeshCollider>().sharedMesh=mesh;
 Check(keep.Count>0&&asphalt.GetComponent<Renderer>().sharedMaterial.color.r<.1f,scene.name+" local black asphalt retained with terminal parking");
 Check(Enumerable.Range(0,keep.Count/3).All(i=>{var q=home.InverseTransformPoint((v[keep[i*3]]+v[keep[i*3+1]]+v[keep[i*3+2]])/3);return q.x>=13&&q.z>=-24;}),scene.name+" no asphalt/pool-loop continuation beyond parking");
 var concrete=GameObject.Find("Ground_Dan beige concrete descent and parking").GetComponent<MeshFilter>();Check(concrete.sharedMesh==concrete.GetComponent<MeshCollider>().sharedMesh,scene.name+" concrete renderer and collision match");
 foreach(var post in GameObject.Find("House 3 / Rocky Way Acres entrance").GetComponentsInChildren<Transform>().Where(t=>t.name=="Tall grounded entrance post")){
 var bottom=post.position-Vector3.up*post.localScale.y*.5f;var hit=Physics.RaycastAll(bottom+Vector3.up*20,Vector3.down,40,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>Math.Abs(h.point.y-bottom.y)).First();Check(Math.Abs(hit.point.y-bottom.y-.06f)<.025f,scene.name+" entrance post meets ground within 2.5cm");
 var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();float station=street.Project(bottom,out float distance);Check(distance>street.HalfWidth(station)+1,scene.name+" entrance post outside road traffic width");}
 Check(race.GetComponent<Wildlife>().coyoteCalls.Length==1&&AssetDatabase.GetAssetPath(race.GetComponent<Wildlife>().coyoteCalls[0]).EndsWith("ANMLWdog-coyote_howling-Elevenlabs.mp3"),scene.name+" authoritative audio assigned");
 EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
 }
 var deps=AssetDatabase.GetDependencies(paths,true);foreach(var obsolete in new[]{"Coyote-original-1.wav","Coyote-original-2.wav","Coyotes-NPS-Mojave-source.mp3"}){string p="Assets/Audio/Wildlife/"+obsolete;Check(!deps.Contains(p),"Obsolete audio not referenced by playable scenes: "+obsolete);if(!AssetDatabase.DeleteAsset(p))throw new Exception("Cannot remove "+p);}
 PlayerSettings.bundleVersion="0.34.0-review1";AssetDatabase.SaveAssets();return string.Join("\n",rows);
 }
}

