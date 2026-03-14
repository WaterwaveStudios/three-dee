using UnityEngine;
using ThreeDee.Core;

namespace ThreeDee.Units
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 6f;

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        public bool IsMoving { get; private set; }
        public Vector3 MoveDirection { get; private set; }

        private Rigidbody _rb;
        private Animator _animator;
        private Vector3 _inputDir;

        public void Init(Vector3 position)
        {
            transform.position = position;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            Debug.Log($"[UnitController] Start — GameInput={(GameInput.Instance != null ? "OK" : "NULL")}  rb={(_rb != null ? "OK" : "NULL")}  animator={(_animator != null ? _animator.name : "none")}");
        }

        private void Update()
        {
            // Read input and update animation/rotation every frame
            var input = GameInput.Instance;
            if (input == null)
            {
                Debug.LogWarning("[UnitController] GameInput.Instance is NULL — cannot move");
                return;
            }

            Vector2 move = input.MoveInput;
            _inputDir = CalculateCameraRelativeMoveVector(move.x, move.y);
            IsMoving = _inputDir.sqrMagnitude > 0.01f;
            MoveDirection = _inputDir;
            _animator?.SetBool("IsMoving", IsMoving);

            if (!IsMoving) return;

            float angle = Mathf.Atan2(_inputDir.x, _inputDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        private void FixedUpdate()
        {
            // Apply movement via Rigidbody so physics collision is respected
            if (!IsMoving || _rb == null) return;
            _rb.MovePosition(_rb.position + _inputDir * _moveSpeed * Time.fixedDeltaTime);
        }

        private static Vector3 CalculateCameraRelativeMoveVector(float horizontal, float vertical)
        {
            var cam = UnityEngine.Camera.main;
            if (cam == null)
                return new Vector3(horizontal, 0f, vertical).normalized;

            var camForward = cam.transform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            var camRight = cam.transform.right;
            camRight.y = 0f;
            camRight.Normalize();

            return (camForward * vertical + camRight * horizontal).normalized;
        }

        public static Vector3 CalculateMoveVector(float horizontal, float vertical)
        {
            return new Vector3(horizontal, 0f, vertical).normalized;
        }
    }
}
