# three-dee

3D strategy/base-building game prototype built in Unity 6 (URP), inspired by Last War. A testbed for exploring 3D mechanics, rendering, and asset pipelines — with 3D models generated via Meshy AI.

## Project Structure

```
Assets/
  Scenes/          # Unity scenes
  Scripts/         # C# game scripts
  Prefabs/         # Reusable game objects
  Materials/       # URP materials and shaders
  Models/          # 3D models (from Meshy AI or Blender)
    Units/         # Character/unit models
    Buildings/     # Building models
  Textures/        # Texture files
  UI/              # UI elements
Packages/          # Unity package manifest
ProjectSettings/   # Unity project settings
Blender/           # Blender source files (.blend)
```

## Tech Stack

- **Engine**: Unity 6 (6000.x)
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Language**: C#
- **3D Modelling**: Meshy AI (generate via web UI, download FBX to Assets/Models/)
- **Target**: Mobile (Last War-style strategy)

## Workflow

- Use `/game-dev` for all feature implementation, debugging, and game system architecture
- Follow TDD where possible: write tests first, then implement

## Development

### Build
```bash
# Open in Unity Hub (Windows), or compile-check from WSL:
bash scripts/unity-compile.sh /mnt/c/Users/awest/Aigames/three-dee
# Note: if Unity Editor is open, script detects the lockfile and tells you to press Ctrl+R instead
```

### Test
```bash
# Run EditMode tests
dotnet test
# Or via Unity Test Runner (Window > General > Test Runner)
```

## Code Style

- C# with Unity conventions
- PascalCase for public members, _camelCase for private fields
- MonoBehaviour components for game objects
- ScriptableObjects for data-driven configuration
- URP Shader Graph for custom materials

## Physics

- Player units use **Rigidbody + CapsuleCollider** (not CharacterController — it clips through colliders)
- Ground uses a single thick **BoxCollider**, not individual MeshColliders per grid cell
- FBX models from Meshy may include colliders — remove them before adding physics components
- Character rotation: Y-axis only via `Mathf.Atan2` — never use `LookRotation` for upright characters
- **Camera-relative movement**: project `cam.transform.forward` and `cam.transform.right` onto XZ plane (zero Y, normalize), then combine with input axes — makes W always look like "up" on screen regardless of camera angle
- **Border walls**: `GridManager.CreateBorderWalls()` spawns 4 invisible BoxCollider walls at map edges — call at end of `GenerateGrid()`
- Models in `Assets/Resources/Models/` for `Resources.Load` at runtime

## UI

- Use `UnityEngine.UI` (not TextMeshPro) — TMP requires a manual "Import TMP Essential Resources" step that can't be automated
- **Always add `using UnityEngine.UI;`** to any script using `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `Image`, `Text`, `Button` — even in non-UI scripts like `GameBootstrap`
- Screen-space HUD: `Canvas` with `RenderMode.ScreenSpaceOverlay`, `sortingOrder = 10`
- Game Over overlay: separate `Canvas` with `sortingOrder = 100` so it renders on top
- Call `EnsureEventSystem()` before creating any interactive UI — buttons need `EventSystem` + `StandaloneInputModule`
- Health bar: `Image` with `type = Image.Type.Filled`, `fillMethod = FillMethod.Horizontal`, update `fillAmount` (0–1)
- Fonts: `Font.CreateDynamicFontFromOSFont("Arial", size)` for built-in fonts
- Retry button: `SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex)` restarts cleanly

## Enemy AI

- Zombies use **NavMeshAgent** for pathfinding — no manual movement code needed (agent drives position)
- `ZombieController.Update()` calls `_agent.SetDestination(target.position)` each frame
- Rotation driven manually from `_agent.velocity` (same Y-axis Atan2 pattern as player)
- `agent.updateRotation = false` to prevent NavMeshAgent overriding rotation
- Stagger `agent.avoidancePriority` (e.g. `50 + index`) across zombies to reduce clumping
- **Build NavMesh before spawning agents** — call `NavMeshSetup.Build()` after all physics colliders (ground, buildings, walls) exist but before any NavMeshAgent is added
- `NavMeshSetup.Build()` uses `NavMeshBuilder.CollectSources(NavMeshCollectGeometry.PhysicsColliders)` — only objects with colliders are included in the bake

## Animation

- AnimatorController lives in `Assets/Resources/Animations/` — loaded at runtime via `Resources.Load<RuntimeAnimatorController>()`
- `MeshyMergedAnimationsPostprocessor` (in `Assets/Editor/`) auto-creates the controller from FBX clips on Reimport
- Always set `animator.applyRootMotion = false` when using manual transform-based movement (UnitController pattern)
- `UnitController` drives a single `IsMoving` bool parameter on the Animator

## Asset Pipeline

- **Primary**: Meshy AI for 3D model generation (text/image-to-3D)
  - Free tier: generate on meshy.ai web UI, download FBX
  - Pro tier ($10/mo annual): REST API + Unity plugin access
  - Unity plugin requires API key (Pro subscription)
- **Fallback**: Blender for manual modelling — source .blend files in `Blender/`
- Export/download as `.fbx` format into `Assets/Models/`
- Unit models go in `Assets/Models/Units/`
- Building models go in `Assets/Models/Buildings/`
- Materials use URP Lit shader or custom Shader Graph materials
- Textures: PNG or TGA, power-of-2 dimensions preferred for mobile
- **Meshy prompt style**: include "low-poly", "mobile game asset", "isometric game asset", "clean geometry", "stylized" for consistent art direction
- **Spawning models**: `GameBootstrap.SpawnModel(resourcePath, name, position, modelRotation?, texturePath?)` — pass `modelRotation: Quaternion.identity` for Merged Animations FBX (bakeAxisConversion handles axis flip); pass explicit `texturePath` when texture filename differs from FBX filename

## Key Concepts

- **Strategy/base-building**: Core gameplay loop around building and defending a base
- **3D rendering**: Experimenting with URP features — lighting, shadows, post-processing
- **AI-generated assets**: Using Meshy AI for 3D model generation; Tripo AI as backup for higher volume needs
