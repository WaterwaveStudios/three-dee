using NUnit.Framework;
using UnityEngine;
using ThreeDee.Camera;

namespace ThreeDee.Tests.EditMode
{
    public class IsometricCameraTests
    {
        [Test]
        public void Create_SetsOrthographicProjection()
        {
            var isoCamera = IsometricCamera.Create(Vector3.zero, 15f);
            var cam = isoCamera.GetComponent<UnityEngine.Camera>();

            Assert.IsTrue(cam.orthographic);
            Assert.AreEqual(15f, cam.orthographicSize, 0.01f);

            Object.DestroyImmediate(isoCamera.gameObject);
        }

        [Test]
        public void Create_SetsIsometricRotation()
        {
            var isoCamera = IsometricCamera.Create(Vector3.zero);

            // Default rotation: 30 X, 45 Y
            Vector3 euler = isoCamera.transform.eulerAngles;
            Assert.AreEqual(30f, euler.x, 0.5f);
            Assert.AreEqual(45f, euler.y, 0.5f);

            Object.DestroyImmediate(isoCamera.gameObject);
        }

        [Test]
        public void Create_SetsPosition()
        {
            var pos = new Vector3(5f, 20f, -10f);
            var isoCamera = IsometricCamera.Create(pos);

            Assert.AreEqual(pos, isoCamera.transform.position);

            Object.DestroyImmediate(isoCamera.gameObject);
        }

        [Test]
        public void ZoomLevel_ReturnsOrthographicSize()
        {
            var isoCamera = IsometricCamera.Create(Vector3.zero, 12f);

            Assert.AreEqual(12f, isoCamera.ZoomLevel, 0.01f);

            Object.DestroyImmediate(isoCamera.gameObject);
        }
    }
}
