using UnityEngine;

namespace BugZoo.Part1.Q09
{
    public sealed class HitReceiver : MonoBehaviour
    {
        [SerializeField] private EnemyStateMachine _stateMachine;
        private System.Random _random = new(12345);

        public bool UseRequestQueue { get; set; }
        public float HitProbability { get; set; } = 0.02f;

        public void ResetSeed(int seed)
        {
            _random = new System.Random(seed);
        }

        private void Update()
        {
            if (_stateMachine == null) return;

            if (_random.NextDouble() < HitProbability)
            {
                if (UseRequestQueue)
                {
                    _stateMachine.RequestState(EnemyState.Stun, priority: 2, "hit");
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
