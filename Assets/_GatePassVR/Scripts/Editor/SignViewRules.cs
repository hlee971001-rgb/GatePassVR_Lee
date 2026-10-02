// VR에서 표지판을 편하게 볼 수 있는 범위(좌우·위 각도)와 기준 눈높이, Quest 3S 시야를 모아 둔 공통 규칙
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public static class SignViewRules
    {
        // 서서 하는 사용자와 앉아서 하는 사용자의 눈높이(m). 앉은 쪽이 표지판을 더 올려다봐야 한다.
        public const float StandingEyeHeight = 1.6f;
        public const float SeatedEyeHeight = 1.2f;

        // 고개를 크게 돌리거나 들지 않고 편하게 보는 범위(일반적인 VR 설계 권장값). 실기기 확인 후 조정한다.
        public const float MaxYaw = 30f;
        public const float MaxElevation = 25f;

        // Meta Quest 3S 시야(대략): 좌우 96°, 위아래 90°
        public const float Quest3SHorizontalFov = 96f;
        public const float Quest3SVerticalFov = 90f;

        // 눈에서 본 점의 올려다보는 각도(°). +는 위.
        public static float Elevation(Vector3 eye, Vector3 point)
        {
            var offset = point - eye;
            float up = offset.y;
            offset.y = 0f;
            return Mathf.Atan2(up, offset.magnitude) * Mathf.Rad2Deg;
        }

        public static Vector3 Eye(Transform dest, float eyeHeight) => dest.position + Vector3.up * eyeHeight;
    }
}
