using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// On import of zombie.fbx:
///   1. Applies the zombie texture to all materials.
///   2. Sets walk/idle clips to loop.
///   3. Creates an AnimatorController with a walk state driven by IsMoving.
/// </summary>
public class ZombiePostprocessor : AssetPostprocessor
{
    private const string FbxPath = "Assets/Resources/Models/Units/zombie.fbx";
    private const string TexturePath = "Assets/Resources/Models/Units/zombie texutre.png";
    private const string ControllerPath = "Assets/Resources/Animations/ZombieAnimations.controller";

    // --- Texture ---

    void OnPostprocessMaterial(Material material)
    {
        if (assetPath != FbxPath) return;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture == null)
        {
            Debug.LogWarning($"[ZombiePostprocessor] Texture not found at: {TexturePath}");
            return;
        }

        material.SetTexture("_BaseMap", texture);
        Debug.Log($"[ZombiePostprocessor] Applied texture to '{material.name}'");
    }

    // --- Loop clips ---

    void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        if (assetPath != FbxPath) return;

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        Debug.Log($"[ZombiePostprocessor] Set loop on clip '{clip.name}'");
    }

    // --- AnimatorController ---

    static void OnPostprocessAllAssets(
        string[] importedAssets, string[] deletedAssets,
        string[] movedAssets, string[] movedFromPaths)
    {
        if (!importedAssets.Contains(FbxPath)) return;

        var clips = AssetDatabase.LoadAllAssetsAtPath(FbxPath)
            .OfType<AnimationClip>()
            .Where(c => !c.name.StartsWith("__preview__"))
            .ToArray();

        if (clips.Length == 0)
        {
            Debug.LogWarning("[ZombiePostprocessor] No animation clips found in zombie FBX.");
            return;
        }

        Debug.Log($"[ZombiePostprocessor] Found clips: {string.Join(", ", clips.Select(c => c.name))}");
        CreateAnimatorController(clips);
    }

    static void CreateAnimatorController(AnimationClip[] clips)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));

        if (File.Exists(ControllerPath))
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        var sm = controller.layers[0].stateMachine;

        // Prefer "unsteady" clip for walk; fall back to any walk/run clip, then first clip
        var walkClip = clips.FirstOrDefault(c => c.name.ToLower().Contains("unsteady"))
                    ?? clips.FirstOrDefault(c => c.name.ToLower().Contains("walk") || c.name.ToLower().Contains("run"))
                    ?? clips[0];

        var idleClip = clips.FirstOrDefault(c => c.name.ToLower().Contains("idle") && c != walkClip)
                    ?? (clips.Length > 1 ? clips.First(c => c != walkClip) : walkClip);

        var idleState = sm.AddState("Idle");
        idleState.motion = idleClip;
        sm.defaultState = idleState;

        var walkState = sm.AddState("Walk");
        walkState.motion = walkClip;

        var toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "IsMoving");
        toWalk.hasExitTime = false;
        toWalk.duration = 0.1f;

        var toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsMoving");
        toIdle.hasExitTime = false;
        toIdle.duration = 0.1f;

        AssetDatabase.SaveAssets();
        Debug.Log($"[ZombiePostprocessor] AnimatorController saved to {ControllerPath}");
    }
}
