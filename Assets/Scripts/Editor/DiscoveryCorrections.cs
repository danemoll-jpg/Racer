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
        const string RepairRoot="CR097 supported properties and camp";
        public static void CorrectPropertiesAndProps(bool force=false)
        {
            Directory.CreateDirectory(Evidence);
            foreach(var scene in ReverseReviewRelease.Scenes)
            {
                EditorSceneManager.OpenScene(scene);
                owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();
                var prior=GameObject.Find(RepairRoot);if(prior&&!force)continue;
                if(prior){Object.DestroyImmediate(prior);var c=owner.GetComponent<ExplorationCollection>();c.routes=c.routes.Where(r=>r).ToArray();}
                worldRoot=new GameObject(RepairRoot).transform;piece=1000;
                var home=GameObject.Find("Dan - blue X").transform;var h2=GameObject.Find("Original house 2").transform;
                UnbatchHouse(h2);h2.rotation=home.rotation;
                foreach(var fence in Object.FindObjectsByType<BreakableProp>().Where(p=>p.name.StartsWith("Friend across street - blue circle")&&p.name.Contains("fence")).ToArray())Object.DestroyImmediate(fence.gameObject);
                HouseThreeDrive();
                foreach(var old in Object.FindObjectsByType<BreakableProp>().Where(f=>f.name=="Property white X"||f.name=="Grounded retained neighbor fence").ToArray()){
                    var t=old.transform;var bounds=old.GetComponent<BoxCollider>();var a=t.Find("Fence endpoint A");var b=t.Find("Fence endpoint B");
                    var fenceA=a?a.position:t.position-t.right*bounds.size.x*.5f;var fenceB=b?b.position:t.position+t.right*bounds.size.x*.5f;var parent=t.parent;
                    Fence(fenceA,fenceB,false);var replacement=worldRoot.GetChild(worldRoot.childCount-1);replacement.name="Grounded retained neighbor fence";replacement.SetParent(parent,true);Object.DestroyImmediate(old.gameObject);
                }
                // Rebuild the three-property roadside run at current poses, retaining all
                // four driveway openings. Other street-side properties are untouched.
                foreach(var fence in Object.FindObjectsByType<BreakableProp>().Where(p=>p.transform.parent&&p.transform.parent.name=="CR091-096 exploration world"&&(p.name=="Grounded roadside chain-link"||p.name=="Grounded wood crossbuck")).ToArray())Object.DestroyImmediate(fence.gameObject);
                float sd=street.Project(home.position,out _),s2=street.Project(h2.position,out _),s3=street.Project(GameObject.Find("Original house 3").transform.position,out _);
                float direction=Mathf.Sign(Mathf.DeltaAngle(sd/street.Length*360,s2/street.Length*360));
                Vector3 Front(float s){var p=street.At(s,out var f);var side=Vector3.Cross(Vector3.up,f).normalized;if(Vector3.Dot(side,home.position-p)<0)side=-side;return p+side*11;}
                float[] gates={sd-direction*20,sd+direction*20,s2,street.Project(GameObject.Find("House 3 / Rocky Way Acres entrance").transform.position,out _)};
                float from=sd-direction*35,length=Mathf.Abs(s3+direction*36-from);
                for(float t=0;t<length;t+=2){float a=from+direction*t,b=from+direction*Mathf.Min(length,t+2);if(gates.Any(g=>Mathf.Abs((a+b)*.5f-g)<6))continue;Fence(Front(a),Front(b),direction*((a+b)*.5f-s2)<-18);}
                // The side boundary starts at the roadside beside (not inside) each gate,
                // runs between the properties, and joins the existing rear enclosure.
                foreach(float side in new[]{-1f,1f}){
                    var back=home.TransformPoint(new Vector3(side*29,0,-27));
                    var front=home.TransformPoint(new Vector3(side*29,0,8));
                    float station=street.Project(front,out _);var roadside=Front(station);
                    RunFence(roadside,front,false);RunFence(front,back,false);
                }
                RunFence(home.TransformPoint(new Vector3(-29,0,-27)),home.TransformPoint(new Vector3(29,0,-27)),false);
                DomeAndCairns();BindFenceActivities();
                File.WriteAllText(Evidence+"/properties-"+owner.gameObject.scene.name+".txt",$"CR097: house poses retained; House 2 front parallel to Dan {home.forward}; full frontage plus both side/rear boundaries, two Dan gate openings. Kyle named fence generators disabled and all three remaining Kyle sections removed. House 3 grade route authored below sign; 3m segmented grounded fence.\n");
                Save();
            }
        }
        public static void RefreshFenceActivities()
        {
            foreach(var scene in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();worldRoot=GameObject.Find(RepairRoot).transform;BindFenceActivities();Save();}
        }
        static void BindFenceActivities()
        {
            foreach(var smash in Object.FindObjectsByType<ActivitySite>().Where(s=>s.kind==ActivitySite.Kind.Smash)){
                var roadside=worldRoot.GetComponentsInChildren<BreakableProp>().Where(p=>p.name.StartsWith("Grounded ")).Select(p=>{float s=street.Project(p.transform.position,out float d);return (prop:p,station:s,lateral:d);}).Where(p=>p.lateral>9&&p.lateral<13).OrderBy(p=>p.station).ToArray();
                BreakableProp[] best=null;float bestScore=float.PositiveInfinity;
                for(int i=0;i<=roadside.Length-14;i++){var run=roadside.Skip(i).Take(14).ToArray();if(Enumerable.Range(1,13).Any(j=>run[j].station-run[j-1].station>3.5f||run[j].station-run[j-1].station<1))continue;var a=run[0].prop.transform.position;var b=run[^1].prop.transform.position;if(run.Max(p=>p.prop.transform.position.y)-run.Min(p=>p.prop.transform.position.y)>3)continue;var heading=Vector3.ProjectOnPlane(b-a,Vector3.up).normalized;var approach=new[]{a-heading*12,b+heading*12};if(Object.FindObjectsByType<Collider>().Any(c=>!c.isTrigger&&(c.name.IndexOf("trunk",StringComparison.OrdinalIgnoreCase)>=0||c.name.IndexOf("tree",StringComparison.OrdinalIgnoreCase)>=0)&&Near(c.bounds.center,approach,out _)<3))continue;float bend=run.Max(p=>Near(p.prop.transform.position,new[]{a,b},out _));float score=bend*100+Vector3.Distance((a+b)*.5f,smash.transform.position)*.1f;if(score>=bestScore)continue;bestScore=score;best=run.Select(p=>p.prop).ToArray();}
                if(best==null||best.Length<smash.gold)throw new Exception("No continuous replacement targets for "+smash.id);
                smash.props=best;smash.transform.position=(best[0].transform.position+best[^1].transform.position)*.5f;smash.forward=Vector3.ProjectOnPlane(best[^1].transform.position-best[0].transform.position,Vector3.up).normalized;
                var label=Object.FindObjectsByType<TextMesh>().FirstOrDefault(t=>t.text.StartsWith("FENCE LINE SMASH"));var sign=label?label.GetComponentInParent<BreakableProp>():null;if(sign)sign.transform.SetPositionAndRotation(Ground(best[0].transform.position-smash.forward*8),Quaternion.LookRotation(smash.forward));
                File.WriteAllLines(Evidence+"/smash-targets-"+owner.gameObject.scene.name+".txt",best.Select(p=>p.name+" "+p.transform.position));
            }
        }
        static Vector3[] Curve(Vector3[] knots)
        {
            var result=new List<Vector3>();
            for(int i=0;i<knots.Length-1;i++){
                var a=knots[Mathf.Max(0,i-1)];var b=knots[i];var c=knots[i+1];var d=knots[Mathf.Min(knots.Length-1,i+2)];
                int n=Mathf.CeilToInt(Vector3.Distance(b,c)/1.5f);
                for(int j=0;j<n;j++){float t=j/(float)n;result.Add(.5f*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));}
            }result.Add(knots.Last());return result.ToArray();
        }
        static void HouseThreeDrive()
        {
            var house=GameObject.Find("Original house 3").transform;var sign=GameObject.Find("House 3 / Rocky Way Acres entrance").transform;
            var top=Ground(street.At(street.Project(sign.position,out _),out _));var gate=sign.position;
            var end=house.position+house.forward*20;end.y=house.position.y;
            var path=Curve(new[]{top,gate, new Vector3(490,78,-145),new(478,73,-169),new(456,62,-199),new(420,49,-222),new(395,39,-242),new(371,34,-224),new(375,33,-195),new(399,33,-183),new(433,33,-183),end});
            Terrain("house3-supported",p=>{
                if(p.x<340||p.x>530||p.z<-280||p.z>-126)return p;
                street.Project(p,out float roadDistance);if(roadDistance<7)return p;
                float d=Near(p,path,out var at);if(d<18)p.y=Mathf.Lerp(p.y,at.y,(1-Smooth(6,18,d))*Smooth(7,10,roadDistance));
                float pad=Vector3.ProjectOnPlane(p-end,Vector3.up).magnitude;if(pad<12)p.y=Mathf.Lerp(p.y,end.y,1-Smooth(8,12,pad));return p;
            });
            var road=new GameObject("House 3 valley driveway").AddComponent<RaceRoad>();road.transform.SetParent(worldRoot);road.points=path;road.forestTrail=true;road.Initialize();
            var collection=owner.GetComponent<ExplorationCollection>();collection.routes=collection.routes.Append(road).ToArray();
            var mesh=DriveMesh(path);
            var surface=new GameObject("Ground_House3 supported valley driveway",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));surface.transform.SetParent(road.transform);surface.GetComponent<MeshFilter>().sharedMesh=mesh;surface.GetComponent<MeshCollider>().sharedMesh=mesh;surface.GetComponent<Renderer>().sharedMaterial=Mat("House3 gravel",new(.42f,.4f,.34f));
            Physics.SyncTransforms();ClearTrees(p=>Near(p,path,out _)<9);
            foreach(var fence in Object.FindObjectsByType<BreakableProp>().Where(f=>(f.name=="Property white X"||f.name=="Grounded retained neighbor fence")&&Near(f.transform.position,path,out _)<6).ToArray())Object.DestroyImmediate(fence.gameObject);
            File.WriteAllLines(Evidence+"/house3-route-"+owner.gameObject.scene.name+".csv",new[]{"x,y,z"}.Concat(path.Select(p=>$"{p.x:F3},{p.y:F3},{p.z:F3}")));
        }
        static Mesh DriveMesh(Vector3[] path)
        {
            var v=new List<Vector3>();var triangles=new List<int>();const int columns=9;
            for(int i=0;i<path.Length;i++){
                var forward=path[Mathf.Min(path.Length-1,i+1)]-path[Mathf.Max(0,i-1)];var side=Vector3.Cross(Vector3.up,forward).normalized*4;
                for(int j=0;j<columns;j++){
                    var point=path[i]+side*(j/4f-1);street.Project(point,out float distance);
                    if(distance<10)point.y=Mathf.Lerp(Ground(point).y,point.y,Smooth(6,10,distance));
                    v.Add(point+Vector3.up*.065f);
                    if(i<path.Length-1&&j<columns-1){int n=i*columns+j;triangles.AddRange(new[]{n,n+columns,n+1,n+1,n+columns,n+columns+1});}
                }
            }
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return MeshAsset(mesh,owner.gameObject.scene.name+"-house3-drive");
        }
        public static void RefineDriveAprons()
        {
            foreach(var scene in ReverseReviewRelease.Scenes){EditorSceneManager.OpenScene(scene);owner=Object.FindAnyObjectByType<RaceDirector>();street=owner.ambientRoad?owner.ambientRoad:owner.road;street.Initialize();var road=GameObject.Find("House 3 valley driveway").GetComponent<RaceRoad>();var surface=road.GetComponentInChildren<MeshFilter>();var collider=surface.GetComponent<MeshCollider>();collider.enabled=false;Physics.SyncTransforms();var mesh=DriveMesh(road.points);surface.sharedMesh=mesh;collider.sharedMesh=null;collider.sharedMesh=mesh;collider.enabled=true;Physics.SyncTransforms();Save();}
        }
        static void DomeAndCairns()
        {
            var camp=GameObject.Find("Permanent mountainside camp / two seated guys").transform;
            foreach(Transform p in camp.Cast<Transform>().Where(t=>t.name=="Small round pop-up tent"||t.name=="Tent doorway"||t.name=="Small domed camping tent").ToArray())Object.DestroyImmediate(p.gameObject);
            var tent=new GameObject("Small domed camping tent").transform;tent.SetParent(camp);tent.position=Ground(camp.TransformPoint(new Vector3(5,0,1)));
            Terrain("flat-tent-footprint",p=>{float d=Vector3.ProjectOnPlane(p-tent.position,Vector3.up).magnitude;if(d<4)p.y=Mathf.Lerp(p.y,tent.position.y,1-Smooth(2.4f,4,d));return p;});
            var fabric=Mat("Tent weathered teal fabric",new(.09f,.39f,.39f));var seams=Mat("Tent light poles",new(.67f,.69f,.59f));var dark=Mat("Tent entrance shadow",new(.035f,.065f,.055f));
            Part(tent,"Flat grounded sewn floor",new(0,.04f,0),new(3.7f,.08f,3.4f),dark);
            var verts=new List<Vector3>();var tris=new List<int>();int rings=10,steps=32;
            for(int ring=0;ring<=rings;ring++){float a=ring/(float)rings*Mathf.PI*.5f;for(int j=0;j<=steps;j++){float b=j/(float)steps*2*Mathf.PI;verts.Add(new(Mathf.Cos(a)*Mathf.Cos(b)*1.85f,.08f+Mathf.Sin(a)*1.85f,Mathf.Cos(a)*Mathf.Sin(b)*1.7f));}}
            for(int ring=0;ring<rings;ring++)for(int j=0;j<steps;j++){int n=ring*(steps+1)+j;tris.AddRange(new[]{n,n+steps+1,n+1,n+1,n+steps+1,n+steps+2});}
            var mesh=new Mesh();mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();var roof=new GameObject("Arched fabric roof",typeof(MeshFilter),typeof(MeshRenderer));roof.transform.SetParent(tent,false);roof.GetComponent<MeshFilter>().sharedMesh=MeshAsset(mesh,"dome-tent-roof");roof.GetComponent<Renderer>().sharedMaterial=fabric;
            foreach(float yaw in new[]{45f,135f}){var rotation=Quaternion.Euler(0,yaw,0);for(int i=0;i<20;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;Beam(tent,"Crossing arched pole",rotation*new Vector3(1.87f*Mathf.Cos(a),.10f+1.86f*Mathf.Sin(a),0),rotation*new Vector3(1.87f*Mathf.Cos(b),.10f+1.86f*Mathf.Sin(b),0),.045f,seams);}}
            Part(tent,"Open entrance and zipped fabric flap",new(-1.68f,.65f,0),new(.07f,1.25f,1.05f),dark);
            Beam(tent,"Entrance seam left",new(-1.73f,.08f,-.56f),new(-1.40f,1.40f,-.25f),.04f,seams);Beam(tent,"Entrance seam right",new(-1.73f,.08f,.56f),new(-1.40f,1.40f,.25f),.04f,seams);
            var stoneMeshes=Enumerable.Range(0,3).Select(i=>MeshAsset(CairnMesh(i),"irregular-cairn-"+i)).ToArray();
            foreach(var clue in Object.FindObjectsByType<Transform>().Where(t=>t.name.StartsWith("Acorn clue / ")).ToArray()){
                foreach(Transform child in clue.Cast<Transform>().ToArray())Object.DestroyImmediate(child.gameObject);
                clue.position=Ground(clue.position);
                for(int i=0;i<3;i++){
                    var stone=Part(clue,"Irregular weathered stacked stone",new(i==1?.05f:-.03f,.14f+i*.23f,i==2?.035f:0),new(.80f-i*.19f,.28f,.64f-i*.13f),Mat("Cairn gray stone "+i,new(.36f+i*.045f,.38f+i*.035f,.37f+i*.03f)),false,PrimitiveType.Cube);
                    stone.localRotation=Quaternion.Euler(i*4,19+i*47,i*-3);
                    stone.GetComponent<MeshFilter>().sharedMesh=stoneMeshes[i];
                }
            }
        }
        static Mesh CairnMesh(int seed)
        {
            var rings=new Vector3[24];for(int r=0;r<3;r++)for(int j=0;j<8;j++){
                float a=j*Mathf.PI/4,rad=(r==1?.5f:.32f)*(1+.14f*Mathf.Sin(j*2.7f+seed));
                rings[r*8+j]=new(Mathf.Cos(a)*rad,(r-1)*.46f+(r==1?.08f*Mathf.Cos(j+seed):0),Mathf.Sin(a)*rad);
            }
            var v=new List<Vector3>();void Face(Vector3 a,Vector3 b,Vector3 c){v.Add(a);v.Add(b);v.Add(c);}
            for(int j=0;j<8;j++){int k=(j+1)%8;Face(new(0,-.46f,0),rings[j],rings[k]);Face(new(0,.46f,0),rings[16+k],rings[16+j]);for(int r=0;r<2;r++){int n=r*8;Face(rings[n+j],rings[n+8+j],rings[n+k]);Face(rings[n+k],rings[n+8+j],rings[n+8+k]);}}
            var mesh=new Mesh();mesh.SetVertices(v);mesh.SetTriangles(Enumerable.Range(0,v.Count).ToArray(),0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}

