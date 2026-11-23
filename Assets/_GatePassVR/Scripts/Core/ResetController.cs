// 현재 단계의 Pad 게이지·Grab 물건 위치·Placement Zone 판정을 한 번에 되돌리는 복구 컴포넌트(B/Y 버튼으로 실행)
using GatePassVR.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Core
{
    public class ResetController : MonoBehaviour
    {
        [SerializeField] private PointAndHoldTarget[] holdTargets = new PointAndHoldTarget[0];
        [Tooltip("떨어뜨리거나 던져도 처음 위치로 돌려놓을 물건")]
        [SerializeField] private XRGrabInteractable[] items = new XRGrabInteractable[0];
        [SerializeField] private PlacementZone[] zones = new PlacementZone[0];
        [SerializeField] private HandOverZone[] handOverZones = new HandOverZone[0];
        [SerializeField] private ScannerZone[] scannerZones = new ScannerZone[0];
        [Tooltip("Reset 입력. 기본값은 양손 컨트롤러 secondaryButton(오른손 B, 왼손 Y)")]
        [SerializeField] private InputAction resetAction = CreateDefaultResetAction();

        [SerializeField] private UnityEvent onReset;

        private Pose[] initialPoses;
        // Reset 때 물건을 돌려놓을 위치. 처음엔 시작 위치이고, SaveCheckpoint를 부르면 그때 위치로 바뀐다.
        // (이씨 2026-11-23 추가: 공항처럼 넓은 씬에서 시작 위치로 돌리면 손이 닿지 않아 진행이 막힌다.)
        private Pose[] checkpointPoses;

        private void Awake()
        {
            initialPoses = new Pose[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    initialPoses[i] = new Pose(items[i].transform.position, items[i].transform.rotation);
                }
            }

            checkpointPoses = (Pose[])initialPoses.Clone();
        }

        // 지금 물건 위치를 Reset 복귀 위치로 기억한다.
        // ScenarioManager.onStepEntered와 FadeMoveController.onMoveCompleted(이동 뒤 들고 있는 물건 위치)에 연결한다.
        public void SaveCheckpoint()
        {
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null)
                {
                    checkpointPoses[i] = new Pose(items[i].transform.position, items[i].transform.rotation);
                }
            }
        }

        // 처음 상태로 완전히 되돌린다(시나리오를 처음부터 다시 할 때). 물건은 시작 위치로 간다.
        public void ResetAll()
        {
            checkpointPoses = (Pose[])initialPoses.Clone();
            ResetCurrent();
        }

        private void OnEnable()
        {
            resetAction.performed += OnResetPerformed;
            resetAction.Enable();
        }

        private void OnDisable()
        {
            resetAction.performed -= OnResetPerformed;
            resetAction.Disable();
        }

        // 현재 단계를 처음 상태로 되돌린다. Zone을 먼저 풀어야 고정된 물건을 옮길 수 있다.
        public void ResetCurrent()
        {
            foreach (var zone in zones)
            {
                if (zone != null)
                {
                    zone.ResetZone();
                }
            }

            foreach (var zone in handOverZones)
            {
                if (zone != null)
                {
                    zone.ResetZone();
                }
            }

            foreach (var zone in scannerZones)
            {
                if (zone != null)
                {
                    zone.ResetZone();
                }
            }

            for (int i = 0; i < items.Length; i++)
            {
                RestoreItem(items[i], checkpointPoses[i]);
            }

            RearmHoldTargets();
            onReset?.Invoke();
        }

        // 완료된 Pad를 다시 Hold할 수 있게 한다. 이동이 끝날 때 FadeMoveController.onMoveCompleted로 호출한다.
        public void RearmHoldTargets()
        {
            foreach (var target in holdTargets)
            {
                if (target != null)
                {
                    target.ResetHold();
                }
            }
        }

        private void OnResetPerformed(InputAction.CallbackContext context)
        {
            ResetCurrent();
        }

        private static void RestoreItem(XRGrabInteractable item, Pose pose)
        {
            if (item == null)
            {
                return;
            }

            // 손에 들고 있으면 먼저 놓게 해야 원위치로 옮길 수 있다.
            if (item.isSelected && item.interactionManager != null)
            {
                item.interactionManager.CancelInteractableSelection((IXRSelectInteractable)item);
            }

            item.transform.SetPositionAndRotation(pose.position, pose.rotation);
            var body = item.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = pose.position;
                body.rotation = pose.rotation;
                if (!body.isKinematic)
                {
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        private static InputAction CreateDefaultResetAction()
        {
            var action = new InputAction("Reset", InputActionType.Button);
            action.AddBinding("<XRController>{LeftHand}/secondaryButton");
            action.AddBinding("<XRController>{RightHand}/secondaryButton");
            return action;
        }
    }
}
