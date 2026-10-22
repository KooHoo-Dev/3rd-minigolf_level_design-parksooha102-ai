using System;
using UnityEngine;

namespace Study3D.MiniGolf
{
    public enum GolfBallVisual
    {
        Ready,    // 발사 준비 완료
        Hover,    // 호버 중 / 드래그 중
        Disabled, // 쏠 수 없음
        Unexpected,
    }

    public class GolfBall : MonoBehaviour
    {
        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [Header("Settings")]
        [SerializeField] private float stopSpeed = 0.1f;
        [SerializeField] private float stopDelay = 0.3f;
        // 공을 다이나믹으로 사용하기 때문에 게임적 표현을
        // 회전 스피드에 제한을 활용하여 간접적으로 제어 합니다.
        [SerializeField] private float maxAngularSpeed = 150f;

        [Header("Colors")]
        [SerializeField] private Color readyColor = Color.white;
        [SerializeField] private Color hoverColor = Color.green;
        [SerializeField] private Color disabledColor = Color.crimson;
        [SerializeField] private Color UnexpectedColor = Color.gold;

        [Header("Ref")]
        [SerializeField] private Renderer ballRenderer;

        private Rigidbody rBody;
        private SphereCollider sphere;
        private Material mat;
        private Vector3 lastPosition; // 유저가 마지막으로 공을 건드린 위치
        private Vector3 shotVelocity; // 공의 방향 * 세기가 연산되는 변수
        private bool shotRequested;
        private float slowTime;

        public bool IsStopped => (shotRequested == false) && slowTime >= stopDelay;

        // 공의 월드 반지름
        public float Radius => sphere.radius * transform.lossyScale.x;

        private void Awake()
        {
            rBody = GetComponent<Rigidbody>();
            sphere = GetComponent<SphereCollider>();
            rBody.maxAngularVelocity = maxAngularSpeed;
            lastPosition = transform.position;

            mat = ballRenderer.material;
            SetVisual(GolfBallVisual.Ready);
        }

        // 공의 물리를 사용할 예정이라 Update가 아닌 FixedUpdate를 사용합니다.
        private void FixedUpdate()
        {
            if (shotRequested)
            {
                shotRequested = false;
                rBody.AddForce(shotVelocity, ForceMode.VelocityChange);

                slowTime = 0.0f;
                return;
            }

            if (rBody.linearVelocity.magnitude < stopSpeed)
                slowTime += Time.fixedDeltaTime;
            else
                slowTime = 0;
        }

        // 외부에서 Shoot함수를 호출하여 동작하도록 함수를 public으로 선언합니다
        public void Shoot(Vector3 direction, float power, bool isPassive = false)
        {
            direction.Normalize();
            shotRequested = true;
            shotVelocity = direction * power;

            if (isPassive == false)
                lastPosition = transform.position;
        }

        public void SetVisual(GolfBallVisual visual)
        {
            Color color = visual switch
            {
                GolfBallVisual.Ready    => readyColor,
                GolfBallVisual.Hover    => hoverColor,
                GolfBallVisual.Disabled => disabledColor,
                _ => UnexpectedColor
            };

            mat.SetColor(BaseColor, color);
        }

        // 공을 position의 위치로 옮기고
        // 다시 shoot을 할 수 있는 상태로 바꿔 줍니다.
        private void Teleport(Vector3 position)
        {
            shotRequested = false;
            // 리지드 바디의 운동량(선 운동량, 각 운동량)을 0으로 바꿔줍니다.
            rBody.linearVelocity = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;
            
            // 공을 물리를 처리하는 개체이므로
            // transform과 리지드 바디의 position을 모두 변경해줍니다.
            // (Interpolate를 사용하는 Rigidbody는 transform도 함께 옮겨야
            // 끌려오듯이 안보일 거입니다.)
            rBody.position = position;
            transform.position = position;
        }

        // public으로 선언해서 외부에서 호출 할 수 있도록 해줍니다.
        public void ReturnToLastShotPosition()
        {
            Teleport(lastPosition);
        }

        // 스테이지 시작시 공을 놓기 위한 함수
        public void PlaceAt(Vector3 position)
        {
            lastPosition = position;
            Teleport(position);
        }
    }
}