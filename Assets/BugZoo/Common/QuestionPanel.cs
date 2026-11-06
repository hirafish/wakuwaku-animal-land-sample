using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugZoo.Common
{
    public sealed class QuestionPanel : MonoBehaviour
    {
        [SerializeField] private string _questionId = "Step 01";
        [SerializeField] private string _title = "";
        [SerializeField, TextArea(3, 10)] private string _steps = "";

        private readonly List<IPanelSection> _sections = new();
        private Vector2 _scroll;
        private bool _hidden;

        public void Configure(string questionId, string title, string steps)
        {
            _questionId = questionId;
            _title = title;
            _steps = steps;
        }

        private void Start()
        {
            // 動物アセット（Quirky Series）未導入のままシーンを直接開いた場合でも、
            // 親の Cube / Capsule が非表示にならないようフォールバックする
            foreach (var mr in FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if (!mr.enabled && mr.GetComponentsInChildren<Renderer>(true).Length <= 1)
                {
                    mr.enabled = true;
                }
            }

            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IPanelSection section)
                {
                    _sections.Add(section);
                }
            }
        }

        private void OnGUI()
        {
            var scale = Mathf.Max(1f, Screen.height / 900f);
            GUI.skin.label.fontSize = Mathf.RoundToInt(14 * scale);
            GUI.skin.button.fontSize = Mathf.RoundToInt(14 * scale);
            GUI.skin.box.fontSize = Mathf.RoundToInt(15 * scale);

            if (_hidden)
            {
                if (GUI.Button(new Rect(12f, 12f, 160f * scale, 36f * scale), "パネルを表示する"))
                {
                    _hidden = false;
                }
                return;
            }

            var width = Mathf.Min(440f * scale, Screen.width * 0.45f);
            var area = new Rect(12f, 12f, width, Screen.height - 24f);
            GUI.Box(area, $"{_questionId}: {_title}");

            GUILayout.BeginArea(new Rect(area.x + 10f, area.y + 32f * scale, area.width - 20f, area.height - 42f * scale));
            _scroll = GUILayout.BeginScrollView(_scroll);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("パネルを隠す", GUILayout.Height(28f * scale)))
            {
                _hidden = true;
            }
            if (GUILayout.Button("メニューに戻る", GUILayout.Height(28f * scale)))
            {
                SceneManager.LoadScene("Menu");
            }
            GUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(_steps))
            {
                GUILayout.Space(6f);
                GUILayout.Label("【手順】\n" + _steps);
            }

            foreach (var section in _sections)
            {
                GUILayout.Space(10f);
                GUILayout.Box(section.SectionTitle, GUILayout.ExpandWidth(true));
                section.DrawSection();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
