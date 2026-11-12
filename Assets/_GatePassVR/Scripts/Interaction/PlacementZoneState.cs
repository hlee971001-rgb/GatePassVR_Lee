// Placement Zone의 순수 판정 로직(맞는 물건 안착, 틀린 물건, 1회 성공 보호, Reset). MonoBehaviour와 분리해 EditMode 테스트가 가능하다.
using UnityEngine;

namespace GatePassVR.Interaction
{
    public enum PlacementResult
    {
        None,
        Placed,       // 맞는 물건이 손에서 놓인 채 settleTime 동안 가만히 영역 안에 있었다(한 번만).
        WrongObject,  // 받지 않는 물건이 영역에 놓였다(들어올 때마다 한 번).
    }

    public class PlacementZoneState
    {
        private readonly float settleTime;
        private float restingTime;
        private bool wrongReported;

        public bool IsPlaced { get; private set; }

        // 0~1. 맞는 물건이 안착 시간을 얼마나 채웠는지(성공 후 1).
        public float SettleProgress => IsPlaced ? 1f : (settleTime <= 0f ? 0f : Mathf.Clamp01(restingTime / settleTime));

        public PlacementZoneState(float settleTime)
        {
            this.settleTime = Mathf.Max(0f, settleTime);
        }

        // 매 프레임 영역 안 상태를 넣는다. 이번 호출에서 일어난 일을 반환한다.
        public PlacementResult Tick(bool acceptedInside, bool acceptedHeld, bool acceptedMoving, bool wrongInside, float deltaTime)
        {
            if (IsPlaced)
            {
                return PlacementResult.None;
            }

            if (acceptedInside && !acceptedHeld && !acceptedMoving)
            {
                restingTime += Mathf.Max(0f, deltaTime);
                if (restingTime >= settleTime)
                {
                    IsPlaced = true;
                    return PlacementResult.Placed;
                }
            }
            else
            {
                restingTime = 0f;
            }

            if (wrongInside)
            {
                if (!wrongReported)
                {
                    wrongReported = true;
                    return PlacementResult.WrongObject;
                }
            }
            else
            {
                wrongReported = false;
            }
            return PlacementResult.None;
        }

        // 이 Step을 다시 시도할 수 있도록 처음 상태로 되돌린다.
        public void Reset()
        {
            IsPlaced = false;
            restingTime = 0f;
            wrongReported = false;
        }
    }
}
