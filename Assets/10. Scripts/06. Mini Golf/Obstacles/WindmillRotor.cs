using UnityEngine;

namespace Study3D.MiniGolf
{
    public class WindmillRotor: MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] public float rotateSpeed;

        private void FixedUpdate()
        {
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }
    }
}