using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static class ContinuationTerrain
    {
        public static void Apply()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit mode required");
            foreach(var traffic in Object.FindObjectsByType<ContinuationTraffic>())
            {
                var root=traffic.transform;bool highway=root.name.StartsWith("Hwy");float width=highway?16.4f:9;
                var priorHeights=traffic.heights;float priorStart=traffic.start,priorStep=traffic.step;
                float PriorHeight(float z){if(priorHeights==null||priorHeights.Length<2)return 0;float f=Mathf.Clamp((z-priorStart)/priorStep,0,priorHeights.Length-1);int i=Mathf.Min((int)f,priorHeights.Length-2);return Mathf.Lerp(priorHeights[i],priorHeights[i+1],f-i);}
                float begin=highway?-150:-10;int count=(int)((440-begin)/5)+1;
                traffic.start=begin;traffic.step=5;traffic.heights=new float[count];
                float Ground(float x,float z)
                {
                    var world=root.TransformPoint(new Vector3(x,0,z));
                    var hits=Physics.RaycastAll(world+Vector3.up*500,Vector3.down,1000,1,QueryTriggerInteraction.Ignore);
                    var ground=hits.Where(h=>!h.collider.transform.IsChildOf(root)&&h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).ToArray();
                    return ground.Length==0?0:ground[0].point.y-root.position.y;
                }
                // Follow the actual terrain, staying above its whole road cross-section.
                // No original terrain, houses, yards or race-road points are modified.
                for(int i=0;i<count;i++){float z=begin+i*5,h=0;foreach(float x in new[]{-width*.5f,0,width*.5f})h=Mathf.Max(h,Ground(x,z));traffic.heights[i]=h+.09f;}
                var vertices=new List<Vector3>();var triangles=new List<int>();
                for(int i=0;i<count;i++)
                {
                    float z=begin+i*5,h=traffic.heights[i];
                    vertices.Add(new(-width*.5f,h,z));vertices.Add(new(width*.5f,h,z));
                    if(i<count-1){int k=i*2;triangles.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}
                }
                var mesh=new Mesh{name=root.name+" supported terrain profile"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
                string path="Assets/Track/CombinedReview/"+root.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing){EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,path);
                var deck=root.Find("Decorative Road pavement");deck.localPosition=Vector3.zero;deck.localRotation=Quaternion.identity;deck.localScale=Vector3.one;
                if(deck.TryGetComponent<BoxCollider>(out var box))Object.DestroyImmediate(box);
                deck.GetComponent<MeshFilter>().sharedMesh=mesh;var collider=deck.GetComponent<MeshCollider>();if(!collider)collider=deck.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=mesh;
                root.Find("Supported continuation embankment").gameObject.SetActive(false);
                // Fill any profile elevation above the terrain with sloped shoulder skirts.
                var shoulder=root.Find("Profile shoulders");if(!shoulder){shoulder=new GameObject("Profile shoulders",typeof(MeshFilter),typeof(MeshRenderer)).transform;shoulder.SetParent(root,false);}
                vertices.Clear();triangles.Clear();
                for(int side=-1;side<=1;side+=2)for(int i=0;i<count;i++)
                {
                    float z=begin+i*5;int k=vertices.Count;
                    vertices.Add(new(side*width*.5f,traffic.heights[i]-.02f,z));vertices.Add(new(side*(width*.5f+6),Ground(side*(width*.5f+6),z)-.04f,z));
                    if(i<count-1)triangles.AddRange(side<0?new[]{k,k+1,k+2,k+1,k+3,k+2}:new[]{k,k+2,k+1,k+1,k+2,k+3});
                }
                var skirt=new Mesh{name=root.name+" supported shoulders"};skirt.SetVertices(vertices);skirt.SetTriangles(triangles,0);skirt.RecalculateNormals();skirt.RecalculateBounds();
                string skirtPath="Assets/Track/CombinedReview/"+root.name+" shoulders.asset";var oldSkirt=AssetDatabase.LoadAssetAtPath<Mesh>(skirtPath);
                if(oldSkirt){EditorUtility.CopySerialized(skirt,oldSkirt);Object.DestroyImmediate(skirt);skirt=oldSkirt;}else AssetDatabase.CreateAsset(skirt,skirtPath);
                shoulder.GetComponent<MeshFilter>().sharedMesh=skirt;shoulder.GetComponent<MeshRenderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/CombinedReview/Supported earth.mat");
                var shoulderCollider=shoulder.GetComponent<MeshCollider>();if(!shoulderCollider)shoulderCollider=shoulder.gameObject.AddComponent<MeshCollider>();shoulderCollider.sharedMesh=skirt;
                foreach(Transform t in root)
                {
                    var p=t.localPosition;
                    if(t.name=="Lane dash"||t.name=="Highway lane divider")
                    {p.y=traffic.Height(p.z)+.04f;t.localPosition=p;t.localRotation=Quaternion.LookRotation(new Vector3(0,traffic.Height(p.z+1)-traffic.Height(p.z-1),2));}
                    else if(t.name.Contains("Closure")||t.name=="Solid race closure"||t.name=="Event gantry"||t.name=="Race boundary sign"||t.name.StartsWith("Traffic tunnel")||t.name=="Distant tunnel backdrop")
                    {p.y+=traffic.Height(p.z)-PriorHeight(p.z);t.localPosition=p;if(t.name=="Race boundary sign")t.localRotation=Quaternion.identity;}
                }
                EditorUtility.SetDirty(traffic);
            }
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        }
    }
}
