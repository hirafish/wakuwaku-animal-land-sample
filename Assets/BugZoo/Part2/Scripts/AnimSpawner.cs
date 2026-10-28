using System.Collections.Generic;
using UnityEngine;

namespace BugZoo.Part2
{
    public sealed class AnimSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        private readonly List<GameObject> _spawned = new();

        [ContextMenu("Spawn 300")]
        public void Spawn300()
        {
            ClearAll();
            if (_prefab == null) return;

            const int cols = 20;
            const int total = 300;
            for (var i = 0; i < total; i++)
            {
                var row = i / cols;
                var col = i % cols;
                var pos = new Vector3((col - cols * 0.5f) * 1.5f, 0.5f, 10f + row * 1.5f);
                var go = Instantiate(_prefab, pos, Quaternion.identity, transform);
                _spawned.Add(go);
            }
        }

        public void ClearAll()
        {
            foreach (var go in _spawned)
            {
                if (go != null) Destroy(go);
            }
            _spawned.Clear();
        }
    }
}
