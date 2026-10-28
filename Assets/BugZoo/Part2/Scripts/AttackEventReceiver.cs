using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part2
{
    public sealed class AttackEventReceiver : MonoBehaviour
    {
        public void OnAttackHit()
        {
            FrameLog.Info("OnAttackHit (Animation Event を受信)", this);
        }
    }
}
