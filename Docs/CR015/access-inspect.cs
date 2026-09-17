var rows=new System.Collections.Generic.List<string>();var h=GameObject.Find("Original house 3").transform;
foreach(var t in new[]{GameObject.Find("Original house 2").transform,h})foreach(var c in GameObject.Find("Woods replacing later subdivisions").GetComponentsInChildren<BoxCollider>()){var p=c.bounds.center;var d=p-t.position;d.y=0;if(d.magnitude<18)rows.Add(t.name+" tree "+c.name+" "+c.transform.position+" distance "+d.magnitude);}
for(float z=-220;z<=-140;z+=5)rows.Add("Valley x440 z="+z+" y="+Racer.Editor.Phase6Buildings.Ground(new Vector3(440,0,z)));
return rows;
