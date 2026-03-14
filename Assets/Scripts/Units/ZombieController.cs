using UnityEngine;
using UnityEngine.AI;
using ThreeDee.Core;

namespace ThreeDee.Units
{
    public class ZombieController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 2.5f;
        [SerializeField] private float _stopDistance = 1.5f;
        [SerializeField] private float _damageRange = 1.8f;
        [SerializeField] private float _damageInterval = 1f;

        private Transform _target;
        private NavMeshAgent _agent;
        private Animator _animator;
        private PlayerHealth _playerHealth;
        private float _nextDamageTime;

        public void Init(Transform target, PlayerHealth playerHealth = null)
        {
            _target = target;
            _playerHealth = playerHealth;
        }

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponentInChildren<Animator>();

            if (_agent != null)
            {
                _agent.speed = _moveSpeed;
                _agent.stoppingDistance = _stopDistance;
                _agent.acceleration = 8f;
                _agent.updateRotation = false; // we rotate manually
            }
        }

        private void Update()
        {
            if (_target == null || _agent == null) return;

            _agent.SetDestination(_target.position);

            var velocity = _agent.velocity;
            velocity.y = 0f;
            bool isMoving = velocity.sqrMagnitude > 0.1f;
            _animator?.SetBool("IsMoving", isMoving);

            // Face movement direction (Y-axis only)
            if (isMoving)
            {
                float angle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // Deal damage when in attack range
            if (_playerHealth != null && Time.time >= _nextDamageTime)
            {
                float dist = Vector3.Distance(transform.position, _target.position);
                if (dist <= _damageRange)
                {
                    _playerHealth.TakeDamage(1);
                    _nextDamageTime = Time.time + _damageInterval;
                }
            }
        }
    }
}
