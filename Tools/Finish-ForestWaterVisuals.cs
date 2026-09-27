var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();
if(Application.isPlaying||race.gameObject.scene.name!="ForestLoopReverse")throw new Exception("Forest edit mode required");
bool WaterFootprint(Vector3 p)=>Math.Abs(p.x-416)<10.5f&&Math.Abs(p.z+197)<9.5f||Math.Pow((p.x-384)/20.5f,2)+Math.Pow((p.z+194.1f)/13,2)<1;
var author=typeof(Racer.Editor.DiscoveryAuthoring);var flags=System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic;author.GetField("owner",flags).SetValue(null,race);author.GetMethod("ClearCompleteTrees",flags).Invoke(null,new object[]{(Func<Vector3,bool>)WaterFootprint});
var root=GameObject.Find("Route atlas direction guidance");int lifted=0,hidden=0;
foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true)){
 var centre=mf.GetComponent<Renderer>().bounds.center;
 if(centre.x>357&&centre.x<431&&Math.Abs(centre.z+197)<16){mf.gameObject.SetActive(false);hidden++;continue;}
 if(centre.x<432||centre.x>478||Math.Abs(centre.z+200)>12)continue;
 var mesh=UnityEngine.Object.Instantiate(mf.sharedMesh);var v=mesh.vertices;
 for(int i=0;i<v.Length;i++){var p=mf.transform.TransformPoint(v[i]);var h=Physics.RaycastAll(new Vector3(p.x,150,p.z),Vector3.down,200,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).First();p.y=h.point.y+.075f;v[i]=mf.transform.InverseTransformPoint(p);}
 mesh.vertices=v;mesh.RecalculateBounds();mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,"Assets/Track/ForestWaterJump/ramp-arrow-"+lifted+".asset");mf.sharedMesh=mesh;lifted++;
}
var sign=UnityEngine.Object.FindObjectsByType<TextMesh>().Single(t=>t.text=="GAP / STRAIGHT LANDING");var owner=sign.GetComponentInParent<Racer.PhysicalSign>().transform;var axis=new Vector3(-1,0,.08f).normalized;var side=Vector3.Cross(Vector3.up,axis);var pos=new Vector3(474,0,-201.3f)+axis*12+side*8;
var ground=Physics.RaycastAll(new Vector3(pos.x,150,pos.z),Vector3.down,200,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).First();pos.y=ground.point.y+2.5f;owner.SetPositionAndRotation(pos,Quaternion.LookRotation(axis));sign.text="POOL + LAKE\nSTRAIGHT JUMP";EditorUtility.SetDirty(sign);
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(race.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(race.gameObject.scene);AssetDatabase.SaveAssets();
System.IO.File.WriteAllText("Docs/ForestWaterJump/local-visuals.txt",$"Trees cleared only within water footprints using existing whole-tree helper. Ramp arrows resampled={lifted}; obsolete ground-flight arrows hidden={hidden}; existing gap sign updated/moved beside approach. No guidance colliders added.");return "Water footprint trees and ramp guidance corrected";
