EditorApplication.delayCall+=()=>{try{
if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Compiled edit mode required");
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");
EditorSettings.enterPlayModeOptionsEnabled=bool.Parse(System.IO.File.ReadAllText("Temp/worldmap-editor-options.txt"));
var glazing=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/VehicleGlazing.mat");glazing.SetFloat("_SrcBlend",1);EditorUtility.SetDirty(glazing);PlayerSettings.bundleVersion="0.44.0-review1";AssetDatabase.SaveAssets();
System.IO.File.WriteAllText("Docs/WorldMap/test-state-restored.txt","Saved scene restored without saving fixtures; original Enter Play Mode options restored; test-mutated glazing SrcBlend restored to checkpoint value 1; release version 0.44.0-review1.");
}catch(Exception e){System.IO.File.WriteAllText("Docs/WorldMap/test-restore-error.txt",e.ToString());}};return "Scheduled restoration of isolated test state";
