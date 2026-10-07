using UnityEngine;

namespace Study3D.MiniGolf
{
    public class MaterialScroller : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector2 speed;
        [SerializeField] private Renderer renderer;

        private Vector2 offset;
        private Material mat;

        private void Awake()
        {
            mat = renderer.material;
        }

        private void Update()
        {
            offset += speed * Time.deltaTime;

            offset.x = Mathf.Repeat(offset.x, 1f);
            offset.y = Mathf.Repeat(offset.y, 1f);

            mat.mainTextureOffset = offset;
        }
    }
}