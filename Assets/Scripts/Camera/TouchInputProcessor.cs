using UnityEngine;

namespace ThreeDee.Camera
{
    public enum TouchGesture
    {
        None,
        Pan,
        Zoom,
        Rotate
    }

    public class TouchInputProcessor
    {
        private const float GestureThreshold = 5f;

        public float CalculatePinchZoomDelta(Vector2 prevTouch0, Vector2 prevTouch1, Vector2 curTouch0, Vector2 curTouch1)
        {
            float prevDistance = Vector2.Distance(prevTouch0, prevTouch1);
            float curDistance = Vector2.Distance(curTouch0, curTouch1);
            return curDistance - prevDistance;
        }

        public static float ClampZoom(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public Vector2 CalculateTwoFingerPanDelta(Vector2 prevTouch0, Vector2 prevTouch1, Vector2 curTouch0, Vector2 curTouch1)
        {
            Vector2 prevMidpoint = (prevTouch0 + prevTouch1) * 0.5f;
            Vector2 curMidpoint = (curTouch0 + curTouch1) * 0.5f;
            return curMidpoint - prevMidpoint;
        }

        public float CalculateRotationDelta(Vector2 prevTouch0, Vector2 prevTouch1, Vector2 curTouch0, Vector2 curTouch1)
        {
            Vector2 prevDir = prevTouch1 - prevTouch0;
            Vector2 curDir = curTouch1 - curTouch0;

            if (prevDir.sqrMagnitude < 0.001f || curDir.sqrMagnitude < 0.001f)
                return 0f;

            float prevAngle = Mathf.Atan2(prevDir.y, prevDir.x) * Mathf.Rad2Deg;
            float curAngle = Mathf.Atan2(curDir.y, curDir.x) * Mathf.Rad2Deg;
            return Mathf.DeltaAngle(prevAngle, curAngle);
        }

        public TouchGesture DetectGesture(Vector2 prevTouch0, Vector2 prevTouch1, Vector2 curTouch0, Vector2 curTouch1)
        {
            float pinchDelta = Mathf.Abs(CalculatePinchZoomDelta(prevTouch0, prevTouch1, curTouch0, curTouch1));
            Vector2 panDelta = CalculateTwoFingerPanDelta(prevTouch0, prevTouch1, curTouch0, curTouch1);
            float rotationDelta = Mathf.Abs(CalculateRotationDelta(prevTouch0, prevTouch1, curTouch0, curTouch1));

            float panMagnitude = panDelta.magnitude;

            if (pinchDelta < GestureThreshold && panMagnitude < GestureThreshold && rotationDelta < 2f)
                return TouchGesture.None;

            // Dominant gesture wins
            if (pinchDelta > panMagnitude && pinchDelta > rotationDelta)
                return TouchGesture.Zoom;

            if (rotationDelta > panMagnitude && rotationDelta > 5f)
                return TouchGesture.Rotate;

            return TouchGesture.Pan;
        }
    }
}
