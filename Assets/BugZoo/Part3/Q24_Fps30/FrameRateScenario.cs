using System.Diagnostics;
using BugZoo.Common;
using Unity.Profiling;
using UnityEngine;

namespace BugZoo.Part3.Q24
{
    public sealed class FrameRateScenario : BugScenario
    {
        private static readonly ProfilerMarker BusyMarker = new("BugZoo.Busy");
        private int _busyMs;

        private void Start()
        {
            var refreshRate = Screen.currentResolution.refreshRateRatio.value;
            FrameLog.Info($"起動時 targetFrameRate={Application.targetFrameRate}, vSyncCount={QualitySettings.vSyncCount}, refreshRate={refreshRate:F1}", this);
            ApplyTargetFrameRate();
        }

        protected override void OnFixChanged()
        {
            ApplyTargetFrameRate();
        }

        private void ApplyTargetFrameRate()
        {
            if (_applyFix)
            {
                // FIX: 画面のリフレッシュレート（または60fps）を明示的に設定する
                var rate = Mathf.Max(60, (int)Screen.currentResolution.refreshRateRatio.value);
                Application.targetFrameRate = rate;
            }
            else
            {
                // BUG: -1（既定値）のままだとモバイル実機では 30fps 上限になる
                Application.targetFrameRate = -1;
            }
        }

        private void Update()
        {
            if (_busyMs <= 0) return;

            using (BusyMarker.Auto())
            {
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < _busyMs)
                {
                }
            }
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"Application.targetFrameRate: {Application.targetFrameRate}");
            GUILayout.Label($"擬似CPU負荷: {_busyMs} ms/frame");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("負荷 0 ms")) _busyMs = 0;
            if (GUILayout.Button("負荷 10 ms")) _busyMs = 10;
            if (GUILayout.Button("負荷 30 ms")) _busyMs = 30;
            GUILayout.EndHorizontal();
        }
    }
}
