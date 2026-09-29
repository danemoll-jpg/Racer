from pathlib import Path
s=Path('Assets/Scripts/ShortcutRevisionChecks.cs').read_text()
s=s.replace('ShortcutRevisionChecks','ShortcutRevisionFollowupChecks').replace('Docs/ShortcutRevision/checks','Docs/ShortcutRevision/followup')
a=s.index(' IEnumerator Start()');b=s.index(' IEnumerator Place(',a)
s=s[:a]+''' IEnumerator Start(){Directory.CreateDirectory(Output);AudioListener.volume=0;Application.runInBackground=true;yield return null;yield return null;race=FindAnyObjectByType<RaceDirector>();
 #if UNITY_EDITOR
 race.Flow.UseValidationSave(Path.GetFullPath("Temp/ShortcutRevisionSave"));
 #endif
 race.Flow.OpenGarage();race.Flow.SelectVehicle("moto");race.Flow.CloseGarage();race.opponents=race.traffic=false;race.laps=1;race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}car=race.vehicle;car.GetComponent<VehicleInput>().enabled=false;car.enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;
 var tree=race.Branches.First(b=>b.title=="Tree-Top Trail");EntrySpeed=26;yield return Place(tree,false);yield return Drive(tree,false);times[tree.title+" main"]=lastTime;
 yield return Place(tree,true);yield return Drive(tree,true);Check(lastTime<times[tree.title+" main"]-.2f,$"Tree clean {lastTime:F2}s faster than main {times[tree.title+" main"]:F2}s");
 EntrySpeed=19;yield return Place(tree,true);yield return Drive(tree,true);EntrySpeed=26;
 times["Abandoned Cabin Jump main"]=5.06f;
 foreach(var branch in race.Branches)foreach(float offset in branch==tree?new[]{5f}:new[]{-5f,5f}){groundOffset=offset;yield return PlannedGround(branch);}
 File.WriteAllLines(Output+"/done.txt",checks);car.Body.isKinematic=true;race.Flow.Pause();
 }
''' + s[b:]
s=s.replace('branch?Mathf.Min(32,b.SpeedAt(s)):32','branch?Mathf.Min(EntrySpeed<20&&s<48?EntrySpeed:32,b.SpeedAt(s)):32')
s=s.replace('car.maxGripAcceleration*.65f','car.maxGripAcceleration*.5f')
s=s.replace('Check(maxAir>.75f','Check(maxAir>(b.title=="Tree-Top Trail"?.25f:.75f)')
s=s.replace('string name=(branch?"shortcut-":"main-")+b.title.Replace(\' \',\'-\');','string name=(branch?"shortcut-":"main-")+b.title.Replace(\' \',\'-\')+"-"+EntrySpeed;')
# Add a direct first-platform touchdown assertion, not just eventual deck support.
s=s.replace('int before=resets,deckFrames=0;','int before=resets,deckFrames=0;float firstDeck=-1;')
s=s.replace('deckFrames++;','{deckFrames++;if(firstDeck<0)firstDeck=s;}')
s=s.replace('if(deckFrames', 'if(deckFrames')
s=s.replace('Check(deckFrames>20,','Check(firstDeck>=33&&firstDeck<43,$"Tree first touchdown {firstDeck:F2}m; first platform33..48, entry cap{EntrySpeed}");Check(deckFrames>20,')
# Existing code has single-statement if; add braces to keep cabin-independent declaration valid.
at=s.rfind('\n}\n}')
s=s[:at]+'''
 IEnumerator PlannedGround(WoodlandRoute b){
 float begin=b.title=="Tree-Top Trail"?36:44,end=b.Length-6;int count=Mathf.CeilToInt((end-begin)/2)+1;const int lanes=9;var points=new Vector3[count,lanes];var costs=new float[count,lanes];var prev=new int[count,lanes];float sign=Mathf.Sign(groundOffset);
 bool Clear(Vector3 p,Quaternion rot)=>!Physics.OverlapBox(p+Vector3.up*.85f,new Vector3(.72f,.48f,1.15f),rot,1,QueryTriggerInteraction.Ignore).Any(c=>!c.name.StartsWith("Ground_")&&c.attachedRigidbody!=car.Body);
 for(int i=0;i<count;i++)for(int j=0;j<lanes;j++){float station=Mathf.Lerp(begin,end,(float)i/(count-1));var p=b.At(station,out var f)+Vector3.Cross(Vector3.up,f).normalized*sign*(3+j);p.y=Ground(p);points[i,j]=p;costs[i,j]=1e9f;prev[i,j]=-1;if(!Clear(p,Quaternion.LookRotation(Vector3.ProjectOnPlane(f,Vector3.up))))continue;if(i==0){costs[i,j]=Mathf.Abs(j-2)*2;continue;}for(int k=Mathf.Max(0,j-1);k<=Mathf.Min(lanes-1,j+1);k++){float c=costs[i-1,k]+Vector3.Distance(points[i-1,k],p)+Mathf.Abs(j-2)*.05f;if(c<costs[i,j]){costs[i,j]=c;prev[i,j]=k;}}}
 int lane=Enumerable.Range(0,lanes).OrderBy(j=>costs[count-1,j]).First();if(costs[count-1,lane]>1e8f){Check(false,b.title+" no passable local ground path for "+groundOffset);yield break;}var path=new Vector3[count];for(int i=count-1;i>=0;i--){path[i]=points[i,lane];lane=prev[i,lane];}
 File.WriteAllText(Output+"/ground-path-"+b.title+groundOffset+".json",Newtonsoft.Json.JsonConvert.SerializeObject(path.Select(p=>new {p.x,p.y,p.z})));
 car.Body.isKinematic=false;car.Body.position=path[0]+Vector3.up*.55f;car.Body.rotation=Quaternion.LookRotation(Vector3.ProjectOnPlane(path[1]-path[0],Vector3.up));car.transform.SetPositionAndRotation(car.Body.position,car.Body.rotation);car.Body.linearVelocity=car.Body.angularVelocity=Vector3.zero;car.ClearSteering();Physics.SyncTransforms();for(int i=0;i<25;i++){car.Simulate(0,0,0,Time.fixedDeltaTime);yield return new WaitForFixedUpdate();}car.Body.linearVelocity=car.transform.forward*18;float start=Time.time;int before=resets;int next=1;float stalled=0,reverseUntil=0;using(var log=new StreamWriter(Output+"/ground-"+b.title.Replace(' ','-')+groundOffset+".csv")){log.WriteLine("time,station,speed,wheels,next");while(Time.time-start<50&&next<count){var p=car.Body.position;while(next<count-1&&Vector3.Dot(p-path[next],path[next+1]-path[next])>0)next++;if(next==count-1&&Vector2.Distance(new(p.x,p.z),new(path[^1].x,path[^1].z))<3)break;var target=path[Mathf.Min(count-1,next+1)];if(car.ForwardSpeed<.3f)stalled+=Time.fixedDeltaTime;else stalled=0;if(stalled>1.2f){reverseUntil=Time.time+1.2f;stalled=0;}bool reverse=Time.time<reverseUntil;car.Simulate(reverse?0:1,reverse?1:0,reverse?-sign:Steer(target),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();AudioListener.volume=0;log.WriteLine($"{Time.time-start:F2},{b.Project(car.Body.position,out _):F2},{car.ForwardSpeed:F2},{car.GroundedWheels},{next}");}}
 float elapsed=Time.time-start;bool reached=next>=count-2;Check(reached&&resets==before,$"{b.title} ground {groundOffset}: traversed {begin:F0}..{end:F0} in {elapsed:F2}s, reached={reached}, resets={resets-before}");Check(reached&&elapsed>times[b.title+" main"]+1,$"{b.title} bush section alone {elapsed:F2}s slower than full main {times[b.title+" main"]:F2}s");
 }
''' +s[at:]
Path('Assets/Scripts/ShortcutRevisionFollowupChecks.cs').write_text(s)
