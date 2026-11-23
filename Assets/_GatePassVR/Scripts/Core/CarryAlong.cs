// 여권·탑승권처럼 다음 구역에서도 써야 하는 물건을, 이동할 때 멀리 두고 왔으면 도착 지점 발밑으로 가져다 놓는 컴포넌트 (이씨)
// 보안검색에서 양손을 비우려고 내려놓은 여권을 두고 탑승구로 가면 스캔 Step에서 막히기 때문이다.
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Core
{
    public class CarryAlong : MonoBehaviour
    {
        [Tooltip("이동할 때 따라올 물건")]
        [SerializeField] private XRGrabInteractable[] items = new XRGrabInteractable[0];
        [Tooltip("플레이어(XR Origin)")]
        [SerializeField] private Transform player;
        [Tooltip("이 거리(m)보다 멀리 있으면 가져온다")]
        [SerializeField] private float maxDistance = 4f;
        [Tooltip("플레이어 오른쪽 발옆으로 내려놓을 거리(m). 보안검색처럼 바로 앞에 벨트·책상이 있는 지점이 있어 앞이 아니라 옆에 둔다.")]
        [SerializeField] private float sideOffset = 0.35f;
        [Tooltip("물건끼리 겹치지 않게 벌리는 간격(m)")]
        [SerializeField] private float sideSpacing = 0.25f;
        [SerializeField] private float dropHeight = 0.15f;

        // FadeMoveController.onMoveCompleted에 연결한다. ResetController.SaveCheckpoint보다 먼저 불러야 Reset 위치도 발밑이 된다.
        public void BringFarItems()
        {
            if (player == null)
            {
                return;
            }

            var forward = Vector3.ProjectOnPlane(player.forward, Vector3.up).normalized;
            var right = Vector3.Cross(Vector3.up, forward);
            int placed = 0;

            foreach (var item in items)
            {
                if (item == null || item.isSelected || !item.enabled)
                {
                    continue; // 손에 든 물건, 직원에게 건넨 물건(Grab 비활성)은 그대로 둔다.
                }

                var body = item.GetComponent<Rigidbody>();
                if (body != null && body.isKinematic)
                {
                    continue; // 칸에 고정된 물건
                }

                var flat = item.transform.position - player.position;
                flat.y = 0f;
                if (flat.magnitude <= maxDistance)
                {
                    continue;
                }

                var pos = player.position + right * (sideOffset + placed * sideSpacing) + Vector3.up * dropHeight;
                var rot = Quaternion.LookRotation(forward, Vector3.up);
                item.transform.SetPositionAndRotation(pos, rot);
                if (body != null)
                {
                    body.position = pos;
                    body.rotation = rot;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }

                placed++;
            }
        }
    }
}
