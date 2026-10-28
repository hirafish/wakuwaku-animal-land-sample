using System;
using System.Collections;
using System.IO;
using BugZoo.Common;
using UnityEngine;
using UnityEngine.Networking;

namespace BugZoo.Part1.Q10
{
    public sealed class ConfigLoader : BugScenario
    {
        private string _lastResult = "未読み込み";

        public override string SectionTitle => "(a) StreamingAssets の読み込み";

        protected override void DrawControls()
        {
            GUILayout.Label($"結果: {_lastResult}");
            if (GUILayout.Button("設定ファイルを読む", GUILayout.Height(32f)))
            {
                var path = Path.Combine(Application.streamingAssetsPath, "bugzoo_config.json");
                if (_applyFix)
                {
                    StartCoroutine(LoadWithWebRequestOrFileRoutine(path));
                }
                else
                {
                    LoadWithFileOnly(path);
                }
            }
        }

        private void LoadWithFileOnly(string path)
        {
            try
            {
                // BUG: Android や Web では StreamingAssets のパスが URL になるため File.ReadAllText が失敗する
                _lastResult = File.ReadAllText(path);
                FrameLog.Info($"読み込み成功: {_lastResult}", this);
            }
            catch (Exception ex)
            {
                _lastResult = $"失敗: {ex.GetType().Name}";
                FrameLog.Error($"StreamingAssets 読み込みエラー: {ex.Message}", this);
            }
        }

        private IEnumerator LoadWithWebRequestOrFileRoutine(string path)
        {
            // FIX: URL 形式（jar:file:// や http）の場合は UnityWebRequest を使う
            if (path.Contains("://") || path.Contains(":///"))
            {
                using var req = UnityWebRequest.Get(path);
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    _lastResult = req.downloadHandler.text;
                    FrameLog.Info($"UnityWebRequest で読み込み成功: {_lastResult}", this);
                }
                else
                {
                    _lastResult = $"失敗: {req.error}";
                    FrameLog.Error($"UnityWebRequest エラー: {req.error}", this);
                }
            }
            else
            {
                LoadWithFileOnly(path);
            }
        }
    }
}
