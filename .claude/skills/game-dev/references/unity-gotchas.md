# Unity Gotchas

## Runtime Bootstrap Pattern

When building UI and game objects entirely at runtime (no scene configuration):

- `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]` runs after the scene loads — use for bootstrapping
- Scene only needs a Camera tagged `MainCamera`
- Use reflection to set `[SerializeField]` fields on runtime-created MonoBehaviours:
  ```csharp
  var field = target.GetType().GetField(fieldName,
      BindingFlags.NonPublic | BindingFlags.Instance);
  field.SetValue(target, value);
  ```

## Instantiate Preserves Active State

`Object.Instantiate()` copies `activeSelf` from the source. If the template is inactive, clones are invisible:
```csharp
GameObject go = Instantiate(prefab, pos, Quaternion.identity);
go.SetActive(true); // Required if prefab was SetActive(false)
```

## TextMeshPro Requires Manual Import

Adding `com.unity.textmeshpro` to `manifest.json` is NOT enough. TMP needs "Import TMP Essential Resources" via an editor dialog. Without it, TMP silently fails at runtime.

**Use instead**: `UnityEngine.UI.Text` (screen space), `TextMesh` (world space).

## UI Buttons Need EventSystem

Unity UI buttons require `EventSystem` + `StandaloneInputModule` to receive clicks:
```csharp
if (EventSystem.current == null)
{
    var go = new GameObject("EventSystem");
    go.AddComponent<EventSystem>();
    go.AddComponent<StandaloneInputModule>();
}
```

## Unity Lock File

Batch mode CLI commands fail if the editor has the project open. Remove lock file first:
```bash
rm -f Temp/UnityLockfile
```

## Assembly Definitions

- Test assemblies (`.asmdef`) cannot reference the default `Assembly-CSharp`
- Game scripts need their own `.asmdef` so test assemblies can reference them
- Test asmdefs need `"overrideReferences": true` and `"precompiledReferences": ["nunit.framework.dll"]`

## Scene Files

Hand-written Unity scene YAML is fragile — script GUIDs won't match. Prefer minimal scenes + runtime bootstrap over complex scene configurations.

## Editor Caching

Unity editor sometimes caches old compilation errors after file changes. Fix: close and reopen the project from Unity Hub, or `Assets > Refresh` (Cmd+R).

## Object.Destroy vs DestroyImmediate

`Object.Destroy()` is deferred — it does not run immediately. In EditMode tests, this causes errors:
```
Destroy may not be called from edit mode! Use DestroyImmediate instead.
```
Use `Application.isPlaying` to choose:
```csharp
if (Application.isPlaying)
    Object.Destroy(obj);
else
    Object.DestroyImmediate(obj);
```

## URP Pipeline Setup

Adding `com.unity.render-pipelines.universal` to `manifest.json` is NOT enough. You must also:
1. Create a `UniversalRendererData` asset
2. Create a `UniversalRenderPipelineAsset` referencing the renderer
3. Assign it to `GraphicsSettings.defaultRenderPipeline`
4. Assign it to every quality level in `QualitySettings.renderPipeline`

Without this, all materials render pink. Use `Shader.Find("Universal Render Pipeline/Lit")` with a fallback to `"Standard"`.

## CharacterController vs Rigidbody for Player Movement

**CharacterController** clips through thin colliders (even BoxColliders). Prefer **Rigidbody + CapsuleCollider** for player units:
- `RigidbodyConstraints.FreezeRotation` to prevent tumbling
- `CollisionDetectionMode.Continuous` to prevent clipping
- Set velocity directly in `FixedUpdate`, preserving `velocity.y` for gravity
- Remove any colliders imported with FBX models — they conflict with the added physics

## Rotation for Upright Characters

`Quaternion.LookRotation(direction)` rotates on ALL axes — this flips models onto their stomach. For characters that should stay upright, use Y-axis rotation only:
```csharp
float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
transform.rotation = Quaternion.Euler(0f, angle, 0f);
```

## Ground Colliders for Physics

Many small MeshColliders (from Plane primitives) have gaps between them. CharacterControllers and Rigidbodies fall through. Use a single large BoxCollider as the ground:
```csharp
var box = ground.AddComponent<BoxCollider>();
box.size = new Vector3(totalWidth, 1f, totalDepth); // thick enough
```
Remove individual MeshColliders from visual grid cells.

## Meshy AI FBX Models

Meshy exports FBX with a separate texture PNG (same filename). At runtime:
1. Load with `Resources.Load<GameObject>(path)` for FBX
2. Load with `Resources.Load<Texture2D>(path)` for texture
3. Create a URP Lit material and assign the texture
4. FBX models may include colliders — remove them before adding your own physics

## macOS Trackpad Input

`Input.GetAxis("Mouse ScrollWheel")` can behave unexpectedly on macOS trackpads with natural scrolling. Use `Input.mouseScrollDelta.y` instead for reliable bidirectional zoom. Scale with `Time.deltaTime` for smooth trackpad scrolling.

## Editor Scripts Need Assembly Definitions

Editor scripts that reference URP types (e.g., `UnityEngine.Rendering.Universal`) need their own `.asmdef` with `"references": ["Unity.RenderPipelines.Universal.Runtime"]` and `"includePlatforms": ["Editor"]`.
