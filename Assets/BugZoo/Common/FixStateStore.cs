using System.Collections.Generic;
using UnityEngine;

namespace BugZoo.Common
{
    public static class FixStateStore
    {
        private static readonly Dictionary<string, bool> States = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStore()
        {
            States.Clear();
        }

        public static bool TryGet(string key, out bool applyFix)
        {
            return States.TryGetValue(key, out applyFix);
        }

        public static void Set(string key, bool applyFix)
        {
            States[key] = applyFix;
        }
    }
}
