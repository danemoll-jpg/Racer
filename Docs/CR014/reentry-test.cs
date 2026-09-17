var method=typeof(Racer.Editor.CR014Woodland).GetMethod("Path",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
var points=(UnityEngine.Vector3[])method.Invoke(null,new object[]{new UnityEngine.Vector3(-525,0,-320),new UnityEngine.Vector3(-636,0,-320)});
var woods=UnityEngine.GameObject.Find("Woods replacing later subdivisions").transform;
for(int pass=0;pass<30;pass++)for(int i=1;i<points.Length-1;i++){
 var q=UnityEngine.Vector3.Lerp(points[i],(points[i-1]+points[i+1])*.5f,.4f);
 if(UnityEngine.Physics.OverlapSphere(q+UnityEngine.Vector3.up*3,2.5f,1).Any(c=>c.transform.IsChildOf(woods)))continue;
 q.y=Racer.Editor.Phase6Buildings.Ground(q);points[i]=q;
}
var plan=new Racer.WoodlandBenchmark.Plan{seconds=40,routes=new[]{new Racer.WoodlandBenchmark.Route{name="west-forest-road-reentry",points=points,speed=3.5f,lookAhead=5}}};
System.IO.File.WriteAllText("Docs/CR014/reentry-routes.json",UnityEngine.JsonUtility.ToJson(plan,true));
var b=Racer.WoodlandBenchmark.Begin("Docs/CR014/reentry-routes.json","Docs/CR014/reentry.csv");b.repeats=1;
return "ordinary-frame forest-to-road test started; no world modifications";
