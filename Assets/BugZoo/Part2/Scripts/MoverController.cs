using UnityEngine;

namespace BugZoo.Part2
{
    public sealed class MoverController : MonoBehaviour
    {
        public void MoveToX(float x)
        {
            var pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }
    }
}
