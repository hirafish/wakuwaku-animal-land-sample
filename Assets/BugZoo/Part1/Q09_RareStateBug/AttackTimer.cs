using UnityEngine;

namespace BugZoo.Part1.Q09
{
    public sealed class AttackTimer : MonoBehaviour
    {
        [SerializeField] private EnemyStateMachine _stateMachine;
        private float _timer;

        public bool UseRequestQueue { get; set; }

        private void Update()
        {
            if (_stateMachine == null) return;

            _timer += Time.deltaTime;
            if (_timer >= 2f)
            {
                _timer = 0f;
                if (UseRequestQueue)
                {
                    _stateMachine.RequestState(EnemyState.Attack, priority: 1, "timer");
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
