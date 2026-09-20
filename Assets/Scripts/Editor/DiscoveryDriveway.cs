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
        public static void RefineDriveway()
        {
            foreach(var scene in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();
                var house=GameObject.Find("Friend across street - blue circle").transform;var end=house.position+house.forward*13;float station=street.Project(house.position,out _);var top=street.At(station-38,out var f);var side=Vector3.Cross(Vector3.up,f).normalized;if(Vector3.Dot(side,house.position-top)<0)side=-side;
                var corner=top+side*24;corner.y=top.y-3;var near=end-f*20;near.y=end.y+2;
                var path=new[]{top,top+side*7,corner,near,end};
                float minX=path.Min(p=>p.x)-15,maxX=path.Max(p=>p.x)+15,minZ=path.Min(p=>p.z)-15,maxZ=path.Max(p=>p.z)+15;
                Terrain("kyle-smooth-drive",p=>{if(p.x<minX||p.x>maxX||p.z<minZ||p.z>maxZ)return p;street.Project(p,out float lateral);if(lateral<7)return p;float d=Near(p,path,out var at);if(d<12)p.y=Mathf.Lerp(p.y,at.y,(1-Smooth(4,12,d))*Smooth(7,10,lateral));return p;});
                var old=GameObject.Find("Kyle descending driveway");if(old)Object.DestroyImmediate(old);var root=new GameObject("Kyle descending driveway").transform;var road=root.gameObject.AddComponent<RaceRoad>();road.points=path;
                var collection=owner.GetComponent<ExplorationCollection>();collection.routes=collection.routes.Where(r=>r&&r.name!="Kyle descending driveway").Append(road).ToArray();
                var v=new List<Vector3>();var tris=new List<int>();for(int segment=1;segment<path.Length;segment++){var a=path[segment-1];var b=path[segment];var right=Vector3.Cross(Vector3.up,b-a).normalized*3.8f;int start=v.Count;v.Add(a-right+Vector3.up*.05f);v.Add(a+right+Vector3.up*.05f);v.Add(b-right+Vector3.up*.05f);v.Add(b+right+Vector3.up*.05f);tris.AddRange(new[]{start,start+2,start+1,start+1,start+2,start+3});}
                var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=MeshAsset(mesh,owner.gameObject.scene.name+"-kyle-driveway");var surface=new GameObject("Ground_Kyle descending drive");surface.transform.SetParent(root,false);surface.AddComponent<MeshFilter>().sharedMesh=mesh;surface.AddComponent<MeshRenderer>().sharedMaterial=Mat("Driveway gravel",new(.38f,.35f,.28f));surface.AddComponent<MeshCollider>().sharedMesh=mesh;
                ClearTrees(p=>Near(p,path,out _)<7);File.WriteAllLines(Evidence+"/kyle-driveway-"+owner.gameObject.scene.name+".txt",path.Select(p=>p.ToString("F3")));Save();
            }
        }
    }
}
