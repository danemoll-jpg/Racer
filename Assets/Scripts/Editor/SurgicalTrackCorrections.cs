using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;

public static partial class LocalCorrectiveRoads
{
    // This pass never changes recovery components or the destination road.
    public static string SurgicalLaurel()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
        var race=Object.FindAnyObjectByType<RaceDirector>();var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");
        var source=File.ReadAllText("Docs/SurgicalTracks/checkpoint-branch.txt");
        var old=System.Text.RegularExpressions.Regex.Matches(source,@"- \{x: (.*?), y: (.*?), z: (.*?)\}").Cast<System.Text.RegularExpressions.Match>().Select(m=>new Vector3(float.Parse(m.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),float.Parse(m.Groups[3].Value,System.Globalization.CultureInfo.InvariantCulture))).ToArray();
        var axis=new Vector3(-.1f,0,1).normalized;var lip=new Vector3(498.8f,88.2f,-115);var start=lip-axis*35;start.y=87.5f;
        int join=Enumerable.Range(0,old.Length).OrderBy(i=>(old[i]-new Vector3(475,72,-209)).sqrMagnitude).First();
        var points=old.Take(join+1).Select(p=>p+Vector3.up*.04f).ToList();
        points.AddRange(Hermite(points[^1],start,(old[join]-old[join-1]).normalized,axis).Skip(1));
        for(int i=1;i<=140;i++){float s=i*.25f,u=Mathf.Clamp01((s-10)/25);float y=.7f*(-2*u*u*u+3*u*u)+.03f*25*(u*u*u-u*u);points.Add(start+axis*s+Vector3.up*y);}
        ground=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<Collider>();Physics.SyncTransforms();
        // The inherited connector was as much as 38m above the actual valley.
        // Follow the restored ground instead; only the short final ramp is raised.
        int straightStart=points.Count-141;var stations=new float[points.Count];for(int i=1;i<points.Count;i++)stations[i]=stations[i-1]+Flat(points[i-1],points[i]);
        var heights=points.Select(p=>Ground(p)+.04f).ToArray();float begin=stations[straightStart];
        for(int i=1;i<straightStart;i++){
            float total=0,weight=0;for(int j=Math.Max(0,i-40);j<=Math.Min(straightStart,i+40);j++){float distance=Math.Abs(stations[i]-stations[j]);if(distance>8)continue;float w=Mathf.Exp(-distance*distance/18);total+=heights[j]*w;weight+=w;}
            var p=points[i];float terrain=total/weight;float blend=Smooth((stations[i]-(begin-24))/24);p.y=Mathf.Lerp(terrain,start.y,blend);points[i]=p;
        }
        var root=new GameObject("Laurel separate shortcut ramp").transform;
        var strip=new Strip("Local shortcut",points.ToArray(),2.5f);var roadTris=Ribbon(strip,last:axis);var footprint=new Surface();foreach(var t in roadTris)footprint.Add(t);
        ground=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<Collider>();Physics.SyncTransforms();
        // Narrow side support only: no broad earth bank or landing-road apron.
        var banks=new List<Tri>();for(int sign=-1;sign<=1;sign+=2){Vector3 prevA=default,prevB=default;for(int i=0;i<strip.p.Length;i++){var f=strip.p[Math.Min(i+1,strip.p.Length-1)]-strip.p[Math.Max(0,i-1)];var side=Side(f)*sign;var a=strip.p[i]+side*2.5f;var b=a+side*.12f;b.y=Mathf.Min(a.y,Ground(b));if(i>0){banks.Add(new(prevA,a,prevB));banks.Add(new(prevB,a,b));}prevA=a;prevB=b;}}
        // Prohibit the shortcut footprint from altering the existing main road.
        var main=new Surface();var mainPoints=new List<Vector3>();for(float s=3750;s<=4030;s+=.5f)mainPoints.Add(race.road.At(s,out _));foreach(var t in Ribbon(new Strip("Protected main road",mainPoints.ToArray(),4.55f)))main.Add(t);
        foreach(var t in roadTris.Where(t=>Vector3.Distance(t.Center,points[0])>15)){var center=t.Center;if(main.Nearest(center,.01f,out var d)!=null&&d<.001f)throw new Exception("Shortcut enters protected main road: "+center);}
        var combined=new Surface();foreach(var t in roadTris)combined.Add(t);foreach(var t in banks)combined.Add(t);
        // Subtract only shortcut geometry. Main-road triangles are never seated,
        // moved, elevated or replaced by an embankment.
        foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>()){
            var output=new List<Tri>();bool changed=false;foreach(var t in Triangles(mf)){
                var pieces=new List<List<Vector3>>{new(){t.a,t.b,t.c}};float radius=Mathf.Max(Flat(t.Center,t.a),Flat(t.Center,t.b),Flat(t.Center,t.c));
                foreach(var cut in combined.Near(t.Center,radius)){var next=new List<List<Vector3>>();foreach(var p in pieces)next.AddRange(Subtract(p,cut,false));pieces=next;if(pieces.Count==0)break;}
                if(pieces.Count!=1||pieces[0].Count!=3||pieces[0][0]!=t.a||pieces[0][1]!=t.b||pieces[0][2]!=t.c)changed=true;
                foreach(var p in pieces)for(int i=1;i+1<p.Count;i++)output.Add(new Tri(p[0],p[i],p[i+1]){ca=ColorAt(t,p[0]),cb=ColorAt(t,p[i]),cc=ColorAt(t,p[i+1])});
            }if(changed)Replace(mf,output,"Surgical-shortcut-only");
        }
        foreach(var t in roadTris)t.ca=t.cb=t.cc=new Color(.43f,.38f,.27f);
        var mat=GameObject.Find("Ground_640_240").GetComponent<Renderer>().sharedMaterial;Make(root,"Laurel separate straight ramp",roadTris,mat);Make(root,"Laurel narrow ramp sides",banks,AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Laurel-earth.mat"));
        var continuation=new List<Vector3>();for(float s=3955;s<=3990;s+=.5f)continuation.Add(race.road.At(s,out _));branch.points=points.Select(p=>p-Vector3.up*.04f).Concat(continuation).ToArray();Refresh(branch);
        // Existing route metadata and recovery exclusions are deliberately retained.
        var guidance=new Surface();foreach(var t in roadTris)guidance.Add(t);
        // Reuse the saved visual arrows without introducing collision or duplicating assets.
        int n=0;for(float s=15;s<branch.Length-45;s+=22){var p=branch.At(s,out var f);var t=guidance.Nearest(p,.01f,out _);if(t==null)continue;var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Laurel-arrow-"+n+++".asset");if(!mesh)continue;var go=new GameObject("Laurel visual-only arrow",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);var copy=Object.Instantiate(mesh);var bounds=copy.bounds;var v=copy.vertices;for(int k=0;k<v.Length;k++){var q=v[k]-bounds.center;v[k]=p+Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up))*q;var face=guidance.Nearest(v[k],.1f,out _);if(face!=null)v[k].y=Height(face,v[k])+.04f;}copy.vertices=v;copy.RecalculateBounds();copy.RecalculateNormals();var saved=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Surgical-arrow-"+n+".asset");if(saved){EditorUtility.CopySerialized(copy,saved);Object.DestroyImmediate(copy);copy=saved;}else AssetDatabase.CreateAsset(copy,Folder+"/Surgical-arrow-"+n+".asset");go.GetComponent<MeshFilter>().sharedMesh=copy;go.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");}
        Save();string report=$"Restored baseline 489b220b terrain; separate 5m wide shortcut, 10m straight approach + 25m straight ramp; lip={lip}, axis={axis}, rise=.7m, terminal slope=.03. No destination apron; narrow side support only. Recovery components unchanged.";File.WriteAllText("Docs/SurgicalTracks/laurel.txt",report);return report;
    }
}

