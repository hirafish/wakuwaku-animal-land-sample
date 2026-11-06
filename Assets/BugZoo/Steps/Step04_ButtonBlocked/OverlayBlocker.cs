using System.Collections.Generic;
using BugZoo.Common;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BugZoo.Steps.Step04
{
    public sealed class OverlayBlocker : BugScenario
    {
        [SerializeField] private Image _overlay;
        [SerializeField] private CanvasGroup _shopPanelGroup;

        private void Start()
        {
            ApplyState();
        }

        protected override void OnFixChanged()
        {
            ApplyState();
        }

        private void ApplyState()
        {
            if (_overlay != null)
            {
                // BUG: 透明な全画面 Image の Raycast Target がオンになっている
                // FIX: Raycast Target をオフにする
                _overlay.raycastTarget = !_applyFix;
            }
        }

        protected override void DrawControls()
        {
            if (_overlay != null)
            {
                GUILayout.Label($"FadeOverlay.raycastTarget: {_overlay.raycastTarget}");
            }
            if (_shopPanelGroup != null)
            {
                GUILayout.Label($"ShopPanel.interactable: {_shopPanelGroup.interactable}");
                if (GUILayout.Button("ShopPanel の Interactable を切り替え", GUILayout.Height(30f)))
                {
                    _shopPanelGroup.interactable = !_shopPanelGroup.interactable;
                }
            }

            if (GUILayout.Button("画面中央の RaycastAll 候補をログ出力", GUILayout.Height(32f)))
            {
                ProbeScreenCenter();
            }
        }

        [ContextMenu("画面中央のUI重なりを調査")]
        private void ProbeScreenCenter()
        {
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                FrameLog.Warn("EventSystem がシーンにありません", this);
                return;
            }

            var pointer = new PointerEventData(eventSystem)
            {
                position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            };
            var results = new List<RaycastResult>();
            eventSystem.RaycastAll(pointer, results);

            FrameLog.Info($"--- 画面中央の Raycast ヒット数: {results.Count} ---", this);
            foreach (var r in results)
            {
                FrameLog.Info($"候補: {r.gameObject.name}", r.gameObject);
            }
        }
    }
}
