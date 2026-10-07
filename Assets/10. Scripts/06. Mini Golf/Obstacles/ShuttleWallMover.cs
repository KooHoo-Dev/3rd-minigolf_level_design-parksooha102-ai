using UnityEngine;

namespace Study3D.MiniGolf
{
    public class ShuttleWallMover : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float goalTime = 2.0f;
        [SerializeField] private Vector3 goalPosition;

        private Vector3 startPosition;
        private float currentTime = 0.0f;
        private Rigidbody rBody;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody>();
            rBody.isKinematic = true;

            startPosition = transform.position;
            goalPosition += startPosition;
        }

        private void FixedUpdate()
        {
            currentTime += Time.fixedDeltaTime;

            float progress = currentTime / goalTime;

            Vector3 currentPosition = Vector3.Lerp(startPosition, goalPosition, progress);

            rBody.MovePosition(currentPosition);

            if (currentTime > goalTime)
            {
                currentTime = 0.0f;

                Vector3 temp = startPosition;
                startPosition = goalPosition;
                goalPosition = temp;
            }
        }

    }
}