// UI 그래픽(시작 버튼 빛 등)의 투명도를 천천히 올렸다 내려 숨 쉬듯 보이게 하는 컴포넌트 (이씨)
using UnityEngine;
using UnityEngine.UI;

namespace GatePassVR.UI
{
    [RequireComponent(typeof(Graphic))]
    public class PulseGraphic : MonoBehaviour
    {
        [SerializeField] private float minAlpha = 0.15f;
        [SerializeField] private float maxAlpha = 0.5f;
        [Tooltip("한 번 밝아졌다 어두워지는 데 걸리는 시간(초)")]
        [SerializeField] private float period = 2f;

        private Graphic graphic;

        private void Awake()
        {
            graphic = GetComponent<Graphic>();
        }

        private void Update()
        {
            var c = graphic.color;
            c.a = AlphaAt(Time.time, minAlpha, maxAlpha, period);
            graphic.color = c;
        }

        // 시간 t에서의 투명도. 0초에 가장 어둡고 period/2초에 가장 밝다.
        public static float AlphaAt(float t, float min, float max, float period)
        {
            if (period <= 0f)
            {
                return max;
            }

            float wave = 0.5f - 0.5f * Mathf.Cos(t / period * Mathf.PI * 2f);
            return Mathf.Lerp(min, max, wave);
        }
    }
}
