using UnityEngine;
using UnityEngine.SceneManagement;

namespace BugZoo.Menu
{
    public sealed class SceneMenu : MonoBehaviour
    {
        private Vector2 _scroll;

        private void OnGUI()
        {
            var scale = Mathf.Max(1f, Screen.height / 900f);
            GUI.skin.button.fontSize = Mathf.RoundToInt(15 * scale);
            GUI.skin.box.fontSize = Mathf.RoundToInt(18 * scale);

            var rect = new Rect(24f, 24f, Screen.width - 48f, Screen.height - 48f);
            GUI.Box(rect, "わくわくアニマルランド (Wakuwaku Animal Land) — デバッグ術 練習シーン一覧 (Step 1〜5)");

            GUILayout.BeginArea(new Rect(rect.x + 16f, rect.y + 44f * scale, rect.width - 32f, rect.height - 60f * scale));
            _scroll = GUILayout.BeginScrollView(_scroll);

            var count = SceneManager.sceneCountInBuildSettings;
            for (var i = 0; i < count; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                if (name == "Menu")
                {
                    continue;
                }

                if (GUILayout.Button(name, GUILayout.Height(36f * scale)))
                {
                    SceneManager.LoadScene(name);
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
