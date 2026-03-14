using UnityEngine;

namespace ThreeDee.Units
{
    public class CameraFollower : MonoBehaviour
    {
        [SerializeField] private float _followSpeed = 5f;

        private Transform _target;
        private Vector3 _offset;

        public void Init(Transform target)
        {
            _target = target;
            _offset = transform.position - target.position;
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desiredPosition = CalculateFollowPosition(_target.position, _offset);
            transform.position = SmoothFollow(transform.position, desiredPosition, _followSpeed, Time.deltaTime);
        }

        public static Vector3 CalculateFollowPosition(Vector3 targetPosition, Vector3 offset)
        {
            return targetPosition + offset;
        }

        public static Vector3 SmoothFollow(Vector3 current, Vector3 target, float speed, float deltaTime)
        {
            return Vector3.Lerp(current, target, speed * deltaTime);
        }
    }
}
