using TMPro;
using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfStageUI : MonoBehaviour
    {
        [Header("HUD")]
        [SerializeField] private TMP_Text scoreText;

        [Header("Clear Panel")]
        [SerializeField] private GameObject clearPanel;
        [SerializeField] private TMP_Text resultText;

        public void SetScore(int strokes, int par)
        {
            scoreText.text = $"STROKE {strokes} / PAR {par}";
        }

        public void ShowClear(string result)
        {
            resultText.text = result;
            clearPanel.SetActive(true);
        }

        public void HideClear()
        {
            clearPanel.SetActive(false);
        }
    }
}