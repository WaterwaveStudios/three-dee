using UnityEditor;
using UnityEngine;

namespace ThreeDee.Editor
{
    public class MeshyAssetImporter : AssetPostprocessor
    {
        private bool IsModelPath => assetPath.Contains("Models/Units") || assetPath.Contains("Models/Buildings");

        private void OnPreprocessTexture()
        {
            if (!IsModelPath) return;

            var importer = (TextureImporter)assetImporter;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 4;

            var mobileSettings = importer.GetPlatformTextureSettings("Android");
            mobileSettings.overridden = true;
            mobileSettings.maxTextureSize = 1024;
            mobileSettings.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(mobileSettings);

            var iosSettings = importer.GetPlatformTextureSettings("iPhone");
            iosSettings.overridden = true;
            iosSettings.maxTextureSize = 1024;
            iosSettings.format = TextureImporterFormat.ASTC_6x6;
            importer.SetPlatformTextureSettings(iosSettings);
        }

        private void OnPreprocessModel()
        {
            if (!IsModelPath) return;

            var importer = (ModelImporter)assetImporter;

            // Fix Meshy FBX axis — models come in rotated 90 degrees on X
            importer.bakeAxisConversion = true;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
        }

        [MenuItem("ThreeDee/Reimport All Models")]
        public static void ReimportAllModels()
        {
            var guids = AssetDatabase.FindAssets("t:Model t:Texture2D", new[] { "Assets/Resources/Models" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Debug.Log($"[ThreeDee] Reimported: {path}");
            }
            Debug.Log($"[ThreeDee] Reimported {guids.Length} assets.");
        }
    }
}
