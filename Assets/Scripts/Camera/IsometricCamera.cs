using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ThreeDee.Core;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace ThreeDee.Camera
{
    public class IsometricCamera : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float _panSpeed = 20f;

        [Header("Zoom")]
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 30f;
        [SerializeField] private float _trackpadZoomSpeed = 10f;

        [Header("Rotation")]
        [SerializeField] private float _rotationAngleY = 45f;
        [SerializeField] private float _rotationAngleX = 30f;

        [Header("Touch")]
        [SerializeField] private float _touchZoomSensitivity = 0.05f;
        [SerializeField] private float _touchPanSensitivity = 0.02f;
        [SerializeField] private float _touchRotateSensitivity = 0.5f;

        private UnityEngine.Camera _camera;
        private bool _wasPanning;

        private readonly TouchInputProcessor _touchProcessor = new TouchInputProcessor();
        private Vector2 _prevTouch0;
        private Vector2 _prevTouch1;
        private bool _isTouching;

        private UnityEngine.Camera Camera => _camera ??= GetComponent<UnityEngine.Camera>();
        public float ZoomLevel => Camera != null ? Camera.orthographicSize : 0f;
        public float PanSpeed { get => _panSpeed; set => _panSpeed = value; }
        public float MinZoom => _minZoom;
        public float MaxZoom => _maxZoom;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        private void Update()
        {
            if (Touch.activeTouches.Count >= 2)
            {
                HandleTouchInput();
            }
            else
            {
                _isTouching = false;
                HandleDesktopPan();
                HandleDesktopZoom();
            }
        }

        // --- Touch (mobile) ---

        private void HandleTouchInput()
        {
            var touch0 = Touch.activeTouches[0];
            var touch1 = Touch.activeTouches[1];

            if (!_isTouching)
            {
                _isTouching = true;
                _prevTouch0 = touch0.screenPosition;
                _prevTouch1 = touch1.screenPosition;
                return;
            }

            var gesture = _touchProcessor.DetectGesture(
                _prevTouch0, _prevTouch1,
                touch0.screenPosition, touch1.screenPosition);

            switch (gesture)
            {
                case TouchGesture.Zoom:
                    HandleTouchZoom(touch0.screenPosition, touch1.screenPosition);
                    break;
                case TouchGesture.Pan:
                    HandleTouchPan(touch0.screenPosition, touch1.screenPosition);
                    break;
                case TouchGesture.Rotate:
                    HandleTouchRotate(touch0.screenPosition, touch1.screenPosition);
                    break;
            }

            _prevTouch0 = touch0.screenPosition;
            _prevTouch1 = touch1.screenPosition;
        }

        private void HandleTouchZoom(Vector2 curTouch0, Vector2 curTouch1)
        {
            float pinchDelta = _touchProcessor.CalculatePinchZoomDelta(
                _prevTouch0, _prevTouch1, curTouch0, curTouch1);
            Camera.orthographicSize = TouchInputProcessor.ClampZoom(
                Camera.orthographicSize - pinchDelta * _touchZoomSensitivity,
                _minZoom, _maxZoom);
        }

        private void HandleTouchPan(Vector2 curTouch0, Vector2 curTouch1)
        {
            Vector2 panDelta = _touchProcessor.CalculateTwoFingerPanDelta(
                _prevTouch0, _prevTouch1, curTouch0, curTouch1);
            ApplyPan(-panDelta.x * _touchPanSensitivity, -panDelta.y * _touchPanSensitivity);
        }

        private void HandleTouchRotate(Vector2 curTouch0, Vector2 curTouch1)
        {
            float rotationDelta = _touchProcessor.CalculateRotationDelta(
                _prevTouch0, _prevTouch1, curTouch0, curTouch1);
            ApplyRotation(-rotationDelta * _touchRotateSensitivity);
        }

        // --- Desktop (mouse/keyboard/trackpad) ---

        private void HandleDesktopPan()
        {
            var input = GameInput.Instance;
            if (input == null) return;

            if (input.IsPanning)
            {
                Vector2 delta = input.PointerDelta;
                float scale = _panSpeed * Time.deltaTime * 0.1f;
                ApplyPan(-delta.x * scale, -delta.y * scale);
            }
        }

        private void HandleDesktopZoom()
        {
            if (Camera == null) return;
            var input = GameInput.Instance;
            if (input == null) return;

            float scroll = input.ScrollInput;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // Normalise — Input System scroll values are in pixels (120 per notch)
                float normalised = scroll / 120f;
                float speed = Mathf.Abs(normalised) < 1f ? _trackpadZoomSpeed : _zoomSpeed;
                Camera.orthographicSize = TouchInputProcessor.ClampZoom(
                    Camera.orthographicSize - normalised * speed * Time.deltaTime,
                    _minZoom, _maxZoom);
            }
        }

        // --- Shared movement helpers ---

        private void ApplyPan(float rightAmount, float forwardAmount)
        {
            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = transform.right;
            right.y = 0f;
            right.Normalize();

            transform.position += right * rightAmount + forward * forwardAmount;
        }

        private void ApplyRotation(float degrees)
        {
            transform.RotateAround(GetPivotPoint(), Vector3.up, degrees);
        }

        private Vector3 GetPivotPoint()
        {
            Ray ray = Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
                return ray.GetPoint(distance);
            return transform.position + transform.forward * 20f;
        }

        public void SetIsometricView()
        {
            transform.rotation = Quaternion.Euler(_rotationAngleX, _rotationAngleY, 0f);
        }

        public static IsometricCamera Create(Vector3 position, float orthographicSize = 15f)
        {
            var go = new GameObject("IsometricCamera");
            var cam = go.AddComponent<UnityEngine.Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.3f, 0.4f);

            var isoCamera = go.AddComponent<IsometricCamera>();
            go.transform.position = position;
            isoCamera.SetIsometricView();

            go.AddComponent<AudioListener>();

            return isoCamera;
        }
    }
}
