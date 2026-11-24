// World Space UI를 카메라 앞 고정 위치/각도로 따라다니게 하는 범용 추종 컴포넌트 (lazyAngle을 주면 고개를 많이 돌렸을 때만 천천히 따라온다)
using UnityEngine;

namespace GatePassVR.UI
{
    public class GuideUIFollow : MonoBehaviour
    {
        [SerializeField] private Transform target; // 비워두면 Camera.main을 자동으로 찾는다
        [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.1f, 1.3f);
        [Tooltip("0이면 머리에 딱 붙어 따라온다. 0보다 크면 머리 좌우 방향이 이 각도(도)보다 벌어졌을 때만 다시 앞으로 온다 (이씨 11/24)")]
        [SerializeField] private float lazyAngle;
        [Tooltip("lazyAngle을 쓸 때 다시 앞으로 오는 속도(도/초)")]
        [SerializeField] private float recenterSpeed = 90f;

        private Transform followTarget;
        private float currentYaw;
        private bool recentering;
        private bool yawInitialized;

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                followTarget = target != null ? target : (Camera.main != null ? Camera.main.transform : null);

                if (followTarget == null)
                {
                    return;
                }
            }

            if (lazyAngle <= 0f)
            {
                transform.SetPositionAndRotation(
                    followTarget.TransformPoint(localOffset),
                    followTarget.rotation);
                return;
            }

            float headYaw = followTarget.eulerAngles.y;
            if (!yawInitialized)
            {
                currentYaw = headYaw;
                yawInitialized = true;
            }

            currentYaw = NextYaw(currentYaw, headYaw, ref recentering, lazyAngle, recenterSpeed, Time.deltaTime);

            // 좌우만 따라가고 위아래 고개 움직임에는 흔들리지 않는다. 패널은 머리를 향한다.
            Vector3 position = followTarget.position + Quaternion.Euler(0f, currentYaw, 0f) * localOffset;
            transform.SetPositionAndRotation(position, Quaternion.LookRotation(position - followTarget.position));
        }

        // 패널의 좌우 각도를 한 프레임 진행한다. 벌어짐이 lazyAngle을 넘으면 거의 정면(1도 이내)이 될 때까지 따라간다.
        public static float NextYaw(float currentYaw, float headYaw, ref bool recentering, float lazyAngle, float speed, float deltaTime)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(currentYaw, headYaw)) > lazyAngle)
            {
                recentering = true;
            }

            if (!recentering)
            {
                return currentYaw;
            }

            float next = Mathf.MoveTowardsAngle(currentYaw, headYaw, speed * deltaTime);
            if (Mathf.Abs(Mathf.DeltaAngle(next, headYaw)) < 1f)
            {
                recentering = false;
            }

            return next;
        }
    }
}
