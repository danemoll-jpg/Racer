// Restore only route metadata/guidance for Dan's confirmed existing southern detour.
if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Edit mode required");
if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="ForestLoopReverse")throw new Exception("Open saved ForestLoopReverse first");
var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var road=race.road;road.Initialize();
if(GameObject.Find("House 3 Detour"))throw new Exception("Already restored");
var baseline=JsonUtility.FromJson<InspectRouteAtlas.Atlas>(System.IO.File.ReadAllText("Docs/RouteAtlas/routes-before.json")).courses.Single(c=>c.scene=="ForestLoopReverse");
var old=baseline.routes.Single(r=>r.kind=="main").points;var stations=new float[old.Length];for(int i=1;i<old.Length;i++)stations[i]=stations[i-1]+Vector3.Distance(old[i-1],old[i]);
var former=baseline.routes.Single(r=>r.name=="Granite Saddle");float from=former.entry,to=former.exit;
Vector3 At(float s){int i=1;while(i<stations.Length-1&&stations[i]<s)i++;return Vector3.Lerp(old[i-1],old[i],Mathf.InverseLerp(stations[i-1],stations[i],s));}
var line=new[]{At(from)}.Concat(old.Where((p,i)=>stations[i]>from&&stations[i]<to)).Concat(new[]{At(to)}).ToArray();
// These are the historical centreline samples of the still-present road, not restored geometry.
var b=new GameObject("House 3 Detour").AddComponent<Racer.WoodlandRoute>();b.title="House 3 Detour";b.points=line;b.halfWidth=3.6f;b.recommendedSpeed=32;b.entrySpeed=28;b.entrySpeedDistance=65;b.entryMargin=1.5f;b.aiValidated=true;
b.entryRoad=road.Project(line[0],out _);b.exitRoad=road.Project(line[^1],out _);b.entryInset=24;
for(float s=24;s<100;s+=2){var p=b.At(s,out _);float mainS=road.Project(p,out float distance);if(distance>road.HalfWidth(mainS)+b.halfWidth+4){b.entryInset=s;break;}}
b.bypassedGates=Enumerable.Range(1,race.gates.Length-1).Where(i=>road.Relative(road.Project(race.gates[i].transform.position,out _),b.entryRoad)<road.Relative(b.exitRoad,b.entryRoad)).ToArray();
race.courseId="forest-reverse-v7-water-detour";EditorUtility.SetDirty(race);
var root=new GameObject("House 3 Detour optional visual guidance").transform;
var gold=AssetDatabase.LoadAssetAtPath<Material>("Assets/Track/Discovery/CR117 alternate gold.mat");
float Ground(Vector3 p){var hs=Physics.RaycastAll(new Vector3(p.x,180,p.z),Vector3.down,260,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>Math.Abs(h.point.y-p.y)).ToArray();if(hs.Length==0)throw new Exception("No existing detour ground at "+p);return hs[0].point.y;}
int arrows=0;foreach(float s in new[]{b.entryInset+5,110,190,270,350,b.Length-25}.Distinct()){
 var p=b.At(s,out var heading);var f=Vector3.ProjectOnPlane(heading,Vector3.up).normalized;var right=Vector3.Cross(Vector3.up,f);
 var shape=new[]{new Vector2(-.4f,-3),new Vector2(.4f,-3),new Vector2(.4f,0),new Vector2(1,0),new Vector2(0,3),new Vector2(-1,0),new Vector2(-.4f,0)};
 var v=shape.Select(q=>{var z=p+right*q.x+f*q.y;z.y=Ground(z)+.08f;return z;}).ToArray();var mesh=new Mesh{vertices=v,triangles=new[]{0,6,1,1,6,2,6,5,4,6,4,2,2,4,3}};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,$"Assets/Track/ForestWaterJump/detour-arrow-{arrows++}.asset");
 var go=new GameObject("Optional gold / House 3 Detour",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(root);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<Renderer>().sharedMaterial=gold;
}
var label=new GameObject("House 3 Detour OPTIONAL sign").AddComponent<TextMesh>();label.transform.SetParent(root);var entrance=b.At(b.entryInset,out var ef);var rightSide=Vector3.Cross(Vector3.up,ef).normalized;var at=entrance+rightSide*6;at.y=Ground(at)+2;label.transform.SetPositionAndRotation(at,Quaternion.LookRotation(Vector3.ProjectOnPlane(ef,Vector3.up)));label.text="HOUSE 3 DETOUR\nOPTIONAL";label.characterSize=.12f;label.fontSize=64;label.anchor=TextAnchor.MiddleCenter;label.alignment=TextAlignment.Center;label.color=new Color(1,.72f,.2f);
var samples=new List<string>{"station,x,y,z,supportY"};float maxError=0;for(float s=0;s<b.Length;s+=5){var p=b.At(s,out _);float y=Ground(p);maxError=Math.Max(maxError,Math.Abs(y-p.y));samples.Add($"{s:F2},{p.x:F3},{p.y:F3},{p.z:F3},{y:F3}");}
System.IO.File.WriteAllLines("Docs/ForestWaterJump/detour-support.csv",samples);
System.IO.File.WriteAllText("Docs/ForestWaterJump/shortcut-restoration.txt",$"Dan confirmed the dotted grey former-main southern detour on 2026-09-28.\nCourse ID={race.courseId}; optional WoodlandRoute title/GameObject ID=House 3 Detour.\nHistorical centreline stations {from}..{to}; {line.Length} points; branch length {b.Length:F3}; current main entrance={b.entryRoad:F3}, rejoin={b.exitRoad:F3}; entryInset={b.entryInset}; bypass gates={string.Join(",",b.bypassedGates)}.\nExisting physical geometry unchanged; no mesh/collider/terrain restoration. Only added component, {arrows} collider-free gold arrows and optional label.\nAI uses existing aiValidated route discovery; recovery uses existing WoodlandRoute context; no global code changed. Maximum old centreline/support discrepancy {maxError:F3}m.\n");
PlayerSettings.bundleVersion="0.31.0-review1";
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(race.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();return System.IO.File.ReadAllText("Docs/ForestWaterJump/shortcut-restoration.txt");
