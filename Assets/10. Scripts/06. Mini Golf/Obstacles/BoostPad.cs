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
                Vector3 worldDirection = transform.TransformDirection(boostDirection);

                ball.Shoot(worldDirection, boostPower, true);
            }
        }
    }
}