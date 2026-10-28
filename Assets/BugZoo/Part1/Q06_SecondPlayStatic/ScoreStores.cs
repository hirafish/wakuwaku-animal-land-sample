using UnityEngine;

namespace BugZoo.Part1.Q06
{
    public static class BuggyScoreStore
    {
        // BUG: Enter Play Mode Settings で Reload Domain がオフのとき、前回の Play の値が残る
        public static int Total;
    }

    public static class FixedScoreStore
    {
        public static int Total;

        // FIX: SubsystemRegistration で Play 開始時に必ず初期化する
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Total = 0;
        }
    }
}
