using System.Collections;
using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part3.Q29
{
    public sealed class PopupScenario : BugScenario
    {
        [SerializeField] private LeakyPopup _popupPrefab;
        private int _score;

        protected override void DrawControls()
        {
            GUILayout.Label($"イベント購読数: {GameEvents.SubscriberCount}");
            GUILayout.Label($"生成した Texture2D: {LeakyPopup.CreatedTextures} ／ 破棄した Texture2D: {LeakyPopup.DestroyedTextures}");

            if (GUILayout.Button("ポップアップを10回開いて閉じる", GUILayout.Height(32f)))
            {
                StartCoroutine(OpenAndCloseTenTimesRoutine());
            }

            if (GUILayout.Button("スコア変更イベントを発火", GUILayout.Height(32f)))
            {
                _score += 10;
                GameEvents.Raise(_score);
            }
        }

        private IEnumerator OpenAndCloseTenTimesRoutine()
        {
            for (var i = 0; i < 10; i++)
            {
                if (_popupPrefab != null)
                {
                    var popup = Instantiate(_popupPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity);
                    popup.CleanUpOnDestroy = _applyFix;
                    yield return null;
                    Destroy(popup.gameObject);
                }
                yield return null;
            }
            FrameLog.Info("ポップアップの10回開閉が完了しました", this);
        }
    }
}
