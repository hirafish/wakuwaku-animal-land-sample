using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Steps.Step01
{
    public sealed class ScoreCounter : BugScenario
    {
        [SerializeField] private ScoreView _viewInScene;
        [SerializeField] private ScoreView _viewPrefabAsset;

        private int _count;

        protected override void DrawControls()
        {
            GUILayout.Label($"内部カウント: {_count}");
            if (GUILayout.Button("おやつをあげる (+1)", GUILayout.Height(36f)))
            {
                GiveSnack();
            }
        }

        private void GiveSnack()
        {
            _count++;
            FrameLog.Info("おやつをあげた (GiveSnack)", this);

            if (_applyFix)
            {
                // FIX: シーン上の ScoreView を更新する
                _viewInScene.SetScore(_count);
            }
            else
            {
                // BUG: Project ウィンドウのプレハブ本体（元データ）を更新してしまっている
                _viewPrefabAsset.SetScore(_count);
            }
        }
    }
}
