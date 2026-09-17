var routes=new System.Collections.Generic.List<Racer.WoodlandBenchmark.Route>();
void Path(string name,float speed,params Vector3[] knots){var points=new System.Collections.Generic.List<Vector3>();for(int j=0;j<knots.Length-1;j++){int count=Mathf.CeilToInt(Vector3.Distance(knots[j],knots[j+1])/1.5f);for(int i=0;i<count;i++){var p=Vector3.Lerp(knots[j],knots[j+1],i/(float)count);p.y=Racer.Editor.Phase6Buildings.Ground(p);points.Add(p);}}var last=knots.Last();last.y=Racer.Editor.Phase6Buildings.Ground(last);points.Add(last);routes.Add(new Racer.WoodlandBenchmark.Route{name=name,speed=speed,points=points.ToArray(),lookAhead=7});}
Path("expanded-yard",5,new Vector3(370,0,75),new Vector3(422,0,-9));
Path("house2-access-reentry",4,new Vector3(435,0,-47),new Vector3(456,0,-44),new Vector3(475,0,-39),new Vector3(491,0,-35));
Path("valley-access",4,new Vector3(440,0,-226),new Vector3(440,0,-196),new Vector3(439,0,-178),new Vector3(436,0,-159));
Path("valley-return",4,new Vector3(436,0,-159),new Vector3(439,0,-178),new Vector3(440,0,-196),new Vector3(440,0,-226));
System.IO.File.WriteAllText("Docs/CR015/access-routes.json",JsonUtility.ToJson(new Racer.WoodlandBenchmark.Plan{seconds=30,routes=routes.ToArray()},true));
var b=Racer.WoodlandBenchmark.Begin("Docs/CR015/access-routes.json","Docs/CR015/access-driving.csv");b.repeats=1;return "Four ordinary-frame access drives started";
