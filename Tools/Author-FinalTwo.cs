using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class AuthorFinalTwo {
 const string D="Docs/FinalTwo", F="Assets/Track/FinalTwo";
 public static string Main(){if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="LakeWoods")throw new Exception("Load Forest Forward first");
 var b=Object.FindObjectsByType<WoodlandRoute>().Single(x=>x.title=="Echo Cave");if(GameObject.Find("Echo Cave partial rockfall"))throw new Exception("Already authored");Directory.CreateDirectory(F);AssetDatabase.Refresh();
 var original=b.points.ToArray();File.WriteAllText(D+"/original-cave-line.json",JsonUtility.ToJson(new Points{points=original},true));
 var root=new GameObject("Echo Cave partial rockfall");root.transform.SetParent(b.transform.parent);var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/SevenCorrections/Natural fractured cave rock.mat");
 var notes=new List<string>();
 void Rock(string name,float s,float x,float width,float depth,float height,float skew){var p=b.At(s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,f);p+=side*x;
 float floor=Physics.RaycastAll(p+Vector3.up*2,Vector3.down,8,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).DefaultIfEmpty(p.y).Max();p.y=floor-.22f;
 // Faceted broken blocks: steep visible faces at wheel height, bevels only above it.
 var ring=new[]{new Vector2(-.5f,-.32f),new Vector2(-.34f,-.5f),new Vector2(.34f,-.5f),new Vector2(.5f,-.30f),new Vector2(.5f,.32f),new Vector2(.30f,.5f),new Vector2(-.34f,.5f),new Vector2(-.5f,.30f)};
 var v=new List<Vector3>();for(int j=0;j<3;j++)for(int i=0;i<8;i++){float scale=j==2?.73f:1;v.Add(new(ring[i].x*width*scale+(j==2?skew:0),j==0?0:j==1?height*.7f:height*(.91f+.09f*Mathf.Sin(i*2.1f+s)),ring[i].y*depth*scale));}
 var t=new List<int>();for(int j=0;j<2;j++)for(int i=0;i<8;i++){int a=j*8+i,n=j*8+(i+1)%8;t.AddRange(new[]{a,n,a+8,n,n+8,a+8});}for(int i=1;i<7;i++){t.AddRange(new[]{0,i+1,i,16,16+i,17+i});}
 for(int k=0;k<t.Count;k+=3){int swap=t[k+1];t[k+1]=t[k+2];t[k+2]=swap;} var mesh=new Mesh();mesh.vertices=t.Select(i=>v[i]).ToArray();mesh.triangles=Enumerable.Range(0,t.Count).ToArray();mesh.colors=mesh.vertices.Select(q=>new Color(.82f,.80f,.76f)).ToArray();mesh.uv=mesh.vertices.Select(q=>new Vector2(q.x,q.y)*.3f).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,F+"/"+name+".asset");var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));g.transform.SetParent(root.transform);g.transform.SetPositionAndRotation(p,Quaternion.LookRotation(f));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=material;g.GetComponent<MeshCollider>().sharedMesh=mesh;notes.Add(name+" foot="+p.ToString("F3")+" size="+new Vector3(width,height,depth));}
 Physics.SyncTransforms();Rock("Wall-side fallen slab",211,-4.6f,3.4f,7.2f,3.9f,.15f);Rock("Central fractured boulder",210,-2.15f,3.1f,5.8f,3.3f,-.1f);Rock("Opening-side fallen stone",211,-.2f,2.8f,5.5f,2.65f,-.2f);Rock("Broken rear slab",214,-2.2f,4.8f,4.2f,2.25f,-.2f);
 // Local driving line only; no cave floor, ceiling, wall or formation edits.
 var changed=original.ToArray();for(int i=0;i<original.Length;i++){float s=b.Project(original[i],out _);float shift=3.05f*Mathf.SmoothStep(0,1,Mathf.InverseLerp(169,198,s))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(225,254,s)));b.At(s,out var f);changed[i]+=Vector3.Cross(Vector3.up,f).normalized*shift;}
 var exclusion=root.AddComponent<JumpRecoveryExclusion>();exclusion.start=b.At(188,out _);exclusion.end=b.At(239,out _);exclusion.halfWidth=9;
 b.points=changed;typeof(WoodlandRoute).GetField("lengths",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(b,null);EditorUtility.SetDirty(b);
 var guide=b.GetComponent<ReverseShortcutGuidance>()??b.gameObject.AddComponent<ReverseShortcutGuidance>();guide.lookAhead=8;
 PlayerSettings.bundleVersion="0.58.0-review1";EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();notes.Add("Original station 210-216, right opening approximately 3.8m; line offset +3.05m, approach 169-198, rejoin 225-254. Recovery excluded 188-239 using existing metadata. No reset/collision volume.");File.WriteAllLines(D+"/authoring.txt",notes);return string.Join("\n",notes);
 }
 [Serializable]public class Points{public Vector3[] points;}
}

