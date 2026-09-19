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
    // Scoped re-authoring only. All collision, route and rendering changes are saved assets.
    public static class PlaytestCorrection
    {
        public const string Folder="Assets/Track/PlaytestCorrection", Dir="Docs/CR041-045";
        static Material Mat(string name,Color color)
        {
            string path=Folder+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m) { m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color,enableInstancing=true};AssetDatabase.CreateAsset(m,path); }return m;
        }
        static Transform Cube(Transform root,string name,Vector3 p,Vector3 size,Material mat,bool solid=false)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(root,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;
            if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;
        }
        static void View(string name,Vector3 at,Vector3 eye)
        {
            var go=new GameObject("Correction evidence camera");var c=go.AddComponent<Camera>(); c.transform.position=eye;c.transform.LookAt(at,Mathf.Abs(Vector3.Dot((at-eye).normalized,Vector3.up))>.99f?Vector3.forward:Vector3.up);c.fieldOfView=58;c.farClipPlane=1600;
            var rt=new RenderTexture(1280,720,24); c.targetTexture=rt;c.Render();var previous=RenderTexture.active;RenderTexture.active=rt;
            var t=new Texture2D(1280,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();File.WriteAllBytes(Dir+"/"+name+".png",t.EncodeToPNG());RenderTexture.active=previous;
            c.targetTexture=null;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);Object.DestroyImmediate(go);
        }
        public static void Capture(string label)
        {
            Directory.CreateDirectory(Dir);
            var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Fox Gully");
            float s=branch.Project(new Vector3(44,0,-451),out _);var p=branch.At(s,out var f);
            f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
            View(label+"-gully",p+Vector3.up*5,p-f*42+Vector3.up*10);
            foreach(string name in new[]{"Dan - blue X","Original house 2","Original house 3"})
            {var house=GameObject.Find(name).transform;View(label+"-"+(name.StartsWith("Dan")?"dan":name.Replace(' ','-')),house.position+house.forward*15,house.position+house.forward*15+Vector3.up*160);}
            var ramp=GameObject.Find(Phase4Setup.RootName).transform;View(label+"-jamerson",ramp.position+ramp.forward*35,ramp.position-ramp.forward*35+Vector3.up*35+ramp.right*30);
        }
        public static void Apply()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(Application.isPlaying||scene.isDirty||GameObject.Find("CR041-045 authored correction"))throw new InvalidOperationException("Saved unapplied scene in edit mode required");
            Directory.CreateDirectory(Folder);Directory.CreateDirectory(Dir);AssetDatabase.Refresh();Capture("before");
            var root=new GameObject("CR041-045 authored correction").transform;
            var race=Object.FindAnyObjectByType<RaceDirector>();race.highwayTrafficCount=16;race.trafficCount=4;
            race.gates[14].halfHeight=3;race.gates[14].upperHeight=24; // Jamerson's launch corridor only; no below-road enlargement.
            House(root);Fences(root,race);Jamerson(root);
            PlayerSettings.bundleVersion="0.6.1-review2";
            Phase6Buildings.RefreshVisualBatches(false);
            Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Capture("after");
        }
        public static void Refine()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit mode required");
            // Restore only the unrelated color edits made by the historical frontage refresh.
            foreach(string file in Directory.GetFiles("Temp/correction-original-colors","*.asset"))
            {
                var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Track/StreetLoop/"+Path.GetFileName(file));
                string hex=System.Text.RegularExpressions.Regex.Match(File.ReadAllText(file),@"_typelessdata: ([0-9a-f]+)").Groups[1].Value;
                var bytes=new byte[hex.Length/2];for(int i=0;i<bytes.Length;i++)bytes[i]=Convert.ToByte(hex.Substring(i*2,2),16);
                if(bytes.Length!=mesh.vertexCount*56)throw new InvalidOperationException("Unexpected saved terrain layout");
                var colors=new Color[mesh.vertexCount];for(int i=0;i<colors.Length;i++){int k=i*56+24;colors[i]=new(BitConverter.ToSingle(bytes,k),BitConverter.ToSingle(bytes,k+4),BitConverter.ToSingle(bytes,k+8),BitConverter.ToSingle(bytes,k+12));}mesh.colors=colors;EditorUtility.SetDirty(mesh);
            }
            var race=Object.FindAnyObjectByType<RaceDirector>();race.gates[14].halfHeight=3;race.gates[14].upperHeight=24;
            var house=GameObject.Find("Fox Gully drive-through house (former 44 -451)").transform;
            if(!house.Find("Entry window trim"))
            {
                var trim=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Gully ivory.mat");var glass=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Stunt glass.mat");
                foreach(float x in new[]{-2.8f,2.8f})
                {Cube(house,"Entry window trim",new(x,8.5f,-22.3f),new(2.4f,2.1f,.16f),trim);Cube(house,"Entry upper window",new(x,8.5f,-22.41f),new(2.1f,1.8f,.08f),glass);}
                foreach(float x in new[]{-10.05f,10.05f})Cube(house,"Sliding door frame",new(x,2.7f,-22.2f),new(.16f,5.5f,.18f),trim);
                foreach(float y in new[]{.12f,5.5f})Cube(house,"Sliding door track",new(0,y,-22.2f),new(20.2f,.13f,.18f),trim);
                var doors=house.Find("Breakable sliding glass doors");Cube(doors,"Sliding panel meeting rail",new(0,0,-.7f),new(.012f,.98f,1),trim);
                foreach(float side in new[]{-1f,1f})for(float z=-14;z<=14;z+=14)
                {var window=Cube(house,"Upper storey side window",new(side*13.3f,8.5f,z),new(.12f,2,3),glass);}
            }
            Phase6Buildings.RefreshVisualBatches(false);AssetDatabase.SaveAssets();Capture("after-refined");
        }
        public static void CaptureHistoricalHouse()
        {
            const string copy="Assets/Scenes/CorrectionHistoricalTemporary.unity";
            if(Application.isPlaying||File.Exists(copy))throw new InvalidOperationException("Edit mode and unused temporary archive path required");
            var current=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            File.Copy("Temp/correction-checkpoint-scene.unity",copy);AssetDatabase.ImportAsset(copy);
            var archive=EditorSceneManager.OpenScene(copy,OpenSceneMode.Additive);GameObject clone=null;
            try
            {
                var currentRoots=current.GetRootGameObjects();var active=currentRoots.Select(g=>g.activeSelf).ToArray();
                try
                {
                    foreach(var root in currentRoots)root.SetActive(false);
                    var oldBatches=archive.GetRootGameObjects().Single(g=>g.name=="Phase 6 - architectural render batches");oldBatches.SetActive(false);
                    var oldBuildings=archive.GetRootGameObjects().Single(g=>g.name=="Remembered houses and approximate buildings");foreach(var r in oldBuildings.GetComponentsInChildren<Renderer>())r.enabled=true;
                    Capture("before-archive-clear");
                }
                finally{for(int i=0;i<currentRoots.Length;i++)currentRoots[i].SetActive(active[i]);}
                foreach(var root in archive.GetRootGameObjects())root.SetActive(false);
                var houses=archive.GetRootGameObjects().Single(g=>g.name=="Remembered houses and approximate buildings");
                var original=houses.transform.Cast<Transform>().Single(t=>Vector3.Distance(t.position,new Vector3(44,30.38f,-451))<1);
                clone=Object.Instantiate(original.gameObject);clone.name="Temporary exact checkpoint house";UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone,current);clone.SetActive(true);foreach(var r in clone.GetComponentsInChildren<Renderer>())r.enabled=true;
            }
            finally{EditorSceneManager.CloseScene(archive,true);AssetDatabase.DeleteAsset(copy);}
            var batch=GameObject.Find("Phase 6 - architectural render batches");var house=GameObject.Find("Fox Gully drive-through house (former 44 -451)");
            var renderers=GameObject.Find("Remembered houses and approximate buildings").GetComponentsInChildren<Renderer>();var states=renderers.Select(r=>r.enabled).ToArray();
            try
            {
                batch.SetActive(false);house.SetActive(false);foreach(var r in renderers)r.enabled=true;
                var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Fox Gully");float s=branch.Project(new Vector3(44,0,-451),out _);var p=branch.At(s,out var f);f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;
                View("before-checkpoint-gully",p+Vector3.up*5,p-f*42+Vector3.up*10);
            }
            finally{for(int i=0;i<renderers.Length;i++)renderers[i].enabled=states[i];house.SetActive(true);batch.SetActive(true);Object.DestroyImmediate(clone);EditorSceneManager.SaveScene(current);}
        }
        public static void WidenHouse()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit mode required");
            var house=GameObject.Find("Fox Gully drive-through house (former 44 -451)").transform;
            if(house.Find("Breakable sliding glass doors").localScale.x>15)throw new InvalidOperationException("Already widened");
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/Gully interior.asset");var vertices=mesh.vertices;
            for(int i=0;i<vertices.Length;i++)vertices[i].x+=Mathf.Sign(vertices[i].x)*5;
            mesh.vertices=vertices;mesh.RecalculateBounds();mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);
            var collider=house.Find("Gully supported ramp").GetComponent<MeshCollider>();collider.sharedMesh=null;collider.sharedMesh=mesh;
            foreach(Transform t in house)
            {
                var p=t.localPosition;var size=t.localScale;
                switch(t.name)
                {
                    case "Visible ramp crossbeam":case "Entry upper facade":case "Exit lower facade":case "Exit upper lintel":case "Breakable sliding glass doors":case "Breakable second-story window":case "Sliding door track": size.x+=10;break;
                    case "Supported exterior wall":case "Second floor side room":case "Foundation pier":case "Opening side wall":case "Sliding door frame":case "Upper storey side window":p.x+=Mathf.Sign(p.x)*5;break;
                    case "Pitched roof":p.x+=Mathf.Sign(p.x)*2.5f;size.x+=5;break;
                }
                t.localPosition=p;t.localScale=size;
            }
            File.AppendAllText(Dir+"/gully-site.txt","Physical motorcycle failure retained: original 10m aperture caught 6.9–7.5m lateral drift. Revised supported floor and openings to 20m; exterior walls/piers/roof moved consistently. No vehicle tuning or launch force change.\n");
            Phase6Buildings.RefreshVisualBatches(false);AssetDatabase.SaveAssets();Capture("after-wide");
        }
        public static void RemoveInteriorColliderSeams()
        {
            if(Application.isPlaying)throw new InvalidOperationException("Edit mode required");
            var house=GameObject.Find("Fox Gully drive-through house (former 44 -451)").transform;
            int removed=0;foreach(Transform t in house)if(t.name=="Visible ramp crossbeam" && t.TryGetComponent<Collider>(out var c)){Object.DestroyImmediate(c);removed++;}
            Physics.SyncTransforms();EditorSceneManager.MarkSceneDirty(house.gameObject.scene);EditorSceneManager.SaveScene(house.gameObject.scene);
            File.AppendAllText(Dir+"/gully-site.txt",$"Removed {removed} redundant solid crossbeam colliders beneath the continuous closed ramp mesh after retained motorcycle interior impacts. Visible beams remain.\n");
        }
        static void House(Transform root)
        {
            var branch=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Fox Gully");
            var buildings=GameObject.Find("Remembered houses and approximate buildings").transform;
            var old=buildings.Cast<Transform>().Single(t=>Vector3.Distance(t.position,new Vector3(44,30.38f,-451))<1);
            File.WriteAllText(Dir+"/gully-site.txt",old.name+" original position="+old.position+" rotation="+old.eulerAngles+"\n");
            // Remove only the authorized residence instance, including its old hidden solid box.
            Object.DestroyImmediate(old.gameObject);
            float center=branch.Project(new Vector3(44,0,-451),out _),begin=center-22,end=center+22;
            var entry=branch.At(begin,out _);var exit=branch.At(end,out _);var forward=Vector3.ProjectOnPlane(exit-entry,Vector3.up).normalized;
            var house=new GameObject("Fox Gully drive-through house (former 44 -451)").transform;house.SetParent(buildings);house.position=(entry+exit)*.5f;house.position=new(house.position.x,entry.y,house.position.z);house.rotation=Quaternion.LookRotation(forward);
            var siding=Mat("Gully warm siding",new(.62f,.44f,.26f));var timber=Mat("Gully timber",new(.28f,.17f,.09f));var roof=Mat("Gully roof",new(.18f,.23f,.24f));var glass=Mat("Stunt glass",new(.35f,.65f,.74f));var trim=Mat("Gully ivory",new(.88f,.84f,.7f));
            // Floor deliberately rises six metres to the upper storey. No forces are applied.
            float length=Vector3.ProjectOnPlane(exit-entry,Vector3.up).magnitude;
            var verts=new List<Vector3>();var tris=new List<int>();
            for(int i=0;i<=88;i++)
            {
                float t=i/88f;var p=branch.At(Mathf.Lerp(begin,end,t),out _);p.y=entry.y+6*t*t;
                var local=house.InverseTransformPoint(p);
                foreach(float x in new[]{-10f,10f})verts.Add(local+Vector3.right*x);
                if(i<88){int k=i*2;tris.AddRange(new[]{k,k+2,k+1,k+1,k+2,k+3});}
                if(i%8==0) Cube(house,"Visible ramp crossbeam",local+Vector3.down*.3f,new(20,.6f,.3f),timber);
            }
            // Closed side faces support the rising floor down to the terrain.
            int count=verts.Count;
            for(int i=0;i<count;i++){var p=verts[i];var world=house.TransformPoint(p);p.y=Mathf.Min(p.y-.35f,Phase6Buildings.Ground(world)-house.position.y-.15f);verts.Add(p);}
            for(int i=0;i<88;i++)foreach(int side in new[]{0,1}){int a=i*2+side,b=a+2,c=a+count,d=b+count;tris.AddRange(side==0?new[]{a,c,b,b,c,d}:new[]{a,b,c,b,d,c});}
            var mesh=new Mesh{name="Supported gully interior rise"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Folder+"/Gully interior.asset");
            var deck=new GameObject("Gully supported ramp",typeof(MeshFilter),typeof(MeshRenderer),typeof(MeshCollider));deck.transform.SetParent(house,false);deck.GetComponent<MeshFilter>().sharedMesh=mesh;deck.GetComponent<MeshRenderer>().sharedMaterial=timber;deck.GetComponent<MeshCollider>().sharedMesh=mesh;
            foreach(int side in new[]{-1,1})
            {
                Cube(house,"Supported exterior wall",new(side*13,6,0),new(.5f,12,length+1),siding,true);
                Cube(house,"Second floor side room",new(side*11.6f,6,0),new(2.4f,.4f,length),timber,true);
                for(float z=-length*.5f;z<=length*.5f;z+=5)
                {var world=house.TransformPoint(new(side*13,0,z));float ground=Phase6Buildings.Ground(world)-house.position.y;Cube(house,"Foundation pier",new(side*13,(ground+1)*.5f,z),new(1.1f,Mathf.Max(1,1-ground),1.1f),timber,true);}
                foreach(float z in new[]{-length*.5f,length*.5f})Cube(house,"Opening side wall",new(side*11.6f,6,z),new(3,12,.5f),siding,true);
                var roofHalf=Cube(house,"Pitched roof",new(side*6.7f,13.2f,0),new(13.7f,.4f,length+3),roof,true);roofHalf.localRotation=Quaternion.Euler(0,0,-side*12);
            }
            Cube(house,"Entry upper facade",new(0,9,-length*.5f),new(20,6,.5f),siding,true);
            Cube(house,"Exit lower facade",new(0,2.8f,length*.5f+.4f),new(20,5.6f,.4f),siding,true);
            Cube(house,"Exit upper lintel",new(0,11.8f,length*.5f),new(20,.6f,.5f),trim,true);
            void Pane(string name,float y,float z,float height)
            {var panel=Cube(house,name,new(0,y,z),new(19.6f,height,.08f),glass,true);panel.gameObject.AddComponent<BreakableProp>().surface=SmashAudio.Surface.Glass;panel.GetComponent<BoxCollider>().isTrigger=true;}
            Pane("Breakable sliding glass doors",2.7f,-length*.5f-.15f,5.4f);Pane("Breakable second-story window",8.8f,length*.5f+.15f,5.2f);
            float distance=0;
            for(int i=1;i<branch.points.Length;i++)
            {distance+=Vector2.Distance(new(branch.points[i-1].x,branch.points[i-1].z),new(branch.points[i].x,branch.points[i].z));if(distance>=begin && distance<=end)branch.points[i].y=entry.y+6*Mathf.Pow(Mathf.InverseLerp(begin,end,distance),2);}
            EditorUtility.SetDirty(branch);
            File.AppendAllText(Dir+"/gully-site.txt",$"New position={house.position}; yaw={house.eulerAngles}; route span={begin:F2}..{end:F2}; clear width=20m; rise=6m; run={length:F2}m. Terrain landing remains original supported gully.\n");
            File.WriteAllText(Folder+"/Gully-route.json",JsonUtility.ToJson(branch,true));
        }
        static void Fences(Transform root,RaceDirector race)
        {
            foreach(var p in Object.FindObjectsByType<BreakableProp>().Where(p=>p.name=="Chain-link section"||p.name=="White crossbuck section").ToArray())Object.DestroyImmediate(p.gameObject);
            var white=Mat("Perimeter white",new(.9f,.91f,.87f));var metal=Mat("Perimeter galvanized",new(.45f,.51f,.52f));
            void Section(Vector3 a,Vector3 b,bool chain)
            {
                a.y=Phase6Buildings.Ground(a);b.y=Phase6Buildings.Ground(b);float len=Vector3.Distance(a,b);
                var g=new GameObject(chain?"Property chain-link":"Property white X").transform;g.SetParent(root);g.position=(a+b)*.5f;g.rotation=Quaternion.LookRotation(Vector3.Cross(b-a,Vector3.up));var m=chain?metal:white;
                foreach(float x in new[]{-len*.5f,len*.5f})Cube(g,"Post",new(x,.8f,0),new(.1f,1.6f,.1f),m);
                foreach(float y in new[]{.25f,1.4f})Cube(g,"Rail",new(0,y,0),new(len,.08f,.08f),m);
                void Beam(Vector3 x,Vector3 y,float width){var t=Cube(g,"Fence diagonal",(x+y)*.5f,new(width,width,Vector3.Distance(x,y)),m);t.localRotation=Quaternion.LookRotation(y-x);}
                if(chain)for(float x=-len*.5f;x<len*.5f;x+=.35f){float e=Mathf.Min(len*.5f,x+1.15f);Beam(new(x,.25f,0),new(e,.25f+e-x,0),.018f);Beam(new(x,1.4f,0),new(e,1.4f-e+x,0),.018f);}
                else{Beam(new(-len*.5f,.25f,0),new(len*.5f,1.4f,0),.11f);Beam(new(-len*.5f,1.4f,0),new(len*.5f,.25f,0),.11f);}
                var filters=g.GetComponentsInChildren<MeshFilter>();var mesh=new Mesh();mesh.CombineMeshes(filters.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=g.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());AssetDatabase.CreateAsset(mesh,Folder+"/Fence-"+Guid.NewGuid().ToString("N")+".asset");foreach(var f in filters)Object.DestroyImmediate(f.gameObject);g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=m;
                var c=g.gameObject.AddComponent<BoxCollider>();c.center=new(0,.8f,0);c.size=new(len,1.7f,.35f);c.isTrigger=true;g.gameObject.AddComponent<BreakableProp>().surface=chain?SmashAudio.Surface.ChainLink:SmashAudio.Surface.Wood;
            }
            void Run(Vector3 a,Vector3 b,bool chain){int n=Mathf.CeilToInt(Vector3.Distance(a,b)/4);for(int i=0;i<n;i++)Section(Vector3.Lerp(a,b,i/(float)n),Vector3.Lerp(a,b,(i+1)/(float)n),chain);}
            foreach(string name in new[]{"Dan - blue X","Original house 2","Original house 3"})
            {
                var house=GameObject.Find(name).transform;float s=race.road.Project(house.position,out _);var road=race.road.At(s,out var f);var side=Vector3.ProjectOnPlane(house.position-road,Vector3.up).normalized;var along=Vector3.Cross(Vector3.up,side);float back=Vector3.ProjectOnPlane(house.position-road,Vector3.up).magnitude+20;bool chain=name.StartsWith("Dan");
                Vector3 P(float x,float z)=>road+along*x+side*z;
                Run(P(-18,11),P(-6,11),chain);Run(P(6,11),P(18,11),chain);
                Run(P(-18,11),P(-18,back),chain);Run(P(18,11),P(18,back),chain);
                Run(P(-18,back),P(-3,back),chain);Run(P(3,back),P(18,back),chain);
            }
        }
        static void Jamerson(Transform root)
        {
            var ramp=GameObject.Find(Phase4Setup.RootName).transform;var surface=ramp.GetComponentInChildren<MeshCollider>();
            surface.GetComponent<Renderer>().sharedMaterial=Mat("Raised roadworks asphalt",new(.26f,.27f,.28f));
            var gravel=Mat("Roadworks gravel",new(.47f,.43f,.35f));var orange=Mat("Roadworks orange",new(.85f,.3f,.055f));
            // The existing closed supported takeoff mesh is retained; visible graded shoulders
            // and construction barriers explain the raised pavement while the right lane bypasses it.
            var work=new GameObject("Jamerson raised pavement works").transform;work.SetParent(root);work.SetPositionAndRotation(ramp.position,ramp.rotation);
            for(int i=0;i<20;i++)
            {float z=i*2+1,h=6.2f*Mathf.Pow(z/40,2);Cube(work,"Graded aggregate shoulder",new(-4.9f,h*.5f,z),new(.7f,Mathf.Max(.1f,h),2),gravel,true);}
            foreach(float z in new[]{-20f,-10f,0f,12f,24f,36f})
            {
                var barrel=Cube(work,"Breakable roadworks marker",new(-5.6f,.65f,z),new(.65f,1.3f,.65f),orange,true);barrel.gameObject.AddComponent<BreakableProp>().surface=SmashAudio.Surface.Sign;barrel.GetComponent<BoxCollider>().isTrigger=true;
            }
            var label=ramp.GetComponentsInChildren<TextMesh>(true).FirstOrDefault(t=>t.name=="Recommended speed");if(label)label.text="RAISED PAVEMENT WORKS\nJUMP 125 - 155 km/h\nOPEN LANE ON RIGHT";
        }
    }
}
