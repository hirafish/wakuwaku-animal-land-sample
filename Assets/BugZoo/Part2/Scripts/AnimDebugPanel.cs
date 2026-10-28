using System.Collections;
using System.Collections.Generic;
using BugZoo.Common;
using UnityEngine;
using UnityEngine.Playables;

namespace BugZoo.Part2
{
    public sealed class AnimDebugPanel : MonoBehaviour, IPanelSection
    {
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int IsHitHash = Animator.StringToHash("IsHit");

        [SerializeField] private Animator _targetAnimator;
        [SerializeField] private MoverController _mover;
        [SerializeField] private Animator _pauseIconAnimator;
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private AnimSpawner _spawner;
        [SerializeField] private AttackEndDetector _endDetector;

        private readonly Dictionary<int, string> _stateNames = new();
        private AnimatorControllerParameter[] _parameters;
        private bool _isRecording;
        private bool _isPlayback;
        private float _playbackTime;

        public string SectionTitle => "AnimationLab 操作・状態パネル (Q12〜Q23, Q31)";

        private void Start()
        {
            foreach (var name in new[] { "Idle", "Jump", "Attack", "Hit" })
            {
                _stateNames[Animator.StringToHash(name)] = name;
            }
            if (_targetAnimator != null)
            {
                _parameters = _targetAnimator.parameters;
            }
        }

        public void DrawSection()
        {
            if (_targetAnimator == null)
            {
                GUILayout.Label("Target Animator が未設定です");
                return;
            }

            DrawAnimatorStatus();
            GUILayout.Space(6f);
            DrawActionButtons();
            GUILayout.Space(6f);
            DrawRecordingControls();
        }

        private void DrawAnimatorStatus()
        {
            if (!_targetAnimator.gameObject.activeInHierarchy)
            {
                GUILayout.Label("[状態] Target は現在非アクティブです");
                return;
            }

            var info = _targetAnimator.GetCurrentAnimatorStateInfo(0);
            var stateName = _stateNames.TryGetValue(info.shortNameHash, out var n) ? n : info.shortNameHash.ToString();
            var inTrans = _targetAnimator.IsInTransition(0);
            var nextText = "";
            if (inTrans)
            {
                var nextInfo = _targetAnimator.GetNextAnimatorStateInfo(0);
                var nextName = _stateNames.TryGetValue(nextInfo.shortNameHash, out var nn) ? nn : "?";
                nextText = $" -> {nextName}";
            }

            GUILayout.Label($"ステート: {stateName}{nextText} | normTime: {info.normalizedTime:F2} | 遷移中: {inTrans}");

            if (_parameters != null)
            {
                var paramSummary = "Parameters: ";
                foreach (var p in _parameters)
                {
                    if (p.type == AnimatorControllerParameterType.Bool || p.type == AnimatorControllerParameterType.Trigger)
                    {
                        paramSummary += $"{p.name}={_targetAnimator.GetBool(p.nameHash)}  ";
                    }
                }
                GUILayout.Label(paramSummary);
            }
        }

