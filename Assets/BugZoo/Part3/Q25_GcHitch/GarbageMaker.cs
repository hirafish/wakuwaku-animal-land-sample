using System.Collections.Generic;
using System.Linq;
using BugZoo.Common;
using Unity.Profiling;
using UnityEngine;

namespace BugZoo.Part3.Q25
{
    public sealed class GarbageMaker : BugScenario
    {
        private static readonly ProfilerMarker MakerMarker = new("BugZoo.GarbageMaker");

        private readonly List<int> _reusableSource = new(1024);
        private readonly List<int> _reusableFiltered = new(1024);
        private int _multiplier = 1;
        private int _lastCount = -1;
        private string _cachedStatus = "";

        private void Awake()
        {
            base.Awake();
            for (var i = 0; i < 1000; i++)
            {
                _reusableSource.Add(i);
            }
        }

        private void Update()
        {
            using (MakerMarker.Auto())
            {
                for (var m = 0; m < _multiplier; m++)
                {
                    if (_applyFix)
                    {
                        RunWithoutAllocation();
                    }
                    else
                    {
                        RunWithAllocation();
                    }
                }
            }
        }

        private void RunWithAllocation()
        {
            // BUG: 毎フレーム List 生成、LINQ、ボックス化、文字列連結を行っている
            var list = new List<int>();
            for (var i = 0; i < 400; i++)
            {
                list.Add(i);
            }

            var evens = list.Where(x => x % 2 == 0).ToList();
            object boxed = evens.Count;
            var logText = "";
            for (var i = 0; i < 20; i++)
            {
                logText += i.ToString();
            }
            _cachedStatus = "残り: " + boxed + " (" + logText.Length + ")";
        }

        private void RunWithoutAllocation()
        {
            // FIX: リストを使い回し、LINQ や毎フレームの文字列生成を避ける
            _reusableFiltered.Clear();
            for (var i = 0; i < 400; i++)
            {
                if (i % 2 == 0)
                {
                    _reusableFiltered.Add(i);
                }
            }

            if (_reusableFiltered.Count != _lastCount)
            {
                _lastCount = _reusableFiltered.Count;
                _cachedStatus = $"残り: {_lastCount}";
            }
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"状態文字列: {_cachedStatus}");
            GUILayout.Label($"負荷倍率: ×{_multiplier}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("確保量 ×1")) _multiplier = 1;
            if (GUILayout.Button("確保量 ×10")) _multiplier = 10;
            GUILayout.EndHorizontal();
        }
    }
}
