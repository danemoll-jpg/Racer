var rows=new List<string>();var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var road=race.road;
var start=new Vector3(474,0,-201.3f);var axis=new Vector3(-1,0,.08f).normalized;var side=Vector3.Cross(Vector3.up,axis);
float Ground(Vector3 p){var hs=Physics.RaycastAll(new Vector3(p.x,180,p.z),Vector3.down,250,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).ToArray();if(hs.Length==0)throw new Exception("Missing support "+p);return hs[0].point.y;}
int samples=0;float maxAngle=0;float minGrade=100,maxGrade=-100;int stacks=0;
foreach(float lane in new[]{-3f,0,3f}){Vector3? prev=null;float? lastY=null;for(float s=-2.137f;s<38.8f;s+=.2f){var p=start+axis*s+side*lane;var hs=Physics.RaycastAll(new Vector3(p.x,180,p.z),Vector3.down,250,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderByDescending(h=>h.point.y).ToArray();if(hs.Length==0)throw new Exception("Ramp hole");if(prev.HasValue)maxAngle=Math.Max(maxAngle,Vector3.Angle(prev.Value,hs[0].normal));if(lastY.HasValue&&s>4){float g=(hs[0].point.y-lastY.Value)/.2f;minGrade=Math.Min(minGrade,g);maxGrade=Math.Max(maxGrade,g);}stacks+=hs.Count(h=>Math.Abs(h.point.y-hs[0].point.y)<.01f)>1?1:0;prev=hs[0].normal;lastY=hs[0].point.y;samples++;}}
rows.Add($"Ramp: {samples} support samples across 6m usable width; adjacent normal change {maxAngle:F3} degrees; grades {minGrade:F3}..{maxGrade:F3}; duplicate top hits {stacks}. Straight plan axis {axis}.");
var lip=start+axis*38.8f;lip.y=Ground(lip);var paths=new List<object>();
foreach(float speed in new[]{24f,32f,40f}){
 var velocity=(axis+Vector3.up*.39f).normalized*speed;var path=new List<float[]>();Vector3 contact=Vector3.zero;float distance=0,minWaterClear=float.PositiveInfinity;string obstruction="none";var previous=lip+Vector3.up*.8f;
 for(float t=0;t<8;t+=.025f){var p=lip+Vector3.up*.8f+velocity*t+Physics.gravity*(t*t*.5f);path.Add(new[]{p.x,p.y,p.z});
  if(p.x<426&&p.x>365){float water=p.x>405?34.7f:33.4f;minWaterClear=Math.Min(minWaterClear,p.y-water);}
  if(t>.15f&&Physics.Linecast(previous,p,out var hit,1,QueryTriggerInteraction.Ignore)&&!hit.collider.name.StartsWith("Ground_")&&!hit.rigidbody)obstruction=hit.collider.name;
  float y=Ground(p);if(t>.15f&&p.y<=y+.6f){contact=new Vector3(p.x,y,p.z);distance=Vector3.Dot(p-lip,axis);break;}previous=p;
 }
 road.Project(contact,out float lateral);rows.Add($"Geometric flight {speed}m/s: distance {distance:F2}m, landing {contact}, minimum pool/lake clearance {minWaterClear:F2}m, 3D main-route offset {lateral:F2}m, airborne solid obstruction={obstruction}.");
 paths.Add(new{speed,points=path,landing=new[]{contact.x,contact.y,contact.z},distance,lateral,minWaterClear,obstruction});
}
System.IO.File.WriteAllText("Docs/ForestWaterJump/flights.json",Newtonsoft.Json.JsonConvert.SerializeObject(paths,Newtonsoft.Json.Formatting.Indented));
rows.Add("Local main gates: "+string.Join("; ",race.gates.Where(g=>g.transform.position.x>200&&g.transform.position.x<650&&g.transform.position.z<0).Select(g=>$"{g.name}: {g.transform.position}")));
System.IO.File.WriteAllLines("Docs/ForestWaterJump/geometry-checks.txt",rows);
void Shot(string name,Vector3 position,Vector3 target){var go=new GameObject("Temporary water jump camera",typeof(Camera));var camera=go.GetComponent<Camera>();camera.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));camera.fieldOfView=65;camera.farClipPlane=1200;var rt=new RenderTexture(1440,900,24);var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);var old=RenderTexture.active;try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();System.IO.File.WriteAllBytes("Docs/ForestWaterJump/"+name+".png",tex.EncodeToPNG());}finally{RenderTexture.active=old;UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(go);}}
Shot("overview",new Vector3(470,135,-280),new Vector3(397,42,-194));Shot("approach",new Vector3(477,67,-201.5f),new Vector3(422,70,-197));Shot("water",new Vector3(430,88,-124),new Vector3(391,34,-193));
return string.Join("\n",rows);
