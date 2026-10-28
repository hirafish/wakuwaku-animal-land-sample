using UnityEngine;

namespace BugZoo.Part1.Q02
{
    public sealed class Coin : MonoBehaviour
    {
        [SerializeField] private CoinScore _score;
        private Collider _collider;
        private Renderer _renderer;

        public bool Collected { get; set; }

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _renderer = GetComponent<Renderer>();
        }

        public void ResetCoin()
        {
            Collected = false;
            if (_collider != null) _collider.enabled = true;
            if (_renderer != null) _renderer.enabled = true;
        }

        public void HideVisualOnly()
        {
            if (_renderer != null) _renderer.enabled = false;
        }

        public void DisableBoth()
        {
            if (_collider != null) _collider.enabled = false;
            if (_renderer != null) _renderer.enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_score != null)
            {
                _score.OnCoinTouched(this, other);
            }
        }
    }
}
