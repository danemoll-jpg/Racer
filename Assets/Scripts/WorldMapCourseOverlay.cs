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
 public void SetView(RaceDirector owner,WorldMapVisual map,Vector2 at,float scale,CoursePreviewCatalog.Course course=null){if(race==owner&&visual==map&&center==at&&zoom==scale&&preview==course&&previews==null)return;race=owner;visual=map;center=at;zoom=scale;preview=course;previews=null;SetVerticesDirty();}
 // 0.76: several courses at once (null = the active course); a race-only route (no road in FreeRoamWorld) is dashed.
 public void SetView(RaceDirector owner,WorldMapVisual map,Vector2 at,float scale,System.Collections.Generic.List<CoursePreviewCatalog.Course> courses){race=owner;visual=map;center=at;zoom=scale;preview=null;previews=courses;SetVerticesDirty();}
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
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(!race&&preview==null)return;if(LeviathanMap&&!LeviathanGuide.Disabled)Cover(vh);if(previews!=null){foreach(var c in previews)Course(vh,c);return;}Route(vh,preview!=null?preview.main:race.road.points,Main,true);if(preview!=null){foreach(var b in preview.branches)Route(vh,b.points,Branch,false,b.undergroundStart,b.undergroundEnd);}else foreach(var b in race.Branches)Route(vh,b.points,new Color(1,.72f,.2f),false,b.undergroundStart,b.undergroundEnd);int count=preview!=null?preview.gates.Length:race.gates.Length;for(int i=0;i<count;i++){var p=Point(preview!=null?preview.gates[i]:race.gates[i].transform.position);var c=i==0?Color.white:new Color(.3f,.6f,1);Line(vh,p+Vector2.left*4,p+Vector2.right*4,c,8);}
 // 0.83 (track select): the start as a larger white square with a dark edge
 if(preview!=null&&count>0){var s0=Point(preview.gates[0]);Line(vh,s0+Vector2.left*9,s0+Vector2.right*9,new Color(0,.06f,.07f,.9f),18);Line(vh,s0+Vector2.left*6,s0+Vector2.right*6,Color.white,12);}}
}
}
