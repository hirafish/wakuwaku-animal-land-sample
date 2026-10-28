using System;
using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q01
{
    public sealed class ScoreView : MonoBehaviour
    {
        [NonSerialized] private int _score;

        public void SetScore(int value)
        {
            _score = value;
            // 第2引数（context）に this を渡すと、ログのクリックで「どのオブジェクトか」が分かる
            FrameLog.Info($"SetScore({value})", this);
        }

        private void OnGUI()
        {
            // プレハブアセット側では OnGUI は呼ばれない（シーン上の個体のみ描画される）
            var scale = Mathf.Max(1f, Screen.height / 900f);
            var style = new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.RoundToInt(28 * scale),
                alignment = TextAnchor.MiddleCenter
            };
            var width = 260f * scale;
            var height = 64f * scale;
            GUI.Box(new Rect((Screen.width - width) * 0.5f, 24f, width, height), $"SCORE: {_score}", style);
        }
    }
}
