using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// On import of the Meshy Merged Animations FBX:
///   1. Applies the texture to all materials.
///   2. Creates an AnimatorController asset from the embedded animation clips.
/// </summary>
public class MeshyMergedAnimationsPostprocessor : AssetPostprocessor
{
    private const string FbxPath = "Assets/Resources/Models/Units/Meshy_AI_Meshy_Merged_Animations.fbx";
    private const string TexturePath = "Assets/Resources/Models/Units/Meshy_AI_texture_0 1.png";
    private const string ControllerPath = "Assets/Resources/Animations/MeshyMergedAnimations.controller";

    // --- Texture ---

    void OnPostprocessMaterial(Material material)
    {
        if (assetPath != FbxPath) return;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture == null)
        {
            Debug.LogWarning($"[MeshyPostprocessor] Texture not found at: {TexturePath}");
            return;
        }

        material.SetTexture("_BaseMap", texture);
        Debug.Log($"[MeshyPostprocessor] Applied texture to material '{material.name}'");
    }

    // --- Animation Clip Settings ---

    void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        if (assetPath != FbxPath) return;

        bool isLooping = clip.name.ToLower().Contains("walk")
                      || clip.name.ToLower().Contains("run")
                      || clip.name.ToLower().Contains("idle");

        if (!isLooping) return;

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        Debug.Log($"[MeshyPostprocessor] Set loop on clip '{clip.name}'");
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
            Debug.LogWarning("[MeshyPostprocessor] No animation clips found in FBX — skipping controller creation.");
            return;
        }

        Debug.Log($"[MeshyPostprocessor] Found clips: {string.Join(", ", clips.Select(c => c.name))}");
        CreateAnimatorController(clips);
    }

    static void CreateAnimatorController(AnimationClip[] clips)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ControllerPath));

        // Delete stale controller so we get a clean rebuild
        if (File.Exists(ControllerPath))
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);

        var sm = controller.layers[0].stateMachine;

        var idleClip = clips.FirstOrDefault(c => c.name.ToLower().Contains("idle")) ?? clips[0];
        var walkClip = clips.FirstOrDefault(c =>
            c.name.ToLower().Contains("walk") || c.name.ToLower().Contains("run"));

        var idleState = sm.AddState("Idle");
        idleState.motion = idleClip;
        sm.defaultState = idleState;

        if (walkClip != null)
        {
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
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[MeshyPostprocessor] AnimatorController saved to {ControllerPath}");
    }
}
