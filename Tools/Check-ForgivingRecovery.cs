const System.Reflection.BindingFlags F=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic; string label="after";
  if(Application.isPlaying)throw new Exception("Saved edit mode required");
  string original=UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
  var rows=new List<string>();
  try {
   UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/ForestLoopReverse.unity");
   var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();var car=race.vehicle;var reset=car.GetComponent<Racer.VehicleRespawn>();var road=race.road;
   typeof(Racer.ArcadeVehicle).GetMethod("Awake",F).Invoke(car,null);typeof(Racer.VehicleRespawn).GetMethod("Awake",F).Invoke(reset,null);
   foreach(var body in UnityEngine.Object.FindObjectsByType<Rigidbody>())if(body!=car.Body)body.gameObject.SetActive(false);
   var state=new Racer.RacerState("fixture",car,race.gates.Length-1,3);race.Racers.Clear();race.Racers.Add(state);state.Progress.Cross(0,true,0);
   var record=typeof(Racer.VehicleRespawn).GetMethod("RecordSafePosition",F,null,new[]{typeof(float)},null);
   var supported=typeof(Racer.VehicleRespawn).GetMethod("Supported",F);float now=Time.time+10;
   void Sample(float s,Racer.WoodlandRoute branch=null,int contacts=4) {
    var p=branch?branch.At(s,out var f):road.At(s,out f);
    var a=new object[]{p,Vector3.ProjectOnPlane(f,Vector3.up).normalized,Vector3.zero,Quaternion.identity};
    bool ok=(bool)supported.Invoke(reset,a);var pos=ok?(Vector3)a[2]:p+Vector3.up*.6f;
    var rotation=ok?(Quaternion)a[3]:Quaternion.LookRotation(f);
    if(contacts==0)pos+=Vector3.up*12;
    var before=car.Body.position;car.transform.SetPositionAndRotation(pos,rotation);car.Body.position=pos;car.Body.rotation=rotation;
    car.Body.linearVelocity=Vector3.ProjectOnPlane(f,rotation*Vector3.up).normalized*10;car.Body.angularVelocity=Vector3.zero;
    typeof(Racer.ArcadeVehicle).GetProperty("GroundedWheels").SetValue(car,contacts);Physics.SyncTransforms();
    if(branch)state.Branch.Advance(before,pos,f);
    record.Invoke(reset,new object[]{now});now+=.11f;
   }
   void Segment(string name,float start,float end,Racer.WoodlandRoute branch=null) {
    reset.CancelRecovery();if(branch)state.Branch.Begin(branch);else state.Branch.Clear();
    reset.SeedCoursePosition(branch?branch.At(start,out _):road.At(start,out _));
    if(name=="post-jump")Sample(start-2,branch,0);
    for(float s=start;s<=end;s++)Sample(s,branch);
    float safe=reset.SafeStation;Sample(end+3,branch,0);int gate=state.Progress.NextGate,lap=state.Progress.CompletedLaps;float earned=state.Branch.Earned;
    bool ok=reset.TryRecoverLocal();float selected=branch?branch.Project(car.Body.position,out _):road.Project(car.Body.position,out _);
    rows.Add($"{name}: end={end:F2} safe={safe:F2} selected={selected:F2} loss={end+3-selected:F2} recovered={ok} gateUnchanged={state.Progress.NextGate==gate} lapUnchanged={state.Progress.CompletedLaps==lap} earnedUnchanged={state.Branch.Earned==earned}");
    if(!ok||end+3-selected>8||state.Progress.NextGate!=gate||state.Progress.CompletedLaps!=lap||state.Branch.Earned!=earned)throw new Exception(rows[^1]);
    rows.Add(reset.RecoveryDiagnostic);
   }
   Segment("main",20,65);
   var layout=UnityEngine.Object.FindAnyObjectByType<Racer.ForestLayout>();
   Segment("jump-approach",layout.jumpStarts[0]-60,layout.jumpStarts[0]-4);
   Segment("post-jump",layout.jumpEnds[0]-35,layout.jumpEnds[0]-15);
   var detour=UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>().Single(b=>b.title=="House 3 Detour");
   Segment("shortcut",detour.Length*.55f,detour.Length*.55f+25,detour);
   Segment("blocked-newest setup",20,65);
   var barrier=GameObject.CreatePrimitive(PrimitiveType.Cube);var at=road.At(65,out var forward);
   barrier.transform.SetPositionAndRotation(at+forward*1.5f+Vector3.up*2,Quaternion.LookRotation(forward));barrier.transform.localScale=new Vector3(25,4,.2f);
   Sample(68,null,0);Physics.SyncTransforms();
   if(!reset.TryRecoverLocal()||65-reset.SafeStation>8||reset.SafeStation>=64.9f)throw new Exception("Did not select next recent clear candidate");
   rows.Add("Blocked newest: "+reset.RecoveryDiagnostic);
   float recovered=reset.SafeStation;UnityEngine.Object.DestroyImmediate(barrier);
   if(!reset.TryRecoverLocal()||reset.SafeStation>recovered+.01f)throw new Exception("Repeated reset advanced to rejected history");
   int nextGate=state.Progress.NextGate;int completed=state.Progress.CompletedLaps;
   // Move across the lap boundary through the normal progress API, without recording new history.
   while(state.Progress.NextGate!=0)state.Progress.Cross(state.Progress.NextGate,true,10);
   state.Progress.Cross(0,true,20);
   if(reset.TryRecoverLocal()||state.Progress.CompletedLaps!=completed+1)throw new Exception("Old-lap history was reused or reset awarded lap progress");
   rows.Add("PASS: newest obstruction searches recent history; repeat reset cannot advance; previous-lap samples rejected; unchanged gate/lap/branch entitlement on placement.");
   System.IO.Directory.CreateDirectory("Docs/ForgivingStrategy");System.IO.File.WriteAllLines("Docs/ForgivingStrategy/recovery-"+label+".txt",rows);
   return string.Join("\n",rows);
  } finally {UnityEditor.SceneManagement.EditorSceneManager.OpenScene(original);}

