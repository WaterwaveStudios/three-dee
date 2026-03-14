using NUnit.Framework;
using UnityEngine;
using ThreeDee.Units;

namespace ThreeDee.Tests.EditMode
{
    public class UnitControllerTests
    {
        [Test]
        public void CalculateMoveVector_CardinalDirection_ReturnsNormalized()
        {
            Vector3 result = UnitController.CalculateMoveVector(1f, 0f);

            Assert.AreEqual(1f, result.x, 0.01f);
            Assert.AreEqual(0f, result.y, 0.01f);
            Assert.AreEqual(0f, result.z, 0.01f);
        }

        [Test]
        public void CalculateMoveVector_Diagonal_IsNormalized()
        {
            Vector3 result = UnitController.CalculateMoveVector(1f, 1f);

            Assert.AreEqual(1f, result.magnitude, 0.01f);
        }

        [Test]
        public void CalculateMoveVector_NoInput_ReturnsZero()
        {
            Vector3 result = UnitController.CalculateMoveVector(0f, 0f);

            Assert.AreEqual(Vector3.zero, result);
        }

        [Test]
        public void CalculateMoveVector_NegativeInput_Works()
        {
            Vector3 result = UnitController.CalculateMoveVector(-1f, 0f);

            Assert.AreEqual(-1f, result.x, 0.01f);
            Assert.AreEqual(0f, result.z, 0.01f);
        }

        [Test]
        public void Init_SetsPosition()
        {
            var go = new GameObject("TestUnit");
            var unit = go.AddComponent<UnitController>();
            var pos = new Vector3(5f, 0f, 3f);

            unit.Init(pos);

            Assert.AreEqual(5f, unit.transform.position.x, 0.01f);
            Assert.AreEqual(3f, unit.transform.position.z, 0.01f);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void IsMoving_FalseByDefault()
        {
            var go = new GameObject("TestUnit");
            var unit = go.AddComponent<UnitController>();

            Assert.IsFalse(unit.IsMoving);

            Object.DestroyImmediate(go);
        }
    }

    public class CameraFollowTests
    {
        [Test]
        public void CalculateFollowPosition_MaintainsOffset()
        {
            var offset = new Vector3(0f, 20f, -15f);
            var targetPos = new Vector3(5f, 0f, 3f);

            Vector3 result = CameraFollower.CalculateFollowPosition(targetPos, offset);

            Assert.AreEqual(5f, result.x, 0.01f);
            Assert.AreEqual(20f, result.y, 0.01f);
            Assert.AreEqual(-12f, result.z, 0.01f);
        }

        [Test]
        public void CalculateFollowPosition_AtOrigin_ReturnsOffset()
        {
            var offset = new Vector3(0f, 20f, -15f);

            Vector3 result = CameraFollower.CalculateFollowPosition(Vector3.zero, offset);

            Assert.AreEqual(offset, result);
        }

        [Test]
        public void SmoothFollow_MovesTowardsTarget()
        {
            var current = Vector3.zero;
            var target = new Vector3(10f, 20f, -15f);

            Vector3 result = CameraFollower.SmoothFollow(current, target, 5f, 0.1f);

            Assert.Greater(result.x, 0f);
            Assert.Less(result.x, 10f);
        }
    }
}
