using System;
using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfGoalTrigger : MonoBehaviour
    {
        public event Action BallEntered;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<GolfBall>(out var ball))
                BallEntered?.Invoke();
        }
    }
}