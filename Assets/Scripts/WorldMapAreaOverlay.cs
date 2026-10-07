using UnityEngine;
using UnityEngine.UI;
namespace Racer {
// 0.93 Part C: the acorn areas on the world map, under the race routes and every marker: each region softly tinted with an
// outline in its area's colour (roads and trails show through); the chosen area brighter with a white outline; an area with
// every acorn found dim and grey.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class WorldMapAreaOverlay : MaskableGraphic {
 AcornAreas.Area[] areas;ExplorationCollection collection;WorldMapVisual visual;Vector2 center;float zoom;int selected=-1;
 public void SetView(AcornAreas.Area[] all,ExplorationCollection owner,WorldMapVisual map,Vector2 at,float scale,int chosen){areas=all;collection=owner;visual=map;center=at;zoom=scale;selected=chosen;SetVerticesDirty();}
 Vector2 Point(Vector3 p){var uv=visual?visual.Normalized(p):ExplorationMap.Normalized(p);return Vector2.Scale((uv-center)*zoom,rectTransform.rect.size);}
 static void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint,float width){var side=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int i=vh.currentVertCount;vh.AddVert(a-side,tint,Vector2.zero);vh.AddVert(a+side,tint,Vector2.zero);vh.AddVert(b+side,tint,Vector2.zero);vh.AddVert(b-side,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();if(areas==null||!collection)return;
  for(int n=0;n<areas.Length;n++){var area=areas[n];bool done=area.Done(collection),chosen=n==selected;
   var tint=done?new Color(.6f,.62f,.6f):area.tint;
   var fill=new Color(tint.r,tint.g,tint.b,chosen?.30f:done?.06f:.16f);var edge=chosen?Color.white:new Color(tint.r,tint.g,tint.b,done?.35f:.85f);
   foreach(var r in area.regions){var pts=System.Array.ConvertAll(r.outline,Point);var mid=Vector2.zero;foreach(var p in pts)mid+=p;mid/=pts.Length;
    int c=vh.currentVertCount;vh.AddVert(mid,fill,Vector2.zero);foreach(var p in pts)vh.AddVert(p,fill,Vector2.zero);
    for(int i=0;i<pts.Length;i++)vh.AddTriangle(c,c+1+i,c+1+(i+1)%pts.Length);
    for(int i=0;i<pts.Length;i++)Line(vh,pts[i],pts[(i+1)%pts.Length],edge,chosen?3.5f:2);}}}
}
}
