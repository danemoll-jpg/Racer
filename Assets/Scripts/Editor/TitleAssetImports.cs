using UnityEditor;
using UnityEngine;
namespace Racer.Editor
{
    public sealed class TitleAssetImports:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(assetPath!="Assets/Resources/Title/Artwork.png")return;
            var importer=(TextureImporter)assetImporter;importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;importer.alphaSource=TextureImporterAlphaSource.None;
        }
        void OnPreprocessAudio()
        {
            if(!assetPath.StartsWith("Assets/Resources/Title/"))return;
            var importer=(AudioImporter)assetImporter;var settings=importer.defaultSampleSettings;
            settings.compressionFormat=AudioCompressionFormat.PCM;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;importer.loadInBackground=true;
        }
    }
}
