using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class FinishFirstArrowSurface
{
    public static string Main()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");
        var patch=GameObject.Find("Ground_First Reverse arrow smooth road").GetComponent<MeshFilter>();
        var source=Object.Instantiate(patch.sharedMesh);var v=source.vertices;var ix=source.triangles;
        var first=(v[0]+v[2])*.5f;var last=(v[^2]+v[^1])*.5f;
        var road=Object.FindAnyObjectByType<Racer.RaceDirector>().road;
        float flatEnd=road.At(75,out _).z,span=last.z-flatEnd;
        var prior=(v[^4]+v[^3])*.5f;float slope=(last.y-prior.y)/(last.z-prior.z);
        float dy=last.y-first.y,a=3*dy-slope*span,b=slope*span-2*dy;
        float Target(float z){float t=Mathf.Clamp01((z-flatEnd)/span);return first.y+a*t*t+b*t*t*t;}
        float Cross(Vector3 x,Vector3 y)=>x.x*y.z-x.z*y.x;
        Vector3 Map(Vector3 p)
        {
            if(p.z<first.z-2||p.z>last.z+2||p.x<710||p.x>748||Math.Abs(p.y-Target(p.z))>12)return p;
            // Locate the actual old face, including a short extension at the road edge.
            float best=float.MaxValue,oldY=0;
            for(int n=0;n<ix.Length;n+=3){var x=v[ix[n]];var y=v[ix[n+1]];var z=v[ix[n+2]];float area=Cross(y-x,z-x);if(Math.Abs(area)<.001f)continue;
                float u=Cross(p-x,z-x)/area,w=Cross(y-x,p-x)/area;float outside=Mathf.Max(0,-u)+Mathf.Max(0,-w)+Mathf.Max(0,u+w-1);
                var center=(x+y+z)/3;float distance=Vector2.Distance(new(p.x,p.z),new(center.x,center.z));
                float score=outside*10+distance*.001f;if(score>=best)continue;best=score;oldY=x.y+(y.y-x.y)*u+(z.y-x.y)*w;
            }
            if(best>15)return p;
            // Ends keep their exact shared cross-section; the interior uses one
            // smooth height field instead of twisted, constant-height ribbon rows.
            float s=road.Project(p,out _);var centerline=road.At(s,out _);float d=Vector2.Distance(new(p.x,p.z),new(centerline.x,centerline.z));
            float end=Vector2.Distance(new(p.x,p.z),new(last.x,last.z));
            float forward=Vector3.Dot(p-last,(last-prior).normalized);
            float fade=Mathf.SmoothStep(0,1,Mathf.Clamp01(-forward/12));
            float side=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((d-7)/9));
            p.y+=(Target(p.z)-oldY)*fade*side;return p;
        }
        foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m==patch||m.name=="Ground_CR133 mountain driving surface"||m.name=="Ground_CR133 mountain earth banks")){
            var mesh=Object.Instantiate(mf.sharedMesh);mesh.vertices=mesh.vertices.Select(q=>mf.transform.InverseTransformPoint(Map(mf.transform.TransformPoint(q)))).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
            EditorUtility.CopySerialized(mesh,mf.sharedMesh);Object.DestroyImmediate(mesh);var c=mf.GetComponent<MeshCollider>();c.sharedMesh=null;c.sharedMesh=mf.sharedMesh;EditorUtility.SetDirty(mf.sharedMesh);
        }
        Object.DestroyImmediate(source);Physics.SyncTransforms();
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Smoothed only the first-arrow height field; shared boundary rows retained.";
    }
}
