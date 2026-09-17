var names=new[]{"Docs/PHASE5_MIXED.txt","Docs/PHASE5_RECOVERY.txt","Docs/PHASE4_JUMP.txt","Docs/PHASE4_RACE_REGRESSION.txt","Docs/PHASE6_GEOMETRY.txt"};
var saved=names.ToDictionary(n=>n,n=>System.IO.File.ReadAllBytes(n));
try{
 Racer.Editor.Phase5Validation.Run("mixed");System.IO.File.Copy(names[0],"Docs/CR015/MIXED_LAPS.txt",true);
 Racer.Editor.Phase5Validation.Run("recovery");System.IO.File.Copy(names[1],"Docs/CR015/RECOVERY.txt",true);
 Racer.Editor.Phase4Validation.Jump();System.IO.File.Copy(names[2],"Docs/CR015/JUMP_TESTS.txt",true);
 Racer.Editor.Phase4Validation.RaceRegression();System.IO.File.Copy(names[3],"Docs/CR015/RACE_REGRESSION.txt",true);
 System.IO.File.WriteAllText("Docs/CR015/TESTS_COMPLETE.txt","Completed mixed racing-speed laps, shortcut recovery, jump matrix and race/input/HUD/reset checks. See individual reports for pass/fail results. Virtual input/manual PhysX only.\n");
}finally{foreach(var pair in saved)System.IO.File.WriteAllBytes(pair.Key,pair.Value);}
return "Test reports saved separately; historical reports preserved";

