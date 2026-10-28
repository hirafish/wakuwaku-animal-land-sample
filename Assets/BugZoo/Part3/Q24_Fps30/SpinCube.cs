using UnityEngine;

namespace BugZoo.Part3.Q24
{
    public sealed class SpinCube : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(new Vector3(30f, 60f, 15f) * Time.deltaTime);
        }
    }
}
