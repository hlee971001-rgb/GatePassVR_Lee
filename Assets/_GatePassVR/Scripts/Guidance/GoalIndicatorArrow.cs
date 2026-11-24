// 현재 목표 지점 방향으로 화면 위 화살표를 회전시켜 어디를 봐야 하는지 안내한다
using UnityEngine;
using UnityEngine.UI;

namespace GatePassVR.Guidance
{
    [RequireComponent(typeof(Image))]
    public class GoalIndicatorArrow : MonoBehaviour
    {
        private static Sprite arrowSpriteCache;

        [SerializeField] private Transform initialGoal;

        private Image arrowImage;
        private Transform goal;

        private void Awake()
        {
            arrowImage = GetComponent<Image>();
            arrowImage.sprite = GetOrCreateArrowSprite();
            SetGoal(initialGoal);
        }

        // 다음에 바라봐야 할 목표 지점을 설정한다. null이면 화살표를 숨긴다.
        public void SetGoal(Transform newGoal)
        {
            goal = newGoal;
            gameObject.SetActive(goal != null);
        }

        private void LateUpdate()
        {
            if (goal == null || Camera.main == null)
            {
                return;
            }

            var cam = Camera.main.transform;
            Vector3 toGoal = goal.position - cam.position;
            toGoal.y = 0f;
            // 화살표가 붙은 패널이 바라보는 방향 기준으로 돈다. 머리에 딱 붙은 HUD면 카메라 방향과 같고, 느슨하게 따라오는 HUD에서도 맞다 (이씨 11/24)
            Vector3 forward = transform.forward;
            forward.y = 0f;

            if (toGoal.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float signedAngle = Vector3.SignedAngle(forward, toGoal, Vector3.up);
            transform.localRotation = Quaternion.Euler(0f, 0f, -signedAngle);
        }

        // 코드로 생성한 위쪽을 향하는 화살표(화살촉+몸통) 스프라이트. 실제 아트가 들어오면 Inspector에서 sprite를 직접 지정해 대체하면 된다.
        private static Sprite GetOrCreateArrowSprite()
        {
            if (arrowSpriteCache != null)
            {
                return arrowSpriteCache;
            }

            const int size = 128;
            const float headStartT = 0.5f; // 화살촉이 시작되는 높이(0=아래, 1=위)
            const float headBaseHalfWidth = size * 0.36f;
            const float shaftHalfWidth = size * 0.13f;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                float t = y / (float)(size - 1);
                float halfWidth;

                if (t < headStartT)
                {
                    halfWidth = shaftHalfWidth;
                }
                else
                {
                    float headT = (t - headStartT) / (1f - headStartT);
                    halfWidth = Mathf.Lerp(headBaseHalfWidth, 0f, headT);
                }

                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Abs(x - size * 0.5f);
                    bool inArrow = dx <= halfWidth;
                    pixels[y * size + x] = inArrow ? new Color32(255, 255, 255, 255) : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            arrowSpriteCache = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return arrowSpriteCache;
        }
    }
}
