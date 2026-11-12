// 지정 물체가 손에서 놓인 채 영역 안에 안착하면 성공 이벤트를 1회 발생시키는 Placement Zone 컴포넌트(다음 Step은 ScenarioManager가 정한다)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Interaction
{
    [RequireComponent(typeof(BoxCollider))]
    public class PlacementZone : MonoBehaviour
    {
        [SerializeField] private string zoneId;
        [Tooltip("이 영역이 받는 물건. 여기 없는 집는 물건을 놓으면 onWrongObject가 발생한다.")]
        [SerializeField] private XRGrabInteractable[] acceptedItems = new XRGrabInteractable[0];
        [Tooltip("손에서 놓인 뒤 이 시간(초) 동안 가만히 영역 안에 있으면 성공")]
        [SerializeField] private float settleTime = 0.3f;
        [Tooltip("이 속도(m/s)보다 느리면 가만히 있는 것으로 본다")]
        [SerializeField] private float restingSpeed = 0.05f;
        [Tooltip("영역 판 위로 물건을 찾는 높이(m)")]
        [SerializeField] private float detectHeight = 0.7f;
        [Tooltip("성공하면 물건을 그 자리에 고정하고 다시 집지 못하게 한다(ResetZone으로 풀림)")]
        [SerializeField] private bool lockOnPlaced = true;
        [SerializeField] private Color placedColor = new Color(0.3f, 1f, 0.5f, 0.75f);

        [SerializeField] private UnityEvent<PlacementZone> onPlaced;
        [SerializeField] private UnityEvent<PlacementZone> onWrongObject;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private readonly Collider[] overlapBuffer = new Collider[32];
        private readonly HashSet<XRGrabInteractable> seen = new HashSet<XRGrabInteractable>();

        private BoxCollider zoneCollider;
        private Renderer zoneRenderer;
        private MaterialPropertyBlock propertyBlock;
        private PlacementZoneState state;
        private bool lockedWasKinematic;

        public string ZoneId => zoneId;
        public bool IsPlaced => state != null && state.IsPlaced;
        public XRGrabInteractable PlacedItem { get; private set; }
        public UnityEvent<PlacementZone> OnPlaced => onPlaced;
        public UnityEvent<PlacementZone> OnWrongObject => onWrongObject;

        private void Awake()
        {
            zoneCollider = GetComponent<BoxCollider>();
            zoneCollider.isTrigger = true;
            zoneRenderer = GetComponent<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            state = new PlacementZoneState(settleTime);
        }

        private void Update()
        {
            if (state.IsPlaced)
            {
                return;
            }

            XRGrabInteractable acceptedCandidate = null;
            bool acceptedInside = false, acceptedHeld = false, acceptedMoving = false, wrongInside = false;

            foreach (var item in FindItemsAbove())
            {
                if (!IsCenterInside(item))
                {
                    continue;
                }
                bool held = item.isSelected;
                if (System.Array.IndexOf(acceptedItems, item) >= 0)
                {
                    var body = item.GetComponent<Rigidbody>();
                    bool moving = body != null && !body.isKinematic && body.linearVelocity.magnitude > restingSpeed;
                    // 여러 개 받는 영역이면 가장 먼저 안착 조건을 만족하는 물건을 본다.
                    if (!acceptedInside || (acceptedHeld || acceptedMoving) && !held && !moving)
                    {
                        acceptedCandidate = item;
                        acceptedInside = true;
                        acceptedHeld = held;
                        acceptedMoving = moving;
                    }
                }
                else if (!held)
                {
                    // 틀린 물건은 손에서 놓였을 때만 알린다(들고 지나가는 것은 무시).
                    wrongInside = true;
                }
            }

            switch (state.Tick(acceptedInside, acceptedHeld, acceptedMoving, wrongInside, Time.deltaTime))
            {
                case PlacementResult.Placed:
                    PlacedItem = acceptedCandidate;
                    if (lockOnPlaced)
                    {
                        Lock(acceptedCandidate);
                    }
                    SetColor(placedColor);
                    onPlaced?.Invoke(this);
                    break;
                case PlacementResult.WrongObject:
                    onWrongObject?.Invoke(this);
                    break;
            }
        }

        // 이 Step을 다시 시도할 수 있도록 판정과 물건 고정을 처음 상태로 되돌린다. 물건 위치 복구는 ResetController(김씨) 담당.
        public void ResetZone()
        {
            Unlock();
            PlacedItem = null;
            state?.Reset();
            if (zoneRenderer != null)
            {
                zoneRenderer.SetPropertyBlock(null);
            }
        }

        // 판 아래 5cm부터 판 가운데 위 detectHeight까지의 상자(가로·세로는 판 크기) 안의 집는 물건을 찾는다.
        // 판은 표면에서 조금 떠 있어서, 탑승권처럼 얇은 물건이 표면에 누우면 판보다 낮아진다. 그래서 판 아래까지 본다.
        private IEnumerable<XRGrabInteractable> FindItemsAbove()
        {
            seen.Clear();
            var t = transform;
            var half = Vector3.Scale(zoneCollider.size, t.lossyScale) / 2f;
            float bottom = -half.y - 0.05f;
            float top = detectHeight;
            var center = t.TransformPoint(zoneCollider.center) + t.up * ((bottom + top) / 2f);
            half.y = (top - bottom) / 2f;
            int count = Physics.OverlapBoxNonAlloc(center, half, overlapBuffer, t.rotation, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var body = overlapBuffer[i].attachedRigidbody;
                var item = body != null ? body.GetComponent<XRGrabInteractable>() : null;
                if (item != null && seen.Add(item))
                {
                    yield return item;
                }
            }
        }

        // 물건 가운데(바운즈 중심)가 판의 가로·세로 범위 안에 있어야 "그 자리에 놓았다"로 본다. 걸쳐 놓은 것은 성공이 아니다.
        private bool IsCenterInside(XRGrabInteractable item)
        {
            var colliders = item.colliders;
            if (colliders == null || colliders.Count == 0)
            {
                return false;
            }
            var bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Count; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }
            var local = transform.InverseTransformPoint(bounds.center) - zoneCollider.center;
            var halfSize = zoneCollider.size / 2f;
            return Mathf.Abs(local.x) <= halfSize.x && Mathf.Abs(local.z) <= halfSize.z;
        }

        private void Lock(XRGrabInteractable item)
        {
            if (item == null)
            {
                return;
            }
            var body = item.GetComponent<Rigidbody>();
            if (body != null)
            {
                lockedWasKinematic = body.isKinematic;
                body.isKinematic = true;
            }
            item.enabled = false;
        }

        private void Unlock()
        {
            if (PlacedItem == null || !lockOnPlaced)
            {
                return;
            }
            PlacedItem.enabled = true;
            var body = PlacedItem.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = lockedWasKinematic;
            }
        }

        // Material을 복제하지 않고 이 판의 색만 바꾼다(§24).
        private void SetColor(Color color)
        {
            if (zoneRenderer == null)
            {
                return;
            }
            zoneRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            zoneRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
