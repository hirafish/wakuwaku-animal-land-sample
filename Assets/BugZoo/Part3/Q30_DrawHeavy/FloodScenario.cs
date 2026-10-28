using System.Collections.Generic;
using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part3.Q30
{
    public sealed class FloodScenario : BugScenario
    {
        [SerializeField] private GameObject _cubePrefab;
        [SerializeField] private Material[] _sharedMaterials;

        private readonly List<GameObject> _spawned = new();
        private readonly List<Material> _duplicatedMaterials = new();

        protected override void OnFixChanged()
        {
            if (_spawned.Count > 0)
            {
                Spawn3000();
            }
        }

        private void OnDestroy()
        {
            ClearAll();
        }

        private void Spawn3000()
        {
            ClearAll();
            if (_cubePrefab == null) return;

            var rng = new System.Random(123);
            const int cols = 60;
            const int rows = 50;

            for (var r = 0; r < rows; r++)
            {
                for (var c = 0; c < cols; c++)
                {
                    var pos = new Vector3((c - cols * 0.5f) * 0.6f, (r - rows * 0.5f) * 0.6f, 15f);
                    var go = Instantiate(_cubePrefab, pos, Quaternion.identity, transform);
                    go.transform.localScale = Vector3.one * 0.4f;
                    var renderer = go.GetComponent<Renderer>();

                    if (_applyFix)
                    {
                        // FIX: 共通のマテリアル（8種類）を sharedMaterial で割り当てる
                        if (_sharedMaterials != null && _sharedMaterials.Length > 0)
                        {
                            renderer.sharedMaterial = _sharedMaterials[(r * cols + c) % _sharedMaterials.Length];
                        }
                    }
                    else
                    {
                        // BUG: 1個ずつ renderer.material に触ってマテリアルを3000個に複製してしまう
                        var mat = renderer.material;
                        mat.color = Color.HSVToRGB((float)rng.NextDouble(), 0.7f, 0.9f);
                        _duplicatedMaterials.Add(mat);
                    }

                    _spawned.Add(go);
                }
            }

            FrameLog.Info($"3000個配置完了 ({(_applyFix ? "sharedMaterial 8種共有" : "material 3000個個別複製")})", this);
        }

        private void ClearAll()
        {
            foreach (var mat in _duplicatedMaterials)
            {
                if (mat != null) Destroy(mat);
            }
            _duplicatedMaterials.Clear();

            foreach (var go in _spawned)
            {
                if (go != null) Destroy(go);
            }
            _spawned.Clear();
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"配置数: {_spawned.Count}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("3000個出す", GUILayout.Height(32f)))
            {
                Spawn3000();
            }
            if (GUILayout.Button("全部消す", GUILayout.Height(32f)))
            {
                ClearAll();
            }
            GUILayout.EndHorizontal();
        }
    }
}
