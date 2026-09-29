using UnityEngine;
namespace Racer {
// Generated from the saved scene by Tools/Capture-WorldMaps.cs. No gameplay state.
public sealed class WorldMapVisual : ScriptableObject {
 public Rect bounds;
 public Texture2D image;
 public string sourceScene;
 public Vector2 Normalized(Vector3 p)=>new((p.x-bounds.xMin)/bounds.width,(p.z-bounds.yMin)/bounds.height);
 public Vector3 WorldPoint(Vector2 uv)=>new(bounds.xMin+uv.x*bounds.width,0,bounds.yMin+uv.y*bounds.height);
}
}
