using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;
public static class FinishArrowFacePlanes
{
    public static string Main()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");
        var road=Object.FindAnyObjectByType<Racer.RaceDirector>().road;
        var patch=GameObject.Find("Ground_First Reverse arrow smooth road").GetComponent<MeshFilter>();
        var v=patch.sharedMesh.vertices;var first=(v[0]+v[2])*.5f;var last=(v[^2]+v[^1])*.5f;
        float plateau=road.At(75,out _).z;road.At(100,out var endDirection);float gradient=endDirection.y/endDirection.z;
        float length=last.z-plateau,dy=last.y-first.y,a=3*dy-gradient*length,b=gradient*length-2*dy;
        float Height(float z){if(z>last.z)return last.y+gradient*(z-last.z);float t=Mathf.Clamp01((z-plateau)/length);return first.y+a*t*t+b*t*t*t;}
        foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m==patch||m.name=="Ground_CR133 mountain driving surface"||m.name=="Ground_CR133 mountain earth banks")){
            var mesh=Object.Instantiate(mf.sharedMesh);var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++){
                var p=mf.transform.TransformPoint(vertices[i]);
                if(p.z<first.z-2||p.z>last.z+14||p.x<710||p.x>755||Math.Abs(p.y-Height(p.z))>12)continue;
                float station=road.Project(p,out _);var at=road.At(station,out _);float lateral=Vector2.Distance(new(p.x,p.z),new(at.x,at.z));
                float endFade=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((p.z-last.z-2)/12));
                if(mf==patch)p.y=Height(p.z);
                else if(mf.name=="Ground_CR133 mountain driving surface"&&lateral<8)p.y=Mathf.Lerp(p.y,Height(p.z),endFade);
                else {float side=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((lateral-7)/9));p.y+=(Height(p.z)-at.y-.04f)*endFade*side;}
                vertices[i]=mf.transform.InverseTransformPoint(p);
            }
            mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.CopySerialized(mesh,mf.sharedMesh);Object.DestroyImmediate(mesh);var collider=mf.GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=mf.sharedMesh;EditorUtility.SetDirty(mf.sharedMesh);
        }
        Physics.SyncTransforms();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "Unified first-arrow triangle heights; matched the existing exit over a 14m local feather. No other Mountain route geometry changed.";
    }
}
