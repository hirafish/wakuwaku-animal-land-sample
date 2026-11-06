using UnityEngine;

namespace BugZoo.Steps.Step02
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerMover : MonoBehaviour
    {
        [SerializeField] private float _speed = 4f;
        private Rigidbody _rb;
        private Vector3 _initialPosition;
        private float _speedMultiplier = 1f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _initialPosition = transform.position;
        }

        public void ResetPosition()
        {
            _rb.position = _initialPosition;
            transform.position = _initialPosition;
        }

        public void ToggleSpeedMultiplier()
        {
            _speedMultiplier = Mathf.Approximately(_speedMultiplier, 1f) ? 2f : 1f;
        }

        public float SpeedMultiplier => _speedMultiplier;

        private void FixedUpdate()
        {
            if (_rb.position.x > 18f)
            {
                return;
            }
            var next = _rb.position + Vector3.right * (_speed * _speedMultiplier * Time.fixedDeltaTime);
            _rb.MovePosition(next);
        }
    }
}
