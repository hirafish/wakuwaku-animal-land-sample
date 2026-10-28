using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q05
{
    public sealed class RunnerMover : MonoBehaviour, IPanelSection
    {
        private const float CodeDefaultSpeed = 10f;

        // コード上の初期値は 10f だが、シーンファイルに 5f がシリアライズされている
        public float speed = CodeDefaultSpeed;

        public string SectionTitle => "RunnerMover (シリアライズ値の確認)";

        private void Update()
        {
            var x = Mathf.PingPong(Time.time * speed, 6f) - 3f;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        public void DrawSection()
        {
            GUILayout.Label($"現在の speed (Inspectorの値): {speed:F1}");
            GUILayout.Label($"コード上の初期値: {CodeDefaultSpeed:F1}");
            GUILayout.Space(4f);
            GUILayout.Label("Hierarchy で Runner を選び、Inspector の Speed が 5 になっていることを確認してください。");
        }
    }
}
