using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class AuthorPropertyCorrections {
 const string Folder="Assets/Track/PropertyCorrections",Evidence="Docs/PropertyCorrections",Audio="Assets/Audio/Wildlife/ANMLWdog-coyote_howling-Elevenlabs.mp3";
 static float Ground(Vector3 p){var hits=Physics.RaycastAll(new Vector3(p.x,400,p.z),Vector3.down,800,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")||h.collider.name=="Decorative Road pavement").OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hits.Length==0)throw new Exception("No ground "+p);return hits[0].point.y;}
 static Material Mat(string name,Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};m.SetFloat("_Smoothness",.12f);AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");return m;}
 public static string Main(){
 if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
 var black=Mat("Local black asphalt",new(.065f,.065f,.06f));var beige=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/FiveUpdates/Beige concrete.mat");var dirt=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/FiveUpdates/Dirt threshold.mat");
 var importer=(AudioImporter)AssetImporter.GetAtPath(Audio);var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;importer.forceToMono=true;importer.SaveAndReimport();
 var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(Audio);clip.LoadAudioData();var pcm=new float[clip.samples*clip.channels];if(!clip.GetData(pcm,0)||!pcm.Any(v=>Math.Abs(v)>.01f))throw new Exception("Supplied clip failed decoding");
 var report=new List<string>{"Supplied MP3: "+Audio+"; full recording "+clip.length+" seconds; mono PCM import; peak="+pcm.Max(v=>Math.Abs(v))+"; original pitch."};
 foreach(string name in new[]{"StreetLoopGreybox","LakeWoods","StreetLoopReverse","ForestLoopReverse","MountainLoop","MountainLoopReverse"}){
 var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var home=GameObject.Find("Dan - blue X").transform;var race=Object.FindAnyObjectByType<RaceDirector>();var street=race.ambientRoad?race.ambientRoad:race.road;street.Initialize();
 var old=GameObject.Find("Dan beige concrete driveway");if(!old)throw new Exception("Original overlay missing");
 var mf=old.GetComponentsInChildren<MeshFilter>().Single(m=>m.name=="Ground_Beige concrete property surface");var mesh=mf.sharedMesh;var v=mesh.vertices;var t=mesh.triangles;var kept=new List<int>();
 for(int i=0;i<t.Length;i+=3){var q=home.InverseTransformPoint(mf.transform.TransformPoint((v[t[i]]+v[t[i+1]]+v[t[i+2]])/3));if(q.x>=13&&q.z>=-24)kept.AddRange(new[]{t[i],t[i+1],t[i+2]});}
 int removed=(t.Length-kept.Count)/3;mesh.triangles=kept.ToArray();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);mf.GetComponent<MeshCollider>().sharedMesh=null;mf.GetComponent<MeshCollider>().sharedMesh=mesh;mf.GetComponent<Renderer>().sharedMaterial=black;mf.name="Ground_Black asphalt driveway and terminal parking";old.name="Other gate black asphalt driveway - stops at parking";
 var obsolete=old.transform.Find("Ground_Dirt path threshold");if(obsolete)Object.DestroyImmediate(obsolete.gameObject);
 Physics.SyncTransforms();
 var root=new GameObject("Dan first gate straight beige driveway").transform;
 Vector3 World(float x,float z){var p=home.TransformPoint(new Vector3(x,-3,z));p.y=Ground(p)+.08f;return p;}
 float station=street.Project(World(-22,10),out _);var roadPoint=street.At(station,out _);float top=home.InverseTransformPoint(roadPoint).z-street.HalfWidth(station)+.15f;
 var verts=new List<Vector3>();var triangles=new List<int>();var cache=new Dictionary<Vector2,int>();
 int Vertex(float x,float z){var key=new Vector2(x,z);if(cache.TryGetValue(key,out int index))return index;index=verts.Count;verts.Add(World(x,z));cache.Add(key,index);return index;}
 void Area(float x0,float z0,float x1,float z1){const float step=.25f;for(float x=x0;x<x1-.001f;x+=step)for(float z=z0;z<z1-.001f;z+=step){float xx=Math.Min(x+step,x1),zz=Math.Min(z+step,z1);int a=Vertex(x,z),b=Vertex(xx,z),c=Vertex(x,zz),d=Vertex(xx,zz);triangles.AddRange(new[]{a,c,b,b,c,d});}}
 void Surface(string title,string suffix,Material mat,bool collision){var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.SetVertices(verts);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();AssetDatabase.CreateAsset(m,Folder+"/"+name+"-"+suffix+".asset");var go=new GameObject(title,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=m;go.GetComponent<Renderer>().sharedMaterial=mat;if(collision)go.AddComponent<MeshCollider>().sharedMesh=m;verts.Clear();triangles.Clear();cache.Clear();}
 Area(-24.5f,-25,-19.5f,top);Area(-44,-12,-24.5f,-6);Area(-19.5f,-12,-17,-6);
 Surface("Ground_Dan beige concrete descent and parking","concrete",beige,true);
 Area(-24.3f,-26,-19.7f,-25.05f);Surface("Ground_Dan paved-to-dirt boundary","dirt",dirt,false);
 // Existing sign only: road-facing beside the actual McFadden entrance; fit each post to its new support.
 var sign=GameObject.Find("House 3 / Rocky Way Acres entrance").transform;var mouth=street.At(street.Project(new Vector3(515.1f,81.8f,-132.7f),out _),out _);var toward=Vector3.ProjectOnPlane(GameObject.Find("Original house 3").transform.position-mouth,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,toward);
 var at=mouth+toward*23+side*15;at.y=Ground(at);sign.SetPositionAndRotation(at,Quaternion.LookRotation(toward));
 var posts=sign.Cast<Transform>().Where(p=>p.name=="Tall grounded entrance post").ToArray();float highest=posts.Max(p=>Ground(new Vector3(p.position.x,at.y,p.position.z)));at.y=highest;sign.position=at;
 foreach(var post in posts){var p=post.position;float bottom=Ground(new Vector3(p.x,at.y,p.z))-.06f;float upper=at.y+6.8f;p.y=(bottom+upper)/2;post.position=p;post.localScale=new(.4f,upper-bottom,.4f);}
 race.GetComponent<Wildlife>().coyoteCalls=new[]{clip};EditorUtility.SetDirty(race.GetComponent<Wildlife>());
 Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
 report.Add(name+": removed pool-loop/apron triangles="+removed+"; first gate="+World(-22,top)+"; parking="+World(-30,-9)+"; concrete end="+World(-22,-25)+"; sign="+sign.position+"; post bases="+string.Join(" / ",posts.Select(p=>(p.position-Vector3.up*p.localScale.y*.5f).ToString("F2"))));
 }
 File.WriteAllLines(Evidence+"/authoring.txt",report);return string.Join("\n",report);
 }
}
