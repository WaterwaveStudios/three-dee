using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace ThreeDee.Tests.EditMode
{
    public class AnimationClipTests
    {
        private const string FbxPath = "Assets/Resources/Models/Units/Meshy_AI_Meshy_Merged_Animations.fbx";

        private AnimationClip[] GetClips()
        {
            return AssetDatabase.LoadAllAssetsAtPath(FbxPath)
                .OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__"))
                .ToArray();
        }

        [Test]
        public void MeshyFbx_HasAnimationClips()
        {
            var clips = GetClips();
            Assert.Greater(clips.Length, 0, "No animation clips found in FBX — check import settings");
        }

        [Test]
        public void WalkClip_Exists()
        {
            var clips = GetClips();
            var walk = clips.FirstOrDefault(c =>
                c.name.ToLower().Contains("walk") || c.name.ToLower().Contains("run"));

            Assert.IsNotNull(walk,
                $"No walk/run clip found. Available clips: {string.Join(", ", clips.Select(c => c.name))}");
        }

        [Test]
        public void WalkClip_IsLooping()
        {
            var clips = GetClips();
            var walk = clips.FirstOrDefault(c =>
                c.name.ToLower().Contains("walk") || c.name.ToLower().Contains("run"));

            Assume.That(walk, Is.Not.Null, "Walk clip not found — skipping loop test");

            var settings = AnimationUtility.GetAnimationClipSettings(walk);
            Assert.IsTrue(settings.loopTime,
                $"Clip '{walk.name}' is not set to loop. Reimport the FBX to trigger the postprocessor.");
        }

        [Test]
        public void IdleClip_IsLooping()
        {
            var clips = GetClips();
            var idle = clips.FirstOrDefault(c => c.name.ToLower().Contains("idle"));

            Assume.That(idle, Is.Not.Null, "Idle clip not found — skipping loop test");

            var settings = AnimationUtility.GetAnimationClipSettings(idle);
            Assert.IsTrue(settings.loopTime,
                $"Clip '{idle.name}' is not set to loop. Reimport the FBX to trigger the postprocessor.");
        }
    }
}
