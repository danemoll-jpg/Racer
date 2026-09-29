using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;using Object=UnityEngine.Object;
public static class FinishShortcutRevision {
 public static string Main(){if(Application.isPlaying)throw new Exception("Edit mode required");EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");var tree=Object.FindObjectsByType<WoodlandRoute>().First(b=>b.title=="Tree-Top Trail");var ramp=GameObject.Find("Takeoff - Short treehouse dirt jump").GetComponent<MeshCollider>();
 // Lower only the last local ramp tangent. Same entry, lip position and landing deck.
 float y0=tree.At(15,out _).y,y1=tree.At(29,out _).y;float Y(float s){float t=Mathf.InverseLerp(15,29,s),t2=t*t,t3=t2*t;return (2*t3-3*t2+1)*y0+(t3-2*t2+t)*14*(-.1f)+(-2*t3+3*t2)*y1+(t3-t2)*14*.09f;}
 float d=0;for(int i=1;i<tree.points.Length;i++){d+=Vector2.Distance(new(tree.points[i-1].x,tree.points[i-1].z),new(tree.points[i].x,tree.points[i].z));if(d>=15&&d<=29)tree.points[i].y=Y(d);}
 var mesh=ramp.sharedMesh;var v=mesh.vertices;for(int i=0;i<v.Length;i++){float s=tree.Project(v[i],out _);v[i].y=Y(s);}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();ramp.sharedMesh=null;ramp.sharedMesh=mesh;EditorUtility.SetDirty(mesh);EditorUtility.SetDirty(tree);
 var foliage=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/BackyardShortcuts/Dense forest undergrowth.mat");foliage.SetFloat("_Smoothness",0);foliage.SetFloat("_Glossiness",0);EditorUtility.SetDirty(foliage);
 // Replace smooth sphere leaves with low-poly irregular clusters and fern fronds.
 foreach(var brush in Object.FindObjectsByType<ShortcutUndergrowth>()){
 var old=brush.GetComponent<MeshFilter>().sharedMesh;var verts=new List<Vector3>();var tris=new List<int>();var random=new System.Random(brush.name.Length*137);
 float Ground(Vector3 p)=>Physics.RaycastAll(p+Vector3.up*200,Vector3.down,400,1).Where(h=>h.collider.name.StartsWith("Ground_")).First().point.y;
 float minX=brush.centres.Min(p=>p.x)-brush.radius,maxX=brush.centres.Max(p=>p.x)+brush.radius,minZ=brush.centres.Min(p=>p.z)-brush.radius,maxZ=brush.centres.Max(p=>p.z)+brush.radius;
 for(float x=minX;x<maxX;x+=1.9f)for(float z=minZ;z<maxZ;z+=1.9f){var p=new Vector3(x+(float)random.NextDouble(),0,z+(float)random.NextDouble());if(brush.Coverage(p)<.18f)continue;p.y=Ground(p);for(int clump=0;clump<3;clump++){float h=.8f+(float)random.NextDouble()*1.25f;var centre=p+new Vector3((float)random.NextDouble()-.5f,0,(float)random.NextDouble()-.5f);int k=verts.Count;verts.Add(centre+Vector3.up*h);verts.Add(centre+Vector3.down*.05f);for(int j=0;j<6;j++){float angle=j*Mathf.PI/3;verts.Add(centre+new Vector3(Mathf.Cos(angle)*1.15f,h*.4f,Mathf.Sin(angle)*1.15f));}for(int j=0;j<6;j++){int a=k+2+j,b=k+2+(j+1)%6;tris.AddRange(new[]{k,b,a,k+1,a,b});}}
 for(int frond=0;frond<5;frond++){float angle=(float)random.NextDouble()*Mathf.PI*2;var outwards=new Vector3(Mathf.Sin(angle),0,Mathf.Cos(angle));var side=Vector3.Cross(Vector3.up,outwards);int k=verts.Count;verts.Add(p);verts.Add(p+outwards*.8f+side*.3f+Vector3.up*.8f);verts.Add(p+outwards*1.5f+Vector3.up*.9f);verts.Add(p+outwards*.8f-side*.3f+Vector3.up*.8f);tris.AddRange(new[]{k,k+1,k+2,k,k+2,k+3,k+2,k+1,k,k+3,k+2,k});}}
 old.Clear();old.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;old.SetVertices(verts);old.SetTriangles(tris,0);old.RecalculateNormals();old.RecalculateBounds();EditorUtility.SetDirty(old);
 }
 EditorSceneManager.MarkSceneDirty(tree.gameObject.scene);EditorSceneManager.SaveScene(tree.gameObject.scene);AssetDatabase.SaveAssets();return "Local tree takeoff tangent eased; matte irregular foliage replaces spheres without changing resistance footprint";
 }
}
