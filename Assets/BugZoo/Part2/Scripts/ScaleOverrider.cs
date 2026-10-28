using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part2
{
    public sealed class ScaleOverrider : BugScenario
    {
        private bool _overrideEnabled;
        private float _logTimer;

        public override string SectionTitle => "Q16: スクリプトからの Scale 変更";

        private void Update()
        {
            if (!_overrideEnabled) return;

            if (!_applyFix)
            {
                // BUG: Update で書き換えても、直後の Animator 評価で上書きされる
                transform.localScale = Vector3.one * 2f;
            }
        }

        private void LateUpdate()
        {
            if (!_overrideEnabled) return;

            if (_applyFix)
            {
                // FIX: Animator 評価が終わった後の LateUpdate で書き換える
                transform.localScale = Vector3.one * 2f;
            }

            _logTimer += Time.unscaledDeltaTime;
            if (_logTimer >= 1f)
            {
                _logTimer = 0f;
                FrameLog.Info($"LateUpdate 終了時点の localScale = {transform.localScale}", this);
            }
        }

        protected override void DrawControls()
        {
            var label = _overrideEnabled ? "大きくする (2倍): ON" : "大きくする (2倍): OFF";
            if (GUILayout.Button(label, GUILayout.Height(30f)))
            {
                _overrideEnabled = !_overrideEnabled;
            }
        }
    }
}
