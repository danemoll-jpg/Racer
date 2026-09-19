using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static class ForestCourseBuild
    {
        public const string Folder="Assets/Track/ForestLoop", Evidence="Docs/CR056";
        static RaceRoad road,street; static WoodlandRoute cave; static Transform root;
        static Vector3[] main; static Material rock,water,leaf,bark,dirt; static Vector3 lake=new(642,78,-20);
        static readonly float[] starts={340,610,900,1020,1310,1540};
        static readonly float[] runs={34,42,24,28,36,38}, heights={4,6,2.5f,3,6,4.5f}, gaps={16,23,9,12,24,18};
        static readonly string[] titles={"J1 Creek crossing","J2 Deep gully","J3 Root roller","J4 Linked kicker","J5 Ridge drop","J6 Homeward leap"};
        static float Blend(float a,float b,float v)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,v));
        static Material Mat(string name,Color c)
        {var m=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/"+name+".mat");if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=c,enableInstancing=true};AssetDatabase.CreateAsset(m,Folder+"/"+name+".mat");}return m;}
        static Mesh Save(Mesh mesh,string name)
        {var old=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/"+name+".asset");if(old){EditorUtility.CopySerialized(mesh,old);Object.DestroyImmediate(mesh);return old;}AssetDatabase.CreateAsset(mesh,Folder+"/"+name+".asset");return mesh;}
        static Vector3[] Curve(Vector3[] knots,bool closed)
        {
            var points=new List<Vector3>();int n=knots.Length;Vector3 K(int i)=>knots[closed?(i+n)%n:Mathf.Clamp(i,0,n-1)];
            for(int i=0;i<(closed?n:n-1);i++){var a=K(i-1);var b=K(i);var c=K(i+1);var d=K(i+2);int count=Mathf.CeilToInt(Vector3.Distance(b,c));for(int j=0;j<count;j++){float t=j/(float)count;points.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}}
            if(!closed)points.Add(knots[^1]);return points.ToArray();
        }
        static float Shape(float x,float run,float h,float gap)
        {
            if(x<0||x>run+gap+75)return 0;
            if(x<run)return h*(x/run)*(x/run);
            if(x<run+8)return Mathf.Lerp(h,-5,Blend(run,run+8,x));
            return -5*(1-Blend(run+gap,run+gap+75,x));
        }
        static void Sculpt(Vector3[] points,float[] js,float[] rs,float[] hs,float[] gs)
        {float distance=0;var original=(Vector3[])points.Clone();for(int i=0;i<points.Length;i++){if(i>0)distance+=Vector3.Distance(original[i-1],original[i]);for(int j=0;j<js.Length;j++)points[i].y+=Shape(distance-js[j],rs[j],hs[j],gs[j]);}}
        static float Near(Vector3 p,Vector3[] path,out Vector3 at)
        {float best=float.MaxValue;at=default;for(int i=1;i<path.Length;i++){var a=path[i-1];var v=path[i]-a;v.y=0;var q=p-a;q.y=0;float t=Mathf.Clamp01(Vector3.Dot(q,v)/Mathf.Max(.001f,v.sqrMagnitude));float dd=(q-v*t).sqrMagnitude;if(dd<best){best=dd;at=Vector3.Lerp(a,path[i],t);}}return Mathf.Sqrt(best);}
        static Transform Primitive(string name,PrimitiveType type,Vector3 p,Vector3 size,Material mat,bool collision=false)
        {var g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collision)Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
        static void Sign(Vector3 p,Vector3 f,string text)
        {var t=new GameObject(text).AddComponent<TextMesh>();t.transform.SetParent(root);t.transform.SetPositionAndRotation(p+Vector3.up*2.5f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));t.text=text;t.fontSize=56;t.characterSize=.12f;t.anchor=TextAnchor.MiddleCenter;t.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Woodland/Lettering.mat");}
        public static void Apply()
        {
            if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Saved edit mode required");
            Directory.CreateDirectory(Folder);Directory.CreateDirectory(Evidence);AssetDatabase.Refresh();
            // Read original assets without ever saving or altering the original course.
            EditorSceneManager.OpenScene(StreetLoopBuilder.ScenePath);
            var sources=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>().ToDictionary(m=>m.name,m=>m.sharedMesh);
            EditorSceneManager.OpenScene(LakeCourseBuild.ScenePath);var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var race=Object.FindAnyObjectByType<RaceDirector>();
            var sites=GameObject.Find("Remembered houses and approximate buildings").transform.Cast<Transform>().ToArray();
            File.WriteAllLines(Evidence+"/sites-before.txt",sites.Select(t=>t.name+" "+t.position.ToString("F5")+" "+t.rotation.ToString("F5")));
            var previous=GameObject.Find("Friend's lake");if(previous)File.WriteAllText(Evidence+"/lake-inspection.txt",$"Existing water {previous.transform.position}, scale {previous.transform.localScale}; grid {race.vehicle.GetComponent<VehicleRespawn>().spawnPoint.position}. Retain lake centre and water elevation; open shoreline and move grid behind friend.\n");
            var template=Object.Instantiate(race.gates[1]);template.gameObject.SetActive(false);
            var old=GameObject.Find("Lake & Woods circuit");if(old)Object.DestroyImmediate(old);
            var prior=GameObject.Find("CR056 Forest Loop");if(prior)Object.DestroyImmediate(prior);
            foreach(var branch in Object.FindObjectsByType<WoodlandRoute>())Object.DestroyImmediate(branch);
            root=new GameObject("CR056 Forest Loop").transform;
            street=Object.FindObjectsByType<RaceRoad>().First(r=>r.points.Length>100);street.Initialize();race.ambientRoad=street;
            road=new GameObject("Forest race route").AddComponent<RaceRoad>();road.transform.SetParent(root);road.forestTrail=true;
            road.points=Curve(new Vector3[]{new(566,82,-40),new(566,82,40),new(551,81,118),new(458,77,184),new(350,68,242),new(230,54,295),new(65,37,255),new(-85,23,145),new(-125,20,0),new(-65,23,-110),new(80,33,-145),new(200,44,-175),new(310,50,-245),new(410,55,-290),new(545,65,-240),new(608,77,-138),new(574,82,-85)},true);
            for(int i=0;i<road.points.Length;i++){float distance=Near(road.points[i],street.points,out var streetPoint);float shoulder=48+72*Blend(400,450,road.points[i].x)*(1-Blend(-180,-110,road.points[i].z));if(distance<shoulder)road.points[i].y=Mathf.Lerp(streetPoint.y,road.points[i].y,Blend(14,shoulder,distance));}
            Sculpt(road.points,starts,runs,heights,gaps);road.Initialize();race.road=road;race.courseId="lake-v2-forest";race.courseName="Forest Loop";
            var layout=root.gameObject.AddComponent<ForestLayout>();layout.jumpStarts=starts;layout.jumpEnds=starts.Select((s,i)=>s+runs[i]+gaps[i]+75).ToArray();layout.jumpNames=titles;
            cave=new GameObject("Echo Cave shortcut").AddComponent<WoodlandRoute>();cave.transform.SetParent(root);cave.title="Echo Cave";cave.entryRoad=565;cave.exitRoad=1250;cave.halfWidth=4.2f;cave.recommendedSpeed=32;cave.entryInset=65;cave.entryMargin=2;
            var a=road.At(cave.entryRoad,out var af);var b=road.At(cave.exitRoad,out var bf);var delta=b-a;var right=Vector3.Cross(Vector3.up,delta).normalized;
            cave.points=Curve(new[]{a,a+af*20,new Vector3(150,47,260),new Vector3(100,41,180),new Vector3(60,35,90),new Vector3(-45,24,-85),b-bf*15,b},false);
            for(int i=0;i<cave.points.Length;i++){float shared=Near(cave.points[i],road.points,out var sharedRoad);if(shared<25)cave.points[i].y=Mathf.Lerp(sharedRoad.y,cave.points[i].y,Blend(5,25,shared));}
            Sculpt(cave.points,new[]{360f},new[]{27f},new[]{2.5f},new[]{12f});cave.Initialize();
            rock=Mat("Cave granite",new(.26f,.29f,.25f));water=Mat("Lake blue water",new(.055f,.38f,.52f));water.SetFloat("_Smoothness",.86f);leaf=Mat("Undergrowth",new(.17f,.29f,.08f));bark=Mat("Tree bark",new(.24f,.16f,.085f));dirt=Mat("Trail soil",new(.38f,.23f,.11f));
            main=road.points.Append(road.points[0]).ToArray();
            // One shared collision/render heightfield; no separate ramp slabs or overlapping lip colliders.
            int count=0;foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
            {
                if(!sources.TryGetValue(mf.name,out var source))continue;var original=source.vertices;
                bool nearby=original.Any(p=>p.x>-170&&p.x<745&&p.z>-335&&p.z<345);
                if(!nearby){mf.sharedMesh=source;if(mf.TryGetComponent<MeshCollider>(out var mc))mc.sharedMesh=source;continue;}
                var mesh=Object.Instantiate(source);var v=new List<Vector3>(mesh.vertices);var colors=new List<Color>(mesh.colors);if(colors.Count!=v.Count)colors=Enumerable.Repeat(Color.white,v.Count).ToList();var triangles=new List<int>();var edges=new Dictionary<(int,int),int>();
                int Mid(int i,int j){var key=i<j?(i,j):(j,i);if(edges.TryGetValue(key,out int index))return index;index=v.Count;v.Add((v[i]+v[j])*.5f);colors.Add((colors[i]+colors[j])*.5f);edges[key]=index;return index;}
                var oldTris=mesh.triangles;for(int i=0;i<oldTris.Length;i+=3){int x=oldTris[i],y=oldTris[i+1],z=oldTris[i+2],xy=Mid(x,y),yz=Mid(y,z),zx=Mid(z,x);triangles.AddRange(new[]{x,xy,zx,xy,y,yz,zx,yz,z,xy,yz,zx});}
                for(int i=0;i<v.Count;i++)
                {
                    var p=v[i];if(p.x<-170||p.x>745||p.z<-335||p.z>345)continue;
                    float d=Near(p,main,out var at),cd=Near(p,cave.points,out var cp),md=d,my=at.y;if(cd<d){d=cd;at=cp;}if(md<20&&cd<20){float mw=Mathf.Pow(1-Blend(0,20,md),2),cw=Mathf.Pow(1-Blend(0,20,cd),2);at.y=(my*mw+cp.y*cw)/(mw+cw);}
                    float sd=Near(p,street.points,out var sp),site=sites.Min(t=>Vector2.Distance(new(p.x,p.z),new(t.position.x,t.position.z)));
                    float preserve=Blend(8,16,sd)*Blend(22,35,site);
                    float width=3.6f;float station=d<19?road.Project(at,out _):200;if(station<110||station>road.Length-25)width=6;
                    else if(Mathf.Repeat(station,260)<42)width=5;
                    if(cd<=d+.001f&&cave.Project(cp,out _) <135)width=Mathf.Lerp(6.5f,3.6f,Blend(85,135,cave.Project(cp,out _)));
                    if(d<19){p.y=Mathf.Lerp(p.y,at.y,(1-Blend(width+1,19,d))*preserve);float rut=.035f*(Mathf.Exp(-Mathf.Pow((d-1.1f)*3,2)));p.y-=rut*preserve;colors[i]=Color.Lerp(colors[i],new Color(.39f,.265f,.135f),(1-Blend(width-.5f,width+1,d))*preserve);}
                    float ld=Vector2.Distance(new(p.x,p.z),new(lake.x,lake.z));
                    if(ld<87&&d>7){p.y=Mathf.Lerp(p.y,74.5f,(1-Blend(61,87,ld))*Blend(8,18,sd)*Blend(23,38,site));colors[i]=Color.Lerp(colors[i],new Color(.46f,.41f,.26f),1-Blend(61,85,ld));}
                    v[i]=p;
                }
                mesh.Clear();mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.SetVertices(v);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,"Terrain-"+count++);mf.sharedMesh=mesh;mf.GetComponent<MeshCollider>().sharedMesh=mesh;
            }
            Physics.SyncTransforms();
            Primitive("Friend's lake - visible shoreline",PrimitiveType.Cylinder,lake,new(134,.035f,134),water);
            for(int i=0;i<starts.Length;i++)
            {var p=road.At(starts[i]-18,out var f);Sign(p-Vector3.Cross(Vector3.up,f).normalized*4.8f,f,titles[i]);
                if(i==0){p=road.At(starts[i]+runs[i]+12,out f);var creek=Primitive("J1 flowing creek",PrimitiveType.Cube,p+Vector3.up*2.2f,new(65,.05f,20),water);creek.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up));}}
            CaveShell();Vegetation(sites);
            var gates=new List<RaceGate>();foreach(float s in new[]{65f,260f,850f,1490f,1900f})
            {if(s>road.Length-50)continue;var g=Object.Instantiate(template,root);g.gameObject.SetActive(true);int i=gates.Count;g.name=i==0?"Forest START FINISH":$"Forest CP {i:00}";var p=road.At(s,out var f);g.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));g.halfWidth=8;g.upperHeight=30;foreach(var t in g.GetComponentsInChildren<TextMesh>())if(t.name=="Gate label")t.text=i==0?"FOREST LOOP START / FINISH":$"CP {i:00}";gates.Add(g);}
            Object.DestroyImmediate(template.gameObject);race.gates=gates.ToArray();cave.bypassedGates=Enumerable.Range(1,gates.Count-1).Where(i=>{float s=road.Project(gates[i].transform.position,out _);return s>cave.entryRoad&&s<cave.exitRoad;}).ToArray();
            var spawn=race.vehicle.GetComponent<VehicleRespawn>().spawnPoint;spawn.position=road.At(30,out var face)+Vector3.up*.7f;spawn.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(face,Vector3.up));race.vehicle.transform.SetPositionAndRotation(spawn.position,spawn.rotation);
            Sign(road.At(2,out var sf)+Vector3.left*5,sf,"FOREST LOOP / MOTO + ATV");Sign(a+Vector3.Cross(Vector3.up,af)*5,af,"ECHO CAVE / OPTIONAL / BRAKE FOR BENDS");
            EditorUtility.SetDirty(race);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            File.WriteAllLines(Evidence+"/sites-after.txt",sites.Select(t=>t.name+" "+t.position.ToString("F5")+" "+t.rotation.ToString("F5")));
            File.WriteAllText(Evidence+"/layout.txt",$"Forest Loop {road.Length:F1}m; six main jumps (previously three), one cave jump. Terrain tiles {count}. Cave {cave.Length:F1}m versus main {cave.exitRoad-cave.entryRoad}m; bypass {string.Join(",",cave.bypassedGates)}\n"+string.Join("\n",starts.Select((s,i)=>$"{titles[i]} approach {s}, run {runs[i]}, height {heights[i]}, gap {gaps[i]}, landing +75m; {road.At(s,out _)}")));
            Capture("grid",spawn.position+new Vector3(0,3,-7),spawn.position+new Vector3(25,0,35));Capture("lake-opening",road.At(90,out _)+new Vector3(0,4,-7),lake);Capture("cave-entrance",a-af*12+Vector3.up*3,a+af*30+Vector3.up*2);
            Map();
        }
        static void CaveShell()
        {
            float begin=18,end=cave.Length-20;while(begin<cave.Length*.4f&&Near(cave.At(begin,out _),main,out _)<18)begin+=2;
            // Stop at the FIRST return toward the main trail, not a later point beyond a crossing.
            for(float s=begin+35;s<end;s+=2)if(Near(cave.At(s,out _),main,out _)<18){end=s-2;break;}
            var vertices=new List<Vector3>();var tris=new List<int>();int sections=Mathf.CeilToInt((end-begin)/2);
            for(int i=0;i<=sections;i++){float s=Mathf.Lerp(begin,end,i/(float)sections);var p=cave.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;float roof=Mathf.Max(p.y,cave.At(s-30,out _).y,cave.At(s+30,out _).y)+13;for(int j=0;j<=12;j++){float angle=j*Mathf.PI/12;vertices.Add(p+r*(Mathf.Cos(angle)*(s<145?7f:6.5f))+Vector3.up*(.2f+Mathf.Sin(angle)*(roof-p.y)));}}
            rock.SetFloat("_Cull",0);
            rock.SetFloat("_Smoothness",.04f);
            for(int i=0;i<sections;i++)for(int j=0;j<12;j++){int a=i*13+j,b=a+13;tris.AddRange(new[]{a,b,a+1,a+1,b,b+1});}
            var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,"Echo cave rock enclosure");var go=new GameObject("Echo Cave enclosed rock",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=rock;go.GetComponent<MeshCollider>().sharedMesh=mesh;
            for(float s=begin;s<end;s+=22){var p=cave.At(s,out var f);var r=Vector3.Cross(Vector3.up,f).normalized;foreach(int side in new[]{-1,1})Primitive("Cave rock buttress",PrimitiveType.Sphere,p+r*side*7+Vector3.up*3,new(5,9,6),rock);var lamp=new GameObject("Cave warm wayfinding").AddComponent<Light>();lamp.transform.SetParent(root);lamp.transform.position=p+Vector3.up*6;lamp.type=LightType.Point;lamp.range=18;lamp.intensity=2.5f;lamp.color=new(1,.76f,.43f);lamp.shadows=LightShadows.None;}
            Sign(cave.At(330,out var direction)+Vector3.up,direction,"CAVE GAP / STRAIGHT LANDING");
        }
        public static void RefreshCave()
        {
            root=GameObject.Find("CR056 Forest Loop").transform;cave=Object.FindAnyObjectByType<WoodlandRoute>();road=Object.FindAnyObjectByType<RaceDirector>().road;main=road.points.Append(road.points[0]).ToArray();rock=Mat("Cave granite",new(.26f,.29f,.25f));
            foreach(var t in root.Cast<Transform>().Where(t=>t.name=="Echo Cave enclosed rock"||t.name=="Cave rock buttress"||t.name=="Cave warm wayfinding"||t.name=="CAVE GAP / STRAIGHT LANDING").ToArray())Object.DestroyImmediate(t.gameObject);
            CaveShell();EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);AssetDatabase.SaveAssets();
        }
        public static void AlignCaveFlight()
        {
            var race=Object.FindAnyObjectByType<RaceDirector>();road=race.road;street=race.ambientRoad;main=road.points.Append(road.points[0]).ToArray();cave=Object.FindAnyObjectByType<WoodlandRoute>();
            var previous=cave.points.ToArray();var a=road.At(cave.entryRoad,out var af);var b=road.At(cave.exitRoad,out var bf);var right=Vector3.Cross(Vector3.up,b-a).normalized;
            cave.points=Curve(new[]{a,a+af*20,new Vector3(150,47,260),new Vector3(100,41,180),new Vector3(60,35,90),new Vector3(-45,24,-85),b-bf*15,b},false);
            for(int i=0;i<cave.points.Length;i++){float shared=Near(cave.points[i],road.points,out var sharedRoad);if(shared<25)cave.points[i].y=Mathf.Lerp(sharedRoad.y,cave.points[i].y,Blend(5,25,shared));}
            Sculpt(cave.points,new[]{360f},new[]{27f},new[]{2.5f},new[]{12f});
            var sites=GameObject.Find("Remembered houses and approximate buildings").transform.Cast<Transform>().ToArray();
            foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
            {
                if(!AssetDatabase.GetAssetPath(mf.sharedMesh).StartsWith(Folder))continue;var mesh=mf.sharedMesh;var v=mesh.vertices;bool changed=false;
                for(int i=0;i<v.Length;i++)
                {var p=v[i];if(p.x< -120||p.x>240||p.z< -170||p.z>325)continue;float d=Near(p,previous,out var old);if(d>19)continue;Near(p,cave.points,out var now);if(Mathf.Abs(now.y-old.y)<.001f)continue;if(Near(p,main,out _)<=d)continue;
                    float preserve=Blend(8,16,Near(p,street.points,out _))*Blend(22,35,sites.Min(t=>Vector2.Distance(new(p.x,p.z),new(t.position.x,t.position.z))));
                    p.y+=(now.y-old.y)*(1-Blend(4.6f,19,d))*preserve;v[i]=p;changed=true;
                }
                if(!changed)continue;mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);var mc=mf.GetComponent<MeshCollider>();mc.sharedMesh=null;mc.sharedMesh=mesh;
            }
            cave.entryInset=65;cave.entryMargin=2;EditorUtility.SetDirty(cave);Physics.SyncTransforms();RefreshCave();RefreshLandingClearance();ExportMap();
            File.WriteAllText(Evidence+"/cave-flight.txt","Cave gap moved from the entry bend to the long diagonal at 360m. Takeoff 387m; 12m gap; supported 75m landing. Ceiling follows a raised clearance envelope.\n");
        }
        static void Vegetation(Transform[] sites)
        {
            // Existing batched forest remains. Only new close corridor detail is added, combined by cell.
            var batches=new Dictionary<Vector2Int,List<CombineInstance>>();var crown=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/IrregularCrown.asset");var trunk=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Vegetation/Phase6/Trunk.asset");var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Vegetation/Phase6/Forest.mat");
            var random=new System.Random(56);
            var woods=GameObject.Find("Woods replacing later subdivisions");
            foreach(var box in woods.GetComponentsInChildren<BoxCollider>()){float d=Near(box.bounds.center,main,out _),cd=Near(box.bounds.center,cave.points,out _);if(Mathf.Min(d,cd)<7||LandingClearance(box.bounds.center)||Vector2.Distance(new(box.bounds.center.x,box.bounds.center.z),new(lake.x,lake.z))<76)Object.DestroyImmediate(box.gameObject);}
            // Rebuild the copied forest batches from their remaining collision authoring data.
            // Otherwise removed trunks remain visible in the old combined meshes.
            crown=Object.Instantiate(crown);crown.colors=Enumerable.Repeat(new Color(.27f,.39f,.17f),crown.vertexCount).ToArray();
            trunk=Object.Instantiate(trunk);trunk.colors=Enumerable.Repeat(new Color(.26f,.18f,.10f),trunk.vertexCount).ToArray();
            foreach(var box in woods.GetComponentsInChildren<BoxCollider>())
            {
                var bounds=box.bounds;var p=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);float d=Near(p,main,out _),cd=Near(p,cave.points,out _);float h=bounds.size.y*2;
                if(Mathf.Min(d,cd)<25){float y=Phase6Buildings.Ground(p);box.transform.position+=Vector3.up*(y-p.y);p.y=y;}
                float radius=Mathf.Clamp(Mathf.Min(d,cd)-4.5f,.5f,h*.34f);var cell=new Vector2Int(Mathf.FloorToInt(p.x/90),Mathf.FloorToInt(p.z/90));if(!batches.TryGetValue(cell,out var batch))batches[cell]=batch=new();
                batch.Add(new CombineInstance{mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.36f,Quaternion.identity,new(radius,h*.65f,radius))});
                batch.Add(new CombineInstance{mesh=trunk,transform=box.transform.localToWorldMatrix*Matrix4x4.TRS(box.center,Quaternion.identity,box.size)});
            }
            foreach(var mf in woods.GetComponentsInChildren<MeshFilter>())Object.DestroyImmediate(mf.gameObject);
            for(float s=120;s<road.Length-30;s+=5)
            foreach(int side in new[]{-1,1})
            {var p=road.At(s,out var f)+Vector3.Cross(Vector3.up,f).normalized*side*(7+(float)random.NextDouble()*9);if(LandingClearance(p)||Near(p,street.points,out _)<18||Near(p,cave.points,out _)<10||sites.Any(t=>Vector3.Distance(p,t.position)<36)||Vector2.Distance(new(p.x,p.z),new(lake.x,lake.z))<77)continue;p.y=Phase6Buildings.Ground(p);float h=9+(float)random.NextDouble()*11;var cell=new Vector2Int(Mathf.FloorToInt(p.x/90),Mathf.FloorToInt(p.z/90));if(!batches.TryGetValue(cell,out var batch))batches[cell]=batch=new();batch.Add(new CombineInstance{mesh=crown,transform=Matrix4x4.TRS(p+Vector3.up*h*.4f,Quaternion.Euler(0,random.Next(360),0),new(3,h*.65f,3))});batch.Add(new CombineInstance{mesh=trunk,transform=Matrix4x4.TRS(p+Vector3.up*h*.28f,Quaternion.identity,new(.45f,h*.6f,.45f))});
                var collider=new GameObject("Forest trunk collision").AddComponent<CapsuleCollider>();collider.transform.SetParent(root);collider.transform.position=p+Vector3.up*h*.3f;collider.height=h*.6f;collider.radius=.25f;
                if(random.Next(4)==0)Primitive("Trail fern",PrimitiveType.Sphere,p+Vector3.up*.4f,new(2,.9f,2),leaf);
            }
            foreach(var pair in batches){var m=new Mesh{indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.CombineMeshes(pair.Value.ToArray());m=Save(m,$"Close forest {pair.Key.x} {pair.Key.y}");var g=new GameObject("Forest detail batch",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(root);g.GetComponent<MeshFilter>().sharedMesh=m;g.GetComponent<MeshRenderer>().sharedMaterial=mat;}
            Object.DestroyImmediate(crown);Object.DestroyImmediate(trunk);
        }
        public static void FinishCaveApron()
        {
            var race=Object.FindAnyObjectByType<RaceDirector>();road=race.road;road.Initialize();cave=Object.FindAnyObjectByType<WoodlandRoute>();cave.recommendedSpeed=32;EditorUtility.SetDirty(cave);
            foreach(var mf in GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>())
            {
                if(!AssetDatabase.GetAssetPath(mf.sharedMesh).StartsWith(Folder))continue;var mesh=mf.sharedMesh;var v=mesh.vertices;bool changed=false;
                for(int i=0;i<v.Length;i++)
                {
                    var p=v[i];if(p.x<110||p.x>235||p.z<220||p.z>320)continue;float d=Near(p,cave.points,out var at),md=Near(p,road.points,out var mp);if(d>20)continue;
                    float s=cave.Project(at,out _);if(s>135)continue;float width=Mathf.Lerp(6.5f,4.6f,Blend(85,135,s));
                    if(md<20){float mw=Mathf.Pow(1-Blend(0,20,md),2),cw=Mathf.Pow(1-Blend(0,20,d),2);p.y=Mathf.Lerp(p.y,(mp.y*mw+at.y*cw)/(mw+cw),1-Blend(115,135,s));}else p.y=Mathf.Lerp(p.y,at.y,1-Blend(width,12,d));v[i]=p;changed=true;
                }
                if(!changed)continue;mesh.vertices=v;mesh.RecalculateNormals();mesh.RecalculateBounds();EditorUtility.SetDirty(mesh);var mc=mf.GetComponent<MeshCollider>();mc.sharedMesh=null;mc.sharedMesh=mesh;
            }
            root=GameObject.Find("CR056 Forest Loop").transform;Sign(road.At(cave.entryRoad-45,out var f)-Vector3.Cross(Vector3.up,f)*5,f,"CAVE LEFT / BRAKE FOR TURN");
            Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);AssetDatabase.SaveAssets();
            File.WriteAllText(Evidence+"/cave-apron.txt","Shared fork heights matched; first cave bend has a supported 13m apron tapering back to the narrow trail. Entry/exit require braking; clear interior recommended speed 32m/s.\n");
        }
        static bool LandingClearance(Vector3 p)
        {
            float d=Near(p,main,out var at);if(d>16)return false;float s=road.Project(at,out _);
            // Only the two measured long-flight landing fans need this wider tree clearance.
            foreach(int i in new[]{0,5})if(s>starts[i]+runs[i]+25&&s<starts[i]+190)return true;
            return false;
        }
        public static void RefreshLandingClearance()
        {
            root=GameObject.Find("CR056 Forest Loop").transform;var race=Object.FindAnyObjectByType<RaceDirector>();road=race.road;street=race.ambientRoad;cave=Object.FindAnyObjectByType<WoodlandRoute>();main=road.points.Append(road.points[0]).ToArray();leaf=Mat("Undergrowth",new(.17f,.29f,.08f));
            foreach(var t in root.Cast<Transform>().Where(t=>t.name=="Forest detail batch"||t.name=="Forest trunk collision"||t.name=="Trail fern").ToArray())Object.DestroyImmediate(t.gameObject);
            Vegetation(GameObject.Find("Remembered houses and approximate buildings").transform.Cast<Transform>().ToArray());
            EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);AssetDatabase.SaveAssets();
        }
        static void Capture(string name,Vector3 eye,Vector3 target)
        {var c=new GameObject("Evidence camera").AddComponent<Camera>();c.transform.position=eye;c.transform.LookAt(target);c.fieldOfView=70;c.farClipPlane=1800;var rt=new RenderTexture(1280,720,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Evidence+"/"+name+".png",t.EncodeToPNG());RenderTexture.active=old;Object.DestroyImmediate(c.gameObject);Object.DestroyImmediate(t);Object.DestroyImmediate(rt);}
        static void Map()
        {string P(Vector3 p)=>$"{(p.x+180).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)},{(360-p.z).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)}";var text=new System.Text.StringBuilder("<svg xmlns='http://www.w3.org/2000/svg' width='1100' height='850' viewBox='-20 -30 1000 800'><rect x='-20' y='-30' width='1000' height='800' fill='#18291c'/><text x='10' y='0' fill='white' font-size='22'>Forest Loop — six main jumps + Echo Cave</text>");text.Append($"<circle cx='{lake.x+180}' cy='{360-lake.z}' r='67' fill='#1687b3'/><polyline points='{string.Join(" ",main.Select(P))}' fill='none' stroke='#d5aa6c' stroke-width='7'/><polyline points='{string.Join(" ",cave.points.Select(P))}' fill='none' stroke='#adadbb' stroke-width='6'/>");for(int i=0;i<starts.Length;i++){var p=road.At(starts[i],out _);text.Append($"<circle cx='{p.x+180}' cy='{360-p.z}' r='7' fill='#ffbb33'/><text x='{p.x+190}' y='{354-p.z}' fill='white' font-size='15'>{titles[i]}</text>");}text.Append("<text x='750' y='330' fill='white'>START / friend’s lake</text><text x='140' y='270' fill='white'>Echo Cave / gap</text><text x='10' y='755' fill='white'>Gold: main trail · grey: optional cave · blue: lake · previous build: 3 main jumps</text></svg>");File.WriteAllText(Evidence+"/route-map.svg",text.ToString());}
        public static void ExportMap()
        {
            road=Object.FindAnyObjectByType<RaceDirector>().road;cave=Object.FindAnyObjectByType<WoodlandRoute>();main=road.points.Append(road.points[0]).ToArray();Map();
            var extra=new System.Text.StringBuilder();
            for(int i=0;i<starts.Length;i++)
            {
                var launch=road.At(starts[i]+runs[i],out _);var landing=road.At(starts[i]+runs[i]+gaps[i]+45,out _);
                extra.Append($"<circle cx='{launch.x+180}' cy='{360-launch.z}' r='5' fill='#ffe900'/><circle cx='{landing.x+180}' cy='{360-landing.z}' r='5' fill='#45dfc1'/><text x='{landing.x+188}' y='{376-landing.z}' fill='#45dfc1' font-size='12'>J{i+1} landing</text>");
            }
            var cl=cave.At(387,out _);var ca=cave.At(445,out _);extra.Append($"<circle cx='{cl.x+180}' cy='{360-cl.z}' r='6' fill='#ffe900'/><text x='{cl.x+190}' y='{360-cl.z}' fill='white' font-size='14'>Cave takeoff</text><circle cx='{ca.x+180}' cy='{360-ca.z}' r='6' fill='#45dfc1'/><text x='{ca.x+190}' y='{360-ca.z}' fill='#45dfc1' font-size='14'>Cave landing</text><text x='10' y='725' fill='white' font-size='15'>Yellow dots: takeoffs · turquoise dots: supported landing zones</text>");
            string path=Evidence+"/route-map.svg";File.WriteAllText(path,File.ReadAllText(path).Replace("</svg>",extra+"</svg>"));
        }
    }
}










