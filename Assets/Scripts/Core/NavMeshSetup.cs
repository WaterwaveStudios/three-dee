using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ThreeDee.Core
{
    /// <summary>
    /// Builds a NavMesh at runtime from the scene's physics colliders.
    /// Call Build() after all ground and buildings are spawned, before spawning agents.
    /// </summary>
    public static class NavMeshSetup
    {
        public static void Build(float totalWidth, float totalDepth)
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();
            var bounds = new Bounds(Vector3.zero, new Vector3(totalWidth + 4f, 8f, totalDepth + 4f));

            NavMeshBuilder.CollectSources(
                bounds, ~0,
                NavMeshCollectGeometry.PhysicsColliders,
                0, markups, sources);

            var settings = NavMesh.GetSettingsByID(0);
            var data = NavMeshBuilder.BuildNavMeshData(
                settings, sources, bounds, Vector3.zero, Quaternion.identity);

            NavMesh.AddNavMeshData(data);
            Debug.Log($"[NavMesh] Built with {sources.Count} sources.");
        }
    }
}
