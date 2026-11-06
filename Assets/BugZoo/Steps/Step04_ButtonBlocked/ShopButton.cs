using System.Collections;
using BugZoo.Common;
using UnityEngine;
using UnityEngine.UI;

namespace BugZoo.Steps.Step04
{
    [RequireComponent(typeof(Button))]
    public sealed class ShopButton : MonoBehaviour
    {
        [SerializeField] private Text _feedbackText;
        private Coroutine _feedbackRoutine;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnBuy);
        }

        public void SetFeedbackLabel(Text label)
        {
            _feedbackText = label;
        }

        private void OnBuy()
        {
            FrameLog.Info("買うボタンが押されました（おやつを購入）", this);
            if (_feedbackText != null)
            {
                if (_feedbackRoutine != null) StopCoroutine(_feedbackRoutine);
                _feedbackRoutine = StartCoroutine(ShowFeedbackRoutine());
            }
        }

        private IEnumerator ShowFeedbackRoutine()
        {
            _feedbackText.gameObject.SetActive(true);
            _feedbackText.text = "特製おやつを購入しました (-500 G)";
            yield return new WaitForSeconds(2.0f);
            _feedbackText.gameObject.SetActive(false);
        }
    }
}
