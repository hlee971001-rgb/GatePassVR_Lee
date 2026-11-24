// XR Hands 손 모델의 손가락을 컨트롤러를 쥔 모양으로 한 번 굽혀 고정하는 컴포넌트 (이씨)
using UnityEngine;

namespace GatePassVR.VR
{
    public class ControllerHandPose : MonoBehaviour
    {
        [Tooltip("검지 Proximal·Intermediate·Distal 굽힘 각도(도)")]
        [SerializeField] private Vector3 indexCurl = new Vector3(55f, 55f, 30f);
        [Tooltip("중지·약지·소지 Proximal·Intermediate·Distal 굽힘 각도(도)")]
        [SerializeField] private Vector3 fingerCurl = new Vector3(90f, 95f, 55f);
        [Tooltip("엄지 뿌리(Metacarpal)를 손등 축으로 돌리는 각도(도). 오른손 기준, 왼손(L_)은 반대로 돈다")]
        [SerializeField] private float thumbSwing = -30f;
        [Tooltip("엄지 Proximal·Distal 굽힘 각도(도), thumbAxis 기준")]
        [SerializeField] private Vector2 thumbCurl = new Vector2(20f, 25f);
        [Tooltip("엄지 관절을 굽힐 로컬 축")]
        [SerializeField] private Vector3 thumbAxis = Vector3.right;

        private bool applied;

        private void Awake()
        {
            Apply();
        }

        // 관절 이름 끝(예: R_IndexProximal)으로 찾아 굽힌다. 두 번 불러도 한 번만 적용된다.
        public void Apply()
        {
            if (applied)
            {
                return;
            }
            applied = true;

            foreach (var bone in GetComponentsInChildren<Transform>(true))
            {
                string n = bone.name;
                if (n.Contains("ThumbMetacarpal"))
                {
                    float swing = n.StartsWith("L_") ? -thumbSwing : thumbSwing;
                    bone.localRotation = Quaternion.AngleAxis(swing, Vector3.up) * bone.localRotation;
                    continue;
                }
                if (n.EndsWith("Tip") || n.EndsWith("Metacarpal"))
                {
                    continue;
                }

                if (n.Contains("Thumb"))
                {
                    float t = n.EndsWith("Proximal") ? thumbCurl.x : n.EndsWith("Distal") ? thumbCurl.y : 0f;
                    bone.localRotation *= Quaternion.AngleAxis(t, thumbAxis);
                    continue;
                }

                Vector3 curl = n.Contains("Index") ? indexCurl
                    : n.Contains("Middle") || n.Contains("Ring") || n.Contains("Little") ? fingerCurl
                    : Vector3.zero;
                float angle = n.EndsWith("Proximal") ? curl.x
                    : n.EndsWith("Intermediate") ? curl.y
                    : n.EndsWith("Distal") ? curl.z
                    : 0f;
                bone.localRotation *= Quaternion.Euler(angle, 0f, 0f);
            }
        }
    }
}
