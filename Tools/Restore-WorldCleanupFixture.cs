EditorApplication.delayCall+=()=>{try{
if(Application.isPlaying||EditorApplication.isCompiling)throw new Exception("Compiled edit mode required");
UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DansBackyardForward.unity");
EditorSettings.enterPlayModeOptionsEnabled=bool.Parse(System.IO.File.ReadAllText("Temp/worldcleanup-editor-options.txt"));
var glazing=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/VehicleGlazing.mat");glazing.SetFloat("_SrcBlend",1);EditorUtility.SetDirty(glazing);PlayerSettings.bundleVersion="0.45.0-review1";AssetDatabase.SaveAssets();
System.IO.File.WriteAllText("Docs/WorldCleanup/test-state-restored.txt","Saved scene restored without saving fixtures; original Enter Play Mode options restored; test-mutated glazing SrcBlend restored to checkpoint value 1; release version 0.45.0-review1.");
}catch(Exception e){System.IO.File.WriteAllText("Docs/WorldCleanup/test-restore-error.txt",e.ToString());}};return "Scheduled restoration of isolated test state";
