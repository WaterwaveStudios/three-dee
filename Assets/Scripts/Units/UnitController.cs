using UnityEngine;
using ThreeDee.Core;

namespace ThreeDee.Units
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 6f;
        [SerializeField] private float _turnSpeed = 720f;

        public float MoveSpeed { get => _moveSpeed; set => _moveSpeed = value; }
        public bool IsMoving { get; private set; }
        public Vector3 MoveDirection { get; private set; }

        private Animator _animator;

        public void Init(Vector3 position)
        {
            transform.position = position;
        }

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            Debug.Log($"[UnitController] Start — GameInput.Instance={(GameInput.Instance != null ? "OK" : "NULL")}  animator={(_animator != null ? _animator.name : "none")}");
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            var input = GameInput.Instance;
            if (input == null)
            {
                Debug.LogWarning("[UnitController] GameInput.Instance is NULL — cannot move");
                return;
            }
            Vector2 move = input.MoveInput;
            if (move.sqrMagnitude > 0.01f)
                Debug.Log($"[UnitController] move={move}  pos={transform.position}");
            var inputDir = CalculateMoveVector(move.x, move.y);
            IsMoving = inputDir.sqrMagnitude > 0.01f;
            MoveDirection = inputDir;
            _animator?.SetBool("IsMoving", IsMoving);

            if (!IsMoving) return;

            // Move
            transform.position += inputDir * _moveSpeed * Time.deltaTime;

            // Snap to face movement direction instantly (Y-axis only)
            float angle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        public static Vector3 CalculateMoveVector(float horizontal, float vertical)
        {
            return new Vector3(horizontal, 0f, vertical).normalized;
        }
    }
}
