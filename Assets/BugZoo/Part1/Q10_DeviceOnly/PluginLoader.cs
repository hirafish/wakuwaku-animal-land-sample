using System;
using BugZoo.Common;
using UnityEngine;
using UnityEngine.Scripting;

namespace BugZoo.Part1.Q10
{
    public interface IHiddenFeature
    {
        string Describe();
    }

    // BUG: どこからも静的に参照されていないため、Managed Code Stripping で削られることがある
    public sealed class HiddenFeature : IHiddenFeature
    {
        public string Describe() => "HiddenFeature が動きました";
    }

    // FIX: [Preserve] を付けるとビルド時のコード削除から保護される
    [Preserve]
    public sealed class PreservedFeature : IHiddenFeature
    {
        [Preserve]
        public PreservedFeature()
        {
        }

        public string Describe() => "PreservedFeature が動きました";
    }

    public sealed class PluginLoader : BugScenario
    {
        private string _status = "未実行";

        public override string SectionTitle => "(b) リフレクションと Code Stripping";

        protected override void DrawControls()
        {
            GUILayout.Label($"結果: {_status}");
            if (GUILayout.Button("機能を動的に読み込む", GUILayout.Height(32f)))
            {
                var typeName = _applyFix
                    ? "BugZoo.Part1.Q10.PreservedFeature"
                    : "BugZoo.Part1.Q10.HiddenFeature";

                var type = Type.GetType(typeName);
                if (type == null)
                {
                    _status = $"型が見つかりません: {typeName}";
                    FrameLog.Error(_status, this);
                    return;
                }

                var instance = (IHiddenFeature)Activator.CreateInstance(type);
                _status = instance.Describe();
                FrameLog.Info(_status, this);
            }
        }
    }
}
