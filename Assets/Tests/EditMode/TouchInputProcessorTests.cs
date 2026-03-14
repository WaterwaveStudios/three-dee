using NUnit.Framework;
using UnityEngine;
using ThreeDee.Camera;

namespace ThreeDee.Tests.EditMode
{
    public class TouchInputProcessorTests
    {
        private TouchInputProcessor _processor;

        [SetUp]
        public void SetUp()
        {
            _processor = new TouchInputProcessor();
        }

        // --- Pinch Zoom ---

        [Test]
        public void CalculatePinchZoomDelta_FingersMovingApart_ReturnsPositive()
        {
            // Fingers spreading = zoom in = positive delta
            var prevTouch0 = new Vector2(100, 200);
            var prevTouch1 = new Vector2(200, 200);
            var curTouch0 = new Vector2(80, 200);
            var curTouch1 = new Vector2(220, 200);

            float delta = _processor.CalculatePinchZoomDelta(prevTouch0, prevTouch1, curTouch0, curTouch1);

            Assert.Greater(delta, 0f);
        }

        [Test]
        public void CalculatePinchZoomDelta_FingersMovingTogether_ReturnsNegative()
        {
            // Fingers pinching = zoom out = negative delta
            var prevTouch0 = new Vector2(80, 200);
            var prevTouch1 = new Vector2(220, 200);
            var curTouch0 = new Vector2(100, 200);
            var curTouch1 = new Vector2(200, 200);

            float delta = _processor.CalculatePinchZoomDelta(prevTouch0, prevTouch1, curTouch0, curTouch1);

            Assert.Less(delta, 0f);
        }

        [Test]
        public void CalculatePinchZoomDelta_NoMovement_ReturnsZero()
        {
            var touch0 = new Vector2(100, 200);
            var touch1 = new Vector2(200, 200);

            float delta = _processor.CalculatePinchZoomDelta(touch0, touch1, touch0, touch1);

            Assert.AreEqual(0f, delta, 0.001f);
        }

        [Test]
        public void ClampZoom_ClampsToMinMax()
        {
            Assert.AreEqual(5f, TouchInputProcessor.ClampZoom(3f, 5f, 30f));
            Assert.AreEqual(30f, TouchInputProcessor.ClampZoom(35f, 5f, 30f));
            Assert.AreEqual(15f, TouchInputProcessor.ClampZoom(15f, 5f, 30f));
        }

        // --- Two-Finger Pan ---

        [Test]
        public void CalculateTwoFingerPanDelta_ReturnsAverageDelta()
        {
            var prevTouch0 = new Vector2(100, 200);
            var prevTouch1 = new Vector2(200, 200);
            var curTouch0 = new Vector2(110, 210);
            var curTouch1 = new Vector2(210, 210);

            Vector2 delta = _processor.CalculateTwoFingerPanDelta(prevTouch0, prevTouch1, curTouch0, curTouch1);

            // Average movement: (10,10) + (10,10) / 2 = (10,10)
            Assert.AreEqual(10f, delta.x, 0.01f);
            Assert.AreEqual(10f, delta.y, 0.01f);
        }

        [Test]
        public void CalculateTwoFingerPanDelta_NoMovement_ReturnsZero()
        {
            var touch0 = new Vector2(100, 200);
            var touch1 = new Vector2(200, 200);

            Vector2 delta = _processor.CalculateTwoFingerPanDelta(touch0, touch1, touch0, touch1);

            Assert.AreEqual(0f, delta.x, 0.001f);
            Assert.AreEqual(0f, delta.y, 0.001f);
        }

        // --- Two-Finger Rotate ---

        [Test]
        public void CalculateRotationDelta_ClockwiseRotation_ReturnsPositive()
        {
            // Previous: horizontal line
            var prevTouch0 = new Vector2(100, 200);
            var prevTouch1 = new Vector2(200, 200);
            // Current: rotated clockwise (touch1 moved down)
            var curTouch0 = new Vector2(100, 200);
            var curTouch1 = new Vector2(200, 180);

            float delta = _processor.CalculateRotationDelta(prevTouch0, prevTouch1, curTouch0, curTouch1);

            // Angle changed, should be non-zero
            Assert.AreNotEqual(0f, delta);
        }

        [Test]
        public void CalculateRotationDelta_NoRotation_ReturnsZero()
        {
            var touch0 = new Vector2(100, 200);
            var touch1 = new Vector2(200, 200);

            float delta = _processor.CalculateRotationDelta(touch0, touch1, touch0, touch1);

            Assert.AreEqual(0f, delta, 0.001f);
        }

        [Test]
        public void CalculateRotationDelta_ParallelTranslation_ReturnsZero()
        {
            // Both fingers move the same direction — no rotation
            var prevTouch0 = new Vector2(100, 200);
            var prevTouch1 = new Vector2(200, 200);
            var curTouch0 = new Vector2(110, 200);
            var curTouch1 = new Vector2(210, 200);

            float delta = _processor.CalculateRotationDelta(prevTouch0, prevTouch1, curTouch0, curTouch1);

            Assert.AreEqual(0f, delta, 0.01f);
        }

        // --- Gesture Detection ---

        [Test]
        public void DetectGesture_LargePinchDelta_ReturnsZoom()
        {
            // Fingers spreading significantly
            var prevTouch0 = new Vector2(100, 200);
            var prevTouch1 = new Vector2(200, 200);
            var curTouch0 = new Vector2(50, 200);
            var curTouch1 = new Vector2(250, 200);

            var gesture = _processor.DetectGesture(prevTouch0, prevTouch1, curTouch0, curTouch1);

            Assert.AreEqual(TouchGesture.Zoom, gesture);
        }

        [Test]
        public void DetectGesture_ParallelMovement_ReturnsPan()
        {
            // Both fingers move same direction
            var prevTouch0 = new Vector2(100, 200);
            var prevTouch1 = new Vector2(200, 200);
            var curTouch0 = new Vector2(130, 230);
            var curTouch1 = new Vector2(230, 230);

            var gesture = _processor.DetectGesture(prevTouch0, prevTouch1, curTouch0, curTouch1);

            Assert.AreEqual(TouchGesture.Pan, gesture);
        }
    }
}
