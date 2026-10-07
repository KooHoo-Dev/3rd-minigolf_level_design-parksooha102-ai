using UnityEngine;

namespace Study3D.MiniGolf
{
    // 스테이지 진행의 허브. 스테이지 목록 / 공 / 플레이어 / 스테이지 UI를 알고,
    // 스테이지 진입, 리셋, 클리어, 타수 집계를 한 곳에서 결정합니다.
    // 스테이지 데이터(par, 시작점, 골)는 각 스테이지의 GolfStage가 가집니다.
    // 골 센서와 UI는 이 매니저를 모릅니다.
    public class GolfStageManager : MonoBehaviour
    {
        [Header("Stages")]
        [SerializeField] private GolfStage[] stages; // 진행 순서대로

        [Header("Ref")]
        [SerializeField] private GolfBall ball;
        [SerializeField] private GolfPlayer player;
        [SerializeField] private GolfStageUI ui;

        private int stageIndex;
        private int strokes;

        private GolfStage Current => stages[stageIndex];

        private void OnEnable()
        {
            player.Shot += OnShot;
            Current.Goal.BallEntered += OnBallEntered;
        }

        private void OnDisable()
        {
            player.Shot -= OnShot;
            Current.Goal.BallEntered -= OnBallEntered;
        }

        private void Start()
        {
            for (int i = 0; i < stages.Length; i++)
                stages[i].gameObject.SetActive(false);

            EnterStage(0);
        }

        public void ResetStage()
        {
            ui.HideClear();
            player.ResetInput(); // 잠금 해제 + 드래그 중이었다면 취소

            strokes = 0;
            ui.SetScore(strokes, Current.Par);

            ball.PlaceAt(Current.StartPosition);
        }

        public void NextStage()
        {
            EnterStage((stageIndex + 1) % stages.Length);
        }

        // 이전 스테이지의 골 구독을 끊고, 새 스테이지를 켜서 골을 구독한 뒤 처음 상태로 만듭니다.
        private void EnterStage(int index)
        {
            Current.Goal.BallEntered -= OnBallEntered;
            Current.gameObject.SetActive(false);

            stageIndex = index;

            Current.gameObject.SetActive(true);
            Current.Goal.BallEntered += OnBallEntered;

            ResetStage();
        }

        private void OnShot()
        {
            strokes++;
            ui.SetScore(strokes, Current.Par);
        }

        private void OnBallEntered()
        {
            player.Lock();
            ui.ShowClear(GetScoreName(strokes, Current.Par));
        }

        private static string GetScoreName(int strokes, int par)
        {
            if (strokes == 1) return "HOLE IN ONE!";

            int diff = strokes - par;
            return diff switch
            {
                <= -2 => "EAGLE!",
                -1    => "BIRDIE!",
                0     => "PAR",
                1     => "BOGEY",
                _     => $"+{diff}",
            };
        }
    }
}