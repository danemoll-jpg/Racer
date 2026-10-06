using UnityEditor;using UnityEngine;
// 0.83 Part E: import settings for the loading-screen poster (Assets/Resources/LoadingPoster.png, a byte copy of
// SourceArt/Poster/WoodstockRushPoster.png): full size, uncompressed, no mipmaps (it is only ever shown larger than its
// 1672 px), bilinear, clamped, no resizing to a power of two.
public static class Report083Import {
 public static void Run(){const string p="Assets/Resources/LoadingPoster.png";AssetDatabase.ImportAsset(p);var t=(TextureImporter)AssetImporter.GetAtPath(p);
  t.textureType=TextureImporterType.Default;t.sRGBTexture=true;t.alphaSource=TextureImporterAlphaSource.None;t.mipmapEnabled=false;t.npotScale=TextureImporterNPOTScale.None;
  t.filterMode=FilterMode.Bilinear;t.wrapMode=TextureWrapMode.Clamp;t.anisoLevel=1;t.maxTextureSize=2048;t.textureCompression=TextureImporterCompression.Uncompressed;t.SaveAndReimport();
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(p);Debug.Log($"REPORT083 poster imported {tex.width}x{tex.height} {tex.format} mips {tex.mipmapCount}");EditorApplication.Exit(0);}
}
