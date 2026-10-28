using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part2
{
    public sealed class AnimatorStateLogger : StateMachineBehaviour
    {
        [SerializeField] private string _label = "";

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            FrameLog.Info($"Enter {_label}", animator);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            FrameLog.Info($"Exit {_label} t={stateInfo.normalizedTime:F2}", animator);
        }
    }
}
