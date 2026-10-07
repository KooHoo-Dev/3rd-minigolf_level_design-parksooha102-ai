using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfAimIndicator : MonoBehaviour
    {
        [Header("Settings")] 
        public float minLength;
        public float maxLength;
        [SerializeField] private float hideProjectionThreshold = 0.001f;

        [Header("Ref")]
        [SerializeField] private LineRenderer launchLine;
        [SerializeField] private LineRenderer projectionLine;

        private void Awake()
        {
            SetupLine(launchLine);
            SetupLine(projectionLine);

            Hide();
        }

        private void SetupLine(LineRenderer lr)
        {
            lr.useWorldSpace = true; // 월드 공간 좌표계를 사용한다고 설정합니다.
            lr.positionCount = 2;    // 점 두개의 위치로 선을 그린다고 설정합니다.
        }

        // direction : 높이가 포함된 발사 방향(단위 벡터)
        public void Show(Vector3 origin, Vector3 direction, float power01)
        {
            // 0~1 사이의 power 값을 최소/최대 길이에 맞게 보간
            float length = Mathf.Lerp(minLength, maxLength, power01);

            // direction의 크기를 1로 정규화하여 방향만 추출
            Vector3 dirNormalized = direction.normalized;

            // 1. 발사 벡터 선 그리기
            DrawLine(launchLine, origin, origin + dirNormalized * length);

            // 2. y축 성분의 크기(절댓값)가 임계값 이하이면 정사영 감춤
            if (Mathf.Abs(dirNormalized.y) <= hideProjectionThreshold)
            {
                projectionLine.enabled = false;
                return;
            }

            // 임계값보다 크면 다시 선을 활성화
            projectionLine.enabled = true;

            // 3. XZ 평면 정사영 벡터 계산
            Vector3 projected = new Vector3(dirNormalized.x, 0.0f, dirNormalized.z);

            // 4. 정사영 선 그리기
            DrawLine(projectionLine, origin, origin + projected * length);
        }

        public void Hide()
        {
            launchLine.enabled = false;
            projectionLine.enabled = false;
        }

        private void DrawLine(LineRenderer lr, Vector3 from, Vector3 to)
        {
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);
            lr.enabled = true;
        }
    }
}