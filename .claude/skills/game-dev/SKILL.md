---
name: game-dev
description: Unity game development with C#. Covers feature implementation (code + tests + compile check), debugging Unity issues (blank screens, missing components, compile errors), and game system architecture (state machines, scoring, tile/grid management). Invoke explicitly with /game-dev.
---

# Unity Game Development

## When to Use This Skill

Invoke explicitly with `/game-dev` for Unity + C# game development tasks:
- Implementing new game features end-to-end
- Debugging Unity runtime issues (blank screens, UI not responding, compile errors)
- Designing game systems (state machines, scoring, grid/tile management)
- Setting up Unity project structure and test infrastructure

## Feature Implementation Workflow

Every new feature follows this sequence. Do not skip steps.

### 1. Write Tests First

Write EditMode tests for pure logic (no MonoBehaviour dependency) and PlayMode tests for runtime behaviour:

- **EditMode**: Match detection, scoring, state transitions, utility functions
- **PlayMode**: Board lifecycle, component creation, integration tests

Test assemblies require assembly definitions. Use templates from `assets/`:
- `assets/asmdef-game.json` — game scripts assembly (replace `GAME_NAME` and `GAME_NAMESPACE`)
- `assets/asmdef-editmode-tests.json` — EditMode test assembly
- `assets/asmdef-playmode-tests.json` — PlayMode test assembly

To test MonoBehaviour logic in EditMode, create real GameObjects:
```csharp
var go = new GameObject("TestTile");
go.AddComponent<SpriteRenderer>();
var tile = go.AddComponent<Tile>();
// Test tile behaviour, destroy in TearDown with Object.DestroyImmediate
```

### 2. Implement Feature

Write the minimum code to pass tests. Follow these Unity-specific patterns:

**Runtime bootstrap pattern** — Create game objects at runtime via `[RuntimeInitializeOnLoadMethod]` instead of configuring scenes manually. Use reflection to set `[SerializeField]` fields:
```csharp
var field = target.GetType().GetField(fieldName,
    BindingFlags.NonPublic | BindingFlags.Instance);
field.SetValue(target, value);
```

**State machine for game flow** — Use a `GameManager` singleton with enum states (Menu, Playing, GameOver). Show/hide UI panels per state. Avoid multi-scene architecture for simple games.

**Procedural sprites** — Generate textures at runtime with `Texture2D` + `Sprite.Create()` for prototyping. Avoids asset pipeline dependency.

### 3. Compile and Test

Always verify before committing. Use the bundled scripts:

```bash
# Compile check
scripts/unity-compile.sh /path/to/project

# Run EditMode tests
scripts/unity-test.sh /path/to/project EditMode

# Run PlayMode tests
scripts/unity-test.sh /path/to/project PlayMode
```

Or manually:
```bash
rm -f Temp/UnityLockfile
/Applications/Unity/Hub/Editor/6000.3.11f1/Unity.app/Contents/MacOS/Unity \
  -runTests -projectPath . -testPlatform EditMode \
  -testResults /tmp/results.xml -batchmode -logFile /tmp/test.log
```

## Debugging Unity Issues

Read `references/unity-gotchas.md` for the full list. The most common issues:

### Blank Screen
1. **Inactive prefab clones** — `Instantiate` copies `activeSelf`. Add `go.SetActive(true)` after Instantiate if the template was deactivated.
2. **Missing camera** — Ensure a Camera tagged `MainCamera` exists. Bootstrap should create one as fallback.
3. **Script not running** — Check Unity console for compile errors. Scripts with errors are silently skipped.
4. **TMP not imported** — TextMeshPro requires "Import TMP Essential Resources" dialog. Use `UnityEngine.UI.Text` and `TextMesh` instead.

### UI Buttons Not Responding
- Missing `EventSystem` + `StandaloneInputModule`. Create one in bootstrap before any UI.
- Missing `GraphicRaycaster` on the Canvas.

