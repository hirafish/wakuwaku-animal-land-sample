using System.Collections;
using System.Collections.Generic;
using BugZoo.Common;
using UnityEngine;
using UnityEngine.Pool;

namespace BugZoo.Part3.Q26
{
    public sealed class SwarmScenario : BugScenario
    {
        [SerializeField] private SwarmEnemy _enemyPrefab;
        [SerializeField] private GameObject _projectilePrefab;

        private readonly List<SwarmEnemy> _enemies = new();
        private ObjectPool<GameObject> _pool;
        private int _targetCount = 100;

        protected override void Awake()
        {
            base.Awake();
            _pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(_projectilePrefab),
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go => go.SetActive(false),
                actionOnDestroy: Destroy,
                defaultCapacity: 500,
                maxSize: 5000);
        }

        private void Start()
        {
            RespawnAll();
        }

        protected override void OnFixChanged()
        {
            RespawnAll();
        }

        public void SpawnProjectile(Vector3 position)
        {
            if (_projectilePrefab == null) return;

            if (_applyFix)
            {
                // FIX: ObjectPool から使い回す
                var go = _pool.Get();
                go.transform.position = position;
                StartCoroutine(ReleaseAfterRoutine(go, 0.5f));
            }
            else
            {
                // BUG: 毎回 Instantiate と Destroy を繰り返す
                var go = Instantiate(_projectilePrefab, position, Quaternion.identity);
                Destroy(go, 0.5f);
            }
        }

        private IEnumerator ReleaseAfterRoutine(GameObject go, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (go != null && go.activeSelf)
            {
                _pool.Release(go);
            }
        }

        private void RespawnAll()
        {
            foreach (var e in _enemies)
            {
                if (e != null) Destroy(e.gameObject);
            }
            _enemies.Clear();

            if (_enemyPrefab == null) return;

            const int cols = 50;
            for (var i = 0; i < _targetCount; i++)
            {
                var r = i / cols;
                var c = i % cols;
                var pos = new Vector3((c - cols * 0.5f) * 1.2f, 0.5f, (r - 10) * 1.2f);
                var enemy = Instantiate(_enemyPrefab, pos, Quaternion.identity, transform);
                enemy.Setup(this, _applyFix, i);
                _enemies.Add(enemy);
            }
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"現在の敵の数: {_enemies.Count}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("100体")) { _targetCount = 100; RespawnAll(); }
            if (GUILayout.Button("1000体")) { _targetCount = 1000; RespawnAll(); }
            if (GUILayout.Button("3000体")) { _targetCount = 3000; RespawnAll(); }
            GUILayout.EndHorizontal();
        }
    }
}
