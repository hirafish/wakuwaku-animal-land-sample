using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q02
{
    public sealed class CoinScore : BugScenario
    {
        [SerializeField] private PlayerMover _player;
        [SerializeField] private Coin[] _coins;

        private int _score;

        public void OnCoinTouched(Coin coin, Collider other)
        {
            FrameLog.Info($"{coin.name} に {other.name} が触れた", coin);

            if (_applyFix)
            {
                // FIX: 取得済みなら2回目以降の接触を無視し、コライダーごと無効化する
                if (coin.Collected)
                {
                    return;
                }
                coin.Collected = true;
                _score += 1;
                coin.DisableBoth();
            }
            else
            {
                // BUG: Body と Feet の2つのコライダーが触れると2回加算される
                _score += 1;
                coin.HideVisualOnly();
            }
        }

        protected override void OnFixChanged()
        {
            RestartRun();
        }

        private void RestartRun()
        {
            _score = 0;
            if (_player != null)
            {
                _player.ResetPosition();
            }
            if (_coins != null)
            {
                foreach (var coin in _coins)
                {
                    if (coin != null) coin.ResetCoin();
                }
            }
        }

        protected override void DrawControls()
        {
            var totalCoins = _coins != null ? _coins.Length : 8;
            GUILayout.Label($"スコア: {_score} ／ コインの数: {totalCoins}");
            if (GUILayout.Button("もう一周（最初から）", GUILayout.Height(32f)))
            {
                RestartRun();
            }
            if (_player != null && GUILayout.Button($"速度 ×{_player.SpeedMultiplier:F0}（クリックで切替）", GUILayout.Height(32f)))
            {
                _player.ToggleSpeedMultiplier();
            }
        }
    }
}
