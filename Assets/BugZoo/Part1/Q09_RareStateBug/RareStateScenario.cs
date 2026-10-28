using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q09
{
    public sealed class RareStateScenario : BugScenario
    {
        [SerializeField] private EnemyStateMachine _stateMachine;
        [SerializeField] private AttackTimer _attackTimer;
        [SerializeField] private HitReceiver _hitReceiver;

        private bool _fixedSeed = true;

        private void Start()
        {
            ApplyMode();
        }

        protected override void OnFixChanged()
        {
            ApplyMode();
        }

        private void ApplyMode()
        {
            if (_attackTimer != null) _attackTimer.UseRequestQueue = _applyFix;
            if (_hitReceiver != null) _hitReceiver.UseRequestQueue = _applyFix;
        }

        protected override void DrawControls()
        {
            if (_stateMachine != null)
            {
                GUILayout.Label($"現在の状態: {_stateMachine.State}");
                GUILayout.Label($"攻撃要求: {_stateMachine.AttackRequests} ／ 実際の攻撃: {_stateMachine.ActualAttacks} ／ 上書き消滅: {_stateMachine.OverwrittenAttacks}");

                var breakLabel = _stateMachine.BreakOnDoubleChange ? "2回変更で止める: ON" : "2回変更で止める: OFF";
                if (GUILayout.Button(breakLabel))
                {
                    _stateMachine.BreakOnDoubleChange = !_stateMachine.BreakOnDoubleChange;
                }
            }

            if (_hitReceiver != null)
            {
                var probLabel = $"ひるみ確率: {_hitReceiver.HitProbability * 100f:F0}% (クリックで 2% / 20% 切替)";
                if (GUILayout.Button(probLabel))
                {
                    _hitReceiver.HitProbability = Mathf.Approximately(_hitReceiver.HitProbability, 0.02f) ? 0.20f : 0.02f;
                }

                if (GUILayout.Button(_fixedSeed ? "乱数シード固定: ON (12345)" : "乱数シード固定: OFF"))
                {
                    _fixedSeed = !_fixedSeed;
                    _hitReceiver.ResetSeed(_fixedSeed ? 12345 : System.Environment.TickCount);
                }
            }

            var capLabel = Time.captureFramerate > 0 ? $"captureFramerate: {Time.captureFramerate} (ON)" : "captureFramerate: 0 (OFF)";
            if (GUILayout.Button(capLabel))
            {
                Time.captureFramerate = Time.captureFramerate > 0 ? 0 : 30;
            }
        }
    }
}
