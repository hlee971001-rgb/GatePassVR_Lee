// 정적 NPC(애니메이션 없음)의 역할과 그 NPC에게 하는 상호작용 지점(Hand-over·Scanner 자리)을 묶어 두는 표시용 컴포넌트
using UnityEngine;

namespace GatePassVR.Interaction
{
    public class NpcStation : MonoBehaviour
    {
        [Tooltip("역할 ID (예: CheckInStaff, ImmigrationOfficer)")]
        [SerializeField] private string roleId;
        [Tooltip("화면·문서에 쓰는 역할 이름 (guide-texts.md 용어)")]
        [SerializeField] private string displayName;
        [Tooltip("이 NPC에게 하는 상호작용 지점. 없으면 서 있기만 하는 NPC")]
        [SerializeField] private Transform interactionPoint;

        public string RoleId => roleId;
        public string DisplayName => displayName;
        public Transform InteractionPoint => interactionPoint;
        public PlacementZone InteractionZone => interactionPoint != null ? interactionPoint.GetComponent<PlacementZone>() : null;
    }
}
