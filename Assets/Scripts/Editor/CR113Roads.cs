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
        public static void CR113Repairs(){foreach(var path in CR112Scenes){EditorSceneManager.OpenScene(path);CR113CurrentScene();Save();}File.WriteAllText("Docs/CR112-118/roads-done.txt","Highway seam and Kyle driveway entry generation complete");}
        public static void CR113CurrentScene()
        {
            owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();
            var previous=GameObject.Find("CR113 continuous highway seams");if(previous)throw new Exception("CR113 already authored; use local revisions");
            worldRoot=new GameObject("CR113 continuous highway seams").transform;
            var joins=new List<Vector3[]>();
            foreach(bool east in new[]{false,true}){
                var root=GameObject.Find(east?"Hwy 92 east":"Hwy 92 west").transform;
                var old=root.Find("Decorative Road pavement").GetComponent<MeshFilter>();var vertices=old.sharedMesh.vertices;
                float Height(float z){var sorted=vertices.Where(v=>Mathf.Abs(v.z-z)<.1f).ToArray();return sorted.Length>0?sorted.Average(v=>v.y):vertices.OrderBy(v=>Mathf.Abs(v.z-z)).First().y;}
                var a=street.At(east?4510:3840,out var heading);if(!east)heading=-heading;
                var b=root.TransformPoint(new Vector3(0,Height(150),150));float distance=Vector3.Distance(a,b);
                var endHeading=(root.TransformPoint(new Vector3(0,Height(160),160))-root.TransformPoint(new Vector3(0,Height(140),140))).normalized;
                var points=new List<Vector3>();int count=Mathf.CeilToInt(distance/2);
                for(int i=0;i<=count;i++){float t=(float)i/count,t2=t*t,t3=t2*t;points.Add((2*t3-3*t2+1)*a+(t3-2*t2+t)*heading*distance+(-2*t3+3*t2)*b+(t3-t2)*endHeading*distance);}
                for(float z=155;z<=440;z+=5)points.Add(root.TransformPoint(new Vector3(0,Height(z),z)));
                old.gameObject.SetActive(false);var shoulders=root.Find("Profile shoulders");if(shoulders)shoulders.gameObject.SetActive(false);
                foreach(Transform t in root)if(t.name=="Lane dash"||t.name=="Highway lane divider")t.gameObject.SetActive(false);
                var path=points.ToArray();joins.Add(path);
                float minX=path.Min(p=>p.x)-18,maxX=path.Max(p=>p.x)+18,minZ=path.Min(p=>p.z)-18,maxZ=path.Max(p=>p.z)+18;
                Ribbon113(east?"east four lane seam":"west four lane seam",path,8.2f,Mat("CR113 highway asphalt",new(.22f,.23f,.24f)),true);
                Terrain(east?"CR113 east support":"CR113 west support",p=>{if(p.x<minX||p.x>maxX||p.z<minZ||p.z>maxZ)return p;float d=Near(p,path,out var q);if(d<18)p.y=Mathf.Lerp(p.y,q.y-.04f,1-Smooth(8.3f,18,d));return p;});
                foreach(float side in new[]{-4.1f,-.16f,.16f,4.1f}){
                    var line=new List<Vector3>();for(int i=0;i<path.Length;i++){var f=path[Math.Min(i+1,path.Length-1)]-path[Math.Max(i-1,0)];line.Add(path[i]+Vector3.Cross(Vector3.up,f).normalized*side+Vector3.up*.025f);}
                    if(Mathf.Abs(side)<1)Ribbon113("double yellow",line.ToArray(),.065f,Mat("CR113 yellow",new(.9f,.68f,.22f)),false);
                    else for(int i=0;i+2<line.Count;i+=7)Ribbon113("white lane dash",line.Skip(i).Take(3).ToArray(),.075f,Mat("CR113 white",new(.9f,.9f,.82f)),false);
                }
            }
            var through=joins[0].Reverse().Concat(Enumerable.Range(1,335).Select(i=>street.At(3840+i*2,out _))).Concat(joins[1].Skip(1)).ToArray();
            if(owner.throughRoad)Object.DestroyImmediate(owner.throughRoad.gameObject);owner.throughRoad=new GameObject("CR113 highway through route").AddComponent<RaceRoad>();owner.throughRoad.transform.SetParent(worldRoot);owner.throughRoad.points=through;owner.throughRoad.openHighway=true;owner.throughRoad.Initialize();
            var drive=GameObject.Find("Kyle descending driveway").GetComponent<RaceRoad>();var dp=drive.points.ToArray();
            // The visible frontage patch belongs to Kyle's driveway: its old first quad
            // begins on the street centreline. Meet the street edge with a mitered ribbon.
            dp[0]=Vector3.Lerp(dp[0],dp[1],4.5f/Vector3.Distance(dp[0],dp[1]));dp[0].y=CR105Authoring.Ground(dp[0]);drive.points=dp;
            var surface=drive.GetComponentInChildren<MeshFilter>();var oldDrive=surface.gameObject;oldDrive.SetActive(false);
            Ribbon113("Kyle driveway edge join",dp,3.8f,surface.GetComponent<Renderer>().sharedMaterial,true);
        }
        static void Ribbon113(string name,Vector3[] points,float halfWidth,Material material,bool solid)
        {
            var v=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<points.Length;i++){var f=points[Math.Min(i+1,points.Length-1)]-points[Math.Max(i-1,0)];var side=Vector3.Cross(Vector3.up,f).normalized*halfWidth;v.Add(points[i]-side+Vector3.up*.02f);v.Add(points[i]+side+Vector3.up*.02f);if(i+1<points.Length){int n=i*2;triangles.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}}
            var m=new Mesh();m.SetVertices(v);m.SetTriangles(triangles,0);m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject("Ground_CR113 "+name,typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(worldRoot);go.GetComponent<MeshFilter>().sharedMesh=MeshAsset(m,owner.gameObject.scene.name+"-CR113-"+name+"-"+worldRoot.childCount);go.GetComponent<Renderer>().sharedMaterial=material;if(solid)go.AddComponent<MeshCollider>().sharedMesh=go.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
