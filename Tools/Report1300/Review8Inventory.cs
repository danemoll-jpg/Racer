using System;using System.IO;using System.Linq;using System.Text;using System.Text.RegularExpressions;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;
// 1.3.0-review8 (temporary editor tool): every solid static collider in each scene whose name or parents' names look like a barrier, grouped by name.
public static class Review8Inventory { public static void Main(){
 var sb=new StringBuilder();
 try{
  var rx=new Regex("rail|guard|barri|wall|fence|closed|closure|parapet|bridge|bollard|block|boundary|curb|kerb|edge|cliff|barricade|jersey|post|log|stop",RegexOptions.IgnoreCase);
  foreach(var path in Directory.GetFiles("Assets/Scenes","*.unity")){
   var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);sb.AppendLine("=== "+scene.name);
   var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Collider>(true)).Where(c=>!c.isTrigger&&!c.attachedRigidbody).ToList();
   string Path(Transform t){var s=t.name;for(var p=t.parent;p&&s.Length<160;p=p.parent)s=p.name+"/"+s;return s;}
   var hits=all.Where(c=>rx.IsMatch(c.name)||(c.transform.parent&&rx.IsMatch(c.transform.parent.name))||(c.transform.parent&&c.transform.parent.parent&&rx.IsMatch(c.transform.parent.parent.name)));
   foreach(var g in hits.GroupBy(c=>c.GetType().Name+" | "+c.name+" | parent "+(c.transform.parent?Regex.Replace(c.transform.parent.name,@"\d+","#"):"-")).OrderBy(g=>g.Key)){
    var c=g.First();bool brk=c.GetComponentInParent<Racer.BreakableProp>(true);bool sign=c.GetComponentInParent<Racer.PhysicalSign>(true);
    sb.AppendLine($"{g.Count(),5} {g.Key}{(brk?" [BREAKABLE]":"")}{(sign?" [SIGN]":"")}{(g.Any(x=>!x.gameObject.activeInHierarchy)?" [some inactive]":"")} e.g. {Path(c.transform)} at {c.bounds.center:F0} size {c.bounds.size:F1} layer {c.gameObject.layer} mat {(c.sharedMaterial?c.sharedMaterial.name:"-")}");
   }
  }
 }catch(Exception e){sb.AppendLine(e.ToString());}
 var outDir=Environment.GetEnvironmentVariable("PROBE_OUT")??".";File.WriteAllText(System.IO.Path.Combine(outDir,"inventory.txt"),sb.ToString());EditorApplication.Exit(0);}
}