### Compile Errors from CLI but Not Editor
- Stale `Temp/UnityLockfile` — remove it before batch mode.
- Editor caches old errors — restart Unity or `Assets > Refresh` (Cmd+R).

### Assembly Reference Errors in Tests
- Test `.asmdef` files need to reference the game assembly by name.
- Game scripts must have their own `.asmdef` — test assemblies cannot reference the default `Assembly-CSharp`.

## Game Architecture Patterns

### Grid-Based Games (Match-3, Puzzle)
- Store grid as `Tile[rows, cols]` — row 0 at bottom
- Keep match detection as a **static pure function** (`MatchFinder.FindAllMatches(grid, rows, cols)`) for easy testing
- Board fill: place tiles left-to-right, bottom-to-top, avoiding 3-matches by checking the two tiles to the left and two tiles below
- Match-clear-collapse-refill loop runs as a coroutine chain

### Scoring
- Separate `ScoreManager` (plain C# class, not MonoBehaviour) for testability
- Chain multiplier: increment per cascade step, reset at start of each swap
- Expose `CalculatePoints(matchSize)` separately from `AddMatchScore()` so callers can preview points (e.g., for popups)

### Isometric Strategy / Base Builder
- Use runtime bootstrap pattern — `[RuntimeInitializeOnLoadMethod]` creates the entire scene
- Grid as visual only (checkerboard Plane primitives) with a single thick BoxCollider for ground
- Remove MeshColliders from visual grid cells to prevent gaps
- Player units: `Rigidbody` + `CapsuleCollider` (NOT CharacterController — clips through)
- Strip imported FBX colliders before adding your own physics components
- Camera follow: `CameraFollower` component with offset, smooth lerp in `LateUpdate`
- Y-axis rotation only for upright characters: `Mathf.Atan2(dir.x, dir.z)` → `Quaternion.Euler(0, angle, 0)`
- Meshy AI models: load FBX + matching texture via `Resources.Load`, create URP Lit material at runtime
- `MeshyTextureImporter` (AssetPostprocessor) auto-configures import quality for model textures

### UI
- `UIHelper` static class for runtime UI creation (Canvas, Text, Button)
- Always call `EnsureEventSystem()` before creating interactive UI
- Use `Font.CreateDynamicFontFromOSFont("Arial", size)` for built-in fonts
- Score popups: world-space `TextMesh` with rise + fade coroutine, `MeshRenderer.sortingOrder = 100`

## Code Style

- PascalCase for public members, `_camelCase` for private fields
- One class per file, namespaced by feature area
- `[SerializeField]` for inspector-exposed private fields
- Thin MonoBehaviours — delegate logic to plain C# classes
- Composition over inheritance for game objects

## Project Structure

```
Assets/
  Scripts/
    {GameName}.asmdef       # Assembly definition for game code
    Board/                  # Grid management, spawning, game loop
    Core/                   # GameManager, UIHelper, shared utilities
    Tiles/                  # Tile types, data, visual representation
    Matching/               # Match detection (pure logic)
    Scoring/                # Score tracking (plain C# class)
    UI/                     # HUD, popups, display components
  Tests/
    EditMode/
      EditModeTests.asmdef  # References game assembly + test runner
      *Tests.cs             # Pure logic tests
    PlayMode/
      PlayModeTests.asmdef  # References game assembly + test runner
      *Tests.cs             # Runtime/lifecycle tests
  Scenes/
    Game.unity              # Minimal scene (camera only)
```

## Resources

### scripts/
- `unity-compile.sh` — Batch mode compile check with error reporting
- `unity-test.sh` — Run EditMode/PlayMode tests with result parsing

### references/
- `unity-gotchas.md` — Common Unity pitfalls and solutions discovered during development

### assets/
- `asmdef-game.json` — Template assembly definition for game scripts
- `asmdef-editmode-tests.json` — Template for EditMode test assembly
- `asmdef-playmode-tests.json` — Template for PlayMode test assembly
