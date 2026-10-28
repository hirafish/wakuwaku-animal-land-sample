using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q08
{
    public sealed class TriggerZoneScenario : BugScenario
    {
        [SerializeField] private GameObject _zoneWalker;

        public override string SectionTitle => "(a) Trigger が反応しない";

        private void Start()
        {
            ApplyRigidbodyState();
        }

        protected override void OnFixChanged()
        {
            ApplyRigidbodyState();
        }

        private void ApplyRigidbodyState()
        {
            if (_zoneWalker == null) return;

            var rb = _zoneWalker.GetComponent<Rigidbody>();
            if (_applyFix)
            {
                // FIX: 動く側に Kinematic な Rigidbody を付ける
                if (rb == null)
                {
                    rb = _zoneWalker.AddComponent<Rigidbody>();
                }
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            else
            {
                // BUG: どちらにも Rigidbody がないと OnTriggerEnter は呼ばれない
                if (rb != null)
                {
                    Destroy(rb);
                }
            }
        }

        protected override void DrawControls()
        {
            var hasRb = _zoneWalker != null && _zoneWalker.GetComponent<Rigidbody>() != null;
            GUILayout.Label($"ZoneWalker の Rigidbody: {(hasRb ? "あり" : "なし")}");
        }
    }
}
