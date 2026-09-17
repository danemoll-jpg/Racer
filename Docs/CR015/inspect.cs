

var road=Racer.Editor.StreetLoopBuilder.Route();
var lines=new System.Collections.Generic.List<string>();
foreach(string n in new[]{"Dan - blue X","Original house 2","Original house 3","Friend across street - blue circle"}) {var t=GameObject.Find(n).transform;float d=Racer.Editor.StreetLoopBuilder.Nearest(t.position,road,out var r);lines.Add(n+" "+t.position.ToString("F4")+" road="+r.ToString("F3")+" center setback="+d);foreach(Transform c in t)lines.Add("  "+c.name+" local="+c.localPosition+" scale="+c.localScale);}
foreach(float z in new[]{-40f,-60,-80,-100,-120,-140,-160,-180,-200,-220}) {var p=new Vector3(429,0,z);Racer.Editor.StreetLoopBuilder.Nearest(p,road,out var r);lines.Add("H3 candidate "+p+" ground="+Racer.Editor.Phase6Buildings.Ground(p)+" road="+r);}
System.IO.File.WriteAllLines("Docs/CR015/inspection.txt",lines);
return lines;

