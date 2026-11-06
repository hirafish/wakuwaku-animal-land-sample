using UnityEngine;

namespace BugZoo.Steps.Step05
{
    public sealed class AttackTimer : MonoBehaviour
    {
        [SerializeField] private EnemyStateMachine _stateMachine;
        private float _timer;

        public bool UseRequestQueue { get; set; }

        public void TriggerAttackNow()
        {
            _timer = 2.0f; // 次の Update で即座に発動
        }

        private void Update()
        {
            if (_stateMachine == null) return;

            _timer += Time.deltaTime;
            if (_timer >= 2f)
            {
                _timer = 0f;
                if (UseRequestQueue)
                {
                    _stateMachine.RequestState(EnemyState.Attack, priority: 2, "timer");
                }
                else
                {
                    // BUG: 直接状態を書き換えるため、同じフレームの別の Update に上書きされることがある
                    _stateMachine.ChangeStateImmediate(EnemyState.Attack, "timer");
                }
            }
        }
    }
}
