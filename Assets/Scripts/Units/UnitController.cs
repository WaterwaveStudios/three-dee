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

        public void Init(Vector3 position)
        {
            transform.position = position;
        }

        private void Update()
        {
            HandleMovement();
        }

        private void HandleMovement()
        {
            var input = GameInput.Instance;
            Vector2 move = input != null ? input.MoveInput : Vector2.zero;
            var inputDir = CalculateMoveVector(move.x, move.y);
            IsMoving = inputDir.sqrMagnitude > 0.01f;
            MoveDirection = inputDir;

            if (!IsMoving) return;

            // Move
            transform.position += inputDir * _moveSpeed * Time.deltaTime;

            // Face movement direction (Y-axis only)
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg;
            var targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
        }

        public static Vector3 CalculateMoveVector(float horizontal, float vertical)
        {
            return new Vector3(horizontal, 0f, vertical).normalized;
        }
    }
}