        private void DrawActionButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Jump (Q12)", GUILayout.Height(30f)))
            {
                _targetAnimator.SetTrigger(JumpHash);
            }
            if (GUILayout.Button("Attack (Q13/Q21)", GUILayout.Height(30f)))
            {
                if (_endDetector != null) _endDetector.NotifyAttackStarted();
                _targetAnimator.SetTrigger(AttackHash);
            }
            if (GUILayout.Button("Hit ON (Q15)", GUILayout.Height(30f)))
            {
                _targetAnimator.SetBool(IsHitHash, true);
            }
            if (GUILayout.Button("Hit OFF", GUILayout.Height(30f)))
            {
                _targetAnimator.SetBool(IsHitHash, false);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Q14 typo: SetTrigger("jump")", GUILayout.Height(28f)))
            {
                // BUG: 小文字の "jump" は存在しないため Console に警告が出る
                _targetAnimator.SetTrigger("jump");
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Q18 隠す→1秒後に表示", GUILayout.Height(28f)))
            {
                StartCoroutine(HideAndShowRoutine());
            }
            var keepLabel = $"keepStateOnDisable: {_targetAnimator.keepAnimatorStateOnDisable}";
            if (GUILayout.Button(keepLabel, GUILayout.Height(28f)))
            {
                _targetAnimator.keepAnimatorStateOnDisable = !_targetAnimator.keepAnimatorStateOnDisable;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Q19 timeScale: {Time.timeScale:F0} (切替)", GUILayout.Height(28f)))
            {
                Time.timeScale = Time.timeScale > 0f ? 0f : 1f;
            }
            if (_pauseIconAnimator != null && GUILayout.Button($"PauseIcon Mode: {_pauseIconAnimator.updateMode}", GUILayout.Height(28f)))
            {
                _pauseIconAnimator.updateMode = _pauseIconAnimator.updateMode == AnimatorUpdateMode.Normal
                    ? AnimatorUpdateMode.UnscaledTime
                    : AnimatorUpdateMode.Normal;
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Q20 イベントの受け取り先 切り替え (Target ⇔ Mover)", GUILayout.Height(28f)))
            {
                ToggleEventReceiver();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Q22 Attack + 画面外へ", GUILayout.Height(28f)))
            {
                if (_mover != null) _mover.MoveToX(50f);
                if (_endDetector != null) _endDetector.NotifyAttackStarted();
                _targetAnimator.SetTrigger(AttackHash);
            }
            if (GUILayout.Button("画面内に戻す", GUILayout.Height(28f)))
            {
                if (_mover != null) _mover.MoveToX(0f);
            }
            if (GUILayout.Button($"Culling: {_targetAnimator.cullingMode}", GUILayout.Height(28f)))
            {
                _targetAnimator.cullingMode = _targetAnimator.cullingMode switch
                {
                    AnimatorCullingMode.AlwaysAnimate => AnimatorCullingMode.CullUpdateTransforms,
                    AnimatorCullingMode.CullUpdateTransforms => AnimatorCullingMode.CullCompletely,
                    _ => AnimatorCullingMode.AlwaysAnimate
                };
            }
            GUILayout.EndHorizontal();

            if (_director != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Q23 Timeline 再生", GUILayout.Height(28f)))
                {
                    _director.Play();
                }
                if (GUILayout.Button($"WrapMode: {_director.extrapolationMode}", GUILayout.Height(28f)))
                {
                    _director.extrapolationMode = _director.extrapolationMode == DirectorWrapMode.Hold
                        ? DirectorWrapMode.None
                        : DirectorWrapMode.Hold;
                }
                GUILayout.EndHorizontal();
            }

            if (_spawner != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Q31 300体出す", GUILayout.Height(28f)))
                {
                    _spawner.Spawn300();
                }
                if (GUILayout.Button("全部消す", GUILayout.Height(28f)))
                {
                    _spawner.ClearAll();
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawRecordingControls()
        {
            GUILayout.Label("【M22: Animator 記録・巻き戻し再生】");
            GUILayout.BeginHorizontal();
            if (!_isRecording && !_isPlayback && GUILayout.Button("記録開始"))
            {
                _targetAnimator.StartRecording(300);
                _isRecording = true;
            }
            if (_isRecording && GUILayout.Button("記録終了して再生"))
            {
                _targetAnimator.StopRecording();
                _isRecording = false;
                _targetAnimator.StartPlayback();
                _isPlayback = true;
                _playbackTime = _targetAnimator.recorderStartTime;
            }
            if (_isPlayback && GUILayout.Button("再生終了"))
            {
                _targetAnimator.StopPlayback();
                _isPlayback = false;
            }
            GUILayout.EndHorizontal();

            if (_isPlayback && _targetAnimator.recorderStopTime > _targetAnimator.recorderStartTime)
            {
                _playbackTime = GUILayout.HorizontalSlider(
                    _playbackTime,
                    _targetAnimator.recorderStartTime,
                    _targetAnimator.recorderStopTime);
                _targetAnimator.playbackTime = _playbackTime;
            }
        }

        private IEnumerator HideAndShowRoutine()
        {
            _targetAnimator.gameObject.SetActive(false);
            yield return new WaitForSecondsRealtime(1f);
            _targetAnimator.gameObject.SetActive(true);
        }

        private void ToggleEventReceiver()
        {
            var onTarget = _targetAnimator.GetComponent<AttackEventReceiver>();
            if (onTarget != null)
            {
                Destroy(onTarget);
                if (_mover != null && _mover.GetComponent<AttackEventReceiver>() == null)
                {
                    _mover.gameObject.AddComponent<AttackEventReceiver>();
                }
                FrameLog.Info("AttackEventReceiver を親(Mover)だけにしました（バグ状態：no receiver エラーが出ます）", this);
            }
            else
            {
                if (_mover != null && _mover.TryGetComponent<AttackEventReceiver>(out var onMover))
                {
                    Destroy(onMover);
                }
                _targetAnimator.gameObject.AddComponent<AttackEventReceiver>();
                FrameLog.Info("AttackEventReceiver を Animator と同じ Target に付けました（修正状態）", this);
            }
        }
    }
}
