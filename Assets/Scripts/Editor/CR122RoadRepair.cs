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
        // One visible/collision ribbon per connected driving surface. Existing
        // centreline elevations remain authoritative; flight gaps stay open.
        sealed class RepairStrip
        {
            public Vector3[] points;
            public float[] widths;
            public RepairStrip(Vector3[] p,float w){points=p;widths=Enumerable.Repeat(w,p.Length).ToArray();}
        }
        static readonly List<RepairStrip> repairStrips=new();
        static void RepairRibbon(string label,RepairStrip strip,bool trimMain=false)
        {
            var v=new List<Vector3>();var tri=new List<int>();
            for(int i=0;i<strip.points.Length;i++){
                var f=strip.points[Math.Min(i+1,strip.points.Length-1)]-strip.points[Math.Max(0,i-1)];
                var side=Vector3.Cross(Vector3.up,f).normalized*strip.widths[i];
                v.Add(strip.points[i]-side+Vector3.up*.04f);v.Add(strip.points[i]+side+Vector3.up*.04f);
                if(i>0){int n=2*(i-1);tri.AddRange(new[]{n,n+2,n+1,n+1,n+2,n+3});}
            }
            if(trimMain){
                // Clip branch triangles at the main ribbon edge, so the mouth
                // shares the road height without stacked driving colliders.
                float Field(Vector3 p){float s=owner.road.Project(p,out float d);return Math.Abs(p.y-owner.road.At(s,out _).y)<6?d-7:10;}
                Vector3 Seat(Vector3 p){var hits=Physics.RaycastAll(p+Vector3.up*8,Vector3.down,16,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_CR122 main section")).ToArray();if(hits.Length>0)p.y=hits.OrderBy(h=>Math.Abs(h.point.y-p.y)).First().point.y;return p;}
                var clipped=new List<Vector3>();var indices=new List<int>();
                for(int t=0;t<tri.Count;t+=3){var poly=new List<Vector3>();for(int j=0;j<3;j++){
                    var a=v[tri[t+j]];var b=v[tri[t+(j+1)%3]];float fa=Field(a),fb=Field(b);if(fa>=0)poly.Add(fa<.2f?Seat(a):a);
                    if((fa>=0)!=(fb>=0))poly.Add(Seat(Vector3.Lerp(a,b,fa/(fa-fb))));
                }int n=clipped.Count;clipped.AddRange(poly);for(int j=1;j+1<poly.Count;j++)indices.AddRange(new[]{n,n+j,n+j+1});}
                v=clipped;tri=indices;
            }
            var mesh=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetTriangles(tri,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject("Ground_CR122 "+label,typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(worldRoot);
            var asset=MeshAsset(mesh,owner.gameObject.scene.name+"-CR122-"+label);
            go.GetComponent<MeshFilter>().sharedMesh=asset;go.GetComponent<MeshCollider>().sharedMesh=asset;
            go.GetComponent<Renderer>().sharedMaterial=Mat("Summit packed earth",new(.43f,.31f,.17f));repairStrips.Add(strip);
        }
        static Vector3[] RepairHermite(Vector3 a,Vector3 b,Vector3 ta,Vector3 tb)
        {
            float length=Vector3.Distance(a,b);int n=Mathf.CeilToInt(length/.75f);var p=new Vector3[n+1];
            for(int i=0;i<=n;i++){float t=(float)i/n,t2=t*t,t3=t2*t;p[i]=(2*t3-3*t2+1)*a+(t3-2*t2+t)*ta*length+(-2*t3+3*t2)*b+(t3-t2)*tb*length;}return p;
        }
        static void ExcludeRecovery(Vector3 a,Vector3 b,float width)
        {var zone=new GameObject("Jump run-up / no recovery checkpoint").AddComponent<JumpRecoveryExclusion>();zone.transform.SetParent(worldRoot);zone.start=a;zone.end=b;zone.halfWidth=width;}
        // A spatial index bounds the terrain pass to these ribbons. This lowers
        // intruding subgrade; it never flattens the road or raises valley terrain.
        static void RepairSubgrade()
        {
            var bins=new Dictionary<Vector2Int,List<(Vector3 a,Vector3 b,float w)>>();
            foreach(var strip in repairStrips)for(int i=1;i<strip.points.Length;i++){
                var a=strip.points[i-1];var b=strip.points[i];float w=Math.Max(strip.widths[i-1],strip.widths[i])+5;
                for(int x=Mathf.FloorToInt((Math.Min(a.x,b.x)-w)/24);x<=Mathf.FloorToInt((Math.Max(a.x,b.x)+w)/24);x++)
                for(int z=Mathf.FloorToInt((Math.Min(a.z,b.z)-w)/24);z<=Mathf.FloorToInt((Math.Max(a.z,b.z)+w)/24);z++){
                    var key=new Vector2Int(x,z);if(!bins.TryGetValue(key,out var list))bins[key]=list=new();list.Add((a,b,w-5));
                }
            }
            var terrain=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>().Concat(Object.FindObjectsByType<MeshFilter>().Where(m=>m.name=="Ground_CR117 outer ravine"));
            foreach(var mf in terrain){var vertices=mf.sharedMesh.vertices;bool changed=false;
                for(int i=0;i<vertices.Length;i++){
                    var p=mf.transform.TransformPoint(vertices[i]);if(!bins.TryGetValue(new(Mathf.FloorToInt(p.x/24),Mathf.FloorToInt(p.z/24)),out var list))continue;
                    float y=p.y;
                    foreach(var seg in list){var f=seg.b-seg.a;f.y=0;var q=p-seg.a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,f)/Math.Max(.001f,f.sqrMagnitude));float d=(q-f*t).magnitude;
                        if(d>seg.w+5)continue;float target=Mathf.Lerp(seg.a.y,seg.b.y,t)-1.2f;
                        y=Math.Min(y,Mathf.Lerp(p.y,target,1-Smooth(seg.w+1,seg.w+5,d)));
                    }
                    if(y<p.y-.001f){p.y=y;vertices[i]=mf.transform.InverseTransformPoint(p);changed=true;}
                }
                if(!changed)continue;var copy=Object.Instantiate(mf.sharedMesh);copy.vertices=vertices;copy.RecalculateNormals();copy.RecalculateBounds();
                mf.sharedMesh=MeshAsset(copy,owner.gameObject.scene.name+"-CR122-subgrade-"+mf.name);if(mf.TryGetComponent<MeshCollider>(out var collider)){collider.sharedMesh=null;collider.sharedMesh=mf.sharedMesh;}
            }
        }
        public static void CR122Mountain(bool reverse)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoop"+(reverse?"Reverse":"")+".unity");
            owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();
            if(GameObject.Find("CR122 continuous mountain support"))throw new Exception("Already repaired");
            worldRoot=new GameObject("CR122 continuous mountain support").transform;repairStrips.Clear();
            var flights=owner.GetComponent<MountainFlights>().flights;
            var gaps=new List<(float a,float b)>();
            foreach(var flight in flights){
                // Navigation is sampled THROUGH the air; its point spacing is
                // not evidence of physical support. Use the authored catch lip.
                bool gully=flight.name.Contains("Gully");float catchDistance=gully?320:(reverse?370:330);
                var catchPoint=flight.start+flight.forward*catchDistance;
                catchPoint.y=gully?124:LandingHeight(catchDistance);
                gaps.Add((road.Project(flight.lip,out _),road.Project(catchPoint,out _)));
            }
            if(gaps.Count!=2)throw new Exception("Expected two authored main flight gaps, found "+gaps.Count);
            var oldSurfaces=Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR110")||m.name.StartsWith("Ground_CR103")||(m.name.StartsWith("Ground_CR117")&&m.name!="Ground_CR117 outer ravine")||m.name.StartsWith("Ground_CR121")).ToArray();
            foreach(var m in oldSurfaces)m.gameObject.SetActive(false);
            // Round only abrupt slope changes, over 8 m each side; never touch a ramp.
            var joints=new List<float>();float sum=0;
            for(int i=1;i<road.points.Length-1;i++){
                sum+=Vector3.Distance(road.points[i-1],road.points[i]);var a=(road.points[i]-road.points[i-1]).normalized;var b=(road.points[i+1]-road.points[i]).normalized;
                if(Math.Abs(a.y-b.y)>.10f&&!flights.Any(f=>road.Relative(sum,f.approachStation)<road.Relative(f.endStation,f.approachStation))&&(joints.Count==0||sum-joints[^1]>16))joints.Add(sum);
            }
            Vector3 Point(float s){var p=road.At(s,out _);foreach(float join in joints)if(Math.Abs(s-join)<8){var a=road.At(join-8,out var fa);var b=road.At(join+8,out var fb);float t=(s-join+8)/16,t2=t*t,t3=t2*t;p.y=(2*t3-3*t2+1)*a.y+(t3-2*t2+t)*fa.y*16+(-2*t3+3*t2)*b.y+(t3-t2)*fb.y*16;break;}return p;}
            float Width(float s){float w=7;foreach(var flight in flights){float along=road.Relative(s,flight.approachStation),end=road.Relative(flight.endStation,flight.approachStation);if(along<end){float lip=road.Relative(road.Project(flight.lip,out _),flight.approachStation);float target=along<=lip?14:24;w=Math.Max(w,Mathf.Lerp(7,target,Smooth(0,30,Math.Min(along,end-along))));}}return w;}
            var segments=new List<(float a,float b)>();float at=0;foreach(var gap in gaps.OrderBy(g=>g.a)){segments.Add((at,gap.a));at=gap.b;}segments.Add((at,road.Length));
            var nav=new List<Vector3>();int number=0;
            foreach(var seg in segments){var p=new List<Vector3>();var widths=new List<float>();for(float s=seg.a;s<seg.b;s+=.75f){p.Add(Point(s));widths.Add(Width(s));}p.Add(Point(seg.b));widths.Add(Width(seg.b));var strip=new RepairStrip(p.ToArray(),7){widths=widths.ToArray()};RepairRibbon("main section "+number++,strip);nav.AddRange(p);}
            // Retain station metric for gates/flight metadata after local rounding.
            road.points=nav.Take(nav.Count-1).ToArray();road.Initialize();
            if(reverse)CR122RoundSummitJoin();
            Physics.SyncTransforms();
            foreach(var flight in flights){flight.approachStation=road.Project(flight.start,out _);flight.endStation=road.Project(flight.landingEnd,out _);}
            var branches=Object.FindObjectsByType<WoodlandRoute>();
            foreach(var branch in branches){
                if(reverse&&branch.title=="Downhill Ridge Cut"){RepairReverseCrossing(branch);continue;}
                var p=new List<Vector3>();for(float s=0;s<branch.Length;s+=.75f)p.Add(branch.At(s,out _));p.Add(branch.points[^1]);
                // The first arrow was on a raised intersecting branch edge. Blend
                // the mouth onto the main surface before the two roads separate.
                for(int i=0;i<p.Count;i++){var q=p[i];float s=road.Project(q,out float d);var main=road.At(s,out _);if(Math.Abs(q.y-main.y)<6)q.y=Mathf.Lerp(main.y,q.y,Smooth(7,16,d));p[i]=q;}
                branch.points=p.ToArray();branch.Initialize();RepairRibbon(branch.title,new(branch.points,branch.halfWidth),true);
            }
            foreach(var branch in branches){branch.entryRoad=road.Project(branch.points[0],out _);branch.exitRoad=road.Project(branch.points[^1],out _);branch.bypassedGates=Enumerable.Range(1,owner.gates.Length-1).Where(i=>road.Relative(road.Project(owner.gates[i].transform.position,out _),branch.entryRoad)<road.Relative(branch.exitRoad,branch.entryRoad)).ToArray();}
            RepairSubgrade();Physics.SyncTransforms();
            foreach(var sign in Object.FindObjectsByType<PhysicalSign>().Where(s=>s.name.StartsWith("BIG FLIGHT AHEAD"))){
                var flight=flights.OrderBy(f=>Vector3.Distance(f.lip,sign.transform.position)).First();
                var p=flight.start+flight.forward*120+Vector3.Cross(Vector3.up,flight.forward)*19;p.y=CR105Authoring.Ground(p);sign.transform.SetPositionAndRotation(p,Quaternion.LookRotation(flight.forward));
            }
            // Paint has no colliders. Seat existing arrows back on the new surface.
            foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.Contains("arrow")&&m.transform.position.x>650)){
                if(mf.transform.position==Vector3.zero)continue;var p=mf.transform.position;p.y=CR105Authoring.Ground(p)+.10f;mf.transform.position=p;
            }
            owner.courseId=reverse?"mountain-reverse-v4-crossing":"mountain-forward-v3-continuous";
            Save();RepairSanity(reverse?"reverse":"forward");
        }
        public static void CR122RoundSummitJoin(bool refreshBranch=false)
        {
            if(GameObject.Find("CR122 rounded summit junction"))return;
            owner=Object.FindAnyObjectByType<RaceDirector>();worldRoot=GameObject.Find("CR122 continuous mountain support").transform;
            var road=owner.road;float join=road.Project(new Vector3(990,164,115),out _);
            var a=road.At(join-18,out var fa);var b=road.At(join+18,out var fb);float length=Vector3.Distance(a,b);
            Vector3 Map(Vector3 p){float s=road.Project(p,out _);if(Math.Abs(s-join)>=18)return p;float t=(s-join+18)/36,t2=t*t,t3=t2*t;return (2*t3-3*t2+1)*a+(t3-2*t2+t)*fa*length+(-2*t3+3*t2)*b+(t3-t2)*fb*length;}
            var mf=GameObject.Find("Ground_CR122 main section 0").GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;
            var centers=Enumerable.Range(0,v.Length/2).Select(i=>mf.transform.TransformPoint((v[i*2]+v[i*2+1])*.5f)-Vector3.up*.04f).ToArray();
            var strip=new RepairStrip(centers.Select(Map).ToArray(),7){widths=Enumerable.Range(0,v.Length/2).Select(i=>Vector3.Distance(v[i*2],v[i*2+1])*.5f).ToArray()};
            var nav=road.points.Select(Map).ToArray();mf.gameObject.SetActive(false);RepairRibbon("main section 0",strip);
            road.points=nav;
            typeof(RaceRoad).GetField("distance",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(road,null);road.Initialize();Physics.SyncTransforms();
            if(refreshBranch){
                var branch=Object.FindObjectsByType<WoodlandRoute>().Single(r=>r.title=="Summit Traverse");
                var old=GameObject.Find("Ground_CR122 Summit Traverse");old.SetActive(false);RepairRibbon("Summit Traverse",new(branch.points,branch.halfWidth),true);
                foreach(var f in owner.GetComponent<MountainFlights>().flights){f.approachStation=road.Project(f.start,out _);f.endStation=road.Project(f.landingEnd,out _);}
                foreach(var br in Object.FindObjectsByType<WoodlandRoute>()){br.entryRoad=road.Project(br.points[0],out _);br.exitRoad=road.Project(br.points[^1],out _);}
                RepairSubgrade();
            }
            new GameObject("CR122 rounded summit junction").transform.SetParent(worldRoot);Save();
        }
        static void RepairReverseCrossing(WoodlandRoute branch)
        {
            var road=owner.road;var oldEntry=branch.points[0];
            var crossing=road.At(road.Project(new Vector3(728.32f,86.49f,-134.55f),out _),out var mainForward);
            var forward=Vector3.Cross(Vector3.up,mainForward).normalized;
            if(Vector3.Dot(forward,crossing-oldEntry)<0)forward=-forward;
            var basePoint=crossing-forward*61;basePoint.y=crossing.y;
            var lip=crossing-forward*16;lip.y=crossing.y+6.5f;
            var landing=crossing+forward*18;landing.y=crossing.y-1.5f;
            var run=Enumerable.Range(0,91).Select(i=>{float s=i*.5f;return basePoint+forward*s+Vector3.up*(6.5f*s*s/(45*45));}).ToArray();
            var entry=RepairHermite(oldEntry,basePoint,(branch.At(8,out _)-oldEntry).normalized,forward);
            var catchEnd=landing+forward*27;catchEnd.y=landing.y-3;
            var catchStrip=RepairHermite(landing,catchEnd,(forward-Vector3.up*.22f).normalized,forward);
            var end=road.At(road.Project(new Vector3(722,79,-48),out _),out var exitForward);
            var exit=RepairHermite(catchEnd,end,forward,exitForward);
            var approach=entry.Concat(run.Skip(1)).ToArray();var departure=catchStrip.Concat(exit.Skip(1)).ToArray();
            branch.points=approach.Concat(departure).ToArray();branch.Initialize();branch.halfWidth=4.2f;branch.aiValidated=false;branch.entrySpeed=0;branch.recommendedSpeed=28;
            RepairRibbon("reverse shortcut runway",new(approach,4.2f),true);RepairRibbon("reverse shortcut far-side landing",new(departure,5),true);
            ExcludeRecovery(basePoint-forward*30,landing-forward*2,12);
            var timber=Mat("CR120 weathered summit timber",new(.27f,.22f,.15f));
            // The wall runs along the FAR edge of the normal road. It is below
            // the flight arc and never spans the normal road's driving width.
            float cs=road.Project(crossing,out _);
            for(float ds=-40;ds<=38;ds+=3){var p=road.At(cs+ds,out var f);var right=Vector3.Cross(Vector3.up,f).normalized;if(Vector3.Dot(right,forward)<0)right=-right;p+=right*9;p.y=road.At(cs+ds,out _).y;
                var wall=Part(worldRoot,"Far-side shortcut separator",p+Vector3.up*1.5f,new(.65f,3,3.2f),timber,true);wall.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));}
            // Remove the obsolete at-grade final stretch and its tempting gold paint.
            foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.Contains("gold")||m.name.Contains("Gold")))if(mf.transform.position.x>680&&mf.transform.position.x<820&&mf.transform.position.z< -90)mf.gameObject.SetActive(false);
            ClearCompleteTrees(p=>Near(p,approach,out _)<8||Near(p,departure,out _)<9);
            var template=Object.FindObjectsByType<PhysicalSign>().First(s=>s.name.StartsWith("MAIN ROUTE >>>"));var board=Object.Instantiate(template.gameObject,worldRoot);board.name="Shortcut jump over main road";board.transform.SetPositionAndRotation(basePoint-Vector3.Cross(Vector3.up,forward)*8,Quaternion.LookRotation(forward));board.GetComponentInChildren<TextMesh>().text="SHORTCUT JUMP\nSTRAIGHT / CLEAR THE MAIN ROAD";
            File.WriteAllText(CR122Dir+"/shortcut-crossing.txt",$"Main crossing {crossing}; lip {lip}; landing {landing}; 34m gap; 45m curved run-up, 6.5m rise. Far-side separator 3m high, 9m from main centre.");
        }
        public static void CR122Laurel()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();
            if(GameObject.Find("CR122 Laurel local jump"))throw new Exception("Already repaired");worldRoot=new GameObject("CR122 Laurel local jump").transform;repairStrips.Clear();
            var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");
            var drive=GameObject.Find("House 3 valley driveway").GetComponent<RaceRoad>();
            float crossing=0,best=float.MaxValue;
            for(float s=branch.Length*.5f;s<branch.Length-40;s+=.5f){var p=branch.At(s,out _);float d=Near(p,drive.points,out _);if(d<best){best=d;crossing=s;}}
            float begin=crossing-65,lipStation=crossing-9,landStation=crossing+9,end=crossing+55;
            var original=branch.points.ToArray();var approach=new List<Vector3>();var landing=new List<Vector3>();
            for(float s=begin;s<=lipStation;s+=.5f){var p=branch.At(s,out _);float t=Mathf.Clamp01((s-(lipStation-28))/28);p.y+=3.8f*t*t;approach.Add(p);}
            for(float s=landStation;s<=end;s+=.5f){var p=branch.At(s,out _);p.y-=1.2f*(1-Smooth(landStation,end,s));landing.Add(p);}
            // Original local route heights carry the rider above the driveway;
            // the house and valley/driveway remain at their current elevations.
            RepairRibbon("Laurel smooth takeoff",new(approach.ToArray(),5));RepairRibbon("Laurel supported catch",new(landing.ToArray(),6));
            var before=new List<Vector3>();for(float s=0;s<begin;s+=.75f)before.Add(branch.At(s,out _));
            var after=new List<Vector3>();for(float s=end+.75f;s<branch.Length;s+=.75f)after.Add(branch.At(s,out _));after.Add(original[^1]);
            branch.points=before.Concat(approach).Concat(landing).Concat(after).ToArray();branch.Initialize();
            ExcludeRecovery(approach[0],landing[0],12);RepairSubgrade();CR122LaurelJoins();
            // Keep the driveway's own mesh/collider and property objects intact.
            ClearCompleteTrees(p=>Near(p,approach.ToArray(),out _)<7||Near(p,landing.ToArray(),out _)<8);
            Save();RepairSanity("laurel");File.WriteAllText(CR122Dir+"/laurel-repair.txt",$"Crossing station {crossing:F2}; lip {approach[^1]}; landing {landing[0]}; 18m local gap; 28m curved ramp / 3.8m rise. Existing house and valley retained.");
        }
        public static void CR122LaurelJoins()
        {
            owner=Object.FindAnyObjectByType<RaceDirector>();
            var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");
            Vector3 Edge(string name,bool last){var mf=GameObject.Find(name).GetComponent<MeshFilter>();var v=mf.sharedMesh.vertices;int n=last?v.Length-2:0;return mf.transform.TransformPoint((v[n]+v[n+1])*.5f)-Vector3.up*.04f;}
            var ends=new[]{Edge("Ground_CR122 Laurel smooth takeoff",false),Edge("Ground_CR122 Laurel supported catch",true)};
            // Feather the isolated repair back to terrain at both ends, so
            // lowering its subgrade cannot create a new lip at the approach.
            Terrain("CR122 Laurel end joins",p=>{
                float distance=ends.Min(e=>Vector2.Distance(new(p.x,p.z),new(e.x,e.z)));if(distance>18)return p;
                float d=Near(p,branch.points,out var at);float blend=(1-Smooth(12,18,distance))*(1-Smooth(6,9,d));
                p.y=Mathf.Lerp(p.y,at.y-.04f,blend);return p;
            });
            Save();
        }
        public static void CR122SupportCheck()
        {
            var rows=new List<string>();
            foreach(var scene in new[]{"MountainLoop","MountainLoopReverse","StreetLoopReverse"}){
                EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");Physics.SyncTransforms();int checkedPoints=0,misses=0;
                foreach(var mf in Object.FindObjectsByType<MeshFilter>().Where(m=>m.name.StartsWith("Ground_CR122"))){
                    var mesh=mf.sharedMesh;var collider=mf.GetComponent<MeshCollider>();if(!collider||collider.sharedMesh!=mesh)throw new Exception("Mesh/collider mismatch");
                    var v=mesh.vertices;var tri=mesh.triangles;
                    // One sample per 40 triangles is a structural support check,
                    // not a driving/tuning pass. Test each mesh's actual triangles.
                    for(int i=0;i<tri.Length;i+=120){var p=mf.transform.TransformPoint((v[tri[i]]+v[tri[i+1]]+v[tri[i+2]])/3);checkedPoints++;
                        // A wide bend can intersect an adjacent triangle a few
                        // centimetres above this triangle's centroid.
                        if(!collider.Raycast(new Ray(p+Vector3.up,Vector3.down),out var hit,2)||Math.Abs(hit.point.y-p.y)>.08f)misses++;
                    }
                }
                rows.Add(scene+": supported mesh samples="+checkedPoints+", misses="+misses);if(misses>0)throw new Exception(rows[^1]);
            }
            File.WriteAllLines(CR122Dir+"/support-check.txt",rows);PlayerSettings.bundleVersion="0.22.0-review1";AssetDatabase.SaveAssets();
        }
        static void RepairSanity(string label)
        {
            Physics.SyncTransforms();var rows=new List<string>();
            foreach(var mf in worldRoot.GetComponentsInChildren<MeshFilter>())if(mf.name.StartsWith("Ground_CR122")){
                var collider=mf.GetComponent<MeshCollider>();if(!collider||collider.sharedMesh!=mf.sharedMesh)throw new Exception("Visible/collision mesh mismatch "+mf.name);
                if(mf.sharedMesh.vertices.Any(p=>!float.IsFinite(p.x)||!float.IsFinite(p.y)||!float.IsFinite(p.z)))throw new Exception("Nonfinite mesh");
                rows.Add(mf.name+" shared visible/collision mesh; vertices="+mf.sharedMesh.vertexCount);
            }
            File.WriteAllLines(CR122Dir+"/sanity-"+label+".txt",rows);
        }
    }
}
