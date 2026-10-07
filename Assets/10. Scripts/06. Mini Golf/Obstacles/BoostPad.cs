using UnityEngine;

namespace Study3D.MiniGolf
{
    public class BoostPad : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float boostPower = 25f;
        [SerializeField] private Vector3 boostDirection = Vector3.forward;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<GolfBall>(out var ball))
            {
                ball.Shoot(boostDirection, boostPower, true);
            }
        }
    }
}