using UnityEngine;
using UnityEngine.UI;
namespace Racer {
// Full menu map only. Navigation is drawn from the active course's actual data.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WorldMapCourseOverlay : MaskableGraphic {
 RaceDirector race;WorldMapVisual visual;Vector2 center;float zoom;CoursePreviewCatalog.Course preview;
 public void SetView(RaceDirector owner,WorldMapVisual map,Vector2 at,float scale,CoursePreviewCatalog.Course course=null){if(race==owner&&visual==map&&center==at&&zoom==scale&&preview==course)return;race=owner;visual=map;center=at;zoom=scale;preview=course;SetVerticesDirty();}
 Vector2 Point(Vector3 p){var uv=visual?visual.Normalized(p):ExplorationMap.Normalized(p);return Vector2.Scale((uv-center)*zoom,rectTransform.rect.size);}
 void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint,float width=2){var side=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int i=vh.currentVertCount;vh.AddVert(a-side,tint,Vector2.zero);vh.AddVert(a+side,tint,Vector2.zero);vh.AddVert(b+side,tint,Vector2.zero);vh.AddVert(b-side,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
 void Route(VertexHelper vh,Vector3[] pts,Color tint,bool closed){float distance=0;for(int i=0;i<pts.Length-(closed?0:1);i++){var a=Point(pts[i]);var b=Point(pts[(i+1)%pts.Length]);Line(vh,a,b,new Color(0,.06f,.07f,.9f),5);Line(vh,a,b,tint);distance+=(b-a).magnitude;if(distance>85){distance=0;var f=(b-a).normalized;var side=new Vector2(-f.y,f.x);Line(vh,b-f*8+side*4,b,Color.white);Line(vh,b-f*8-side*4,b,Color.white);}}}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();if(!race)return;Route(vh,preview!=null?preview.main:race.road.points,new Color(.3f,1,.88f),true);if(preview!=null){foreach(var b in preview.branches)Route(vh,b.points,new Color(1,.72f,.2f),false);}else foreach(var b in race.Branches)Route(vh,b.points,new Color(1,.72f,.2f),false);int count=preview!=null?preview.gates.Length:race.gates.Length;for(int i=0;i<count;i++){var p=Point(preview!=null?preview.gates[i]:race.gates[i].transform.position);var c=i==0?Color.white:new Color(.3f,.6f,1);Line(vh,p+Vector2.left*4,p+Vector2.right*4,c,8);}}
}
}
