using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Steps.Step05
{
    public sealed class RareStateScenario : BugScenario
    {
        [SerializeField] private EnemyStateMachine _stateMachine;
        [SerializeField] private AttackTimer _attackTimer;
        [SerializeField] private HitReceiver _hitReceiver;

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
            if (_stateMachine != null) _stateMachine.ResetStats();
        }

        protected override void DrawControls()
        {
            if (_stateMachine != null)
            {
                var originalColor = GUI.contentColor;
                GUI.contentColor = _stateMachine.LastMotionStatus.Contains("棒立ち") ? Color.yellow : Color.white;
                GUILayout.Label($"現在の状態: {_stateMachine.LastMotionStatus}");
                GUI.contentColor = originalColor;

                GUILayout.Label($"攻撃要求: {_stateMachine.AttackRequests} ／ 突進成功: {_stateMachine.ActualAttacks} ／ 上書き消滅: {_stateMachine.OverwrittenAttacks}");

                // ワンクリック再現ボタン
                if (GUILayout.Button("バグを再現する（攻撃＋被弾を同一フレームで発生）", GUILayout.Height(32f)))
                {
                    if (_attackTimer != null) _attackTimer.TriggerAttackNow();
                    if (_hitReceiver != null) _hitReceiver.TriggerHitNow();
                }

                if (GUILayout.Button("通常攻撃（突進）を単発で出す", GUILayout.Height(28f)))
                {
                    if (_attackTimer != null) _attackTimer.TriggerAttackNow();
                }

                var breakLabel = _stateMachine.BreakOnDoubleChange ? "2回変更で止める: ON (Debug.Break)" : "2回変更で止める: OFF";
                if (GUILayout.Button(breakLabel, GUILayout.Height(28f)))
                {
                    _stateMachine.BreakOnDoubleChange = !_stateMachine.BreakOnDoubleChange;
                }
            }

            if (_hitReceiver != null)
            {
                var pct = Mathf.RoundToInt(_hitReceiver.HitProbability * 100f);
                var probLabel = $"自動バグ発生確率: {pct}% (クリックで 50% / 80% / 100% 切替)";
                if (GUILayout.Button(probLabel, GUILayout.Height(26f)))
                {
                    if (pct >= 95) _hitReceiver.HitProbability = 0.50f;
                    else if (pct >= 75) _hitReceiver.HitProbability = 1.0f;
                    else _hitReceiver.HitProbability = 0.80f;
                }
            }
        }
    }
}
