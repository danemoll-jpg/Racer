using System;using System.IO;using UnityEngine;using UnityEditor;
// 0.73 one-off asset authoring (editor only): the frozen-water material (URP Lit, frost dusting texture, matte with a soft
// sheen, environment reflections OFF so the sky is not mirrored - the material keeps that shader variant in the build) and
// the sky-cloud material. Both live in Resources so runtime code can load them.
public static class Report073Assets {
 public static void Run(){
  try{
   const string tex="Assets/Resources/WaterIceFrost.png";AssetDatabase.ImportAsset(tex);
   var ti=(TextureImporter)AssetImporter.GetAtPath(tex);ti.wrapMode=TextureWrapMode.Repeat;ti.mipmapEnabled=true;ti.sRGBTexture=true;ti.maxTextureSize=256;ti.textureCompression=TextureImporterCompression.Compressed;ti.SaveAndReimport();
   var ice=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/WaterIce.mat");
   if(!ice){ice=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(ice,"Assets/Resources/WaterIce.mat");}
   ice.shader=Shader.Find("Universal Render Pipeline/Lit");
   ice.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(tex));ice.SetColor("_BaseColor",new Color(.93f,.97f,1f));
   ice.SetFloat("_Smoothness",.42f);ice.SetFloat("_Metallic",0);ice.SetFloat("_EnvironmentReflections",0);ice.EnableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
   ice.SetFloat("_SpecularHighlights",1);ice.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
   EditorUtility.SetDirty(ice);
   var cloud=AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/SkyClouds.mat");
   var shader=Shader.Find("Racer/StylizedClouds");if(!shader)throw new Exception("cloud shader missing");
   if(!cloud){cloud=new Material(shader);AssetDatabase.CreateAsset(cloud,"Assets/Resources/SkyClouds.mat");}
   cloud.shader=shader;cloud.renderQueue=-1;cloud.enableInstancing=false;EditorUtility.SetDirty(cloud);
   // Needle 600 FBX (Blender, Z forward / Y up, transforms baked): no cameras, lights, animation or rig; file units.
   const string fbx="Assets/Resources/VehicleModels/Needle600.fbx";AssetDatabase.ImportAsset(fbx);
   var mi=(ModelImporter)AssetImporter.GetAtPath(fbx);mi.importCameras=false;mi.importLights=false;mi.importAnimation=false;mi.animationType=ModelImporterAnimationType.None;
   mi.globalScale=1;mi.useFileScale=true;mi.bakeAxisConversion=false;mi.meshCompression=ModelImporterMeshCompression.Off;mi.isReadable=false;mi.importNormals=ModelImporterNormals.Import;
   mi.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;mi.importBlendShapes=false;mi.addCollider=false;mi.SaveAndReimport();
   var go=AssetDatabase.LoadAssetAtPath<GameObject>(fbx);var info=new System.Text.StringBuilder();
   foreach(var t in go.GetComponentsInChildren<Transform>()){var mf=t.GetComponent<MeshFilter>();info.Append($"\n{t.name} pos={t.localPosition:F3} rot={t.localEulerAngles:F1} scale={t.localScale:F3} bounds={(mf?mf.sharedMesh.bounds.ToString("F3"):"")} tris={(mf?mf.sharedMesh.triangles.Length/3:0)}");}
   File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/fbx.txt",info.ToString());
   AssetDatabase.SaveAssets();
   File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/assets.txt","ok "+ice.shaderKeywords.Length+" "+string.Join(",",ice.shaderKeywords));
   EditorApplication.Exit(0);
  }catch(Exception e){File.WriteAllText(Environment.GetEnvironmentVariable("PROBE_OUT")+"/assets.txt",e.ToString());EditorApplication.Exit(1);}
 }
}
