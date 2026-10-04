using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Object=UnityEngine.Object;
// 0.73 read-only: for each water volume, the height of its visible top face above the ground under it on a local grid
// (+ = water face above ground). Tilted volumes (Fern creek) are measured against their true face, not Surface.
public static class Report073Fern {
 public static void Run(){var outDir=Environment.GetEnvironmentVariable("PROBE_OUT");Directory.CreateDirectory(outDir);
  foreach(var scene in Environment.GetEnvironmentVariable("PROBE_SCENES").Split(',')){EditorSceneManager.OpenScene("Assets/Scenes/"+scene+".unity");var rows=new List<string>{"SCENE "+scene};Physics.SyncTransforms();
   foreach(var w in Object.FindObjectsByType<Racer.ShallowWater>(FindObjectsSortMode.None)){var t=w.transform;if(t.lossyScale.x<4&&t.lossyScale.z<4)continue;
    rows.Add($"WATER {t.name} pos={t.position} rot={t.eulerAngles} scale={t.lossyScale} surface={w.Surface:F2}");
    int nx=12,nz=24;for(int j=nz;j>=0;j--){var line=new List<string>();for(int i=0;i<=nx;i++){var lp=new Vector3(-.5f+i/(float)nx,.5f,-.5f+j/(float)nz);if(w.round&&lp.x*lp.x+lp.z*lp.z>.25f){line.Add("   .  ");continue;}
      var top=t.TransformPoint(lp);float? g=null;foreach(var h in Physics.RaycastAll(new Vector3(top.x,top.y+30,top.z),Vector3.down,60,1,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){if(h.collider.attachedRigidbody)continue;g=h.point.y;break;}
      line.Add(g.HasValue?$"{top.y-g.Value,6:F2}":"  none");}rows.Add(" "+string.Join("",line));}}
   File.WriteAllLines($"{outDir}/face-{scene}.txt",rows);}
  EditorApplication.Exit(0);}
}
