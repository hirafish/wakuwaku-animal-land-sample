using UnityEngine;

namespace BugZoo.Steps.Step05
{
    public sealed class HitReceiver : MonoBehaviour
    {
        [SerializeField] private EnemyStateMachine _stateMachine;
        private System.Random _random = new(12345);

        public bool UseRequestQueue { get; set; }
        public float HitProbability { get; set; } = 0.50f; // 攻撃と同時に被弾が重なる確率 (デフォルト 50%)
        public bool TriggerNextFrame { get; set; }

        public void TriggerHitNow()
        {
            TriggerNextFrame = true;
        }

        private void Update()
        {
            if (_stateMachine == null) return;

            // ボタンによる強制トリガー、または自動確率チェック
            var shouldHit = TriggerNextFrame;
            TriggerNextFrame = false;

            // 攻撃が開始された「その同一フレーム」に高確率（80%）で被弾を発生させて衝突させる！
            if (!shouldHit && _stateMachine.State == EnemyState.Attack && _stateMachine.LastChangedFrame == Time.frameCount)
            {
                shouldHit = _random.NextDouble() < HitProbability;
            }

            if (shouldHit)
            {
                if (UseRequestQueue)
                {
                    _stateMachine.RequestState(EnemyState.Stun, priority: 1, "hit");
                }
                else
                {
                    // BUG: AttackTimer の直後に動くと、Attack を一瞬で Stun に上書きしてしまう
                    _stateMachine.ChangeStateImmediate(EnemyState.Stun, "hit");
                }
            }
        }
    }
}
