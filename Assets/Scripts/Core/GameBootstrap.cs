using UnityEngine;
using ThreeDee.Camera;
using ThreeDee.Grid;

namespace ThreeDee.Core
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            Debug.Log("[ThreeDee] Bootstrapping isometric base builder...");

            SetupLighting();
            var grid = SetupGrid();
            SetupCamera();
            PlaceholderBuildings.SpawnDefaultBuildings(grid);

            Debug.Log("[ThreeDee] Bootstrap complete.");
        }

        private static void SetupLighting()
        {
            // Directional light — sun
            var lightGo = new GameObject("DirectionalLight");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Ambient light via RenderSettings
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f);
        }

        private static GridManager SetupGrid()
        {
            var grid = GridManager.Create(width: 10, height: 10, cellSize: 2f);
            grid.GenerateGrid();
            return grid;
        }

        private static void SetupCamera()
        {
            // Remove any existing cameras
            foreach (var existingCam in Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None))
            {
                Object.Destroy(existingCam.gameObject);
            }

            IsometricCamera.Create(
                position: new Vector3(0f, 20f, -15f),
                orthographicSize: 15f
            );
        }
    }
}
