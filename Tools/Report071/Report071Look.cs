using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEngine.Rendering.Universal;
// 0.71 Part C one-time setup: the PC pipeline-asset settings the world look needs that cannot change at runtime without
// rebuilding the pipeline: HDR (tonemapping, bloom) and an 80 m shadow distance (was 40 m; 4 cascades unchanged).
// Restore(): the 0.70 values (HDR off, 40 m), used only to build the "before" evidence player.
public static class Report071Look {
 const string Path="Assets/Settings/PC_RPAsset.asset";
 static void Apply(bool hdr,float shadows){var rp=AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Path);rp.supportsHDR=hdr;rp.shadowDistance=shadows;EditorUtility.SetDirty(rp);AssetDatabase.SaveAssets();
  File.AppendAllText("Docs/Report071/look-setup.txt",$"{DateTime.Now:s} PC_RPAsset HDR {rp.supportsHDR} shadow distance {rp.shadowDistance} cascades {rp.shadowCascadeCount}\n");EditorApplication.Exit(0);}
 public static void Setup()=>Apply(true,80);
 public static void Restore()=>Apply(false,40);
}
