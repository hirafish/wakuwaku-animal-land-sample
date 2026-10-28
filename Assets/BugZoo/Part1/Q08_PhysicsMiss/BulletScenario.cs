using System.Collections;
using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q08
{
    public sealed class BulletScenario : BugScenario
    {
        [SerializeField] private Bullet _bulletPrefab;
        [SerializeField] private Transform _muzzle;

        private float _speed = 200f;
        private int _hits;
        private int _passed;

        public override string SectionTitle => "(b) 速い弾が壁をすり抜ける";

        public void ReportHit()
        {
            _hits++;
        }

        public void ReportPassThrough()
        {
            _passed++;
        }

        protected override void DrawControls()
        {
            GUILayout.Label($"弾の速さ: {_speed:F0} ／ 命中: {_hits} ／ すり抜け: {_passed}");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("速さ 50")) _speed = 50f;
            if (GUILayout.Button("速さ 200")) _speed = 200f;
            if (GUILayout.Button("速さ 500")) _speed = 500f;
            GUILayout.EndHorizontal();

            if (GUILayout.Button("10発撃つ", GUILayout.Height(32f)))
            {
                StartCoroutine(FireBurstRoutine());
            }
        }

        private IEnumerator FireBurstRoutine()
        {
            for (var i = 0; i < 10; i++)
            {
                if (_bulletPrefab != null && _muzzle != null)
                {
                    var bullet = Instantiate(_bulletPrefab, _muzzle.position, Quaternion.identity);
                    // BUG: Discrete だと1フレームで壁を飛び越える
                    // FIX: ContinuousDynamic で連続衝突判定を行う
                    var mode = _applyFix ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
                    bullet.Initialize(this, _speed, mode);
                }
                yield return new WaitForSeconds(0.08f);
            }
        }
    }
}
