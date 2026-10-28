using UnityEngine;

namespace BugZoo.Part2
{
    public sealed class AttackExitNotifier : StateMachineBehaviour
    {
        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (animator.TryGetComponent<AttackEndDetector>(out var detector))
            {
                detector.NotifyExit();
            }
        }
    }
}
