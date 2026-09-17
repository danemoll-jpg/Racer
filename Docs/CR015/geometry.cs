var road=Racer.Editor.StreetLoopBuilder.Route();
var root=GameObject.Find(Racer.Editor.Phase6Review.Root).transform;
var ground=GameObject.Find("Memory loop - north is +Z").transform;
var rows=new System.Collections.Generic.List<string>();int failures=0;
void Check(bool ok,string msg){rows.Add((ok?"PASS ":"FAIL ")+msg);if(!ok)failures++;}
var before=JsonUtility.FromJson<Racer.Editor.CR015Neighborhood.Snapshot>(System.IO.File.ReadAllText("Docs/CR015/before-sites.json"));
var after=JsonUtility.FromJson<Racer.Editor.CR015Neighborhood.Snapshot>(System.IO.File.ReadAllText("Docs/CR015/after-sites.json"));
Check(!root.Find("Original house 1"),"House #1 remains absent");
Check(before.sites[2].position==after.sites[2].position&&before.sites[2].rotation==after.sites[2].rotation,"Friend unchanged");
var h2=after.sites[1];var fr=after.sites[2];var h3=after.sites[3];
var axis=Vector3.ProjectOnPlane(fr.position-fr.road,Vector3.up).normalized;var line=Vector3.ProjectOnPlane(h2.position-fr.road,Vector3.up);float alignment=Vector3.Cross(line,axis).magnitude;
Check(alignment<.05f&&Vector3.Dot(line,axis)<0,$"House #2 opposite friend, cross-street line error {alignment:F4}m; road projection separation {Vector3.Distance(h2.road,fr.road):F3}m");
Check(Vector3.Distance(h2.position,after.sites[0].position)<Vector3.Distance(before.sites[1].position,before.sites[0].position),"House #2 closer to Dan");
Check(h3.road.z>-132&&h3.position.y<40&&h3.roadY-h3.position.y>35,$"House #3 road projection Z {h3.road.z:F3} before big descent knot Z -132; ground {h3.position.y:F3}, road {h3.roadY:F3}, below road {h3.roadY-h3.position.y:F3}m");
Check(Vector3.Distance(h3.position,h2.position)<Vector3.Distance(before.sites[3].position,before.sites[1].position),"House #3 closer to House #2");
int floating=0,buried=0;foreach(Transform t in root){var foundation=t.Find("Foundation");float floor=t.position.y+t.Find("Phase 6 architecture").localPosition.y;foreach(float x in new[]{-.5f,.5f})foreach(float z in new[]{-.5f,.5f}){var p=foundation.TransformPoint(new Vector3(x,-.5f,z));float y=Racer.Editor.Phase6Buildings.Ground(p);if(p.y-y>.05f)floating++;if(y>floor+.05f)buried++;}}
Check(floating==0&&buried==0,$"All 47 foundations: floating corners={floating}, terrain above floor={buried}");
int seam=0,normalSeam=0;var coords=new System.Collections.Generic.Dictionary<Vector2,Vector3>();
foreach(var mf in ground.GetComponentsInChildren<MeshFilter>()){var v=mf.sharedMesh.vertices;var ns=mf.sharedMesh.normals;for(int k=0;k<v.Length;k++){var key=new Vector2(v[k].x,v[k].z);var data=new Vector3(v[k].y,ns[k].x,ns[k].z);if(coords.TryGetValue(key,out var old)){if(Mathf.Abs(old.x-data.x)>.00001f)seam++;if(Vector2.Distance(new Vector2(old.y,old.z),new Vector2(data.y,data.z))>.00001f)normalSeam++;}coords[key]=data;}}
Check(seam==0&&normalSeam==0,$"Terrain seams: height mismatches {seam}, normal mismatches {normalSeam}");
Check(ground.GetComponentsInChildren<MeshFilter>().All(m=>m.GetComponent<MeshCollider>().sharedMesh==m.sharedMesh),"Terrain rendering/collision meshes identical");
int yard=0,block=0;for(float z=-25;z<133;z+=2)for(float x=323;x<447;x+=2){var p=new Vector3(x,0,z);if(Racer.Editor.Phase6Buildings.YardDistance(p)>22||(p-Racer.Editor.Phase6Buildings.Dan).magnitude<18)continue;yard++;if(!Physics.Raycast(new Vector3(x,300,z),Vector3.down,out var hit,600,1)||!hit.collider.transform.IsChildOf(ground))block++;}
Check(block==0,$"Expanded yard {yard} support probes; missing/blocked {block}");
var props=GameObject.Find(Racer.Editor.CR015Neighborhood.Props);Check(props.transform.childCount==6&&props.GetComponentsInChildren<Collider>().Length==0,"4 mailboxes + 2 signs, no small snag/launch colliders");
float setback=999;foreach(Transform p in props.transform)setback=Mathf.Min(setback,Racer.Editor.StreetLoopBuilder.Nearest(p.position,road,out _));Check(setback>11,$"Prop minimum centerline distance {setback:F3}m; ordinary 9m shoulder envelope clear");
var woods=GameObject.Find("Woods replacing later subdivisions");Check(woods.GetComponentsInChildren<BoxCollider>().Length>11850,"Broad expanded woodland retained");
int unsupported=0;foreach(var c in woods.GetComponentsInChildren<BoxCollider>().Where(c=>Vector2.Distance(new Vector2(c.bounds.center.x,c.bounds.center.z),new Vector2(418,-166))<55)){if(Mathf.Abs(c.bounds.min.y-Racer.Editor.Phase6Buildings.Ground(c.bounds.center))>.08f)unsupported++;}Check(unsupported==0,$"Local trees regrounded: unsupported={unsupported}");
rows.Add("RESULT failures="+failures+". Geometric sampling only; dynamic driving separately.");System.IO.File.WriteAllLines("Docs/CR015/geometry.txt",rows);return rows;
