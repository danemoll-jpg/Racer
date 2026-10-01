EditorApplication.delayCall+=()=>{try{
 if(EditorApplication.isPlaying||EditorApplication.isCompiling)throw new Exception("Saved edit mode required");
 var original=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
 if(original.isDirty)throw new Exception("Unsaved scene must be preserved");
 var catalog=new Racer.CoursePreviewCatalog();var courses=new List<Racer.CoursePreviewCatalog.Course>();
 foreach(string name in Racer.RacePlaylists.Scenes){
  string path="Assets/Scenes/"+name+".unity";var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);bool opened=!scene.IsValid()||!scene.isLoaded;
  if(opened)scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene(path,UnityEditor.SceneManagement.OpenSceneMode.Additive);
  try{
   if(scene.isDirty)throw new Exception("Unsaved course: "+name);
   var roots=scene.GetRootGameObjects();var race=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.RaceDirector>(true)).Single();
   courses.Add(new Racer.CoursePreviewCatalog.Course{scene=name,id=race.courseId,main=race.road.points.ToArray(),gates=race.gates.Select(g=>g.transform.position).ToArray(),branches=roots.SelectMany(g=>g.GetComponentsInChildren<Racer.WoodlandRoute>()).Where(b=>b.gameObject.activeInHierarchy).Select(b=>new Racer.CoursePreviewCatalog.Path{points=b.points.ToArray(),undergroundStart=b.undergroundStart,undergroundEnd=b.undergroundEnd}).ToArray()});
  }finally{if(opened)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);}
 }
 UnityEngine.SceneManagement.SceneManager.SetActiveScene(original);catalog.courses=courses.ToArray();
 System.IO.File.WriteAllText("Assets/Resources/WorldMaps/CoursePreviews.json",JsonUtility.ToJson(catalog,true));AssetDatabase.Refresh();
 System.IO.File.WriteAllText("Docs/UI/TrackBrowsing/capture.txt",string.Join("\n",courses.Select(c=>c.scene+" | "+c.id+" | main="+c.main.Length+" branches="+c.branches.Length+" gates="+c.gates.Length)));
}catch(Exception e){System.IO.File.WriteAllText("Docs/UI/TrackBrowsing/capture-error.txt",e.ToString());}};return "Scheduled saved-course preview capture; scenes will not be saved";
