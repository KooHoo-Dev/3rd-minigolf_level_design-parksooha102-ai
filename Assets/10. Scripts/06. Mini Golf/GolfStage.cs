using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfStage : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int par = 3;
        [SerializeField] private Transform startPoint;
        [SerializeField] private GolfGoalTrigger goal;

        public int Par => par;
        public Vector3 StartPosition => startPoint.position;
        public GolfGoalTrigger Goal => goal;
    }
}