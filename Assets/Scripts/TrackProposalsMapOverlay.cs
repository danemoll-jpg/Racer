using UnityEngine;
using UnityEngine.UI;
namespace Racer {
// 0.114 Part B: the combined-course proposals on the big map (ExplorationMap), over the race routes: A orange, B cyan, their shortcuts
// yellow, the numbered corners as small squares in the proposal's colour and the start as a white bar. Drawn while the markers are shown.
[RequireComponent(typeof(CanvasRenderer))]
public sealed class TrackProposalsMapOverlay : MaskableGraphic {
 WorldMapVisual visual;Vector2 center;float zoom;bool shown;
 public void SetView(WorldMapVisual map,Vector2 at,float scale){bool on=TrackProposals.Showing;if(visual==map&&center==at&&zoom==scale&&shown==on)return;visual=map;center=at;zoom=scale;shown=on;SetVerticesDirty();}
 Vector2 Point(Vector3 p){var uv=visual?visual.Normalized(p):ExplorationMap.Normalized(p);return Vector2.Scale((uv-center)*zoom,rectTransform.rect.size);}
 static void Line(VertexHelper vh,Vector2 a,Vector2 b,Color tint,float width){if((b-a).sqrMagnitude<.0001f)return;var side=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;int i=vh.currentVertCount;vh.AddVert(a-side,tint,Vector2.zero);vh.AddVert(a+side,tint,Vector2.zero);vh.AddVert(b+side,tint,Vector2.zero);vh.AddVert(b-side,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
 void Path(VertexHelper vh,Vector3[] pts,Color tint,float width,float side){var edge=rectTransform.rect.size*.5f+Vector2.one*24;var a=Point(pts[0]);
  for(int i=1;i<pts.Length;i++){var b=Point(pts[i]);if((b-a).sqrMagnitude<4&&i<pts.Length-1)continue;
   if(!(Mathf.Abs(a.x)>edge.x&&Mathf.Abs(b.x)>edge.x&&Mathf.Sign(a.x)==Mathf.Sign(b.x)||Mathf.Abs(a.y)>edge.y&&Mathf.Abs(b.y)>edge.y&&Mathf.Sign(a.y)==Mathf.Sign(b.y))){var o=TrackProposals.Beside(b-a)*side*2.5f;Line(vh,a+o,b+o,new Color(0,0,0,.6f),width+2);Line(vh,a+o,b+o,tint,width);}a=b;}}
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();if(!shown||!TrackProposals.Current)return;
  foreach(var (pts,c,shortcut,side) in TrackProposals.Current.Lines)Path(vh,pts,c,shortcut?2:3,side);
  foreach(var p in TrackProposals.Plan.proposals){var pts=TrackProposals.Points(p.pts);var c=TrackProposals.ColorOf(p);
   foreach(var k in p.corners){var q=Point(pts[k.i]);Line(vh,q+Vector2.left*4,q+Vector2.right*4,new Color(0,0,0,.8f),9);Line(vh,q+Vector2.left*3,q+Vector2.right*3,c,6);}
   var s=Point(pts[0]);Line(vh,s+Vector2.left*8,s+Vector2.right*8,new Color(0,.06f,.07f,.9f),14);Line(vh,s+Vector2.left*6,s+Vector2.right*6,Color.white,9);}
 }
}
}
