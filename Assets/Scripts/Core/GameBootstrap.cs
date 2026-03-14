using UnityEngine;
using UnityEngine.UI;
using ThreeDee.Camera;
using ThreeDee.Grid;
using ThreeDee.Units;
using ThreeDee.UI;

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

            // Build NavMesh after ground + buildings are placed, before spawning agents
            float mapSize = grid.GridWidth * grid.CellSize;
            NavMeshSetup.Build(mapSize, mapSize);

            var (player, playerHealth) = SpawnUnits(grid);
            SetupCameraFollow(isoCamera, player);
            SetupHUD(playerHealth);

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

        private static (Transform player, PlayerHealth health) SpawnUnits(GridManager grid)
        {
            // Merged animations model: bakeAxisConversion handles axis flip, so no -90 X needed.
            // Texture has a different filename so pass it explicitly.
            var explorer = SpawnModel(
                "Models/Units/Meshy_AI_Meshy_Merged_Animations",
                "Explorer",
                grid.GridToWorldPosition(3, 5),
                modelRotation: Quaternion.Euler(0f, 180f, 0f),
                texturePath: "Models/Units/Meshy_AI_texture_0 1");

            PlayerHealth playerHealth = null;

            // Explorer is the player-controlled unit
            if (explorer != null)
            {
                AttachAnimator(explorer, "Animations/MeshyMergedAnimations");
                AttachPhysics(explorer);
                var unit = explorer.AddComponent<UnitController>();
                unit.Init(grid.GridToWorldPosition(3, 5));
                playerHealth = explorer.AddComponent<PlayerHealth>();
            }

            // Spawn 11 zombies at spread positions across the map
            var zombieSpawns = new (int x, int z)[]
            {
                (6, 5), (0, 0), (9, 0), (0, 9), (9, 9),
                (8, 2), (0, 4), (9, 6), (2, 9), (7, 1), (0, 8)
            };

            for (int i = 0; i < zombieSpawns.Length; i++)
            {
                var (zx, zz) = zombieSpawns[i];
                var zombie = SpawnModel(
                    "Models/Units/zombie",
                    $"Zombie_{i}",
                    grid.GridToWorldPosition(zx, zz),
                    modelRotation: Quaternion.Euler(0f, 180f, 0f),
                    texturePath: "Models/Units/zombie texutre");

                if (zombie != null)
                {
                    AttachAnimator(zombie, "Animations/ZombieAnimations");
                    AttachZombieAgent(zombie, i);
                    var zombieCtrl = zombie.AddComponent<ZombieController>();
                    if (explorer != null)
                        zombieCtrl.Init(explorer.transform, playerHealth);
                }
            }

            return (explorer != null ? explorer.transform : null, playerHealth);
        }

        private static void SetupHUD(PlayerHealth playerHealth)
        {
            EnsureEventSystem();

            // HUD canvas for health bar
            var hudGo = new GameObject("HUD");
            var hudCanvas = hudGo.AddComponent<Canvas>();
            hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            hudCanvas.sortingOrder = 10;
            hudGo.AddComponent<CanvasScaler>();
            hudGo.AddComponent<GraphicRaycaster>();

            var healthBar = HealthBarUI.Create(hudCanvas);

            var gameOverUI = GameOverUI.Create();

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += (cur, max) => healthBar.SetHealth(cur, max);
                playerHealth.OnDeath += () => gameOverUI.Show();
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;
            var es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static void AttachZombieAgent(GameObject wrapper, int index)
        {
            var agent = wrapper.AddComponent<UnityEngine.AI.NavMeshAgent>();
            agent.radius = 0.4f;
            agent.height = 1.8f;
            agent.avoidancePriority = 50 + index; // stagger priority to reduce clumping
        }

        private static void AttachPhysics(GameObject wrapper)
        {
            var col = wrapper.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.radius = 0.4f;
            col.center = new Vector3(0f, 0.9f, 0f);

            var rb = wrapper.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }

        private static void AttachAnimator(GameObject wrapper, string controllerPath)
        {
            var model = wrapper.transform.Find("Model");
            if (model == null) return;

            var animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.gameObject.AddComponent<Animator>();
            animator.applyRootMotion = false;

            var controller = Resources.Load<RuntimeAnimatorController>(controllerPath);
            if (controller != null)
                animator.runtimeAnimatorController = controller;
            else
                Debug.LogWarning($"[ThreeDee] AnimatorController not found at Resources/{controllerPath} — reimport the FBX first.");
        }

        private static void SetupCameraFollow(IsometricCamera isoCamera, Transform player)
        {
            if (player == null) return;

            // Cast a ray from the screen centre and shift the camera so it lands on the player.
            // This works regardless of the camera's isometric rotation angle.
            var cam = isoCamera.GetComponent<UnityEngine.Camera>();
            var ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float dist))
            {
                var currentLookAt = ray.GetPoint(dist);
                isoCamera.transform.position += player.position - currentLookAt;
            }

            var follower = isoCamera.gameObject.AddComponent<CameraFollower>();
            follower.Init(player);
        }

        private static GameObject SpawnModel(
            string resourcePath, string name, Vector3 gridPosition,
            Quaternion? modelRotation = null, string texturePath = null)
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

            // Default: fix Meshy FBX 90-degree X rotation. Pass Quaternion.identity to skip.
            instance.transform.localRotation = modelRotation ?? Quaternion.Euler(-90f, 0f, 0f);

            instance.SetActive(true);

            // Load texture — use explicit texturePath if provided, otherwise match resourcePath
            var texture = Resources.Load<Texture2D>(texturePath ?? resourcePath);
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
                orthographicSize: 7f
            );
        }
    }
}
