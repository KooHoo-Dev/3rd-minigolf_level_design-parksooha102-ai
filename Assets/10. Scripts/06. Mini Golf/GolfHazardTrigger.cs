using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfHazardTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<GolfBall>(out var ball))
            {
                ball.ReturnToLastShotPosition();
            }
        }
    }
}