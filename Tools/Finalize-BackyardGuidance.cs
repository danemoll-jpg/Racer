EditorApplication.delayCall+=()=>{try{
foreach(string name in new[]{"DansBackyard","DansBackyardReverse"}){
 var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity");var race=UnityEngine.Object.FindAnyObjectByType<Racer.RaceDirector>();race.road.Initialize();
 if(name=="DansBackyard") {var f=race.GetComponent<Racer.MountainFlights>().flights[0];f.landingEnd=new Vector3(240,76.5f,6.2f);f.endStation=race.road.Project(f.landingEnd,out _);}
 // Re-space gates along the final directional route after the local reverse bypass.
 for(int i=0;i<race.gates.Length;i++){float s=i==0?0:95+(race.road.Length-170)*(i-1)/(race.gates.Length-2);var p=race.road.At(s,out var forward);race.gates[i].transform.SetPositionAndRotation(p+Vector3.up*1.6f,Quaternion.LookRotation(Vector3.ProjectOnPlane(forward,Vector3.up)));}
 foreach(var branch in UnityEngine.Object.FindObjectsByType<Racer.WoodlandRoute>())branch.bypassedGates=race.gates.Select((g,i)=>(g,i)).Where(x=>x.i>0&&race.road.Relative(race.road.Project(x.g.transform.position,out _),branch.entryRoad)<race.road.Relative(branch.exitRoad,branch.entryRoad)).Select(x=>x.i).ToArray();
 var file="Docs/Backyard/"+name+"-geometry.json";var data=JsonUtility.FromJson<Racer.Editor.BackyardAuthoring.Export>(System.IO.File.ReadAllText(file));data.gates=race.gates.Select(g=>g.transform.position).ToArray();System.IO.File.WriteAllText(file,JsonUtility.ToJson(data,true));
 UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
}
AssetDatabase.SaveAssets();System.IO.File.WriteAllText("Docs/Backyard/final-guidance-done.txt","Dump guidance ends on far landing; ten gates ordered along each final route.");
}catch(Exception e){System.IO.File.WriteAllText("Docs/Backyard/final-guidance-error.txt",e.ToString());}};return "Scheduled final local guidance correction";
