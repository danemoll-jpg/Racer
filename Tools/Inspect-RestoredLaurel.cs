UnityEditor.EditorApplication.delayCall += () => { try {
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/StreetLoopReverse.unity");Physics.SyncTransforms();
var b=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().Single(b=>b.title=="Laurel Switchbacks");var rows=new List<string>();
for(float s=300;s<b.Length;s+=5){var p=b.At(s,out var f);var hits=Physics.RaycastAll(p+Vector3.up*10,Vector3.down,100,1,QueryTriggerInteraction.Ignore).Where(h=>!h.rigidbody).OrderBy(h=>h.distance).Take(3);rows.Add($"{s} {p} f={f} => "+string.Join(";",hits.Select(h=>$"{h.collider.name} {h.point.y:F3}")));}
System.IO.File.WriteAllLines("Docs/SurgicalTracks/restored-approach.txt",rows);
}catch(Exception e){System.IO.File.WriteAllText("Docs/SurgicalTracks/inspect-restored-error.txt",e.ToString());}};return "Scheduled restored approach sampling";
