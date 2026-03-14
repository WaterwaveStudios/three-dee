using UnityEngine;
using ThreeDee.Camera;
using ThreeDee.Grid;
using ThreeDee.Units;

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
            var isoCamera = SetupCamera();
            PlaceholderBuildings.SpawnDefaultBuildings(grid);
            var player = SpawnUnits(grid);
            SetupCameraFollow(isoCamera, player);

            Debug.Log("[ThreeDee] Bootstrap complete.\n" +
                "[Controls]\n" +
                "  WASD / Arrows    — Move unit\n" +
                "  Scroll           — Zoom in/out\n" +
                "  Right-click drag — Pan camera\n" +
                "  Alt + left-click drag — Pan camera\n" +
                "[Mobile]\n" +
                "  Pinch            — Zoom\n" +
                "  Two-finger drag  — Pan camera");
        }

        private static Transform SpawnUnits(GridManager grid)
        {
            var explorer = SpawnModel("Models/Units/Meshy_AI_Explorer_s_Adventure_0314142745_texture",
                "Explorer", grid.GridToWorldPosition(3, 5));
            SpawnModel("Models/Units/Meshy_AI_full_body_3D_cartoon__0314142802_texture",
                "Soldier", grid.GridToWorldPosition(6, 5));

            // Explorer is the player-controlled unit
            if (explorer != null)
            {
                var unit = explorer.AddComponent<UnitController>();
                unit.Init(grid.GridToWorldPosition(3, 5));
            }

            return explorer != null ? explorer.transform : null;
        }

        private static void SetupCameraFollow(IsometricCamera isoCamera, Transform player)
        {
            if (player == null) return;
            var follower = isoCamera.gameObject.AddComponent<CameraFollower>();
            follower.Init(player);
        }

        private static GameObject SpawnModel(string resourcePath, string name, Vector3 gridPosition)
        {
            var prefab = Resources.Load<GameObject>(resourcePath);
            if (prefab == null)
            {
                Debug.LogWarning($"[ThreeDee] Model not found at Resources/{resourcePath}");
                return null;
            }

            // Wrap in parent so model rotation doesn't affect movement
            var wrapper = new GameObject(name);
            wrapper.transform.position = gridPosition;

            var instance = Object.Instantiate(prefab, wrapper.transform);
            instance.name = "Model";
            instance.transform.localPosition = Vector3.zero;

            // Fix Meshy FBX 90-degree X rotation
            instance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            instance.SetActive(true);

            // Meshy FBX models have a matching texture PNG — apply with URP Lit
            var texture = Resources.Load<Texture2D>(resourcePath);
            if (texture != null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit")
                          ?? Shader.Find("Standard");
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var mat = new Material(shader);
                    mat.mainTexture = texture;
                    mat.SetFloat("_Smoothness", 0.3f);
                    renderer.sharedMaterial = mat;
                }
            }

            return wrapper;
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

        private static IsometricCamera SetupCamera()
        {
            // Remove any existing cameras
            foreach (var existingCam in Object.FindObjectsByType<UnityEngine.Camera>(FindObjectsSortMode.None))
            {
                Object.Destroy(existingCam.gameObject);
            }

            return IsometricCamera.Create(
                position: new Vector3(0f, 20f, -15f),
                orthographicSize: 15f
            );
        }
    }
}
