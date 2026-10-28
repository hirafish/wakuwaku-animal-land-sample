using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q09
{
    public sealed class EnemyStateMachine : MonoBehaviour
    {
        private EnemyState _state = EnemyState.Idle;
        private float _stateTimer;
        private int _lastChangedFrame = -1;
        private Renderer _renderer;

        private bool _hasPendingRequest;
        private EnemyState _pendingState;
        private int _pendingPriority;
        private string _pendingReason;

        public EnemyState State => _state;
        public bool BreakOnDoubleChange { get; set; }
        public int AttackRequests { get; private set; }
        public int ActualAttacks { get; private set; }
        public int OverwrittenAttacks { get; private set; }

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            UpdateColor();
        }

        public void ChangeStateImmediate(EnemyState next, string reason)
        {
            if (next == EnemyState.Attack)
            {
                AttackRequests++;
            }

            if (_lastChangedFrame == Time.frameCount)
            {
                if (_state == EnemyState.Attack && next == EnemyState.Stun)
                {
                    OverwrittenAttacks++;
                }
                FrameLog.Warn($"同一フレームで状態が上書きされました！ {_state} -> {next} ({reason})", this);
                if (BreakOnDoubleChange)
                {
                    Debug.Break();
                }
            }

            FrameLog.Info($"{_state} -> {next} ({reason})", this);
            _lastChangedFrame = Time.frameCount;
            _state = next;
            _stateTimer = next == EnemyState.Attack ? 0.3f : (next == EnemyState.Stun ? 0.5f : 0f);

            if (next == EnemyState.Attack)
            {
                ActualAttacks++;
            }
            UpdateColor();
        }

        public void RequestState(EnemyState next, int priority, string reason)
        {
            if (next == EnemyState.Attack)
            {
                AttackRequests++;
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
                    UpdateColor();
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
