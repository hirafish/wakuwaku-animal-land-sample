using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q06
{
    public sealed class StaticScore : BugScenario
    {
        private void Start()
        {
            FrameLog.Info($"Play 開始時 — BuggyScoreStore.Total={BuggyScoreStore.Total}, FixedScoreStore.Total={FixedScoreStore.Total}", this);
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"BuggyScoreStore.Total (リセットなし): {BuggyScoreStore.Total}");
            GUILayout.Label($"FixedScoreStore.Total (リセットあり): {FixedScoreStore.Total}");

            if (GUILayout.Button("スコア +10", GUILayout.Height(32f)))
            {
                if (_applyFix)
                {
                    FixedScoreStore.Total += 10;
                    FrameLog.Info($"FixedScoreStore.Total = {FixedScoreStore.Total}", this);
                }
                else
                {
                    BuggyScoreStore.Total += 10;
                    FrameLog.Info($"BuggyScoreStore.Total = {BuggyScoreStore.Total}", this);
                }
            }
        }
    }
}
