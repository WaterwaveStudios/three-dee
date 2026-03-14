using UnityEngine;

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
        [SerializeField] private float _keyboardRotateSpeed = 90f;

        [Header("Touch")]
        [SerializeField] private float _touchZoomSensitivity = 0.05f;
        [SerializeField] private float _touchPanSensitivity = 0.02f;
        [SerializeField] private float _touchRotateSensitivity = 0.5f;

        private UnityEngine.Camera _camera;
        private Vector3 _lastMousePosition;
        private bool _isPanning;

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
            if (Input.touchCount >= 2)
            {
                HandleTouchInput();
            }
            else
            {
                _isTouching = false;
                HandleDesktopPan();
                HandleDesktopZoom();
                HandleDesktopRotate();
            }
        }

        // --- Touch (mobile) ---

        private void HandleTouchInput()
        {
            var touch0 = Input.GetTouch(0);
            var touch1 = Input.GetTouch(1);

            if (!_isTouching)
            {
                _isTouching = true;
                _prevTouch0 = touch0.position;
                _prevTouch1 = touch1.position;
                return;
            }

            var gesture = _touchProcessor.DetectGesture(_prevTouch0, _prevTouch1, touch0.position, touch1.position);

            switch (gesture)
            {
                case TouchGesture.Zoom:
                    HandleTouchZoom(touch0.position, touch1.position);
                    break;
                case TouchGesture.Pan:
                    HandleTouchPan(touch0.position, touch1.position);
                    break;
                case TouchGesture.Rotate:
                    HandleTouchRotate(touch0.position, touch1.position);
                    break;
            }

            _prevTouch0 = touch0.position;
            _prevTouch1 = touch1.position;
        }

        private void HandleTouchZoom(Vector2 curTouch0, Vector2 curTouch1)
        {
            float pinchDelta = _touchProcessor.CalculatePinchZoomDelta(_prevTouch0, _prevTouch1, curTouch0, curTouch1);
            Camera.orthographicSize = TouchInputProcessor.ClampZoom(
                Camera.orthographicSize - pinchDelta * _touchZoomSensitivity,
                _minZoom,
                _maxZoom
            );
        }

        private void HandleTouchPan(Vector2 curTouch0, Vector2 curTouch1)
        {
            Vector2 panDelta = _touchProcessor.CalculateTwoFingerPanDelta(_prevTouch0, _prevTouch1, curTouch0, curTouch1);
            ApplyPan(-panDelta.x * _touchPanSensitivity, -panDelta.y * _touchPanSensitivity);
        }

        private void HandleTouchRotate(Vector2 curTouch0, Vector2 curTouch1)
        {
            float rotationDelta = _touchProcessor.CalculateRotationDelta(_prevTouch0, _prevTouch1, curTouch0, curTouch1);
            ApplyRotation(-rotationDelta * _touchRotateSensitivity);
        }

        // --- Desktop (mouse/keyboard/trackpad) ---

        private void HandleDesktopPan()
        {
            // Right-click drag, middle-click drag, or Alt+left-click drag
            bool startPan = Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)
                || (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftAlt));
            bool endPan = Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)
                || Input.GetMouseButtonUp(0);

            if (startPan)
            {
                _isPanning = true;
                _lastMousePosition = Input.mousePosition;
            }

            if (endPan)
            {
                _isPanning = false;
            }

            if (_isPanning)
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                float scale = _panSpeed * Time.deltaTime * 0.1f;
                ApplyPan(-delta.x * scale, -delta.y * scale);
                _lastMousePosition = Input.mousePosition;
            }

            // WASD/Arrow key panning
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                float scale = _panSpeed * Time.deltaTime;
                ApplyPan(h * scale, v * scale);
            }
        }

        private void HandleDesktopZoom()
        {
            if (Camera == null) return;

            // Use mouseScrollDelta for reliable trackpad support on macOS
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // mouseScrollDelta gives larger values on trackpad, smaller on mouse wheel
                // Detect trackpad-style input: trackpad gives fractional, high-frequency values
                float speed = Mathf.Abs(scroll) < 1f ? _trackpadZoomSpeed : _zoomSpeed;
                Camera.orthographicSize = TouchInputProcessor.ClampZoom(
                    Camera.orthographicSize - scroll * speed * Time.deltaTime,
                    _minZoom,
                    _maxZoom
                );
            }
        }

        private void HandleDesktopRotate()
        {
            // Q/E keys for rotation
            if (Input.GetKey(KeyCode.Q))
                ApplyRotation(-_keyboardRotateSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.E))
                ApplyRotation(_keyboardRotateSpeed * Time.deltaTime);
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
