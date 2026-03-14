using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThreeDee.Editor
{
    [InitializeOnLoad]
    public static class URPSetup
    {
        static URPSetup()
        {
            // Only run once — skip if already configured
            if (GraphicsSettings.defaultRenderPipeline != null)
                return;

            EditorApplication.delayCall += SetupURP;
        }

        [MenuItem("ThreeDee/Setup URP Pipeline")]
        public static void SetupURP()
        {
            const string assetPath = "Assets/Settings";
            const string rendererPath = assetPath + "/URP-Renderer.asset";
            const string pipelinePath = assetPath + "/URP-Pipeline.asset";

            if (!AssetDatabase.IsValidFolder(assetPath))
                AssetDatabase.CreateFolder("Assets", "Settings");

            // Create renderer data
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (rendererData == null)
            {
                rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }

            // Create pipeline asset
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipelineAsset == null)
            {
                pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, pipelinePath);
            }

            // Configure pipeline settings
            pipelineAsset.shadowDistance = 50f;

            // Assign to graphics settings
            GraphicsSettings.defaultRenderPipeline = pipelineAsset;

            // Assign to all quality levels
            int qualityLevelCount = QualitySettings.names.Length;
            for (int i = 0; i < qualityLevelCount; i++)
            {
                QualitySettings.SetQualityLevel(i, false);
                QualitySettings.renderPipeline = pipelineAsset;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ThreeDee] URP pipeline configured successfully.");
        }
    }
}
