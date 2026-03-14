using UnityEngine;

namespace ThreeDee.Camera
{
    public class IsometricCamera : MonoBehaviour
    {
        [SerializeField] private float _panSpeed = 20f;
        [SerializeField] private float _zoomSpeed = 5f;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 30f;
        [SerializeField] private float _rotationAngleY = 45f;
        [SerializeField] private float _rotationAngleX = 30f;

        private UnityEngine.Camera _camera;
        private Vector3 _lastMousePosition;
        private bool _isPanning;

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
            HandlePan();
            HandleZoom();
        }

        private void HandlePan()
        {
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                _isPanning = true;
                _lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                _isPanning = false;
            }

            if (_isPanning)
            {
                Vector3 delta = Input.mousePosition - _lastMousePosition;
                Vector3 move = new Vector3(-delta.x, 0f, -delta.y) * _panSpeed * Time.deltaTime * 0.1f;

                // Move relative to camera's forward on the XZ plane
                Vector3 forward = transform.forward;
                forward.y = 0f;
                forward.Normalize();
                Vector3 right = transform.right;
                right.y = 0f;
                right.Normalize();

                transform.position += right * move.x + forward * move.z;
                _lastMousePosition = Input.mousePosition;
            }

            // WASD/Arrow key panning
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f)
            {
                Vector3 forward = transform.forward;
                forward.y = 0f;
                forward.Normalize();
                Vector3 right = transform.right;
                right.y = 0f;
                right.Normalize();

                transform.position += (right * h + forward * v) * _panSpeed * Time.deltaTime;
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f && _camera != null)
            {
                _camera.orthographicSize = Mathf.Clamp(
                    _camera.orthographicSize - scroll * _zoomSpeed,
                    _minZoom,
                    _maxZoom
                );
            }
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

            // Add audio listener
            go.AddComponent<AudioListener>();

            return isoCamera;
        }
    }
}
