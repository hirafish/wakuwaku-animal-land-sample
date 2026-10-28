using UnityEngine;

namespace BugZoo.Part1.Q08
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class Bullet : MonoBehaviour
    {
        private BulletScenario _owner;
        private Vector3 _previousPosition;
        private bool _resolved;

        public void Initialize(BulletScenario owner, float speed, CollisionDetectionMode mode)
        {
            _owner = owner;
            _previousPosition = transform.position;
            var rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
            rb.collisionDetectionMode = mode;
            rb.linearVelocity = Vector3.right * speed;
        }

        private void FixedUpdate()
        {
            Debug.DrawLine(_previousPosition, transform.position, Color.red, 2f);
            _previousPosition = transform.position;

            if (!_resolved && transform.position.x > 12f)
            {
                _resolved = true;
                if (_owner != null) _owner.ReportPassThrough();
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_resolved) return;
            if (collision.gameObject.name.Contains("Wall"))
            {
                _resolved = true;
                if (_owner != null) _owner.ReportHit();
                Destroy(gameObject);
            }
        }
    }
}
