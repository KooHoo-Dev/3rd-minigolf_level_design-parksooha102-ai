using System.Collections.Generic;
using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfClearEffect : MonoBehaviour
    {
        private bool isVisible;
        private List<GameObject> children = new List<GameObject>();

        private void Awake()
        {
            isVisible = false;

            for(int i = 0; i < transform.childCount; i++)
                children.Add(transform.GetChild(i).gameObject);
        }

        public void Show()
        {
            isVisible = true;

            for(int i = 0; i < children.Count; i++)
                children[i].SetActive(true);
        }

        public void Hide()
        {
            isVisible = false;

            for(int i = 0; i < children.Count; i++)
                children[i].SetActive(false);
        }
    }
}