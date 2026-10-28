using System;
using UnityEngine;

namespace BugZoo.Part3.Q29
{
    public static class GameEvents
    {
        public static event Action<int> ScoreChanged;

        public static int SubscriberCount => ScoreChanged != null ? ScoreChanged.GetInvocationList().Length : 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            ScoreChanged = null;
        }

        public static void Raise(int score)
        {
            ScoreChanged?.Invoke(score);
        }
    }
}
