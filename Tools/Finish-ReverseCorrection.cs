using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Racer;
public static class FinishReverseCorrection {
 public static string Main(){if(Application.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new Exception("Saved edit mode required");
 foreach(string name in new[]{"DansBackyardReverse","DansBackyardForward"}){EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");Physics.SyncTransforms();var root=GameObject.Find("Reverse optional forest shortcuts").transform;
 foreach(var b in root.GetComponentsInChildren<WoodlandRoute>()){var guide=b.GetComponent<ReverseShortcutGuidance>()??b.gameObject.AddComponent<ReverseShortcutGuidance>();guide.lookAhead=b.title.StartsWith("Storm")?6:7;if(b.title.StartsWith("Storm")){guide.takeoff=b.Project(new(94,0,-66.1f),out _);guide.landing=b.Project(new(61,0,-66.1f),out _);}}
 foreach(var c in root.GetComponentsInChildren<BoxCollider>().Where(c=>c.name=="Abandoned fallen timber"||c.name=="Weathered stump")){var p=c.bounds.center;float y=Physics.RaycastAll(new(p.x,250,p.z),Vector3.down,500,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).Max(h=>h.point.y);c.transform.position+=Vector3.up*(y-c.bounds.min.y);}
 EditorSceneManager.MarkSceneDirty(root.gameObject.scene);EditorSceneManager.SaveScene(root.gameObject.scene);}
 AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardReverse.unity");return "Local driving guidance attached; ridge timber and stumps reseated on final support";
 }
}
