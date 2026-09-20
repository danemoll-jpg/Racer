using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Racer.Editor { public static partial class ExplorationAuthoring {
        static void RebuildMountainTrees(RaceDirector race,List<string> report)
        {
            var woods=GameObject.Find("Woods replacing later subdivisions").transform;
            var branches=Object.FindObjectsByType<WoodlandRoute>();int removed=0;
            foreach(var box in woods.GetComponentsInChildren<BoxCollider>())
            {
                bool blocked=box.bounds.center.x>700 && box.bounds.center.x<1200 && box.bounds.center.z>-260 && box.bounds.center.z<310;
                if(blocked){Object.DestroyImmediate(box.gameObject);removed++;}
            }
            var boxes=woods.GetComponentsInChildren<BoxCollider>();if(boxes.Length==0)throw new Exception("Missing authored tree colliders");
            var shapes=new[]{"BroadCrown","IrregularCrown","UprightCrown"}.Select(n=>AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/"+n+".asset")).ToArray();
            var trunk=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/Trunk.asset");var batches=new Dictionary<Vector2Int,List<CombineInstance>>();var temps=new List<Mesh>();
            var originalRoad=StreetLoopBuilder.Route();var cut=Phase5Setup.Path();var sites=GameObject.Find(Phase6Review.Root).transform.Cast<Transform>().ToArray();
            foreach(var box in boxes)
            {
                var bounds=box.bounds;var p=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);float h=bounds.size.y*2;
                float patch=Mathf.PerlinNoise((p.x+913)/65,(p.z+771)/65),hash=Mathf.Repeat(Mathf.Sin(p.x*12.9898f+p.z*78.233f)*43758.5453f,1);
                int kind=patch<.43f?2:patch>.57f?1:0;float radius=h*Mathf.Lerp(.27f,.34f,hash);
                if(box.name.StartsWith("CR014 trunk ")||box.name.StartsWith("CR016 trunk "))radius*=1.35f;
                radius=Mathf.Min(radius,Mathf.Max(.1f,Phase6Buildings.YardDistance(p)-27),StreetLoopBuilder.Nearest(p,originalRoad,out _)-16,Phase5Setup.Distance(p,cut,out _)-7.6f);
                if(CompactYard.OldDistance(p)<40)radius=Mathf.Min(radius,Mathf.Max(.1f,CompactYard.AccessDistance(p)-3.5f));
                foreach(var site in sites)radius=Mathf.Min(radius,Mathf.Max(.1f,Vector3.ProjectOnPlane(site.position-p,Vector3.up).magnitude-16));
                foreach(var b in branches){b.Project(p,out var d);radius=Mathf.Min(radius,Mathf.Max(.1f,d-10));}
                radius=Mathf.Max(.1f,radius);
                var key=new Vector2Int(Mathf.FloorToInt(p.x/160),Mathf.FloorToInt(p.z/160));if(!batches.TryGetValue(key,out var batch))batches[key]=batch=new();
                var crown=Object.Instantiate(shapes[kind]);var tint=Color.Lerp(new(.24f,.34f,.19f),new(.39f,.46f,.25f),Mathf.Clamp01(patch*.8f+hash*.2f));crown.colors=crown.colors.Select(c=>c*tint).ToArray();temps.Add(crown);
                batch.Add(new(){mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.36f,Quaternion.Euler(0,hash*360,0),new(radius,h*Mathf.Lerp(.59f,.70f,hash),radius))});
                var bark=Object.Instantiate(trunk);bark.colors=Enumerable.Repeat(new Color(.25f,.20f,.145f),bark.vertexCount).ToArray();temps.Add(bark);batch.Add(new(){mesh=bark,transform=box.transform.localToWorldMatrix*Matrix4x4.TRS(box.center,Quaternion.identity,box.size)});
            }
            foreach(var filter in woods.GetComponentsInChildren<MeshFilter>()){if(filter.GetComponent<Collider>())throw new Exception("Refusing to remove collider-owned tree mesh");Object.DestroyImmediate(filter.gameObject);}
            foreach(var pair in batches)
            {
                var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.ToArray(),true,true);string path=Folder+"/"+race.gameObject.scene.name+"-MountainOldTrees-"+pair.Key.x+"-"+pair.Key.y+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
                var go=new GameObject("Complete authored trees",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(woods,false);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");
            }
            foreach(var mesh in temps)Object.DestroyImmediate(mesh);
            report.Add("Rebuilt coherent complete crowns/trunks from "+boxes.Length+" remaining authored trees into "+batches.Count+" reverse-only batches; removed "+removed+" whole entrance trees. Replaces orphan lobe fragments from earlier centroid trimming. No tree LOD/billboard generators on this root.");
        }

}
}
