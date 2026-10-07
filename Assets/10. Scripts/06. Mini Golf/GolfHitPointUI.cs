using System.Collections.Generic;
using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfHitPointUI : MonoBehaviour
    {
        [Header("Ref")]
        [SerializeField] private RectTransform hitDot; // 타점
        [SerializeField] private RectTransform line;   // 중앙 -> 타점 선 (Pivot X=0, Y=0.5, 길이는 radius)

        [Header("Settings")]
        [SerializeField] private float radius = 50f; // UI 상의 공 반지름(px)

        private bool isVisible;
        private List<GameObject> children = new List<GameObject>();

        private void Awake()
        {
            for (int i = 0; i < transform.childCount; i++)
                children.Add(transform.GetChild(i).gameObject);

            SetVisible(false);
        }

        public void Show(float hitAngle)
        {
            SetVisible(true);

            float rad = hitAngle * Mathf.Deg2Rad;

            // 타점 위치
            hitDot.anchoredPosition = new Vector2(-Mathf.Cos(rad), Mathf.Sin(rad)) * radius;

            // 선 회전
            line.localRotation = Quaternion.Euler(0f, 0f, 180f - hitAngle);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            if (isVisible == visible) return;

            isVisible = visible;
            for (int i = 0; i < children.Count; i++)
                children[i].gameObject.SetActive(visible);
        }
    }
}