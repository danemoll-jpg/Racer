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
        public static void CR117Polish(bool forwardOnly=false)
        {
            foreach(var path in CR112Scenes.Where(p=>p.Contains("MountainLoop")&&(!forwardOnly||!p.Contains("Reverse")))){
                EditorSceneManager.OpenScene(path);owner=Object.FindAnyObjectByType<RaceDirector>();var road=owner.road;road.Initialize();
                if(GameObject.Find("CR117 selected-direction guidance"))throw new Exception("Guidance exists; revise locally");worldRoot=new GameObject("CR117 selected-direction guidance").transform;
                var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");var lettering=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/CR102 lettering.mat");
                float LocalGround(Vector3 p)=>Physics.RaycastAll(p+Vector3.up*4,Vector3.down,8,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).Select(h=>h.point.y).DefaultIfEmpty(p.y).Max();
                bool Supported(Vector3 p)=>Physics.RaycastAll(p+Vector3.up*2,Vector3.down,4,1,QueryTriggerInteraction.Ignore).Any(h=>h.collider.name.StartsWith("Ground_")&&Math.Abs(h.point.y-p.y)<1.5f);
                void Board(Vector3 p,Vector3 forward,string text,bool optional=false){
                    var r=new GameObject(text.Replace('\n',' ')).transform;r.SetParent(worldRoot);p.y=LocalGround(p);r.SetPositionAndRotation(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));
                    var wood=Mat("CR117 dark sign timber",new(.08f,.19f,.18f));Part(r,"Whole timber post",new(0,1.7f,0),new(.2f,3.4f,.2f),wood);Part(r,"Physical sign backing",new(0,2.8f,0),new(9,2.6f,.22f),wood);
                    var label=new GameObject("Direction lettering").AddComponent<TextMesh>();label.transform.SetParent(r,false);label.transform.localPosition=new(0,2.8f,-.13f);label.font=font;label.fontSize=64;label.characterSize=.1f;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=optional?new(1,.79f,.38f):new(.7f,1,.91f);label.text=text;font.RequestCharactersInTexture(text,64);label.GetComponent<Renderer>().sharedMaterial=lettering;var bounds=label.GetComponent<Renderer>().localBounds;label.transform.localScale=Vector3.one*Mathf.Min(8.2f/Math.Max(.01f,bounds.size.x),2.2f/Math.Max(.01f,bounds.size.y));r.gameObject.AddComponent<PhysicalSign>();
                }
                string Arrow(Vector3 forward,Vector3 target,string label){float turn=Vector3.Dot(Vector3.Cross(Vector3.up,forward).normalized,target.normalized);return turn>.18f?label+" >>>":turn<-.18f?"<<< "+label:"^ "+label+" ^";}
                void Paint(Vector3 p,Vector3 f,bool optional){p.y=LocalGround(p)+.065f;var r=new GameObject(optional?"Optional gold trail arrow":"Main teal trail arrow",typeof(MeshFilter),typeof(MeshRenderer));r.transform.SetParent(worldRoot);r.transform.SetPositionAndRotation(p,Quaternion.LookRotation(f));float width=optional?.9f:1.5f;var m=new Mesh{vertices=new[]{new Vector3(-width*.4f,0,-3),new(width*.4f,0,-3),new(width*.4f,0,0),new(width,0,0),new(0,0,3),new(-width,0,0),new(-width*.4f,0,0)},triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};m.RecalculateNormals();m.RecalculateBounds();r.GetComponent<MeshFilter>().sharedMesh=MeshAsset(m,owner.gameObject.scene.name+"-CR117-arrow-"+worldRoot.childCount);r.GetComponent<Renderer>().sharedMaterial=optional?Mat("CR117 alternate gold",new(.88f,.6f,.18f)):Mat("CR117 main teal",new(.2f,.65f,.54f));}
                var branches=Object.FindObjectsByType<WoodlandRoute>();
                foreach(var b in branches){
                    var p=road.At(b.entryRoad-22,out var f);var main=road.At(b.entryRoad+38,out _)-road.At(b.entryRoad,out _);var alternate=b.At(Math.Min(32,b.Length),out _)-b.points[0];
                    Board(p+Vector3.Cross(Vector3.up,f).normalized*10,f,Arrow(f,main,"MAIN ROUTE")+"\nTWO BIG FLIGHTS / FOLLOW TEAL");
                    var bp=b.At(10,out var bf);Board(bp-Vector3.Cross(Vector3.up,bf).normalized*7,bf,"OPTIONAL SHORTCUT\n"+b.title.ToUpperInvariant(),true);
                    for(float s=15;s<Math.Min(b.Length,85);s+=25)Paint(b.At(s,out var heading),heading,true);
                    for(float s=8;s<85;s+=25)Paint(road.At(b.entryRoad+s,out var heading),heading,false);
                    var exit=road.At(b.exitRoad+25,out var ef);Board(exit+Vector3.Cross(Vector3.up,ef).normalized*10,ef,"MAIN ROUTE\nFOLLOW TEAL ARROWS");
                }
                foreach(var flight in owner.GetComponent<MountainFlights>().flights){Board(flight.start+Vector3.Cross(Vector3.up,flight.forward)*13,flight.forward,"MAIN ROUTE / BIG JUMP\n"+flight.name.ToUpperInvariant());Board(flight.lip-flight.forward*60+Vector3.Cross(Vector3.up,flight.forward)*13,flight.forward,"BIG FLIGHT AHEAD\nSTRAIGHT / FULL RUN-UP");}
                for(float s=0;s<road.Length;s+=70){var p=road.At(s,out var f);if(Supported(p))Paint(p,f,false);}
                var stations=new List<float>{road.Project(owner.gates[0].transform.position,out _)};foreach(var b in branches){stations.Add(Mathf.Repeat(b.entryRoad-35,road.Length));stations.Add(Mathf.Repeat(b.exitRoad+40,road.Length));}
                foreach(var flight in owner.GetComponent<MountainFlights>().flights){float s=Mathf.Repeat(flight.approachStation-30,road.Length);if(stations.All(t=>Math.Abs(Mathf.DeltaAngle(t/road.Length*360,s/road.Length*360))*road.Length/360>45))stations.Add(s);}
                var original=owner.gates.Select(g=>g.gameObject).ToArray();var sorted=stations.OrderBy(s=>road.Relative(s,stations[0])).ToArray();var gates=new List<RaceGate>();
                foreach(float s in sorted){var go=Object.Instantiate(original[0],worldRoot);var g=go.GetComponent<RaceGate>();var p=road.At(s,out var f);go.transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)));go.name="CR117 "+(gates.Count==0?"START FINISH":"CP "+gates.Count);foreach(var label in go.GetComponentsInChildren<TextMesh>(true))label.text=gates.Count==0?"START / FINISH":"CP "+gates.Count;gates.Add(g);}
                foreach(var go in original)Object.DestroyImmediate(go);owner.gates=gates.ToArray();foreach(var b in branches)b.bypassedGates=Enumerable.Range(1,gates.Count-1).Where(i=>road.Relative(road.Project(gates[i].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();
                var start=owner.gates[0].transform.position;ClearCompleteTrees(p=>Vector2.Distance(new(p.x,p.z),new(start.x,start.z))<24);
                Save();CR117Overview();
                File.WriteAllLines("Docs/CR112-118/guidance-"+owner.gameObject.scene.name+".txt",branches.Select(b=>$"{b.title}: main interval {road.Relative(b.exitRoad,b.entryRoad):F1} m / shortcut {b.Length:F1} m / bypass gates {string.Join(",",b.bypassedGates)}"));
            }
        }
    }
}
