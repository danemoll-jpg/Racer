using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        // The new outer gully lies beyond the original terrain tiles. Fill that
        // region with a wooded ravine, retaining every existing racing surface.
        public static void CR117Landscape()
        {
            foreach(var scene in CR112Scenes.Where(s=>s.Contains("MountainLoop"))){
                EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();
                if(GameObject.Find("CR117 outer ravine landscape"))throw new Exception("Landscape exists; revise locally");
                worldRoot=new GameObject("CR117 outer ravine landscape").transform;
                var ground=Object.FindObjectsByType<MeshCollider>().Where(c=>c.name.StartsWith("Ground_")&&!c.name.Contains("CR117")&&!c.name.Contains("CR110")&&!c.name.Contains("CR103")&&!c.name.Contains("Summit")).ToArray();
                float Old(Vector3 p){float y=-999;var ray=new Ray(p+Vector3.up*400,Vector3.down);foreach(var c in ground)if(c.bounds.min.x<=p.x&&c.bounds.max.x>=p.x&&c.bounds.min.z<=p.z&&c.bounds.max.z>=p.z&&c.Raycast(ray,out var hit,800))y=Mathf.Max(y,hit.point.y);return y;}
                var paths=Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR117 gully")).Select(m=>m.sharedMesh.vertices.Select(m.transform.TransformPoint).Where((p,i)=>i%2==0).ToArray()).ToArray();
                var flight=owner.GetComponent<MountainFlights>().flights[0];
                float Height(Vector3 p){float existing=Old(p);if(existing>-900)return existing-.1f;float y=76+17*Mathf.PerlinNoise(p.x*.006f+12,p.z*.007f+8);foreach(var path in paths){float d=Near(p,path,out var at);if(d<65)y=Mathf.Lerp(y,at.y-.2f,1-Smooth(16,65,d));}float s=Vector3.Dot(p-flight.start,flight.forward),side=Mathf.Abs(Vector3.Dot(p-flight.start,Vector3.Cross(Vector3.up,flight.forward)));if(s>220&&s<335&&side<85)y=Mathf.Lerp(y,65,1-Smooth(28,85,side));return y;}
                const int nx=176,nz=176;const float step=5;var vertices=new Vector3[(nx+1)*(nz+1)];var colors=new Color[vertices.Length];var triangles=new List<int>();
                for(int z=0;z<=nz;z++)for(int x=0;x<=nx;x++){int i=z*(nx+1)+x;var p=new Vector3(650+x*step,0,-500+z*step);p.y=Height(p);vertices[i]=p;colors[i]=Color.Lerp(new(.22f,.30f,.14f),new(.39f,.42f,.23f),Mathf.PerlinNoise(p.x*.04f,p.z*.04f));}
                for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int i=z*(nx+1)+x;var p=(vertices[i]+vertices[i+nx+2])*.5f;p.y=0;if(Old(p)>-900)continue;triangles.AddRange(new[]{i,i+nx+1,i+1,i+1,i+nx+1,i+nx+2});}
                var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.vertices=vertices;mesh.colors=colors;mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                var go=new GameObject("Ground_CR117 outer ravine",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(worldRoot);go.GetComponent<MeshFilter>().sharedMesh=MeshAsset(mesh,owner.gameObject.scene.name+"-outer-ravine");go.GetComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;go.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");
                var crown=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/IrregularCrown.asset");var trunk=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/Trunk.asset");var pieces=new List<CombineInstance>();var temporary=new List<Mesh>();int trees=0;
                for(int z=0;z<48;z++)for(int x=0;x<45;x++){var p=new Vector3(700+x*18+Mathf.Sin(z*17+x)*6,0,-465+z*17+Mathf.Cos(x*9+z)*6);if(Old(p)>-900||paths.Any(path=>Near(p,path,out _)<32))continue;float s=Vector3.Dot(p-flight.start,flight.forward),side=Mathf.Abs(Vector3.Dot(p-flight.start,Vector3.Cross(Vector3.up,flight.forward)));if(s>140&&s<510&&side<45)continue;p.y=Height(p);float h=12+Mathf.PerlinNoise(p.x*.03f,p.z*.02f)*12;var leaf=Object.Instantiate(crown);leaf.colors=Enumerable.Repeat(Color.Lerp(new(.19f,.30f,.13f),new(.35f,.43f,.20f),Mathf.PerlinNoise(p.x*.02f,p.z*.02f)),leaf.vertexCount).ToArray();temporary.Add(leaf);pieces.Add(new(){mesh=leaf,transform=Matrix4x4.TRS(p+Vector3.up*h*.35f,Quaternion.Euler(0,x*37+z*13,0),new(h*.3f,h*.68f,h*.3f))});var bark=Object.Instantiate(trunk);bark.colors=Enumerable.Repeat(new Color(.25f,.18f,.12f),bark.vertexCount).ToArray();temporary.Add(bark);pieces.Add(new(){mesh=bark,transform=Matrix4x4.TRS(p+Vector3.up*h*.25f,Quaternion.identity,new(.6f,h*.5f,.6f))});trees++;}
                var forest=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};forest.CombineMeshes(pieces.ToArray());var woods=new GameObject("CR117 complete outer crowns",typeof(MeshFilter),typeof(MeshRenderer));woods.transform.SetParent(worldRoot);woods.GetComponent<MeshFilter>().sharedMesh=MeshAsset(forest,owner.gameObject.scene.name+"-outer-woods");woods.GetComponent<Renderer>().sharedMaterial=go.GetComponent<Renderer>().sharedMaterial;foreach(var m in temporary)Object.DestroyImmediate(m);
                Save();File.WriteAllText("Docs/CR112-118/landscape-"+owner.gameObject.scene.name+".txt","Outer ravine terrain and "+trees+" complete trees; main corridor 32m tree-centre clearance, flight corridor45m. Existing terrain/surfaces retained.");
            }
            File.WriteAllText("Docs/CR112-118/landscape-done.txt","complete");
        }
    }
}
