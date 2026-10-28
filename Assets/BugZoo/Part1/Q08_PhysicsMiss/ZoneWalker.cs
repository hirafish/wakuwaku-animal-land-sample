using UnityEngine;

namespace BugZoo.Part1.Q08
{
    public sealed class ZoneWalker : MonoBehaviour
    {
        [SerializeField] private float _speed = 3f;
        private Vector3 _origin;

        private void Awake()
        {
            _origin = transform.position;
        }

        private void Update()
        {
            if (TryGetComponent<Rigidbody>(out _))
            {
                return;
            }
            var offset = Mathf.PingPong(Time.time * _speed, 6f) - 3f;
            transform.position = _origin + new Vector3(offset, 0f, 0f);
        }

        private void FixedUpdate()
        {
            if (!TryGetComponent<Rigidbody>(out var rb))
            {
                return;
            }
            var offset = Mathf.PingPong(Time.fixedTime * _speed, 6f) - 3f;
            rb.MovePosition(_origin + new Vector3(offset, 0f, 0f));
        }
    }
}
