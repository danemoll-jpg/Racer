using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

namespace Racer.Editor
{
    // Focused, reproducible authoring. Never invokes the legacy environment builder.
    public static class WoodlandUpdate
    {
        public const string Folder="Assets/Track/Woodland", Evidence="Docs/CR034-039", Root="CR034-039 Woodland routes";
        static RaceDirector race;
        static Material white,metal,green,amber;
        static List<Vector3> road;
        static float Smooth(float a,float b,float x)=>Mathf.SmoothStep(0,1,Mathf.InverseLerp(a,b,x));
        static Material Mat(string name,Color color)
        {
            string path=Folder+"/"+name+".mat"; var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(!m) { m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color,enableInstancing=true}; AssetDatabase.CreateAsset(m,path); } return m;
        }
        static Transform Cube(Transform parent,string name,Vector3 p,Vector3 size,Material m)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube); g.name=name; g.transform.SetParent(parent,false); g.transform.localPosition=p; g.transform.localScale=size;
            Object.DestroyImmediate(g.GetComponent<Collider>()); g.GetComponent<Renderer>().sharedMaterial=m; return g.transform;
        }
        static void Beam(Transform parent,string name,Vector3 a,Vector3 b,float thickness,Material mat)
        {
            var t=Cube(parent,name,(a+b)*.5f,new(thickness,thickness,Vector3.Distance(a,b)),mat); t.localRotation=Quaternion.LookRotation(b-a);
        }
        static void Sensor(Transform root,SmashAudio.Surface surface)
        {
            var renderers=root.GetComponentsInChildren<Renderer>(); var bounds=new Bounds(root.position,Vector3.zero);
            foreach(var r in renderers) bounds.Encapsulate(r.bounds);
            var box=root.gameObject.AddComponent<BoxCollider>(); box.isTrigger=true;
            box.center=root.InverseTransformPoint(bounds.center); box.size=new Vector3(5,Mathf.Max(1.4f,bounds.size.y),.5f);
            root.gameObject.AddComponent<BreakableProp>().surface=surface;
        }
        static void Sign(Transform parent,Vector3 position,Vector3 forward,string text,Material board,SmashAudio.Surface surface=SmashAudio.Surface.Sign)
        {
            var root=new GameObject("Breakable sign - "+text.Replace('\n',' ')).transform; root.SetParent(parent); position.y=Phase6Buildings.Ground(position); root.SetPositionAndRotation(position,Quaternion.LookRotation(forward));
            Cube(root,"Post",new(0,1.5f,0),new(.12f,3,.12f),metal); Cube(root,"Board",new(0,2.6f,0),new(7,1.6f,.12f),board);
            foreach(int side in new[]{-1,1})
            {
                var label=new GameObject("Road lettering").AddComponent<TextMesh>(); label.transform.SetParent(root,false); label.transform.localPosition=new(0,2.6f,side*.075f);
                label.transform.localRotation=side<0?Quaternion.identity:Quaternion.Euler(0,180,0);
                label.text=text; label.anchor=TextAnchor.MiddleCenter; label.alignment=TextAlignment.Center; label.fontSize=64; label.characterSize=.10f;
                var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.font=font;
                var material=new Material(Shader.Find("Racer/WorldText")); material.mainTexture=font.material.mainTexture;
                // A shared asset keeps lettering included in player shader stripping.
                var existing=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Lettering.mat");
                if(!existing) { AssetDatabase.CreateAsset(material,Folder+"/Lettering.mat"); existing=material; } else Object.DestroyImmediate(material);
                label.GetComponent<Renderer>().sharedMaterial=existing;
            }
            Sensor(root,surface); root.GetComponent<BoxCollider>().size=new(7,3.5f,.5f);
        }
        static Vector3[] Curve(params Vector3[] knots)
        {
            var result=new List<Vector3>();
            for(int i=0;i<knots.Length-1;i++)
            {
                var a=knots[Mathf.Max(0,i-1)]; var b=knots[i]; var c=knots[i+1]; var d=knots[Mathf.Min(knots.Length-1,i+2)];
                int count=Mathf.CeilToInt(Vector3.Distance(b,c)/2);
                for(int j=0;j<count;j++) { float t=j/(float)count; result.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t)); }
            }
            result.Add(knots[^1]); return result.ToArray();
        }
        static WoodlandRoute Route(Transform root,string name,float entry,float exit,float speed,params Vector3[] inner)
        {
            var branch=new GameObject(name).AddComponent<WoodlandRoute>(); branch.transform.SetParent(root);
            branch.halfWidth=9; branch.title=name; branch.entryRoad=entry; branch.exitRoad=exit; branch.recommendedSpeed=speed;
            var a=race.road.At(entry,out var af); var b=race.road.At(exit,out var bf);
            branch.points=Curve(new[]{a,a+af*26}.Concat(inner).Concat(new[]{b-bf*32,b}).ToArray());
            branch.bypassedGates=race.gates.Select((g,i)=>(i,s:race.road.Project(g.transform.position,out _))).Where(g=>g.i>0 && g.s>entry && g.s<exit).Select(g=>g.i).ToArray();
            return branch;
        }
        static float JumpHeight(float s,float start,float run,float height,float gap)
        {
            if(s<start || s>start+run+gap+45) return 0;
            if(s<start+run) return height*Mathf.Pow((s-start)/run,2);
            if(s<start+run+gap) return Mathf.Lerp(height,-4,Smooth(start+run,start+run+5,s));
            return -4*(1-Smooth(start+run+gap,start+run+gap+22,s));
        }
        public static void Apply()
        {
            var scene=SceneManager.GetActiveScene();
            if(Application.isPlaying || scene.isDirty || scene.path!=StreetLoopBuilder.ScenePath || GameObject.Find(Root)) throw new InvalidOperationException("Saved unchanged StreetLoop, edit mode, unapplied woodland update required.");
            Directory.CreateDirectory(Evidence); Directory.CreateDirectory(Folder); AssetDatabase.Refresh();
            race=Object.FindAnyObjectByType<RaceDirector>(); race.road.Initialize(); road=StreetLoopBuilder.Route();
            white=Mat("Fence ivory",new(.9f,.91f,.87f)); metal=Mat("Galvanized steel",new(.42f,.48f,.49f)); green=Mat("Road green",new(.025f,.19f,.11f)); amber=Mat("Trail amber",new(.4f,.16f,.035f));
            var buildings=GameObject.Find("Remembered houses and approximate buildings").transform;
            File.WriteAllLines(Evidence+"/sites-before.txt",buildings.Cast<Transform>().Select(t=>t.name+" "+t.position.ToString("F5")+" "+t.rotation.ToString("F5")));
            var root=new GameObject(Root).transform;
            var creek=Route(root,"Creek Leap",900,1500,34,new(556,27,-321),new(484,25,-364),new(387,25,-362));
            var gully=Route(root,"Fox Gully",1580,2150,30,new(130,43,-315),new(90,30,-385),new(20,22,-470),new(-45,27,-535));
            var ridge=Route(root,"Pine Ridge",2220,3000,40,new(-290,26,-540),new(-460,18,-365),new(-570,12,-280));
            var branches=new[]{creek,gully,ridge};
            // Takeoff/creek/landing sculpt belongs to the same shared visible/collision heightfield.
            foreach(var branch in new[]{creek,ridge})
            {
                float run=branch==creek?32:46, start=branch==creek?140:150, height=branch==creek?5.2f:6f;
                if(branch==ridge)
                {
                    var cumulative=new float[branch.points.Length];
                    for(int k=1;k<cumulative.Length;k++) cumulative[k]=cumulative[k-1]+Vector2.Distance(new(branch.points[k].x,branch.points[k].z),new(branch.points[k-1].x,branch.points[k-1].z));
                    float a=branch.points[0].y,b=branch.points[^1].y;
                    for(int k=0;k<cumulative.Length;k++) branch.points[k].y=Mathf.Lerp(a,b,cumulative[k]/cumulative[^1]);
                }
                float s=0; var basePoints=branch.points.ToArray();
                for(int i=1;i<branch.points.Length;i++) { s+=Vector3.Distance(basePoints[i],basePoints[i-1]); branch.points[i].y+=JumpHeight(s,start,run,height,branch==creek?16:22); }
            }
            // Ease every road crossing into its retained elevation before sculpting the trail.
            // This avoids a short, unintended launch at the protected pavement boundary.
            foreach(var branch in branches)
                for(int i=0;i<branch.points.Length;i++)
                {
                    float d=StreetLoopBuilder.Nearest(branch.points[i],road,out var near);
                    branch.points[i].y=Mathf.Lerp(near.y,branch.points[i].y,Smooth(8,45,d));
                }
            // Legacy cut retains its physical course, now with the same legal-route evidence model.
            var old=new GameObject("Existing Southwest Cut").AddComponent<WoodlandRoute>(); old.transform.SetParent(root); old.title=old.name; old.points=Phase5Setup.Path().ToArray(); old.entryRoad=race.road.Project(old.points[0],out _); old.exitRoad=race.road.Project(old.points[^1],out _); old.halfWidth=3.8f; old.recommendedSpeed=24;
            old.bypassedGates=race.gates.Select((g,i)=>(i,s:race.road.Project(g.transform.position,out _))).Where(g=>g.i>0&&g.s>old.entryRoad&&g.s<old.exitRoad).Select(g=>g.i).ToArray();
            int changed=0,tiles=0;
            var terrain=GameObject.Find("Memory loop - north is +Z").GetComponentsInChildren<MeshFilter>();
            var bounds=branches.Select(b=>{var x=new Bounds(b.points[0],Vector3.zero); foreach(var p in b.points)x.Encapsulate(p); x.Expand(34); return x;}).ToArray();
            foreach(var filter in terrain)
            {
                var mesh=filter.sharedMesh; var vertices=mesh.vertices; var colors=mesh.colors; var originalColors=(Color[])colors.Clone(); bool edited=false;
                for(int i=0;i<vertices.Length;i++)
                {
                    var p=vertices[i]; bool nearHighway=p.z>495&&p.z<595; bool nearBranch=false;
                    for(int b=0;b<branches.Length;b++) if(p.x>=bounds[b].min.x&&p.x<=bounds[b].max.x&&p.z>=bounds[b].min.z&&p.z<=bounds[b].max.z) nearBranch=true;
                    if(!nearHighway&&!nearBranch) continue;
                    float rd=StreetLoopBuilder.Nearest(p,road,out var nearest);
                    if(nearHighway && rd<18)
                    {
                        float s=race.road.Project(nearest,out _),wide=race.road.HighwayBlend(s),hw=race.road.HalfWidth(s);
                        if(wide>.001f)
                        {
                            float target=nearest.y-.4f*Smooth(hw,hw+5,rd);
                            p.y=Mathf.Lerp(p.y,target,wide*(1-Smooth(hw+1,hw+7,rd)));
                            var asphalt=new Color(.24f,.25f,.26f); var shoulder=new Color(.46f,.45f,.39f);
                            if(rd<hw+3) colors[i]=Color.Lerp(asphalt,shoulder,Smooth(hw-.4f,hw+1,rd));
                        }
                    }
                    if(nearBranch)
                        foreach(var branch in branches)
                        {
                            float s=branch.Project(p,out float d); if(d>17) continue;
                            var at=branch.At(s,out _);
                            float preserveRoad=Smooth(5,14,rd);
                            p.y=Mathf.Lerp(p.y,at.y,(1-Smooth(10,17,d))*preserveRoad);
                            colors[i]=Color.Lerp(colors[i],new Color(.47f,.31f,.16f),(1-Smooth(8,10,d))*Smooth(4,9,rd));
                        }
                    if(p!=vertices[i] || colors[i]!=originalColors[i]) { vertices[i]=p; edited=true; changed++; }
                }
                if(!edited)continue;
                mesh.vertices=vertices; mesh.colors=colors; mesh.RecalculateBounds(); EditorUtility.SetDirty(mesh);
                filter.GetComponent<MeshCollider>().sharedMesh=null; filter.GetComponent<MeshCollider>().sharedMesh=mesh; tiles++;
            }
            Normals(terrain); Physics.SyncTransforms();
            // Clear only a narrow corridor; trunks remain the canonical forest placement data.
            var woods=GameObject.Find("Woods replacing later subdivisions"); int removed=0;
            foreach(var box in woods.GetComponentsInChildren<BoxCollider>())
            {
                var p=box.bounds.center; bool cut=branches.Any(b=>{b.Project(p,out var d); return d<12;});
                if(cut){Object.DestroyImmediate(box.gameObject);removed++;continue;}
                if(branches.Any(b=>{b.Project(p,out var d); return d<18;})) box.transform.position+=Vector3.up*(Phase6Buildings.Ground(p)-box.bounds.min.y);
            }
            Highway(root); Fences(root);
            foreach(var prop in Object.FindObjectsByType<BreakableProp>())
            {
                if(prop.name.Contains("Mailbox")) prop.surface=SmashAudio.Surface.Mailbox;
                else if(prop.name.IndexOf("sign",StringComparison.OrdinalIgnoreCase)>=0 || prop.name.Contains("marker")) prop.surface=SmashAudio.Surface.Sign;
                EditorUtility.SetDirty(prop);
                if(PrefabUtility.IsPartOfPrefabInstance(prop)) PrefabUtility.RecordPrefabInstancePropertyModifications(prop);
            }
            foreach(var branch in branches)
            {
                var a=branch.At(0,out var f); Sign(root,a-Vector3.Cross(Vector3.up,f).normalized*9,f,branch.title+"  >\n"+(branch.recommendedSpeed*3.6f).ToString("0")+" km/h",amber);
                for(float s=20;s<branch.Length-15;s+=25)
                {
                    var p=branch.At(s,out var forward); var right=Vector3.Cross(Vector3.up,forward).normalized;
                    foreach(int side in new[]{-1,1}) { var at=p+right*side*9.3f; at.y=Phase6Buildings.Ground(at); Cube(root,"Trail edge",at+Vector3.up*.5f,new(.16f,1,.16f),amber); }
                }
            }
            // A narrow creek cross-section; water is purely visual above the supported bed.
            var water=Mat("Creek water",new(.08f,.31f,.36f)); var wp=creek.At(180,out var wf); wp.y=Phase6Buildings.Ground(wp)+.08f;
            var stream=Cube(root,"Creek surface",wp,new(22,.05f,5),water); stream.rotation=Quaternion.LookRotation(wf);
            PlayerSettings.bundleVersion="0.6.0-review1";
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Phase6Vegetation.Refresh(Evidence+"/forest.txt");
            File.WriteAllLines(Evidence+"/sites-after.txt",buildings.Cast<Transform>().Select(t=>t.name+" "+t.position.ToString("F5")+" "+t.rotation.ToString("F5")));
            File.WriteAllLines(Evidence+"/routes.txt",branches.Append(old).Select(b=>$"{b.title}: road {b.entryRoad:F1} -> {b.exitRoad:F1}; road distance {b.exitRoad-b.entryRoad:F1}m; branch {b.Length:F1}m; bypass CP {string.Join(",",b.bypassedGates)}; entrance {b.points[0]:F2}; exit {b.points[^1]:F2}; target {b.recommendedSpeed*3.6f:F0}km/h"));
            File.WriteAllText(Evidence+"/authoring.txt",$"Local terrain: {tiles} tiles / {changed} vertices; {removed} trunks removed within 12m branch corridors. Houses, yards and shops unchanged. Highway 16.4m asphalt, 4x4.1m lanes, blended 70m merges. No legacy rebuild.\n");
        }
        static void Normals(MeshFilter[] terrain)
        {
            var heights=new Dictionary<Vector2,float>(); foreach(var filter in terrain) foreach(var p in filter.sharedMesh.vertices) heights[new(p.x,p.z)]=p.y;
            foreach(var filter in terrain)
            {
                var mesh=filter.sharedMesh; var vertices=mesh.vertices; var normals=mesh.normals; bool changed=false;
                for(int i=0;i<vertices.Length;i++)
                {
                    var p=vertices[i]; float H(float x,float z)=>heights.TryGetValue(new(x,z),out var h)?h:p.y;
                    var normal=new Vector3(H(p.x-2,p.z)-H(p.x+2,p.z),4,H(p.x,p.z-2)-H(p.x,p.z+2)).normalized;
                    if((normal-normals[i]).sqrMagnitude>1e-8f) { normals[i]=normal; changed=true; }
                }
                if(changed){mesh.normals=normals;EditorUtility.SetDirty(mesh);}
            }
        }
        static void Highway(Transform root)
        {
            for(float s=3720;s<4630;s+=6)
            {
                float blend=race.road.HighwayBlend(s); if(blend<.05f)continue;
                var at=race.road.At(s,out var f); var right=Vector3.Cross(Vector3.up,f).normalized;
                foreach(float side in new[]{-1f,1f})
                {
                    foreach(float offset in new[]{.12f,race.road.HalfWidth(s)-.2f})
                    { var p=at+right*side*offset; p.y=Phase6Buildings.Ground(p)+.025f; var bar=Cube(root,"Highway edge / median",p,new(.12f,.012f,6.1f),offset<1?amber:white); bar.rotation=Quaternion.LookRotation(f); }
                    if((int)(s/6)%2==0) { var p=at+right*side*4.1f*blend; p.y=Phase6Buildings.Ground(p)+.03f; var bar=Cube(root,"Broken lane line",p,new(.12f,.012f,3.3f),white); bar.rotation=Quaternion.LookRotation(f); }
                }
            }
            foreach(var gate in race.gates)
            {
                float s=race.road.Project(gate.transform.position,out _); if(race.road.HighwayBlend(s)<.1f)continue;
                gate.halfWidth=race.road.HalfWidth(s)+2;
                foreach(Transform child in gate.transform)
                {
                    if(Mathf.Abs(child.localPosition.x)>5 && child.localScale.y>3) { var p=child.localPosition; p.x=Mathf.Sign(p.x)*gate.halfWidth; child.localPosition=p; }
                    if(child.localScale.x>10 && child.localScale.y<1) { var scale=child.localScale; scale.x=gate.halfWidth*2; child.localScale=scale; }
                }
            }
            // Retain historical object identities for restoration catalogs; displayed
            // road roles are explicit and do not rename separate Jamerson geometry.
            foreach(var item in new[]{(s:4640f,label:"Hwy 92\nSouth Cherokee Lane",display:"South Cherokee Lane\nJUNCTION: Hwy 92"),(s:3695f,label:"Hwy 92",display:"Trickum Road"),(s:2640f,label:"Jamerson Rd",display:"TO Jamerson Road")})
            { var p=race.road.At(item.s,out var f); Sign(root,p-Vector3.Cross(Vector3.up,f).normalized*14,f,item.label,green);foreach(var lettering in root.GetComponentsInChildren<TextMesh>().Where(t=>t.text==item.label))lettering.text=item.display; }
        }
        static void Fence(Transform root,Vector3 a,Vector3 b,bool chain)
        {
            a.y=Phase6Buildings.Ground(a); b.y=Phase6Buildings.Ground(b); var t=new GameObject(chain?"Chain-link section":"White crossbuck section").transform; t.SetParent(root); t.position=(a+b)*.5f; var axis=b-a; float length=axis.magnitude;
            t.rotation=Quaternion.LookRotation(Vector3.Cross(axis,Vector3.up),Vector3.up); var mat=chain?metal:white;
            foreach(float x in new[]{-length*.5f,length*.5f}) Cube(t,"Post",new(x,.8f,0),new(.10f,1.6f,.10f),mat);
            foreach(float y in new[]{.25f,1.4f}) Cube(t,"Rail",new(0,y,0),new(length,.08f,.08f),mat);
            if(chain)
            {
                for(float x=-length*.5f;x<length*.5f;x+=.35f)
                { float end=Mathf.Min(length*.5f,x+1.15f); Beam(t,"Mesh wire",new(x,.25f,0),new(end,.25f+end-x,0),.018f,mat); Beam(t,"Mesh wire",new(x,1.4f,0),new(end,1.4f-(end-x),0),.018f,mat); }
            }
            else { Beam(t,"Large X",new(-length*.5f,.25f,0),new(length*.5f,1.4f,0),.11f,mat); Beam(t,"Large X",new(-length*.5f,1.4f,.03f),new(length*.5f,.25f,.03f),.11f,mat); }
            // One renderer per section, one trigger, no solid vehicle-launching collision.
            var filters=t.GetComponentsInChildren<MeshFilter>(); var mesh=new Mesh(); mesh.CombineMeshes(filters.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=t.worldToLocalMatrix*f.transform.localToWorldMatrix}).ToArray());
            string path=Folder+"/Fence-"+Guid.NewGuid().ToString("N")+".asset"; AssetDatabase.CreateAsset(mesh,path);
            foreach(var filter in filters) Object.DestroyImmediate(filter.gameObject);
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh; t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;
            Sensor(t,chain?SmashAudio.Surface.ChainLink:SmashAudio.Surface.Wood); t.GetComponent<BoxCollider>().center=new(0,.8f,0); t.GetComponent<BoxCollider>().size=new(length,1.7f,.35f);
        }
        static void Fences(Transform root)
        {
            // Replace the remembered side sections in place; no boundary or building transform edits.
            foreach(var old in Object.FindObjectsByType<BreakableProp>().Where(p=>p.name.StartsWith("Dan west yard fence")||p.name.StartsWith("Original house 2 side fence")).ToArray()) Object.DestroyImmediate(old.gameObject);
            for(int i=0;i<6;i++) Fence(root,new(390,0,-13+i*4),new(390,0,-9+i*4),true);
            foreach(string name in new[]{"Original house 2","Original house 3"})
            {
                var house=GameObject.Find(name).transform;
                for(int i=0;i<5;i++) Fence(root,house.TransformPoint(new(-19,0,-10+i*4)),house.TransformPoint(new(-19,0,-6+i*4)),false);
                // Front stretches stop well short of the central driveway.
                foreach(int side in new[]{-1,1}) for(int i=0;i<2;i++) Fence(root,house.TransformPoint(new(side*(9+i*4),0,17)),house.TransformPoint(new(side*(13+i*4),0,17)),false);
            }
            foreach(float s in new[]{1520f,2170f}) { var p=race.road.At(s,out var f); var right=Vector3.Cross(Vector3.up,f).normalized; Fence(root,p+right*12-f*2,p+right*12+f*2,false); }
        }
    }
}
