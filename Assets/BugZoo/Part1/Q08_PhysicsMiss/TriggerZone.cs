using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q08
{
    public sealed class TriggerZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            FrameLog.Info($"TriggerZone に {other.name} が入った", this);
        }

        private void OnTriggerExit(Collider other)
        {
            FrameLog.Info($"TriggerZone から {other.name} が出た", this);
        }
    }
}
