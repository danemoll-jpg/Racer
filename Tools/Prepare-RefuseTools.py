from pathlib import Path
root=Path(__file__).resolve().parents[1]
for filename in ['Build-Dump.cs','Prepare-Dump.ps1','Stage-Dump.py','Verify-DumpPublication.py','Activate-Dump.ps1','Verify-DumpPlayRacer.ps1','Cleanup-Dump.ps1','Inspect-DumpResult.cs']:
    s=(root/'Tools'/filename).read_text()
    for a,b in [('39000','__NEW__'),('38000','39000'),('37000','38000'),('__NEW__','40000'),('0.39.0-review1','0.40.0-review1'),('DumpCorrection','DumpRefuse'),('dump-','refuse-')]:s=s.replace(a,b)
    if filename=='Prepare-Dump.ps1':
        s=s.replace("'after-heights.csv',",'')
        s=s.replace('a deep drivable bowl with a lower floor, escapable slopes and irregular scattered junk','2,100 overlapping pieces of refuse across the accepted bowl and local grounded resistance that slows escape without solid debris obstacles')
        s=s.replace('deep supported bowl, sloped escape and irregular scattered junk','dense accumulated trash and forgiving grounded traversal resistance')
    (root/'Tools'/filename.replace('Dump','Refuse')).write_text(s)
s=(root/'Assets/Scripts/DumpCorrectionChecks.cs').read_text().replace('DumpCorrectionChecks','DumpRefuseChecks').replace('DumpCorrection','DumpRefuse').replace('-dumpCorrectionCheck','-dumpRefuseCheck')
start=s.index(' yield return Place(lip-axis*38')
end=s.index(' log.Dispose();',start)
s=s[:start]+''' var refuse=FindAnyObjectByType<DumpRefuse>();
 yield return Place(lip-axis*38,axis,32);Seed();yield return Drive("clear",32,13,true,false);Check(race.Progress.NextGate==3&&race.Progress.MissedGates==0,"Normal physical crossing progresses through unchanged CP2");
 foreach(string vehicle in new[]{"moto","atv"}){
  if(vehicle=="atv"){race.Flow.QuitRace();race.Flow.OpenGarage();race.Flow.SelectVehicle(vehicle);race.Flow.CloseGarage();race.Flow.StartRace();while(race.Flow.State!=RaceFlow.Stage.Racing){AudioListener.volume=0;yield return null;}car=race.vehicle;car.enabled=false;car.GetComponent<VehicleInput>().enabled=false;car.GetComponent<VehicleRespawn>().Respawned+=()=>resets++;}
  refuse.enabled=false;yield return Place(lip-axis*2,axis,8);Seed();yield return Drive(vehicle+"-empty",car.topSpeed,30,false,false);float empty=failedTime;
  refuse.enabled=true;yield return Place(lip-axis*2,axis,8);Seed();yield return Drive(vehicle+"-trash",car.topSpeed,30,false,false);
  Check(failedTime>empty+1,vehicle+" trash slows physical escape: "+failedTime.ToString("F2")+"s versus empty "+empty.ToString("F2")+"s");
  yield return Place(lip+axis*20-side*2,axis,0);yield return Drive(vehicle+"-maneuver",6,30,false,true);
 }
 Check(resets==0,"Both vehicles retain normal recovery and escape without reset");
 var junk=GameObject.Find("Old dump - dense push-through refuse");Check(junk&&junk.GetComponentsInChildren<Collider>().Length==0&&junk.GetComponentsInChildren<Renderer>().Length==8,"Dense trash is eight renderers with no snag, flip or launch colliders");
 car.Body.isKinematic=true;
'''+s[end:]
s=s.replace('if(vehicle=="atv"){race.Flow.QuitRace();','if(vehicle=="atv"){race.Flow.Pause();race.Flow.QuitRace();')
s=s.replace('  refuse.enabled=false;', '  Check(car.GetComponent<VehicleConfiguration>().profileId==vehicle,"Actual vehicle profile: "+car.GetComponent<VehicleConfiguration>().profileId);\n  refuse.enabled=false;')
s=s.replace('int supported=0;','int supported=0;float minUp=1,maxUpSpeed=0;')
s=s.replace('Trace(name,Time.time-begin);','Trace(name,Time.time-begin);if(car.GroundedWheels>=2)minUp=Math.Min(minUp,car.transform.up.y);maxUpSpeed=Math.Max(maxUpSpeed,car.Body.linearVelocity.y);')
s=s.replace(' if(success)Check(reached', ' Check(minUp>.5f&&maxUpSpeed<16,$"{name}: no ordinary-contact flip or launch; minUp={minUp:F3}, maxUpSpeed={maxUpSpeed:F2}");\n if(success)Check(reached')
s=s.replace('Short 8m/s jump lands in bowl', '{name} short 8m/s jump lands in bowl')
(root/'Assets/Scripts/DumpRefuseChecks.cs').write_text(s)
print('Prepared local targeted fixture and release 40000 wrappers.')
