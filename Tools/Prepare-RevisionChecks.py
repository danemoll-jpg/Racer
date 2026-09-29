from pathlib import Path
p=Path('Assets/Scripts/BackyardShortcutChecks.cs').read_text()
p=p.replace('BackyardShortcutChecks','ShortcutRevisionChecks').replace('Docs/BackyardShortcuts/checks','Docs/ShortcutRevision/checks').replace('-backyardShortcutCheck','-shortcutRevisionCheck').replace('Temp/BackyardShortcutsSave','Temp/ShortcutRevisionSave')
p=p.replace('float lastTime;', 'float lastTime; public float EntrySpeed=26; float groundOffset; bool groundRun;')
p=p.replace('f.normalized*26','f.normalized*EntrySpeed')
p=p.replace('Check(times[b.title+" main"]>lastTime+.5f','Check(times[b.title+" main"]>lastTime+.2f')
p=p.replace('4.7f-1','3.3f-.4f').replace('4.7m tree-top','3.3m tree-top')
p=p.replace('s>57&&s<100','s>33&&s<98')
# A short slower-entry attempt and two local ground routes, one on each side.
needle=' // Model dimensions'
insert='''
 foreach(var b in race.Branches.Where(b=>Only==""||b.title==Only)){
 if(b.title=="Tree-Top Trail"){EntrySpeed=19;yield return Place(b,true);yield return Drive(b,true);EntrySpeed=26;}
 foreach(float offset in new[]{-5f,5f}){groundRun=true;groundOffset=offset;yield return Place(b,true);yield return GroundDrive(b);groundRun=false;Check(lastTime>times[b.title+" main"]+1,$"{b.title} ground {offset:+0;-0}m: {lastTime:F2}s versus main {times[b.title+" main"]:F2}s");}
 }
'''
p=p.replace(needle,insert+needle)
# Ground setup uses the same entrance longitudinal coordinate, off the ramp, on terrain.
p=p.replace('car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.Body.isKinematic=false;', '''if(groundRun){p+=Vector3.Cross(Vector3.up,f).normalized*groundOffset;p.y=Ground(p);}car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.Body.isKinematic=false;''')
# Underbrush resistance makes a 22s timeout too short for failures, use a bounded 65s recovery test.
end='''
 float Ground(Vector3 p)=>Physics.RaycastAll(p+Vector3.up*150,Vector3.down,300,1,QueryTriggerInteraction.Ignore).Where(h=>h.collider.name.StartsWith("Ground_")).OrderBy(h=>h.distance).First().point.y;
 IEnumerator GroundDrive(WoodlandRoute b){float start=Time.time;int before=resets;bool reached=false;using(var log=new StreamWriter(Output+"/ground-"+b.title.Replace(' ','-')+groundOffset+".csv")){log.WriteLine("time,station,speed,lateral,wheels");while(Time.time-start<65){float s=b.Project(car.Body.position,out var lateral);if(s>b.Length-5){reached=true;break;}var target=b.At(s+5,out var f)+Vector3.Cross(Vector3.up,f).normalized*groundOffset;target.y=Ground(target);car.Simulate(1,0,Steer(target),Time.fixedDeltaTime);yield return new WaitForFixedUpdate();AudioListener.volume=0;log.WriteLine($"{Time.time-start:F2},{s:F2},{car.ForwardSpeed:F2},{lateral:F2},{car.GroundedWheels}");}}lastTime=Time.time-start;Check(reached&&resets==before,$"{b.title}: traversable ground recovery offset={groundOffset} reached={reached} resets={resets-before} time={lastTime:F2}");}
'''
at=p.rfind('\n}\n}')
p=p[:at]+end+p[at:]
Path('Assets/Scripts/ShortcutRevisionChecks.cs').write_text(p)
