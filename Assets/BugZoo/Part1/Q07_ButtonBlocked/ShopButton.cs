using BugZoo.Common;
using UnityEngine;
using UnityEngine.UI;

namespace BugZoo.Part1.Q07
{
    [RequireComponent(typeof(Button))]
    public sealed class ShopButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnBuy);
        }

        private void OnBuy()
        {
            FrameLog.Info("買うボタンが押された！", this);
        }
    }
}
