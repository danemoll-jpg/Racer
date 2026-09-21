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
        const string ReviewDir="Docs/CR120-121";
        public static void CR120Apply()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();
            if(GameObject.Find("CR120 reverse local cleanup"))throw new Exception("Cleanup already applied; revise locally");
            worldRoot=new GameObject("CR120 reverse local cleanup").transform;
            var timber=Mat("CR120 weathered summit timber",new(.27f,.22f,.15f));
            // Outside the west edge of the southbound runway and the incoming main
            // corridor. This hides the inviting side view without blocking the run-up.
            CR120Screens();
            var signs=Object.FindObjectsByType<PhysicalSign>().Where(s=>s.transform.parent&&s.transform.parent.name=="CR117 selected-direction guidance").ToArray();
            void Move(PhysicalSign sign,Vector3 p,string label=null){p.y=CR105Authoring.Ground(p);sign.transform.position=p;if(label!=null){var text=sign.GetComponentInChildren<TextMesh>();text.text=label;sign.name=label.Replace('\n',' ');}}
            var summitMain=signs.Single(s=>s.name.StartsWith("<<< MAIN ROUTE"));
            Move(summitMain,new(960,0,155),"<<< MAIN ROUTE\nLEFT TO FULL RUN-UP");
            var runwaySign=signs.Single(s=>s.name.StartsWith("BIG FLIGHT AHEAD")&&s.transform.position.z>0);
            Move(runwaySign,new(1011,0,125));
            // Remove just the southbound paint visible at the crossing; the proper
            // runway retains its straight guidance before and after this intersection.
            foreach(var t in GameObject.Find("CR117 selected-direction guidance").GetComponentsInChildren<Transform>())
                if(t.name=="Main teal trail arrow"&&Math.Abs(t.position.x-990)<4&&t.position.z>105&&t.position.z<146&&t.forward.z<-.9f)t.gameObject.SetActive(false);
            var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Downhill Ridge Cut");
            Move(signs.Single(s=>s.name.Contains("OPTIONAL SHORTCUT DOWNHILL")),new(802,0,-113),"SHORTCUT MERGE\nMAIN ROUTE >>>");
            Move(signs.Single(s=>s.name.StartsWith("MAIN ROUTE >>>")),new(825,0,-151),"MAIN ROUTE >>>\nGULLY FLIGHT / FOLLOW TEAL");
            var rejoinSign=signs.Single(s=>s.name=="MAIN ROUTE FOLLOW TEAL ARROWS"&&s.transform.position.x>800);
            Move(rejoinSign,new(850,0,-136));
            CR121ExitGuidance();
            Save();File.WriteAllText(ReviewDir+"/implementation.txt","Reverse scene only. Accepted main route, flight geometry, shortcut difficulty/centerline, rejoin, gate credit and layout ID retained. Summit side screen, local paint correction, relocated guidance boards and clear teal main continuation at branch merge. Dan screenshot clarification supersedes the abandoned chicane idea. No AI tuning, terrain or handling changes.");
            CR120FinalVisual();
        }
        static void CR121ExitGuidance()
        {
            var ridge=Object.FindObjectsByType<WoodlandRoute>().Single(b=>b.title=="Downhill Ridge Cut");
            var guidance=GameObject.Find("CR117 selected-direction guidance");
            foreach(var t in guidance.GetComponentsInChildren<Transform>()){
                if(t.name!="Optional gold trail arrow")continue;float s=ridge.Project(t.position,out float lateral);
                if(s<45&&lateral<5)t.gameObject.SetActive(false);
            }
            var template=guidance.GetComponentsInChildren<Transform>().First(t=>t.name=="Main teal trail arrow");
            for(float s=ridge.entryRoad-35;s<ridge.entryRoad+51;s+=15){
                var p=owner.road.At(s,out var f);p.y=CR105Authoring.Ground(p)+.09f;
                var arrow=Object.Instantiate(template.gameObject,worldRoot);arrow.name="CR121 clear main continuation";arrow.transform.SetPositionAndRotation(p,Quaternion.LookRotation(f));
            }
            var merge=Object.FindObjectsByType<PhysicalSign>().Single(s=>s.transform.position.x==802&&s.transform.position.z==-113);
            merge.transform.rotation=Quaternion.LookRotation(new Vector3(.35f,0,.94f));merge.GetComponentInChildren<TextMesh>().text="SHORTCUT MERGE\nMAIN ROUTE >>>";merge.name="CR121 shortcut merge / main continues";
        }
        static void CR120Screens()
        {
            var a=new Vector3(967,0,118);var b=new Vector3(984,0,103);var direction=(b-a).normalized;
            for(int i=0;i<4;i++){
                var p=Vector3.Lerp(a,b,(i+.5f)/4);p.y=CR105Authoring.Ground(p);
                var timber=Mat("CR120 weathered summit timber",new(.27f,.22f,.15f));
                int courses=Mathf.CeilToInt((172-p.y)/1.75f);
                for(int j=0;j<courses;j++){
                    var panel=Part(worldRoot,"Summit side-entry timber screen "+i+"-"+j,p+Vector3.up*(.9f+j*1.75f),new(2.5f,1.8f,6.1f),timber,true);
                    panel.rotation=Quaternion.LookRotation(direction);
                }
            }
        }
        public static void CR121ScreenshotCorrection()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();worldRoot=GameObject.Find("CR120 reverse local cleanup").transform;
            foreach(var t in worldRoot.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Ridge chicane timber")).ToArray())Object.DestroyImmediate(t.gameObject);
            foreach(var t in worldRoot.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Summit side-entry timber screen")||t.name=="Weathered timber seam").ToArray())Object.DestroyImmediate(t.gameObject);
            CR120Screens();
            var main=Object.FindObjectsByType<PhysicalSign>().Single(s=>s.name.StartsWith("MAIN ROUTE >>>"));var mp=new Vector3(825,0,-151);mp.y=CR105Authoring.Ground(mp);main.transform.position=mp;
            CR121ExitGuidance();Save();CR120Inspect("final");
            File.WriteAllText(ReviewDir+"/screenshot-clarification.txt","Dan supplied needs barrier.png and confused.png. Remove unshipped chicane interpretation; preserve accepted shortcut difficulty. Identify branch merge and emphasize main continuation; remove only inviting nearby gold arrows, retain branch rejoin/gate credit.");
        }
        public static void CR120FinalVisual()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");owner=Object.FindAnyObjectByType<RaceDirector>();owner.road.Initialize();worldRoot=GameObject.Find("CR120 reverse local cleanup").transform;
            foreach(var t in worldRoot.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Summit side-entry timber screen")).ToArray())Object.DestroyImmediate(t.gameObject);
            CR120Screens();
            var signs=Object.FindObjectsByType<PhysicalSign>().Where(s=>s.transform.parent&&s.transform.parent.name=="CR117 selected-direction guidance").ToArray();
            var main=signs.Single(s=>s.name.StartsWith("MAIN ROUTE >>>"));var mp=new Vector3(825,0,-151);mp.y=CR105Authoring.Ground(mp);main.transform.position=mp;
            foreach(var sign in signs.Where(s=>s==main||s.name.StartsWith("CR121 shortcut merge")||s.name.StartsWith("<<< MAIN ROUTE")||(s.name=="MAIN ROUTE FOLLOW TEAL ARROWS"&&s.transform.position.x>800))){
                float station=owner.road.Project(sign.transform.position,out _);float h=Mathf.Max(2.8f,owner.road.At(station,out _).y+2.8f-sign.transform.position.y);
                var post=sign.transform.Find("Whole timber post");post.localPosition=new(0,(h+.5f)*.5f,0);post.localScale=new(.2f,h+.5f,.2f);
                foreach(var child in new[]{sign.transform.Find("Physical sign backing"),sign.transform.Find("Direction lettering")}){var p=child.localPosition;p.y=h;child.localPosition=p;}
            }
            Save();CR120Inspect("verified");
        }
        public static void CR120Inspect(string tag="before")
        {
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Save current scene before inspection");
            EditorSceneManager.OpenScene("Assets/Scenes/MountainLoopReverse.unity");
            owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();Directory.CreateDirectory(ReviewDir);
            var lines=new List<string>();
            foreach(var b in Object.FindObjectsByType<WoodlandRoute>()){
                lines.Add($"BRANCH {b.title} entry={b.entryRoad} exit={b.exitRoad} length={b.Length} first={b.points[0]} last={b.points[^1]}");
                var p=road.At(b.entryRoad-25,out var f);CR120Shot(tag+"-"+b.title.Replace(" ","-")+"-approach",p+Vector3.up*4-f*8,road.At(b.entryRoad+25,out _)+Vector3.up*2);
                CR120Shot(tag+"-"+b.title.Replace(" ","-")+"-overhead",b.points[0]+new Vector3(0,130,-35),b.points[0]);
            }
            foreach(var s in Object.FindObjectsByType<PhysicalSign>()){
                float station=road.Project(s.transform.position,out float distance);
                lines.Add($"SIGN {s.name} at={s.transform.position} forward={s.transform.forward} road={station:F1} distance={distance:F1} parent={s.transform.parent?.name}");
            }
            foreach(var f in owner.GetComponent<MountainFlights>().flights)lines.Add($"FLIGHT {f.name} start={f.start} lip={f.lip} heading={f.forward} station={f.approachStation}");
            foreach(var t in Object.FindObjectsByType<Transform>().Where(t=>t.name.Contains("summit launch")))lines.Add($"LAUNCH {t.name} at={t.position} forward={t.forward}");
            for(float s=0;s<road.Length;s+=20)lines.Add($"ROAD {s:F1} {road.At(s,out _)}");
            File.WriteAllLines(ReviewDir+"/"+tag+"-audit.txt",lines);
            var approach=road.At(435,out var heading);CR120Shot(tag+"-summit-close",approach+Vector3.up*3-heading*7,road.At(451,out _)+Vector3.up*2);
        }
        static void CR120Shot(string name,Vector3 p,Vector3 target)
        {
            var go=new GameObject("Temporary review camera");var c=go.AddComponent<Camera>();c.transform.position=p;c.transform.LookAt(target);c.fieldOfView=65;c.farClipPlane=1800;
            var rt=new RenderTexture(1280,720,24);c.targetTexture=rt;c.Render();var old=RenderTexture.active;RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(ReviewDir+"/"+name+".png",image.EncodeToPNG());RenderTexture.active=old;Object.DestroyImmediate(image);c.targetTexture=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(go);
        }
    }
}
