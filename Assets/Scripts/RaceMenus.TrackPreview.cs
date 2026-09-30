using UnityEngine;

namespace Racer
{
    public sealed partial class RaceMenus
    {
        int previewTrack;
        void RenderCoursePreview()
        {
            ClearCore(RacePlaylists.Titles[previewTrack],"Preview · Cyan: main route · Gold: shortcuts · White arrows: direction");
            Row(0,"use-track","USE THIS TRACK",()=>flow.SelectCourseEntry(previewTrack));
            Row(1,"back","Back to tracks",()=>BackPage());
            var visual=Resources.Load<WorldMapVisual>("WorldMaps/PermanentWorld");
            if(!visual||previewTrack>=CoursePreviewCatalog.Courses.Length)return;
            var course=CoursePreviewCatalog.Courses[previewTrack];
            var holder=Rect("Track preview",content);laterLayouts.Add(holder.gameObject);holder.gameObject.AddComponent<UnityEngine.UI.LayoutElement>().preferredHeight=330;
            var map=Rect("Map",holder);map.anchorMin=map.anchorMax=map.pivot=new(.5f,.5f);map.sizeDelta=new(330*visual.bounds.width/visual.bounds.height,330);
            var picture=map.gameObject.AddComponent<UnityEngine.UI.RawImage>();picture.texture=visual.image;picture.raycastTarget=false;map.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var bounds=new Bounds(course.main[0],Vector3.zero);foreach(var p in course.main)bounds.Encapsulate(p);foreach(var branch in course.branches)foreach(var p in branch.points)bounds.Encapsulate(p);
            var center=visual.Normalized(bounds.center);var extent=visual.Normalized(bounds.max)-visual.Normalized(bounds.min);float zoom=Mathf.Clamp(.85f/Mathf.Max(extent.x,extent.y),1,6);
            picture.uvRect=new Rect(center-Vector2.one*.5f/zoom,Vector2.one/zoom);
            var overlay=Rect("Route",map);Stretch(overlay,0,0,0,0);var graphic=overlay.gameObject.AddComponent<WorldMapCourseOverlay>();graphic.raycastTarget=false;graphic.SetView(flow.Race,visual,center,zoom,course);
        }
    }
}
