using BugZoo.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugZoo.Part1.Q03
{
    public sealed class EnemySpawner : BugScenario
    {
        private bool _registered;

        protected override void Awake()
        {
            base.Awake();
            FrameLog.Info("[EnemySpawner] Awake", this);

            if (!_applyFix)
            {
                // BUG: Awake の時点では GameManager.Instance がまだ null のことがある
                GameManager.Instance.Register(this);
                _registered = true;
            }
        }

        private void Start()
        {
            FrameLog.Info("[EnemySpawner] Start", this);

            if (_applyFix)
            {
                // FIX: すべての Awake が終わった後の Start で他のオブジェクトに触る
                GameManager.Instance.Register(this);
                _registered = true;
            }
        }

        protected override void OnFixChanged()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"登録状態: {(_registered ? "登録成功" : "未登録（例外発生）")}");
            if (GUILayout.Button("シーンを読み込み直す", GUILayout.Height(32f)))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
