using UnityEngine;
using UnityEngine.UI;
namespace Racer {
// Full menu map only. Navigation is drawn from the active course's actual data.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WorldMapCourseOverlay : MaskableGraphic {
 // 0.90 Part A: a locked course's route is drawn in grey (Tracks and the playlist editor).
 public bool Locked {get;private set;} public void SetLocked(bool value){if(Locked==value)return;Locked=value;SetVerticesDirty();}
 Color Main=>Locked?new Color(.55f,.57f,.58f):new Color(.3f,1,.88f);Color Branch=>Locked?new Color(.42f,.43f,.44f):new Color(1,.72f,.2f);
 RaceDirector race;WorldMapVisual visual;Vector2 center;float zoom;CoursePreviewCatalog.Course preview;System.Collections.Generic.List<CoursePreviewCatalog.Course> previews;
 public void SetView(RaceDirector owner,WorldMapVisual map,Vector2 at,float scale,CoursePreviewCatalog.Course course=null){if(spot==null&&race==owner&&visual==map&&center==at&&zoom==scale&&preview==course&&previews==null)return;race=owner;visual=map;center=at;zoom=scale;preview=course;previews=null;spot=null;SetVerticesDirty();}
 // 0.76: several courses at once (null = the active course); a race-only route (no road in FreeRoamWorld) is dashed.
 public void SetView(RaceDirector owner,WorldMapVisual map,Vector2 at,float scale,System.Collections.Generic.List<CoursePreviewCatalog.Course> courses){race=owner;visual=map;center=at;zoom=scale;preview=null;previews=courses;spot=null;SetVerticesDirty();}
 Vector2 Point(Vector3 p){var uv=visual?visual.Normalized(p):ExplorationMap.Normalized(p);return Vector2.Scale((uv-center)*zoom,rectTransform.rect.size);}
 void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint,float width=2){var side=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int i=vh.currentVertCount;vh.AddVert(a-side,tint,Vector2.zero);vh.AddVert(a+side,tint,Vector2.zero);vh.AddVert(b+side,tint,Vector2.zero);vh.AddVert(b-side,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
 void Route(VertexHelper vh,Vector3[] pts,Color tint,bool closed,float undergroundStart=0,float undergroundEnd=0,bool dashed=false){float distance=0,station=0,dash=0;Vector3 from=pts.Length>0?pts[0]:default;var edge=rectTransform.rect.size*.5f+Vector2.one*24;int count=pts.Length-(closed?0:1);for(int i=0;i<count;i++){var to=pts[(i+1)%pts.Length];var a=Point(from);var b=Point(to);
 // 0.76: points closer than 2 px on screen are merged (several routes at once stay under the 65,000-vertex UI mesh limit)
 if((b-a).sqrMagnitude<4&&i<count-1)continue;if(Mathf.Abs(a.x)>edge.x&&Mathf.Abs(b.x)>edge.x&&Mathf.Sign(a.x)==Mathf.Sign(b.x)||Mathf.Abs(a.y)>edge.y&&Mathf.Abs(b.y)>edge.y&&Mathf.Sign(a.y)==Mathf.Sign(b.y)){from=to;continue;}// off the map view
 float step=Vector2.Distance(new(from.x,from.z),new(to.x,to.z));bool underground=undergroundEnd>undergroundStart&&station>=undergroundStart&&station<undergroundEnd;station+=step;if(underground&&Mathf.FloorToInt(station/4)%2==1){from=to;continue;}if(dashed){dash+=(b-a).magnitude;if(Mathf.FloorToInt(dash/7)%2==1){from=to;continue;}}Line(vh,a,b,new Color(0,.06f,.07f,.9f),5);Line(vh,a,b,tint);distance+=(b-a).magnitude;if(distance>85&&!underground){distance=0;var f=(b-a).normalized;var side=new Vector2(-f.y,f.x);Line(vh,b-f*8+side*4,b,Color.white);Line(vh,b-f*8-side*4,b,Color.white);}from=to;}}
 void Course(VertexHelper vh,CoursePreviewCatalog.Course c){Route(vh,c.main,Main,true,0,0,c.raceOnly);foreach(var b in c.branches)Route(vh,b.points,Branch,false,b.undergroundStart,b.undergroundEnd,c.raceOnly);for(int i=0;i<c.gates.Length;i++){var p=Point(c.gates[i]);Line(vh,p+Vector2.left*4,p+Vector2.right*4,i==0?Color.white:new Color(.3f,.6f,1),8);}}
 // 0.116 Part A item 4: Leviathan's maps (Tracks preview, loading screen, the race's map): the world map picture shows Free Roam's trails, and
 // two of them are Street Loop Reverse's shortcuts (Granite Creek Cut, Laurel Switchbacks), which Leviathan does not have. They are painted over
 // in the picture's own colour beside them (Resources/Leviathan/MapCover.json, Tools/Report116/py/mapcover.py), under the course lines.
 [System.Serializable] sealed class CoverPath{public string title;public float[] pts,colors;} [System.Serializable] sealed class CoverData{public float width;public CoverPath[] paths;}
 static CoverData cover;static bool coverLoaded;
 bool LeviathanMap=>preview!=null?preview.id!=null&&preview.id.StartsWith("leviathan-"):previews==null&&race&&race.Leviathan;
 void Cover(VertexHelper vh){if(!coverLoaded){coverLoaded=true;var a=Resources.Load<TextAsset>("Leviathan/MapCover");if(a)cover=JsonUtility.FromJson<CoverData>(a.text);}if(cover?.paths==null)return;
  foreach(var c in cover.paths){int n=c.pts.Length/2;for(int i=0;i+1<n;i++){var p=new Vector3(c.pts[2*i],0,c.pts[2*i+1]);var q=new Vector3(c.pts[2*i+2],0,c.pts[2*i+3]);var a=Point(p);var b=Point(q);float w=(Point(p+Vector3.right*cover.width)-a).magnitude;var col=new Color(c.colors[3*i],c.colors[3*i+1],c.colors[3*i+2]);var f=(b-a).normalized;Line(vh,a-f*w*.3f,b+f*w*.3f,col,w);}}}
 // 1.3.0-review4 (Dan: "the stunt course shows up as the street loop race on the map"): a campaign event held at one place, not round a lap
 // (the stunt park; a speed trap, jump, smash, tricks or land-it-clean spot): no race route, the place itself. area: an outline filled in (the
 // park); road: a faint line of the road round the spot; runUp: the run-up from the event's start to the spot, an arrow the way you go; at: the
 // spot, marked with the kind's icon (a disc with a coloured ring and a white sign drawn in lines, so no font glyph is needed).
 public enum SpotKind{Park,SpeedTrap,Jump,Smash,Trick,Clean}
 sealed class Spot{public Vector3 at;public SpotKind kind;public Vector3[] area,road,runUp;}
 Spot spot;
 public void SetSpot(WorldMapVisual map,Vector2 at,float scale,Vector3 site,SpotKind kind,Vector3[] area,Vector3[] road,Vector3[] runUp){race=null;preview=null;previews=null;visual=map;center=at;zoom=scale;spot=new Spot{at=site,kind=kind,area=area,road=road,runUp=runUp};SetVerticesDirty();}
 static readonly Color Dark=new(0,.06f,.07f,.9f);
 void Tri(VertexHelper vh,Vector2 a,Vector2 b,Vector2 c,Color tint){int i=vh.currentVertCount;vh.AddVert(a,tint,Vector2.zero);vh.AddVert(b,tint,Vector2.zero);vh.AddVert(c,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);}
 void Disc(VertexHelper vh,Vector2 c,float r,Color tint){for(int k=0;k<32;k++){float a=k*Mathf.PI/16,b=(k+1)*Mathf.PI/16;Tri(vh,c,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,tint);}}
 void Arc(VertexHelper vh,Vector2 c,float r,float from,float to,Color tint,float width){int n=Mathf.Max(4,Mathf.CeilToInt(Mathf.Abs(to-from)/12));for(int k=0;k<n;k++){float a=Mathf.Deg2Rad*Mathf.Lerp(from,to,k/(float)n),b=Mathf.Deg2Rad*Mathf.Lerp(from,to,(k+1)/(float)n);Line(vh,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r,c+new Vector2(Mathf.Cos(b),Mathf.Sin(b))*r,tint,width);}}
 static Color KindColor(SpotKind k)=>k switch{SpotKind.Park=>new Color(.93f,.45f,.25f),SpotKind.SpeedTrap=>new Color(.35f,.75f,1),SpotKind.Jump=>new Color(1,.62f,.15f),SpotKind.Smash=>new Color(1,.3f,.25f),SpotKind.Trick=>new Color(.75f,.45f,1),_=>new Color(.4f,.9f,.4f)};
 void Icon(VertexHelper vh,Vector2 c,SpotKind kind){float r=24,s=12;var w=Color.white;Disc(vh,c,r+3,Dark);Disc(vh,c,r,KindColor(kind));Disc(vh,c,r-4,new Color(.05f,.08f,.1f,.95f));
  switch(kind){
   case SpotKind.Park: Arc(vh,c+Vector2.up*3,s,180,360,w,3.5f);Line(vh,c+new Vector2(-s,3),c+new Vector2(-s,s),w,3.5f);Line(vh,c+new Vector2(s,3),c+new Vector2(s,s),w,3.5f);Tri(vh,c+new Vector2(-4,s*.2f),c+new Vector2(4,s*.2f),c+new Vector2(0,s*.9f),w);break; // a half-pipe and a rider's arc
   case SpotKind.SpeedTrap: for(int k=-1;k<=1;k+=2){var o=c+Vector2.right*k*5;Line(vh,o+new Vector2(-5,s*.75f),o+new Vector2(4,0),w,3.5f);Line(vh,o+new Vector2(-5,-s*.75f),o+new Vector2(4,0),w,3.5f);}break; // >>
   case SpotKind.Jump: Tri(vh,c+new Vector2(-s,-s*.6f),c+new Vector2(s*.2f,-s*.6f),c+new Vector2(s*.2f,s*.1f),w);Arc(vh,c+new Vector2(s*.6f,-s*.1f),s*.55f,20,160,w,2.5f);break; // a ramp and its flight
   case SpotKind.Smash: for(int k=0;k<4;k++){float a=k*45*Mathf.Deg2Rad;var d=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*s;Line(vh,c-d,c+d,w,k%2==0?3.5f:2.5f);}break; // a burst
   case SpotKind.Trick: Arc(vh,c,s*.8f,60,350,w,3.5f);{var tip=c+new Vector2(Mathf.Cos(60*Mathf.Deg2Rad),Mathf.Sin(60*Mathf.Deg2Rad))*s*.8f;Tri(vh,tip+new Vector2(-6,2),tip+new Vector2(4,6),tip+new Vector2(2,-5),w);}break; // a spin
   default: Line(vh,c+new Vector2(-s*.75f,0),c+new Vector2(-s*.2f,-s*.6f),w,4);Line(vh,c+new Vector2(-s*.2f,-s*.6f),c+new Vector2(s*.8f,s*.6f),w,4);break; // a tick
  }}
 void DrawSpot(VertexHelper vh){
  if(spot.road!=null)for(int i=0;i+1<spot.road.Length;i++){var a=Point(spot.road[i]);var b=Point(spot.road[i+1]);Line(vh,a,b,new Color(0,.06f,.07f,.35f),6);Line(vh,a,b,new Color(1,1,1,.35f),2.5f);}
  if(spot.area!=null&&spot.area.Length>2){var col=KindColor(spot.kind);var p0=Point(spot.area[0]);for(int i=1;i+1<spot.area.Length;i++)Tri(vh,p0,Point(spot.area[i]),Point(spot.area[i+1]),new Color(col.r,col.g,col.b,.3f));for(int i=0;i<spot.area.Length;i++){var a=Point(spot.area[i]);var b=Point(spot.area[(i+1)%spot.area.Length]);Line(vh,a,b,Dark,7);Line(vh,a,b,col,3.5f);}}
  var at=Point(spot.at);
  if(spot.runUp!=null&&spot.runUp.Length>1){
   // the arrow stops short of the icon; its start is a white square, as a course's start
   var pts=new System.Collections.Generic.List<Vector2>();foreach(var p in spot.runUp){var q=Point(p);if(pts.Count==0||(q-pts[pts.Count-1]).sqrMagnitude>4)pts.Add(q);}
   while(pts.Count>2&&(pts[pts.Count-1]-at).magnitude<34)pts.RemoveAt(pts.Count-1);
   if(pts.Count>1){var tint=new Color(1,.85f,.3f);for(int i=0;i+1<pts.Count;i++)Line(vh,pts[i],pts[i+1],Dark,10);for(int i=0;i+1<pts.Count;i++)Line(vh,pts[i],pts[i+1],tint,5);
    var end=pts[pts.Count-1];var f=(end-pts[pts.Count-2]).normalized;var side=new Vector2(-f.y,f.x);Tri(vh,end+f*13,end-f*5+side*11,end-f*5-side*11,Dark);Tri(vh,end+f*10,end-f*3+side*8,end-f*3-side*8,tint);
    var s0=pts[0];Line(vh,s0+Vector2.left*9,s0+Vector2.right*9,Dark,18);Line(vh,s0+Vector2.left*6,s0+Vector2.right*6,Color.white,12);}}
  Icon(vh,at,spot.kind);}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(spot!=null){DrawSpot(vh);return;}if(!race&&preview==null)return;if(LeviathanMap&&!LeviathanGuide.Disabled)Cover(vh);if(previews!=null){foreach(var c in previews)Course(vh,c);return;}Route(vh,preview!=null?preview.main:race.road.points,Main,true);if(preview!=null){foreach(var b in preview.branches)Route(vh,b.points,Branch,false,b.undergroundStart,b.undergroundEnd);}else foreach(var b in race.Branches)Route(vh,b.points,new Color(1,.72f,.2f),false,b.undergroundStart,b.undergroundEnd);int count=preview!=null?preview.gates.Length:race.gates.Length;for(int i=0;i<count;i++){var p=Point(preview!=null?preview.gates[i]:race.gates[i].transform.position);var c=i==0?Color.white:new Color(.3f,.6f,1);Line(vh,p+Vector2.left*4,p+Vector2.right*4,c,8);}
 // 0.83 (track select): the start as a larger white square with a dark edge
 if(preview!=null&&count>0){var s0=Point(preview.gates[0]);Line(vh,s0+Vector2.left*9,s0+Vector2.right*9,new Color(0,.06f,.07f,.9f),18);Line(vh,s0+Vector2.left*6,s0+Vector2.right*6,Color.white,12);}}
}
}
