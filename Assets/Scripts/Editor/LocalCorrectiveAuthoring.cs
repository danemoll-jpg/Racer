using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Racer;
using Object=UnityEngine.Object;

// Explicit, localized authoring only. Never runs in a player or during a build.
public static partial class LocalCorrectiveRoads
{
    const string Evidence="Docs/LocalRecovery";
    static void Init(){Directory.CreateDirectory(Evidence);if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Track","LocalRecovery");}
    static void Save(){Physics.SyncTransforms();var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();}
    static IEnumerable<Tri> Triangles(MeshFilter mf)
    {
        var v=mf.sharedMesh.vertices;var c=mf.sharedMesh.colors;var ix=mf.sharedMesh.triangles;
        for(int i=0;i<ix.Length;i+=3){var t=new Tri(mf.transform.TransformPoint(v[ix[i]]),mf.transform.TransformPoint(v[ix[i+1]]),mf.transform.TransformPoint(v[ix[i+2]]));if(c.Length==v.Length){t.ca=c[ix[i]];t.cb=c[ix[i+1]];t.cc=c[ix[i+2]];}yield return t;}
    }
    static void Replace(MeshFilter mf,IEnumerable<Tri> world,string suffix)
    {
        var local=world.Select(t=>new Tri(mf.transform.InverseTransformPoint(t.a),mf.transform.InverseTransformPoint(t.b),mf.transform.InverseTransformPoint(t.c)){ca=t.ca,cb=t.cb,cc=t.cc});
        mf.sharedMesh=Store(mf.gameObject.scene.name+"-"+suffix+"-"+mf.name,local);
        if(mf.TryGetComponent<MeshCollider>(out var c)){c.sharedMesh=null;c.sharedMesh=mf.sharedMesh;}
        EditorUtility.SetDirty(mf);
    }
    // Subtract the replacement footprint, rather than leaving a hidden second road.
    static void Cut(MeshFilter mf,Surface surface,string suffix)
    {
        var output=new List<Tri>();bool touched=false;
        foreach(var t in Triangles(mf)){
            float radius=Mathf.Max(Flat(t.Center,t.a),Flat(t.Center,t.b),Flat(t.Center,t.c));var cuts=surface.Near(t.Center,radius).ToArray();
            if(cuts.Length==0){output.Add(t);continue;}
            var pieces=new List<List<Vector3>>{new(){t.a,t.b,t.c}};
            foreach(var cut in cuts){var next=new List<List<Vector3>>();foreach(var p in pieces)next.AddRange(Subtract(p,cut,true));pieces=next;if(pieces.Count==0)break;}
            if(pieces.Count!=1||pieces[0].Count!=3||pieces[0][0]!=t.a||pieces[0][1]!=t.b||pieces[0][2]!=t.c)touched=true;
            foreach(var p in pieces)for(int i=1;i+1<p.Count;i++)output.Add(new(p[0],p[i],p[i+1]){ca=ColorAt(t,p[0]),cb=ColorAt(t,p[i]),cc=ColorAt(t,p[i+1])});
        }
        if(touched)Replace(mf,output,suffix);
    }
    static List<Tri> Embankment(Strip route)
    {
        var result=new List<Tri>();
        for(int side=-1;side<=1;side+=2){Vector3[] previous=null;
            for(int i=0;i<route.p.Length;i++){
                var f=route.p[Math.Min(i+1,route.p.Length-1)]-route.p[Math.Max(0,i-1)];var right=Side(f)*side;
                var edge=route.p[i]+right*route.w[i];float span=Mathf.Clamp(Math.Abs(edge.y-Ground(edge))*1.6f+6,8,30);var row=new Vector3[13];
                for(int j=0;j<row.Length;j++){float u=j/12f;var p=edge+right*span*u;p.y=Mathf.Lerp(edge.y,Ground(p),Smooth(u));row[j]=p;}
                if(previous!=null)for(int j=1;j<row.Length;j++){result.Add(new(previous[j-1],row[j-1],previous[j]));result.Add(new(previous[j],row[j-1],row[j]));}previous=row;
            }
        }
        // The takeoff has a closed, earth-supported face, never an accessible underside.
        var end=route.p[^1];var lateral=Side(route.p[^1]-route.p[^2])*route.w[^1];var a=end-lateral;var b=end+lateral;
        var lowA=a;lowA.y=Ground(a);var lowB=b;lowB.y=Ground(b);
        // A slight horizontal batter keeps the face in the same heightfield mesh.
        var fwd=Vector3.ProjectOnPlane(route.p[^1]-route.p[^2],Vector3.up).normalized;
        lowA+=fwd*.35f;lowB+=fwd*.35f;result.Add(new(a,lowA,b));result.Add(new(b,lowA,lowB));
        return result;
    }
    public static string Laurel()
    {
        Init();EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");
        if(GameObject.Find("Local Laurel replacement"))throw new Exception("Already authored; inspect saved result instead of stacking repairs.");
        var race=Object.FindAnyObjectByType<RaceDirector>();var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");
        var oldPoints=branch.points.ToArray();var material=GameObject.Find("Ground_CR133 Laurel driving surface").GetComponent<Renderer>().sharedMaterial;
        // Restore the complete, pre-overlay terrain meshes ONLY in the four affected tiles.
        // Property/driveway edits on the eastern tiles are retained from their later baseline.
        foreach(var tile in new[]{"Ground_480_160","Ground_480_240","Ground_560_240","Ground_640_240"}){
            var mf=GameObject.Find(tile).GetComponent<MeshFilter>();string path="Assets/Track/Exploration/StreetLoopReverse-course-protected-"+tile+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path)??AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Track/ReverseReview/Street-"+tile+".asset");
            if(!mesh)throw new Exception("Missing preserved Laurel terrain: "+tile);mf.sharedMesh=mesh;mf.GetComponent<MeshCollider>().sharedMesh=mesh;
        }
        foreach(var name in new[]{"CR133 straight Laurel","CR122 Laurel local jump","CR133 rerouted shortcut guidance"}){var old=GameObject.Find(name);if(old)Object.DestroyImmediate(old);}
        ground=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<Collider>();Physics.SyncTransforms();
        var root=new GameObject("Local Laurel replacement").transform;
        var landing=race.road.At(3948,out var roadForward)+Vector3.up*.04f;
        var axis=new Vector3(-.05f,0,1).normalized;var lip=landing-axis*16;lip.y=landing.y+.8f;
        var runway=lip-axis*55;runway.y=lip.y-.65f;
        var controls=new[]{oldPoints[0]+Vector3.up*.04f,branch.At(65,out _)+Vector3.up*.04f,new Vector3(300,49,-278),new Vector3(346,43,-278),new Vector3(398,41,-264),new Vector3(441,51,-233),new Vector3(475,72,-209),runway};
        var route=new List<Vector3>();
        for(int i=1;i<controls.Length;i++){
            var startDir=i==1?(oldPoints[1]-oldPoints[0]).normalized:(controls[i]-controls[i-2]).normalized;
            var endDir=i==controls.Length-1?axis:(controls[i+1]-controls[i-1]).normalized;
            route.AddRange(Hermite(controls[i-1],controls[i],startDir,endDir).Skip(i==1?0:1));
        }
        for(int i=1;i<=220;i++){float s=i*.25f,u=Mathf.Clamp01((s-30)/25);route.Add(runway+axis*s+Vector3.up*(.65f*u*u));}
        var strip=new Strip("Laurel grounded shortcut",route.ToArray(),4.5f);
        var driving=new Surface();Union(driving,Ribbon(strip,last:axis));
        // A small shoulder at the existing main road catches the normal speed range.
        // It is terrain-supported, and replaces the old surface within its footprint.
        var apronPoints=new List<Vector3>();for(float s=3929;s<=3999;s+=.5f)apronPoints.Add(race.road.At(s,out _)+Vector3.up*.04f);
        var apron=new Strip("Laurel main road landing shoulder",apronPoints.ToArray(),4.5f);
        for(int i=0;i<apron.p.Length;i++){float s=i*.5f;apron.w[i]=4.5f+2.5f*Smooth(s/15)*(1-Smooth((s-45)/25));}
        // The landing is below the lip: clip its footprint without pulling its
        // boundary upward into a steep connecting triangle at the takeoff edge.
        Union(driving,Ribbon(apron),seat:false);
        var complete=new Surface();foreach(var t in driving.triangles)complete.Add(t);int roadCount=complete.triangles.Count;
        Union(complete,Embankment(strip),seat:false);Union(complete,Embankment(apron),seat:false);
        foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())Cut(mf,complete,"Laurel-subgrade");
        Make(root,"Laurel continuous driving surface",driving.triangles,material);
        var earth=new Material(material){name="Laurel earth",color=new Color(.35f,.39f,.23f)};AssetDatabase.CreateAsset(earth,Folder+"/Laurel-earth.mat");
        Make(root,"Laurel supported earth",complete.triangles.Skip(roadCount),earth);
        var exclusion=new GameObject("Laurel straight runway - no recovery").AddComponent<JumpRecoveryExclusion>();exclusion.transform.SetParent(root);exclusion.start=runway;exclusion.end=lip+axis*2;exclusion.halfWidth=6;
        var continuation=new List<Vector3>();for(float s=3948;s<=3990;s+=.5f)continuation.Add(race.road.At(s,out _));
        branch.points=route.Select(p=>p-Vector3.up*.04f).Concat(continuation).ToArray();Refresh(branch);branch.halfWidth=4.5f;branch.exitRoad=3990;
        branch.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(i=>race.road.Relative(race.road.Project(race.gates[i].transform.position,out _),branch.entryRoad)<race.road.Relative(branch.exitRoad,branch.entryRoad)).ToArray();
        ClearTrees(branch);Physics.SyncTransforms();Guidance(root,branch,driving);SeatLaurelSign();
        race.courseId="street-reverse-local-laurel-v6";EditorUtility.SetDirty(race);Save();
        var report=$"Removed CR122/CR133 elevated Laurel overlays. Grounded valley route; straight 30m approach + 25m quadratic ramp, 0.65m rise; axis={axis}; start={runway}; lip={lip}; main-road target={landing}; existing main-road station=3948; earth-supported 7m maximum half-width landing shoulder. No vehicle changes or boost.";
        File.WriteAllText(Evidence+"/Laurel-geometry.txt",report);return report;
    }
    static void ClearTrees(WoodlandRoute branch)
    {
        var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
        typeof(Racer.Editor.DiscoveryAuthoring).GetField("owner",flags).SetValue(null,Object.FindAnyObjectByType<RaceDirector>());
        Func<Vector3,bool> intersects=p=>{float s=branch.Project(p,out var d);return d<branch.halfWidth+2&&Math.Abs(p.y-branch.At(s,out _).y)<18;};
        typeof(Racer.Editor.DiscoveryAuthoring).GetMethod("ClearCompleteTrees",flags).Invoke(null,new object[]{intersects});
    }
    static void SeatLaurelSign()
    {
        var sign=Object.FindObjectsByType<PhysicalSign>().Single(s=>s.name=="House 3 / Rocky Way Acres entrance");
        var p=sign.transform.position;p.x=487;
        var hits=Physics.RaycastAll(new Vector3(p.x,150,p.z),Vector3.down,150,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).ToArray();
        if(hits.Length==0)throw new Exception("No ground at relocated Laurel roadside sign");p.y=hits[0].point.y;sign.transform.position=p;
    }
    public static string FinishLaurelSign(){EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");SeatLaurelSign();Save();return "Moved the complete entrance sign beside the runway, grounded outside both driving corridors.";}
    static void Guidance(Transform root,WoodlandRoute branch,Surface surface)
    {
        var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");int n=0;
        for(float s=15;s<branch.Length-35;s+=22){var p=branch.At(s,out var f);var axis=Vector3.ProjectOnPlane(f,Vector3.up).normalized;var side=Side(axis);var shape=new[]{new Vector2(-.35f,-2.5f),new(.35f,-2.5f),new(.35f,0),new(.9f,0),new(0,2.5f),new(-.9f,0),new(-.35f,0)};
            var vertices=new List<Vector3>();foreach(var v in shape){var q=p+side*v.x+axis*v.y;var t=surface.Nearest(q,.01f,out _);if(t==null)break;q.y=Height(t,q)+.04f;vertices.Add(q);}if(vertices.Count!=7)continue;
            var mesh=new Mesh{vertices=vertices.ToArray(),triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/Laurel-arrow-"+n+++".asset");
            var go=new GameObject("Laurel visual-only arrow",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=mat;
        }
    }
    public static string MountainArrow()
    {
        Init();EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");var race=Object.FindAnyObjectByType<RaceDirector>();var road=race.road;
        var root=new GameObject("First Reverse arrow local surface correction").transform;var mf=GameObject.Find("Ground_CR133 mountain driving surface").GetComponent<MeshFilter>();
        var source=Triangles(mf).ToArray();var end=road.At(100,out var ef);float y0=road.At(35,out _).y,delta=end.y-y0;
        float endSlope=ef.y/Vector3.ProjectOnPlane(ef,Vector3.up).magnitude;
        float a=3*delta-endSlope*25,b=endSlope*25-2*delta;
        float Profile(float station){float t=Mathf.Clamp01((station-75)/25);return y0+a*t*t+b*t*t*t;}
        var points=new List<Vector3>();for(float s=35;s<=100;s+=.25f){var p=road.At(s,out _);p.y=Profile(s)+.04f;points.Add(p);}
        var strip=new Strip("first-arrow road",points.ToArray(),7);var patch=new Surface();foreach(var t in Ribbon(strip))patch.Add(t);
        Cut(mf,patch,"first-arrow-only");
        Make(root,"First Reverse arrow smooth road",patch.triangles,mf.GetComponent<Renderer>().sharedMaterial);
        // Match the neighbouring shoulder/earth only along this 65m patch.
        foreach(var bank in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Ground_CR133 mountain earth banks")){
            Vector3 Map(Vector3 p){float s=road.Project(p,out var d);if(s<=35||s>=100||d>18)return p;var q=road.At(s,out _);float weight=1-Smooth((d-7)/11);p.y+=(Profile(s)-q.y)*weight;return p;}
            Replace(bank,Triangles(bank).Select(t=>new Tri(Map(t.a),Map(t.b),Map(t.c)){ca=t.ca,cb=t.cb,cc=t.cc}),"first-arrow-shoulder");
        }
        var saved=road.points.ToArray();for(int i=0;i<saved.Length;i++){float s=road.Project(saved[i],out _);if(s>35&&s<100)saved[i].y=Profile(s);}road.points=saved;Refresh(road);
        foreach(var arrow in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Main teal trail arrow")){
            var center=arrow.GetComponent<Renderer>().bounds.center;if(center.z<0||center.z>35||center.x<710||center.x>735)continue;
            foreach(var c in arrow.GetComponents<Collider>())Object.DestroyImmediate(c);
            var mesh=Object.Instantiate(arrow.sharedMesh);var v=mesh.vertices;
            for(int i=0;i<v.Length;i++){var p=arrow.transform.TransformPoint(v[i]);var t=patch.Nearest(p,.01f,out _);if(t!=null)p.y=Height(t,p)+.035f;v[i]=arrow.transform.InverseTransformPoint(p);}mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/Mountain-first-arrow-visual.asset");arrow.sharedMesh=mesh;
        }
        Save();string report="First arrow already had zero colliders. Removed the shallow approach dip and abrupt faceted grades locally at original stations 35–100 (65m). Replacement road is a shared visible/collision mesh sampled at 0.25m, tangent-matched at the far end; arrow remains visual-only. No other Mountain route, ramp or shortcut redesigned.";File.WriteAllText(Evidence+"/Mountain-arrow.txt",report);return report;
    }
}
