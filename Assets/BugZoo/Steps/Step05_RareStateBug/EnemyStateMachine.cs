using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Steps.Step05
{
    public sealed class EnemyStateMachine : MonoBehaviour
    {
        private EnemyState _state = EnemyState.Idle;
        private float _stateTimer;
        private int _lastChangedFrame = -1;
        private Renderer _renderer;
        private Animator _animator;
        private Vector3 _basePosition;

        private bool _hasPendingRequest;
        private EnemyState _pendingState;
        private int _pendingPriority;
        private string _pendingReason;

        private bool _isOverwrittenThisTime;

        public EnemyState State => _state;
        public int LastChangedFrame => _lastChangedFrame;
        public bool BreakOnDoubleChange { get; set; }
        public int AttackRequests { get; private set; }
        public int ActualAttacks { get; private set; }
        public int OverwrittenAttacks { get; private set; }
        public string LastMotionStatus { get; private set; } = "待機中 (Idle)";

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _animator = GetComponentInChildren<Animator>();
            _basePosition = transform.position;
            UpdateColor();
            PlayAnimation("Idle_A", 1f);
        }

        public void ResetStats()
        {
            AttackRequests = 0;
            ActualAttacks = 0;
            OverwrittenAttacks = 0;
            _hasPendingRequest = false;
            _isOverwrittenThisTime = false;
            _state = EnemyState.Idle;
            _stateTimer = 0f;
            LastMotionStatus = "待機中 (Idle)";
            transform.position = _basePosition;
            PlayAnimation("Idle_A", 1f);
            UpdateColor();
        }

        public void ChangeStateImmediate(EnemyState next, string reason)
        {
            if (next == EnemyState.Attack)
            {
                AttackRequests++;
                _isOverwrittenThisTime = false;
                LastMotionStatus = "突進攻撃中 (Attack)";
            }

            // 同じフレームで Attack から Stun に変わる場合にエディタを一時停止する
            if (_lastChangedFrame == Time.frameCount &&
                _state == EnemyState.Attack && next == EnemyState.Stun)
            {
                OverwrittenAttacks++;
                _isOverwrittenThisTime = true;
                LastMotionStatus = "攻撃が消滅して棒立ち (上書きバグ発生)";
                FrameLog.Warn($"同一フレームで状態が上書きされました: {_state} -> {next} ({reason})", this);
                if (BreakOnDoubleChange)
                {
                    Debug.Break();
                }
            }

            FrameLog.Info($"{_state} -> {next} ({reason})", this);
            _lastChangedFrame = Time.frameCount;
            _state = next;
            _stateTimer = 0.6f;

            if (next == EnemyState.Attack)
            {
                ActualAttacks++;
                PlayAnimation("Attack", 1f);
            }
            else if (next == EnemyState.Stun)
            {
                if (_isOverwrittenThisTime)
                {
                    // 攻撃が同一フレームで打ち消された場合：
                    // 突進モーションも消滅し、完全静止して「棒立ち」になる！
                    if (_animator != null)
                    {
                        _animator.Play("Idle_A", 0, 0f);
                        _animator.speed = 0f; // 羽ばたきも停止して完全静止
                    }
                    transform.position = _basePosition;
                }
                else
                {
                    PlayAnimation("Hit", 1f);
                }
            }
            else if (next == EnemyState.Idle)
            {
                PlayAnimation("Idle_A", 1f);
            }

            UpdateColor();
        }

        public void RequestState(EnemyState next, int priority, string reason)
        {
            if (next == EnemyState.Attack)
            {
                AttackRequests++;
            }

            // すでに攻撃モーション中なら、優先度の低いひるみ要求では中断しない
            if (_state == EnemyState.Attack && priority < 2)
            {
                return;
            }

            if (!_hasPendingRequest || priority > _pendingPriority)
            {
                if (_hasPendingRequest)
                {
                    FrameLog.Info($"要求 {_pendingState}({_pendingReason}) より優先度の高い {next}({reason}) を採用", this);
                    if (_pendingState == EnemyState.Attack && next == EnemyState.Stun)
                    {
                        OverwrittenAttacks++;
                    }
                }
                _hasPendingRequest = true;
                _pendingState = next;
                _pendingPriority = priority;
                _pendingReason = reason;
            }
            else
            {
                FrameLog.Info($"要求 {next}({reason}) は {_pendingState}({_pendingReason}) が優先されたためスキップ", this);
                if (next == EnemyState.Attack)
                {
                    OverwrittenAttacks++;
                }
            }
        }

        private void Update()
        {
            if (_state != EnemyState.Idle)
            {
                _stateTimer -= Time.deltaTime;
                if (_stateTimer <= 0f)
                {
                    _state = EnemyState.Idle;
                    _isOverwrittenThisTime = false;
                    transform.position = _basePosition;
                    LastMotionStatus = "待機中 (Idle)";
                    PlayAnimation("Idle_A", 1f);
                    UpdateColor();
                }
                else if (_state == EnemyState.Attack)
                {
                    // 攻撃中は前方にダイナミックにジャンプ突進して戻る！
                    var t = 1.0f - (_stateTimer / 0.6f); // 0 -> 1
                    var forward = Mathf.Sin(t * Mathf.PI) * -2.2f;
                    var upward = Mathf.Sin(t * Mathf.PI) * 1.2f;
                    transform.position = _basePosition + new Vector3(0f, upward, forward);
                }
                else if (_state == EnemyState.Stun)
                {
                    if (_isOverwrittenThisTime)
                    {
                        // 攻撃が上書きされたバグの場合は、突進が消滅して「一歩も動かずに棒立ち」になる！
                        transform.position = _basePosition;
                        if (_animator != null) _animator.speed = 0f;
                    }
                    else
                    {
                        // 通常のひるみは小さく後ろにのけぞる
                        var t = 1.0f - (_stateTimer / 0.6f);
                        var backward = Mathf.Sin(t * Mathf.PI) * 0.3f;
                        transform.position = _basePosition + new Vector3(0f, 0f, backward);
                    }
                }
            }
        }

        private void LateUpdate()
        {
            if (_hasPendingRequest)
            {
                _hasPendingRequest = false;
                ChangeStateImmediate(_pendingState, _pendingReason);
            }
        }

        private void PlayAnimation(string stateName, float speed)
        {
            if (_animator == null) return;
            _animator.speed = speed;
            _animator.Play(stateName, 0, 0f);
        }

        private void UpdateColor()
        {
            if (_renderer == null) return;
            _renderer.material.color = _state switch
            {
                EnemyState.Attack => Color.red,
                EnemyState.Stun => Color.yellow,
                _ => Color.white
            };
        }
    }
}
