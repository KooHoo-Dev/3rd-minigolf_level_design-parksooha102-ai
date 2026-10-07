using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace Study3D.MiniGolf
{
    public class GolfPlayer : MonoBehaviour
    {
        private enum State
        {
            Disabled, // 공이 움직이는 중이라 쏠 수 없음
            Idle,     // 쏠 수 있음, 마우스가 잡기 영역 밖
            Hover,    // 쏠 수 있음, 마우스가 잡기 영역 안
            Dragging, // 드래그 중 (조준 중)
            Locked,   // 외부(스테이지)가 입력을 잠금. 클리어 후 등. ResetInput()으로만 풀림
            Unexpected,
        }

        [Header("Settings")]
        [SerializeField] private float minPower;
        [SerializeField] private float maxPower;

        [Header("Drag Settings")]
        [SerializeField, Min(0.1f)] private float grabAreaScale = 1f;
        [SerializeField, Min(1f)] private float maxPullDistance = 200f;

        [Header("Hit Point Settings")]
        [SerializeField] private float hitAngleStep = 5f;
        [SerializeField] private float minHitAngle = -89f;
        [SerializeField] private float maxHitAngle = 89f;

        [Header("Ref")]
        [SerializeField] private GolfBall ball;
        [SerializeField] private GolfAimIndicator aimIndicator;
        [SerializeField] private GolfHitPointUI hitPointUI;
        [SerializeField] private Camera cam;
        [SerializeField] private GolfPlainCamera plainCamera;
        [SerializeField] private GolfOrbitCamera orbitCamera;

        private State state;

        private float power01;           // 0~1 까지로 치환한 세기
        private Vector3 launchDirection;
        private float hitAngle;

        private Vector2 ballScreenPos;  // 공 중심의 화면 좌표(픽셀). 매 프레임 갱신
        private float grabScreenRadius; // 잡기 영역의 화면상 반지름(픽셀) = 공 화면 반지름 * grabAreaScale. 매 프레임 갱신

        public bool IsDragging => state == State.Dragging;

        public event System.Action Shot;

        // 입력을 잠급니다. 드래그 중이었다면 조준 데이터도 함께 정리됩니다.
        // 잠금은 ResetInput()으로만 풀립니다.
        public void Lock()
        {
            ResetAim();
            state = State.Locked;
        }

        // 잠금 해제 + 드래그 취소. 다음 Update에서 현재 상황에 맞는 상태로 다시 판정됩니다.
        // 스테이지 리셋처럼 "입력 상태를 처음으로" 되돌릴 때 사용합니다.
        public void ResetInput()
        {
            ResetAim();
            state = State.Idle;
        }

        // 한 프레임의 흐름: 화면 정보 갱신 -> 상태 갱신 -> 피드백
        private void Update()
        {
            if (state == State.Locked)
            {
                UpdateFeedback();
                return;
            }

            // 줌이 바뀔 수 있으니 매 프레임 공의 화면 위치와 잡기 영역 반지름을 갱신합니다.
            UpdateGrabArea();

            bool canShoot = ball.IsStopped;
            Vector2 mousePos = SimpleInput.GetMousePosition();

            // 휠은 여기서 한 번만 읽고, 프레임 시작 시점의 상태로 소유자를 정합니다.
            // 드래그 중이면 타점, 아니면 카메라 줌입니다. (Update 실행 순서에 의존하지 않습니다)
            float wheel = SimpleInput.GetMouseScrollDeltaRaw();

            if (state == State.Dragging)
            {
                UpdateDragging(mousePos, canShoot, wheel);
            }
            else
            {
                orbitCamera.Zoom(wheel);

                state = ResolveFreeState(mousePos, canShoot);

                if (state == State.Hover && SimpleInput.GetMouseButtonDown(SimpleInput.MouseButton.Left))
                    BeginDrag();
            }

            UpdateFeedback();
        }

        // 드래그 중이 아닐 때의 상태를 판정합니다. (Locked는 호출 전에 걸러집니다)
        // - 쏠 수 없음                                 : Disabled
        // - 쏠 수 있고 마우스가 잡기 영역 안            : Hover
        // - 그 외                                       : Idle
        private State ResolveFreeState(Vector2 mousePos, bool canShoot)
        {
            if (canShoot == false) return State.Disabled;
            return IsMouseOverGrabArea(mousePos) ? State.Hover : State.Idle;
        }

        private void BeginDrag()
        {
            state = State.Dragging;
            ResetAim();
        }

        // 드래그 상태를 정리하고 자유 상태로 돌아갑니다.
        // 발사 후 / ESC 취소 / 공 안쪽에서 놓음 / 쏠 수 없게 됨 모두 이 한 곳을 지납니다.
        private void EndDrag(Vector2 mousePos, bool canShoot)
        {
            ResetAim();
            state = ResolveFreeState(mousePos, canShoot);
        }

        private void ResetAim()
        {
            power01 = 0.0f;
            launchDirection = Vector3.zero;
            hitAngle = 0.0f;
        }

        // 드래그 중 한 프레임의 처리 순서만 보여주는 목차입니다. 세부 계산은 각 메서드에 있습니다.
        private void UpdateDragging(Vector2 mousePos, bool canShoot, float wheel)
        {
            // 취소: ESC (우클릭은 카메라 오빗에 사용)
            // 드래그 도중 공이 쏠 수 없는 상태가 되어도 취소합니다.
            if (SimpleInput.GetKeyDown(Key.Escape) || canShoot == false)
            {
                EndDrag(mousePos, canShoot);
                return;
            }

            UpdateHitAngle(wheel);

            // 공의 화면 위치 -> 현재 마우스 위치 (화면 기준 벡터)
            Vector2 drag = mousePos - ballScreenPos;
            float dragDistance = drag.magnitude;

            UpdatePower(dragDistance);
            UpdateLaunchDirection(drag);

            // 종료: 좌클릭을 떼면 발사 (잡기 영역 안쪽에서 떼면 발사 없이 종료)
            if (SimpleInput.GetMouseButtonUp(SimpleInput.MouseButton.Left))
            {
                if (dragDistance > grabScreenRadius)
                    Fire();

                EndDrag(mousePos, canShoot);
            }
        }

        // 타점 조절: 휠업 = 타점 아래로(-), 휠다운 = 타점 위로(+)
        private void UpdateHitAngle(float wheel)
        {
            int step = System.Math.Sign(wheel);   // -1, 0, 1
            hitAngle = Mathf.Clamp(hitAngle - step * hitAngleStep, minHitAngle, maxHitAngle);
        }

        // 파워: 잡기 영역 경계에서 0, 경계로부터 maxPullDistance만큼 더 끌면 1
        // (잡기 영역 안쪽은 "잡고 있는 구역", 바깥으로 끌어낸 만큼이 파워)
        // maxPullDistance는 [Min(1)]이 보장하므로 나누기 방어가 필요 없습니다.
        private void UpdatePower(float dragDistance)
        {
            power01 = Mathf.Clamp01((dragDistance - grabScreenRadius) / maxPullDistance);
        }

        // 방향: 화면 드래그 벡터 -> 월드 수평 방향 -> 타점 기울기 적용
        private void UpdateLaunchDirection(Vector2 drag)
        {
            if (Mathf.Approximately(drag.magnitude, 0)) return;

            // 1. (dx, 0, dy)를 현재 카메라 yaw만큼 회전시켜 월드 수평 벡터로 변환
            Vector3 screenVector = new Vector3(drag.x, 0.0f, drag.y);
            Vector3 worldVector = Quaternion.Euler(0.0f, plainCamera.Yaw, 0.0f) * screenVector;

            // 2. 슬링샷이므로 부호 반전 (당긴 반대 방향) + 정규화
            Vector3 horizontalDir = -worldVector.normalized;

            // 3. 발사 방향의 오른쪽 축(Right)을 기준으로 hitAngle만큼 회전
            Vector3 rightDir = Vector3.Cross(Vector3.up, horizontalDir);
            launchDirection = Quaternion.AngleAxis(hitAngle, rightDir) * horizontalDir;
        }

        private void Fire()
        {
            float power = Mathf.Lerp(minPower, maxPower, power01);
            ball.Shoot(launchDirection, power);
            Shot?.Invoke();
        }

        private void UpdateGrabArea()
        {
            // 1. 구의 중심 위치와 월드 반지름(Radius) 준비
            Vector3 centerPos = ball.transform.position;
            float worldRadius = ball.Radius;

            // 2. 중심점과 표면(상단) 점을 각각 화면 픽셀 좌표로 변환
            Vector3 screenCenter = cam.WorldToScreenPoint(centerPos);
            Vector3 screenEdge = cam.WorldToScreenPoint(centerPos + cam.transform.up * worldRadius);

            // 3. 두 화면 좌표 사이의 픽셀 거리가 바로 화면 반지름
            grabScreenRadius = Vector3.Distance(screenCenter, screenEdge) * grabAreaScale;
            ballScreenPos = screenCenter;
        }

        private bool IsMouseOverGrabArea(Vector2 mousePos)
        {
            return (mousePos - ballScreenPos).sqrMagnitude <= grabScreenRadius * grabScreenRadius;
        }

        // 보통 플레이어의 입력정보들에 관련된 UI나 피드백들은
        // 하나의 함수로 모아두는게 좋습니다. (찢지 마세여)
        // (생각외로 엄청 많이 바뀜)
        // 피드백은 bool 조합이 아니라 현재 State만 보고 결정하며,
        // 조준선 / 타점 UI / 공 색 모두 여기서 push 방식으로 한 번에 처리합니다.
        private void UpdateFeedback()
        {
            if (state == State.Dragging)
            {
                // 조준선: 발사 벡터(노랑)와 그 수평 정사영(흰색)을 GolfAimIndicator가 함께 그립니다.
                aimIndicator.Show(ball.transform.position, launchDirection, power01);
                // 타점 UI: 드래그 중에만 표시합니다.
                hitPointUI.Show(hitAngle);
            }
            else
            {
                aimIndicator.Hide();
                hitPointUI.Hide();
            }

            GolfBallVisual visual = state switch
            {
                State.Idle     => GolfBallVisual.Ready,
                State.Hover    => GolfBallVisual.Hover,
                State.Dragging => GolfBallVisual.Hover,
                State.Disabled => GolfBallVisual.Disabled,
                State.Locked   => GolfBallVisual.Disabled,
                _ => GolfBallVisual.Unexpected
            };

            ball.SetVisual(visual);
        }
    }
}