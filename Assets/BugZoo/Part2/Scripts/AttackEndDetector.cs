using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part2
{
    [RequireComponent(typeof(Animator))]
    public sealed class AttackEndDetector : BugScenario
    {
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int IdleHash = Animator.StringToHash("Idle");

        private Animator _animator;
        private int _attackCount;
        private int _detectedEndCount;
        private bool _countedThisAttack;
        private bool _wasInAttack;
        private bool _perFrameLog;

        public override string SectionTitle => "Q21: 攻撃終了の判定 (normalizedTime vs OnStateExit)";

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
        }

        public void NotifyAttackStarted()
        {
            _attackCount++;
            _countedThisAttack = false;
        }

        public void NotifyExit()
        {
            if (_applyFix)
            {
                // FIX: StateMachineBehaviour.OnStateExit から確実に受け取る
                _detectedEndCount++;
                FrameLog.Info("OnStateExit で Attack の終了を検出しました", this);
            }
        }

        private void Update()
        {
            if (_animator == null) return;

            var info = _animator.GetCurrentAnimatorStateInfo(0);
            var inTransition = _animator.IsInTransition(0);
            var isAttack = info.shortNameHash == AttackHash;

            if (_perFrameLog)
            {
                if (isAttack)
                {
                    var transText = inTransition ? " 遷移中" : "";
                    FrameLog.Info($"Attack t={info.normalizedTime:F2}{transText}", this);
                }
                else if (_wasInAttack && info.shortNameHash == IdleHash)
                {
                    FrameLog.Info($"Idle t={info.normalizedTime:F2}", this);
                }
            }

            if (!_applyFix)
            {
                // BUG: Attack のまま normalizedTime >= 1 になる隙間が 10ms しかなく、フレーム刻みによって取りこぼす
                if (isAttack && info.normalizedTime >= 1f && !_countedThisAttack)
                {
                    _countedThisAttack = true;
                    _detectedEndCount++;
                    FrameLog.Info($"normalizedTime >= 1 で終了を検出 (t={info.normalizedTime:F2})", this);
                }
            }

            _wasInAttack = isAttack;
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"攻撃の回数: {_attackCount} ／ 終了を検出した回数: {_detectedEndCount}");
            var logLabel = _perFrameLog ? "フレームごとのログ: ON" : "フレームごとのログ: OFF";
            if (GUILayout.Button(logLabel, GUILayout.Height(28f)))
            {
                _perFrameLog = !_perFrameLog;
            }
        }
    }
}
