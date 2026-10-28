using Unity.Profiling;
using UnityEngine;

namespace BugZoo.Part3.Q26
{
    public sealed class SwarmEnemy : MonoBehaviour, IUpdatable
    {
        private static readonly ProfilerMarker ShootMarker = new("BugZoo.Swarm.Shoot");

        private SwarmScenario _scenario;
        private bool _useManager;
        private float _shootTimer;
        private int _id;

        public void Setup(SwarmScenario scenario, bool useManager, int id)
        {
            _scenario = scenario;
            _useManager = useManager;
            _id = id;
            _shootTimer = (id % 60) * 0.016f;

            if (_useManager && UpdateManager.Instance != null)
            {
                UpdateManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (_useManager && UpdateManager.Instance != null)
            {
                UpdateManager.Instance.Unregister(this);
            }
        }

        private void Update()
        {
            if (_useManager)
            {
                return;
            }
            // BUG: 個々の MonoBehaviour が Update を持ち、毎回 C++ → C# 呼び出しコストがかかる
            Tick(Time.deltaTime);
        }

        public void CustomUpdate(float deltaTime)
        {
            Tick(deltaTime);
        }

        private void Tick(float deltaTime)
        {
            transform.Rotate(0f, 45f * deltaTime, 0f);
            _shootTimer += deltaTime;
            if (_shootTimer >= 1.5f)
            {
                _shootTimer = 0f;
                using (ShootMarker.Auto())
                {
                    if (_scenario != null)
                    {
                        _scenario.SpawnProjectile(transform.position + Vector3.up * 0.5f);
                    }
                }
            }
        }
    }
}
