using UnityEngine;

namespace ThreeDee.Core
{
    public static class MaterialHelper
    {
        public static Material CreateLit(Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard");
            var material = new Material(shader);
            material.color = color;
            return material;
        }
    }
}
