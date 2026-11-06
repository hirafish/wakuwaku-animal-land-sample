using Unity.Profiling;
using UnityEngine;

namespace BugZoo.Common
{
    public sealed class FpsHud : MonoBehaviour
    {
        private ProfilerRecorder _gcAllocRecorder;
        private ProfilerRecorder _mainThreadRecorder;
        private int _frames;
        private float _elapsed;
        private float _fps;
        private string _boxText = "計測中...";

        private void OnEnable()
        {
            _gcAllocRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            _mainThreadRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
        }

        private void OnDisable()
        {
            _gcAllocRecorder.Dispose();
            _mainThreadRecorder.Dispose();
        }

        private void Update()
        {
            _frames++;
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed >= 0.25f)
            {
                _fps = _frames / _elapsed;
                _frames = 0;
                _elapsed = 0f;

                var gcBytes = _gcAllocRecorder.Valid ? $"{_gcAllocRecorder.LastValue:N0} B" : "取得不可";
                var mainMs = _mainThreadRecorder.Valid
                    ? $"{_mainThreadRecorder.LastValue * 1e-6f:F2} ms"
                    : $"{Time.unscaledDeltaTime * 1000f:F2} ms";
                _boxText = $"FPS: {_fps:F1}\nFrame: {mainMs}\nGC Alloc/Frame: {gcBytes}";
            }
        }

        private void OnGUI()
        {
            var scale = Mathf.Max(1f, Screen.height / 900f);
            var width = 280f * scale;
            var height = 96f * scale;
            var rect = new Rect(Screen.width - width - 12f, 12f, width, height);

            GUI.Box(rect, _boxText);
        }
    }
}
