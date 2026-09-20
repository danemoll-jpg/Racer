using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object=UnityEngine.Object;
namespace Racer.Editor
{
    public static partial class DiscoveryAuthoring
    {
        static void Collectibles()
        {
            var c=owner.GetComponent<ExplorationCollection>();var paths=new List<Vector3[]>();var inventory=new List<string>{"id,title,region,x,y,z,accessX,accessY,accessZ,clue"};
            for(int i=0;i<c.sites.Length;i++){
                var site=c.sites[i];var road=i<12?street:c.routes[0];road.Initialize();float station=road.Project(site.position,out _);Vector3 start=Ground(road.At(station,out var forward));Vector3 end=start;
                if(i>=2){bool found=false;foreach(float shift in new[]{0f,18f,-18f,35f,-35f,60f,-60f,90f,-90f,120f,-120f,160f,-160f,210f,-210f,270f,-270f}){
                    start=Ground(road.At(station+shift,out forward));var side=Vector3.Cross(Vector3.up,forward).normalized;
                    foreach(float offset in new[]{30f,-30f,24f,-24f,20f,-20f,16f,-16f}){var candidate=Ground(start+side*offset);if(Mathf.Abs(candidate.y-start.y)>Mathf.Abs(offset)*.30f)continue;
                        if(c.sites.Take(i).Any(s=>Vector3.ProjectOnPlane(s.position-candidate,Vector3.up).magnitude<12))continue;
                        bool blocked=false;foreach(var b in Object.FindObjectsByType<BoxCollider>()){if(b.isTrigger||b.name.ToLowerInvariant().Contains("tree")||b.name.ToLowerInvariant().Contains("trunk")||b.GetComponentInParent<ArcadeVehicle>())continue;var bounds=b.bounds;bounds.Expand(6);if(bounds.Contains(candidate+Vector3.up)) {blocked=true;break;}}
                        if(candidate.x>550&&candidate.x<712&&candidate.z>-78&&candidate.z<60)blocked=true;
                        if(blocked)continue;end=candidate;found=true;break;
                    }if(found)break;
                }if(!found)throw new Exception("No safe hidden location for "+site.id);}
                site.access=start;site.position=end+Vector3.up;site.approach=i<8?"Neighborhood woodland":i<12?"Western gullies":i<18?"Creek pockets":"Ridge woodland";
                if(i>=2){site.title=new[]{"Fallen log pocket","Cairn hollow","Behind the pines","Woodland nook","Quiet gully","Old stone corner"}[i%6];paths.Add(new[]{start,end});
                    var mark=new GameObject("Acorn clue / "+site.id).transform;mark.SetParent(worldRoot);mark.position=Ground(Vector3.Lerp(start,end,.28f));var mat=Mat("Cairn stone",new(.58f,.5f,.32f));for(int j=0;j<3;j++)Part(mark,"Trail cairn",new(0,.15f+j*.22f,0),new(.8f-j*.19f,.3f,.65f-j*.14f),mat,false,PrimitiveType.Sphere);
                }
                inventory.Add($"{site.id},{site.title},{site.approach},{site.position.x:F3},{site.position.y:F3},{site.position.z:F3},{start.x:F3},{start.y:F3},{start.z:F3},three-stone cairn leads into pocket");
            }
            Terrain("acorn-spurs",p=>{foreach(var line in paths){if(Mathf.Abs(p.x-line[0].x)>45||Mathf.Abs(p.z-line[0].z)>45)continue;float d=Near(p,line,out var at);if(d<6)p.y=Mathf.Lerp(p.y,at.y,1-Smooth(3.3f,6,d));}return p;});
            ClearTrees(p=>paths.Any(line=>Mathf.Abs(p.x-line[0].x)<45&&Mathf.Abs(p.z-line[0].z)<45&&Near(p,line,out _)<4));
            foreach(var site in c.sites)site.position=Ground(site.position)+Vector3.up;
            File.WriteAllLines(Evidence+"/collectibles-"+owner.gameObject.scene.name+".csv",inventory);
        }
        static void NewSign(Vector3 p,Vector3 forward,string value)
        {
            var r=new GameObject(value).transform;r.SetParent(worldRoot);r.SetPositionAndRotation(Ground(p),Quaternion.LookRotation(forward));var wood=Mat("Sign timber",new(.22f,.14f,.07f));Part(r,"Grounded sign post",new(0,1.4f,0),new(.18f,2.8f,.18f),wood);Part(r,"Sign board",new(0,2.4f,0),new(6,1.8f,.18f),wood);var t=new GameObject("Physical lettering").AddComponent<TextMesh>();t.transform.SetParent(r,false);t.transform.localPosition=new(0,2.4f,-.12f);t.text=value;t.fontSize=64;t.characterSize=.15f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=new(1,.94f,.74f);r.gameObject.AddComponent<PhysicalSign>();var box=r.gameObject.AddComponent<BoxCollider>();box.center=new(0,1.4f,0);box.size=new(6,2.8f,.3f);r.gameObject.AddComponent<BreakableProp>().surface=SmashAudio.Surface.Wood;
        }
        static void Signs()
        {
            NewSign(new(1060,0,174),Vector3.left,"SUMMIT HOMEWARD FLIGHT\n55–90 mph / HIGH RISK\nLAND STRAIGHT / BRAKE AFTER");
            NewSign(new(813,0,206),Vector3.right,"RETURN VIA RIDGE TRAIL\nDO NOT CLIMB LAUNCH FACE");
            NewSign(new(1000,0,92),Vector3.forward,"SUMMIT CAMP\nLook for the cairns");
            var rows=new List<string>{"object|text|width|height|characterSize|fit"};
            foreach(var text in Object.FindObjectsByType<TextMesh>()){
                if(!text.GetComponent<Renderer>().enabled||!text.gameObject.activeInHierarchy||SceneryText.IsFloating(text))continue;
                var t=text.transform;var parent=t.parent;if(!parent)continue;
                var board=parent.GetComponentsInChildren<MeshFilter>().Where(m=>m.name.IndexOf("board",StringComparison.OrdinalIgnoreCase)>=0||m.name.IndexOf("panel",StringComparison.OrdinalIgnoreCase)>=0||m.name=="Face").OrderBy(m=>Vector3.Distance(m.GetComponent<Renderer>().bounds.center,t.position)).FirstOrDefault();
                if(!board){rows.Add(text.name+"|"+text.text.Replace('\n','/')+"|existing composite backing; manual rendered audit required");continue;}
                var bounds=board.GetComponent<Renderer>().bounds;var local=new Bounds(t.InverseTransformPoint(bounds.center),Vector3.zero);foreach(float x in new[]{bounds.min.x,bounds.max.x})foreach(float y in new[]{bounds.min.y,bounds.max.y})foreach(float z in new[]{bounds.min.z,bounds.max.z})local.Encapsulate(t.InverseTransformPoint(new(x,y,z)));
                float width=local.size.x*.86f,height=local.size.y*.80f;var rb=text.GetComponent<Renderer>().localBounds;
                float ratio=Mathf.Min(width/Mathf.Max(.01f,rb.size.x),height/Mathf.Max(.01f,rb.size.y));
                if(ratio<1){float size=text.characterSize*ratio;if(size<.09f){float grow=.09f/Mathf.Max(.001f,size);board.transform.localScale=Vector3.Scale(board.transform.localScale,new Vector3(grow,grow,1));width*=grow;height*=grow;ratio*=grow;}text.characterSize*=ratio*.98f;}
                if(!parent.GetComponent<PhysicalSign>())parent.gameObject.AddComponent<PhysicalSign>();
                rows.Add(text.name+"|"+text.text.Replace('\n','/')+"|"+width+"|"+height+"|"+text.characterSize+"|contained by measured text/board bounds");
            }
            File.WriteAllLines(Evidence+"/signs-"+owner.gameObject.scene.name+".txt",rows);
        }
        static void Map()
        {
            var map=owner.GetComponent<ExplorationMap>()??owner.gameObject.AddComponent<ExplorationMap>();var collection=owner.GetComponent<ExplorationCollection>();
            var points=new[]{("home","Dan's neighborhood",street.At(street.Project(GameObject.Find("Dan - blue X").transform.position,out _)-28,out _)),("lake","Lake gateway",collection.routes[0].At(12,out _)),("ridge","Ridge overlook",collection.routes[0].At(680,out _)),("market","Highway market",street.At(street.Project(new Vector3(25,0,530),out _),out _)),("west","Trickum woods",street.At(street.Project(new Vector3(-622,0,150),out _),out _))};
            map.destinations=points.Select(p=>{var q=Ground(p.Item3);street.At(street.Project(q,out _),out var f);if(p.Item1=="lake"||p.Item1=="ridge")collection.routes[0].At(collection.routes[0].Project(q,out _),out f);return new ExplorationMap.Destination{id=p.Item1,title=p.Item2,position=q,yaw=Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up)).eulerAngles.y};}).ToArray();
            int w=440,h=320;var texture=new Texture2D(w,h,TextureFormat.RGB24,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};var colors=new Color[w*h];var water=Object.FindObjectsByType<ShallowWater>();
            for(int y=0;y<h;y++)for(int x=0;x<w;x++){var p=ExplorationMap.WorldPoint(new Vector2((x+.5f)/w,(y+.5f)/h));var hits=Physics.RaycastAll(new(p.x,300,p.z),Vector3.down,600).Where(hit=>hit.collider.name.StartsWith("Ground_")).OrderBy(hit=>hit.distance).ToArray();colors[y*w+x]=hits.Length==0?new(.09f,.12f,.13f):Color.Lerp(new(.16f,.29f,.15f),new(.62f,.58f,.38f),Mathf.InverseLerp(0,175,hits[0].point.y));if(hits.Length>0&&water.Any(lake=>lake.Contains(p)&&hits[0].point.y<lake.Surface))colors[y*w+x]=new(.13f,.36f,.46f);}
            foreach(var route in new[]{street,owner.road}.Concat(collection.routes).Distinct()){route.Initialize();float length=route.Length;if(route.name=="Kyle descending driveway"){length=0;for(int i=1;i<route.points.Length;i++)length+=Vector3.Distance(route.points[i-1],route.points[i]);}for(float s=0;s<length;s+=2){var uv=ExplorationMap.Normalized(route.At(s,out _));int x=Mathf.FloorToInt(uv.x*w),y=Mathf.FloorToInt(uv.y*h);if(x>=0&&x<w&&y>=0&&y<h)colors[y*w+x]=route==street?new(.65f,.64f,.58f):new(.61f,.43f,.23f);}}
            texture.SetPixels(colors);texture.Apply();string path=Folder+"/map-"+owner.gameObject.scene.name+".asset";AssetDatabase.CreateAsset(texture,path);map.terrain=texture;
            File.WriteAllText(Evidence+"/map-"+owner.gameObject.scene.name+".json",JsonUtility.ToJson(map,true));
        }
    }
}

