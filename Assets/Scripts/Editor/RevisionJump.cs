using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Racer.Editor
{
    public static class RevisionJump
    {
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();
            if(Application.isPlaying || scene.isDirty || scene.path!=StreetLoopBuilder.ScenePath) throw new InvalidOperationException("Open the saved StreetLoop scene outside Play mode.");
            var root=GameObject.Find(Phase4Setup.RootName).transform;
            var ramp=root.GetComponentInChildren<MeshCollider>();
            var route=StreetLoopBuilder.Route();
            var vertices=new List<Vector3>(); var triangles=new List<int>();
            const int steps=80; const float length=40, height=6.2f;
            for(int i=0;i<=steps;i++)
            {
                float z=length*i/steps, rise=height*(z/length)*(z/length)+.012f;
                foreach(float x in new[]{-4.5f,1.5f})
                {
                    StreetLoopBuilder.Nearest(root.TransformPoint(new Vector3(x,0,z)),route,out var ground);
                    vertices.Add(new(x,ground.y-root.position.y+rise,z));
                }
                if(i<steps) { int k=i*2; triangles.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3}); }
            }
            int count=vertices.Count;
            for(int i=0;i<count;i++) { var p=vertices[i]; p.y=-.3f; vertices.Add(p); }
            for(int i=0;i<steps;i++) foreach(int side in new[]{0,1})
            { int a=i*2+side,b=a+2,c=a+count,d=b+count; triangles.AddRange(side==0?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c}); }
            int last=steps*2; triangles.AddRange(new[]{last,last+count,last+1,last+1,last+count,last+count+1});
            var mesh=ramp.sharedMesh; mesh.Clear(); mesh.SetVertices(vertices); mesh.SetTriangles(triangles,0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            ramp.sharedMesh=null; ramp.sharedMesh=mesh; ramp.GetComponent<MeshFilter>().sharedMesh=mesh;
            ramp.name="Takeoff - 40m x 6m, 6.2m rise";
            var sign=root.GetComponentsInChildren<TextMesh>(true).First(t=>t.name=="Recommended speed"); sign.text="JUMP\n78 - 96 mph\nROAD LANE ON RIGHT";
            // Existing continuous terrain is supported across both shoulders; mark a wider landing corridor.
            foreach(Transform child in root)
                if(child.name=="Landing edge")
                {
                    var p=child.localPosition; p.x=Mathf.Sign(p.x)*12;
                    var world=root.TransformPoint(p);
                    if(Physics.Raycast(world+Vector3.up*30,Vector3.down,out var hit,80,1)) child.position=hit.point+Vector3.up*.04f;
                }
            var road=UnityEngine.Object.FindAnyObjectByType<RaceRoad>();
            road.bypassEnd=road.Project(root.position+root.forward*180,out _);
            PlayerSettings.bundleVersion="0.5.0-review1";
            EditorUtility.SetDirty(mesh); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
        }
    }
}

