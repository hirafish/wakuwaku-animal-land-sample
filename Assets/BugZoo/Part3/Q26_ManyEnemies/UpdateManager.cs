using System.Collections.Generic;
using UnityEngine;

namespace BugZoo.Part3.Q26
{
    public sealed class UpdateManager : MonoBehaviour
    {
        public static UpdateManager Instance { get; private set; }

        private readonly List<IUpdatable> _items = new(4096);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            Instance = null;
        }

        private void Awake()
        {
            Instance = this;
        }

        public void Register(IUpdatable item)
        {
            _items.Add(item);
        }

        public void Unregister(IUpdatable item)
        {
            _items.Remove(item);
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            for (var i = 0; i < _items.Count; i++)
            {
                _items[i].CustomUpdate(dt);
            }
        }
    }
}
