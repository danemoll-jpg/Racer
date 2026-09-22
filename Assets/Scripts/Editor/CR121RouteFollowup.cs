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
        const string CR121Dir="Docs/CR121-followup";
        public static void CR121Repair()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();
            if(GameObject.Find("CR121 continuous reverse guidance"))throw new Exception("Already repaired; revise locally");
            worldRoot=new GameObject("CR121 continuous reverse guidance").transform;
            var summit=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse");var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Downhill Ridge Cut");
            var oldTail=ridge.At(70,out _);var oldEnd=ridge.points[^1];
            foreach(var t in GameObject.Find("CR117 selected-direction guidance").GetComponentsInChildren<Transform>(true))if(t.name=="Optional gold trail arrow"&&ridge.Project(t.position,out float d)<ridge.Length&&d<6)t.gameObject.SetActive(false);
            var old=GameObject.Find("Ground_CR117 reverse ridge shortcut");if(old)old.SetActive(false);
            ridge.points=Curve(new[]{road.At(1510,out _),new Vector3(808,96.8f,-188),new(790,96,-183),new(774,94,-163),oldTail,new(755.09f,88.72f,-136.15f),oldEnd});ridge.entryRoad=1510;ridge.entryInset=12;ridge.entryMargin=1.5f;
            MountainSurface("CR121 early ridge entrance",ridge.points,3.6f);
            ClearCompleteTrees(p=>Near(p,ridge.points,out _)<7);
            var gate=owner.gates[3];var gp=road.At(1480,out var gf);gate.transform.SetPositionAndRotation(gp+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(gf,Vector3.up)));
            foreach(var b in new[]{summit,ridge})b.bypassedGates=Enumerable.Range(1,owner.gates.Length-1).Where(i=>road.Relative(road.Project(owner.gates[i].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();
            owner.courseId="mountain-reverse-v3-clear-junctions";
            Physics.SyncTransforms();
            // A continuous physical separator closes the old late entry; the new
            // entrance passes around its southern end and retains the original exit.
            var timber=Mat("CR120 weathered summit timber",new(.27f,.22f,.15f));
            for(int i=0;i<12;i++){var p=new Vector3(800,0,-172.5f+i*5);p.y=CR105Authoring.Ground(p);Part(worldRoot,"CR121 closed late cut barrier "+i,p+Vector3.up*1.7f,new(2.4f,3.4f,5.1f),timber,true);}
            foreach(var sign in Object.FindObjectsByType<PhysicalSign>().Where(s=>s.name.StartsWith("CR121 shortcut merge")))sign.gameObject.SetActive(false);
            for(float s=85;s<summit.Length-8;s+=15){var p=summit.At(s,out var f);CR121Paint(p,f,true);}
            for(float s=8;s<ridge.Length;s+=13){var p=ridge.At(s,out var f);CR121Paint(p,f,true);}
            var fp=summit.At(206,out var ff);CR121Board(fp-Vector3.Cross(Vector3.up,ff).normalized*6,ff,"<<< LEFT FORK\nSUMMIT TRAVERSE",true);
            var hp=summit.At(380,out var hf);CR121Board(hp+Vector3.Cross(Vector3.up,hf).normalized*11,hf,"HAIRPIN AT REJOIN\nTURN BACK ONTO TEAL",true);
            var p1=road.At(1488,out var f1);CR121Board(p1-Vector3.Cross(Vector3.up,f1).normalized*10,f1,"<<< SHORTCUT 2\nDOWNHILL RIDGE CUT",true);
            var p2=ridge.At(15,out var f2);CR121Board(p2-Vector3.Cross(Vector3.up,f2).normalized*7,f2,"DOWNHILL RIDGE CUT\nFOLLOW GOLD",true);
            var p3=road.At(1548,out var f3);CR121Board(new(798,100,-145),f3,"OLD CUT CLOSED\nMAIN ROUTE >>>",false);
            // Paint a tight return within the existing supported junction. Gold
            // hands off to teal pointing uphill; never toward the closed shortcut.
            foreach(float s in new[]{1585f,1598f,1611f}){var p=road.At(s,out var f);CR121Paint(p,f,false);}
            CR121EntranceVisual();Save();
            var view=summit.At(230,out var vf);CR121Shot("after-summit-fork",view-vf*7+Vector3.up*3,summit.At(254,out _)+Vector3.up*1.3f);
            CR121Trace("after");
        }
        static void CR121Paint(Vector3 p,Vector3 f,bool gold)
        {
            var right=Vector3.Cross(Vector3.up,f).normalized;f=Vector3.ProjectOnPlane(f,Vector3.up).normalized;float w=gold?1.1f:1.5f;
            var shape=new[]{new Vector2(-w*.4f,-3),new(w*.4f,-3),new(w*.4f,0),new(w,0),new(0,3),new(-w,0),new(-w*.4f,0)};
            var vertices=shape.Select(v=>{var q=p+right*v.x+f*v.y;q.y=CR105Authoring.Ground(q)+.12f;return q;}).ToArray();
            var mesh=new Mesh{vertices=vertices,triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();
            var go=new GameObject(gold?"CR121 continuous gold arrow":"CR121 uphill teal handoff",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(worldRoot);go.GetComponent<MeshFilter>().sharedMesh=MeshAsset(mesh,"CR121-arrow-"+worldRoot.childCount);go.GetComponent<Renderer>().sharedMaterial=gold?Mat("CR117 alternate gold",new(.88f,.6f,.18f)):Mat("CR117 main teal",new(.2f,.65f,.54f));
        }
        static void CR121Board(Vector3 p,Vector3 f,string text,bool gold)
        {
            var template=Object.FindObjectsByType<PhysicalSign>().First(s=>s.name.StartsWith("MAIN ROUTE >>>"));var go=Object.Instantiate(template.gameObject,worldRoot);go.name="CR121 "+text.Replace('\n',' ');p.y=CR105Authoring.Ground(p);go.transform.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));
            var post=go.transform.Find("Whole timber post");post.localPosition=new(0,1.7f,0);post.localScale=new(.2f,3.4f,.2f);foreach(string n in new[]{"Physical sign backing","Direction lettering"}){var t=go.transform.Find(n);var q=t.localPosition;q.y=2.8f;t.localPosition=q;}
            var label=go.GetComponentInChildren<TextMesh>();label.text=text;label.color=gold?new(1,.79f,.38f):new(.7f,1,.91f);label.transform.localScale=Vector3.one;label.font.RequestCharactersInTexture(text,label.fontSize);var bounds=label.GetComponent<Renderer>().localBounds;label.transform.localScale=Vector3.one*Mathf.Min(8.2f/Math.Max(.01f,bounds.size.x),2.2f/Math.Max(.01f,bounds.size.y));
        }
        public static void CR121EntranceVisual()
        {
            owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();worldRoot=GameObject.Find("CR121 continuous reverse guidance").transform;
            foreach(var sign in worldRoot.GetComponentsInChildren<PhysicalSign>()){
                float s=owner.road.Project(sign.transform.position,out _);float h=Math.Max(2.8f,owner.road.At(s,out _).y+3.2f-sign.transform.position.y);
                if(sign.name.Contains("LEFT FORK")||sign.name.Contains("HAIRPIN"))continue;
                var post=sign.transform.Find("Whole timber post");post.localPosition=new(0,(h+.5f)*.5f,0);post.localScale=new(.2f,h+.5f,.2f);
                foreach(string n in new[]{"Physical sign backing","Direction lettering"}){var t=sign.transform.Find(n);var p=t.localPosition;p.y=h;t.localPosition=p;}
            }
            var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Downhill Ridge Cut");var mouth=ridge.points[0];CR121Paint(mouth,ridge.At(14,out _)-mouth,true);
            Save();var approach=owner.road.At(1500,out var f);CR121Shot("after-shortcut-2-entrance",approach-f*7+Vector3.up*3, ridge.At(18,out _)+Vector3.up*1.5f);
        }
        public static void CR121Trace(string tag="before")
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save current scene before tracing");
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();Directory.CreateDirectory(CR121Dir);
            var rows=new List<string>{"course="+owner.courseId};
            foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){
                rows.Add($"BRANCH {b.title} entry={b.entryRoad} exit={b.exitRoad} length={b.Length} gates={string.Join(",",b.bypassedGates)}");
                for(float s=0;s<=b.Length;s+=5)rows.Add($"{b.title},{s},{b.At(s,out var f)},{f}");
            }
            for(int i=0;i<owner.gates.Length;i++)rows.Add($"GATE {i} station={owner.road.Project(owner.gates[i].transform.position,out _)} position={owner.gates[i].transform.position}");
            foreach(var c in Object.FindObjectsByType<Collider>())if((c.bounds.ClosestPoint(new(1030,142,-50))-new Vector3(1030,142,-50)).magnitude<25||(c.bounds.ClosestPoint(new(810,98,-142))-new Vector3(810,98,-142)).magnitude<22)rows.Add($"LOCAL COLLIDER {c.name} parent={c.transform.parent?.name} bounds={c.bounds}");
            File.WriteAllLines(CR121Dir+"/"+tag+"-trace.txt",rows);
            var summit=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Summit Traverse");
            foreach(float s in new[]{180f,330f,390f,415f}){var p=summit.At(s,out var f);CR121Shot(tag+"-summit-"+s,p-f*7+Vector3.up*3,summit.At(Math.Min(s+24,summit.Length),out _)+Vector3.up*1.3f);}
            var road=owner.road;foreach(float s in new[]{1480f,1520f,1550f}){var p=road.At(s,out var f);CR121Shot(tag+"-main-"+s,p-f*7+Vector3.up*3,road.At(s+35,out _)+Vector3.up*1.5f);}
            CR121Shot(tag+"-junction-overview",new(890,250,-230),new(820,98,-130));
        }
        static void CR121Shot(string name,Vector3 position,Vector3 target)
        {
            var go=new GameObject("Temporary local route camera");var camera=go.AddComponent<Camera>();camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=65;camera.farClipPlane=1800;
            var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(CR121Dir+"/"+name+".png",image.EncodeToPNG());RenderTexture.active=previous;Object.DestroyImmediate(image);camera.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);
        }
    }
}
