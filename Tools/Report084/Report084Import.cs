using System.Linq;using UnityEditor;using UnityEditor.Build;using UnityEngine;
// 0.84 Part A: the Windows icon. Assets/Branding/WoodstockRushIcon.png is a byte copy of SourceArt/Poster/WoodstockRushIcon.png
// (1024x1024). Imported full size, uncompressed, with mipmaps (Windows asks for 16-256 px), and set as the default icon and
// as every Standalone icon size, so Racer.exe, the window, the taskbar and Alt-Tab use it. productName/companyName unchanged.
public static class Report084Import {
 public static void Run(){const string p="Assets/Branding/WoodstockRushIcon.png";AssetDatabase.ImportAsset(p);var t=(TextureImporter)AssetImporter.GetAtPath(p);
  t.textureType=TextureImporterType.Default;t.sRGBTexture=true;t.alphaSource=TextureImporterAlphaSource.None;t.mipmapEnabled=true;t.npotScale=TextureImporterNPOTScale.None;
  t.filterMode=FilterMode.Trilinear;t.wrapMode=TextureWrapMode.Clamp;t.maxTextureSize=1024;t.textureCompression=TextureImporterCompression.Uncompressed;t.isReadable=true;t.SaveAndReimport();
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(p);
  PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{tex},IconKind.Any);
  var sizes=PlayerSettings.GetIconSizes(NamedBuildTarget.Standalone,IconKind.Any);PlayerSettings.SetIcons(NamedBuildTarget.Standalone,sizes.Select(_=>tex).ToArray(),IconKind.Any);
  AssetDatabase.SaveAssets();
  Debug.Log($"REPORT084 icon {tex.width}x{tex.height}; standalone sizes {string.Join(",",sizes)}; productName {PlayerSettings.productName}, company {PlayerSettings.companyName}");EditorApplication.Exit(0);}
}
