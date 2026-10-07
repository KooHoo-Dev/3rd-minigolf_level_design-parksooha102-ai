using UnityEngine;
using UnityEngine.UI;

namespace Study3D.MiniGolf
{
    public class GolfStageHUD : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text resultText;

        public void SetScore(int strokes, int par)
        {
            scoreText.text = $"STROKE {strokes} / PAR {par}";
        }

        public void ShowResult(string result)
        {
            resultText.text = result;
            resultText.gameObject.SetActive(true);
        }

        public void HideResult()
        {
            resultText.gameObject.SetActive(false);
        }
    }
}