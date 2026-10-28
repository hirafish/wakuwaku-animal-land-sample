using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugZoo.Common
{
    public abstract class BugScenario : MonoBehaviour, IPanelSection
    {
        [SerializeField] protected bool _applyFix;

        public virtual string SectionTitle => GetType().Name;

        private string StoreKey => $"{SceneManager.GetActiveScene().name}:{GetType().FullName}";

        protected virtual void Awake()
        {
            if (FixStateStore.TryGet(StoreKey, out var saved))
            {
                _applyFix = saved;
            }
        }

        public void DrawSection()
        {
            var modeText = _applyFix ? "【現在：修正版（FIX）】" : "【現在：バグ版（BUG）】";
            GUILayout.Label(modeText);

            var buttonLabel = _applyFix ? "バグ版に戻す" : "修正版に切り替える";
            if (GUILayout.Button(buttonLabel, GUILayout.Height(32f)))
            {
                _applyFix = !_applyFix;
                FixStateStore.Set(StoreKey, _applyFix);
                FrameLog.Info(_applyFix ? "修正版に切り替え" : "バグ版に切り替え", this);
                OnFixChanged();
            }

            GUILayout.Space(6f);
            DrawControls();
        }

        protected virtual void OnFixChanged()
        {
        }

        protected abstract void DrawControls();
    }
}
