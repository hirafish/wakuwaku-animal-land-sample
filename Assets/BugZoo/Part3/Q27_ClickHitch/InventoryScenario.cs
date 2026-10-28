using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BugZoo.Common;
using Unity.Profiling;
using UnityEngine;

namespace BugZoo.Part3.Q27
{
    public sealed class InventoryScenario : BugScenario
    {
        private static readonly ProfilerMarker GenerateMarker = new("Inventory.Generate");
        private static readonly ProfilerMarker SortMarker = new("Inventory.Sort");
        private static readonly ProfilerMarker FormatMarker = new("Inventory.Format");
        private static readonly ProfilerMarker FindMarker = new("Inventory.Find");

        private readonly List<int> _precomputedItems = new();
        private bool _openRequested;
        private bool _useMarkers = true;
        private string _summary = "未表示";

        private void Start()
        {
            StartCoroutine(PrecomputeRoutine());
        }

        private IEnumerator PrecomputeRoutine()
        {
            _precomputedItems.Clear();
            var rng = new System.Random(42);
            for (var batch = 0; batch < 20; batch++)
            {
                for (var i = 0; i < 10000; i++)
                {
                    _precomputedItems.Add(rng.Next(1, 1000000));
                }
                yield return null;
            }
            _precomputedItems.Sort();
        }

        private void Update()
        {
            if (!_openRequested) return;
            _openRequested = false;

            if (_applyFix)
            {
                // FIX: 事前に準備・ソートしておいたキャッシュから先頭を表示するだけ
                _summary = _precomputedItems.Count > 0
                    ? $"キャッシュから即座に表示！ 先頭={_precomputedItems[0]} (全{_precomputedItems.Count}件)"
                    : "事前準備中...";
                FrameLog.Info(_summary, this);
            }
            else
            {
                // BUG: ボタンを押した1フレームの中で生成・LINQソート・文字列化・FindObjectsByType連打を行う
                OpenHeavy();
            }
        }

        private void OpenHeavy()
        {
            var rng = new System.Random(42);
            List<int> items;

            if (_useMarkers)
            {
                using (GenerateMarker.Auto())
                {
                    items = new List<int>(200000);
                    for (var i = 0; i < 200000; i++) items.Add(rng.Next(1, 1000000));
                }
                using (SortMarker.Auto())
                {
                    items = items.OrderBy(x => x).ToList();
                }
                using (FormatMarker.Auto())
                {
                    var text = "";
                    for (var i = 0; i < 100; i++) text += $"[{items[i]}] ";
                    _summary = text.Substring(0, 40) + "...";
                }
                using (FindMarker.Auto())
                {
                    for (var i = 0; i < 50; i++)
                    {
                        FindObjectsByType<Transform>(FindObjectsSortMode.None);
                    }
                }
            }
            else
            {
                items = new List<int>(200000);
                for (var i = 0; i < 200000; i++) items.Add(rng.Next(1, 1000000));
                items = items.OrderBy(x => x).ToList();
                var text = "";
                for (var i = 0; i < 100; i++) text += $"[{items[i]}] ";
                _summary = text.Substring(0, 40) + "...";
                for (var i = 0; i < 50; i++)
                {
                    FindObjectsByType<Transform>(FindObjectsSortMode.None);
                }
            }

            FrameLog.Info("重い持ち物一覧を開きました", this);
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"結果: {_summary}");
            if (GUILayout.Button("持ち物一覧を開く", GUILayout.Height(34f)))
            {
                _openRequested = true;
            }
            if (GUILayout.Button(_useMarkers ? "ProfilerMarker: ON" : "ProfilerMarker: OFF", GUILayout.Height(28f)))
            {
                _useMarkers = !_useMarkers;
            }
        }
    }
}
