using System.Collections;
using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q04
{
    public sealed class AimMarker : BugScenario
    {
        [SerializeField] private Transform _target;
        [SerializeField] private GameObject _enemyPrefab;

        private void Update()
        {
            if (_applyFix)
            {
                // FIX: UnityEngine.Object は == null で Destroy 済みかどうかを確かめる
                if (_target == null)
                {
                    return;
                }
                transform.position = _target.position + Vector3.up * 1.2f;
            }
            else
            {
                // BUG: ?. 演算子は Unity の == オーバーロードを通らないため、Destroy 後に MissingReferenceException になる
                var position = _target?.position;
                if (position.HasValue)
                {
                    transform.position = position.Value + Vector3.up * 1.2f;
                }
            }
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"ターゲット状態: {(_target == null ? "なし（または Destroy 済み）" : _target.name)}");

            if (GUILayout.Button("敵を倒す (Destroy)", GUILayout.Height(32f)))
            {
                if (_target != null)
                {
                    StartCoroutine(DestroyAndLogRoutine(_target.gameObject));
                }
            }

            if (GUILayout.Button("敵を出す (Instantiate)", GUILayout.Height(32f)))
            {
                if (_target == null && _enemyPrefab != null)
                {
                    var spawned = Instantiate(_enemyPrefab, new Vector3(3f, 0.5f, 0f), Quaternion.identity);
                    spawned.name = "Enemy";
                    _target = spawned.transform;
                }
            }
        }

        private IEnumerator DestroyAndLogRoutine(GameObject enemy)
        {
            Destroy(enemy);
            FrameLog.Info($"Destroy 直後: _enemy == null は {enemy == null}", this);
            yield return null;
            FrameLog.Info($"次のフレーム: _enemy == null は {enemy == null}", this);
        }
    }
}
