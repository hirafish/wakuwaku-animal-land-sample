using UnityEngine;

namespace BugZoo.Part3.Q29
{
    public sealed class LeakyPopup : MonoBehaviour
    {
        public static int CreatedTextures { get; private set; }
        public static int DestroyedTextures { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCounters()
        {
            CreatedTextures = 0;
            DestroyedTextures = 0;
        }

        public bool CleanUpOnDestroy { get; set; }

        private Texture2D _runtimeTexture;
        private Material _instancedMaterial;

        private void OnEnable()
        {
            GameEvents.ScoreChanged += OnScoreChanged;
        }

        private void Start()
        {
            _runtimeTexture = new Texture2D(1024, 1024, TextureFormat.RGBA32, false);
            CreatedTextures++;

            if (TryGetComponent<Renderer>(out var r))
            {
                // renderer.material に触るとマテリアルが複製される
                _instancedMaterial = r.material;
                _instancedMaterial.mainTexture = _runtimeTexture;
            }
        }

        private void OnDisable()
        {
            if (CleanUpOnDestroy)
            {
                // FIX: static イベントの購読を解除する
                GameEvents.ScoreChanged -= OnScoreChanged;
            }
        }

        private void OnDestroy()
        {
            if (CleanUpOnDestroy)
            {
                // FIX: 動的に作った Texture2D と複製マテリアルを明示的に Destroy する
                if (_runtimeTexture != null)
                {
                    Destroy(_runtimeTexture);
                    DestroyedTextures++;
                }
                if (_instancedMaterial != null)
                {
                    Destroy(_instancedMaterial);
                }
            }
        }

        private void OnScoreChanged(int score)
        {
            // BUG: 購読解除を忘れていると、Destroy 後にここが呼ばれて MissingReferenceException になる
            transform.localScale = Vector3.one * (1f + (score % 3) * 0.2f);
        }
    }
}
