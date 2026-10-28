using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace BugZoo.Common
{
    public static class FrameLog
    {
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Info(string message, Object context = null)
        {
            Debug.Log($"[{Time.frameCount}] {message}", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Warn(string message, Object context = null)
        {
            Debug.LogWarning($"[{Time.frameCount}] {message}", context);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Error(string message, Object context = null)
        {
            Debug.LogError($"[{Time.frameCount}] {message}", context);
        }
    }
}
