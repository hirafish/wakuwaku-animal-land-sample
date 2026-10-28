using BugZoo.Common;
using UnityEngine;

namespace BugZoo.Part1.Q03
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
            FrameLog.Info("[GameManager] Awake で Instance を初期化しました", this);
        }

        public void Register(EnemySpawner spawner)
        {
            FrameLog.Info($"[GameManager] {spawner.name} の登録 OK", this);
        }
    }
}
