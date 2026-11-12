// PlacementZoneState의 성공·틀린 물건·1회 성공 보호·Reset 판정을 검증하는 EditMode 테스트
using GatePassVR.Interaction;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class PlacementZoneStateTests
    {
        const float Settle = 0.3f;
        const float Frame = 0.1f;

        // 맞는 물건이 손에서 놓인 채 가만히 영역 안에 있을 때 한 프레임
        static PlacementResult RestingTick(PlacementZoneState state) =>
            state.Tick(acceptedInside: true, acceptedHeld: false, acceptedMoving: false, wrongInside: false, deltaTime: Frame);

        [Test]
        public void AcceptedItemResting_ForSettleTime_PlacesExactlyOnce()
        {
            var state = new PlacementZoneState(Settle);

            Assert.AreEqual(PlacementResult.None, RestingTick(state));
            Assert.AreEqual(PlacementResult.None, RestingTick(state));
            Assert.AreEqual(PlacementResult.Placed, RestingTick(state));
            Assert.IsTrue(state.IsPlaced);

            // 같은 상태가 계속 들어와도 성공은 다시 나오지 않는다.
            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(PlacementResult.None, RestingTick(state));
            }
        }

        [Test]
        public void AcceptedItemHeldInHand_NeverPlaces()
        {
            var state = new PlacementZoneState(Settle);
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(PlacementResult.None,
                    state.Tick(acceptedInside: true, acceptedHeld: true, acceptedMoving: false, wrongInside: false, deltaTime: Frame));
            }
            Assert.IsFalse(state.IsPlaced);
        }

        [Test]
        public void AcceptedItemStillMoving_NeverPlaces()
        {
            var state = new PlacementZoneState(Settle);
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(PlacementResult.None,
                    state.Tick(acceptedInside: true, acceptedHeld: false, acceptedMoving: true, wrongInside: false, deltaTime: Frame));
            }
            Assert.IsFalse(state.IsPlaced);
        }

        [Test]
        public void AcceptedItemLeavesBeforeSettle_TimerRestarts()
        {
            var state = new PlacementZoneState(Settle);
            RestingTick(state);
            RestingTick(state);
            // 0.2초 머물다 영역 밖으로 나감
            state.Tick(acceptedInside: false, acceptedHeld: false, acceptedMoving: false, wrongInside: false, deltaTime: Frame);

            // 다시 들어오면 처음부터 0.3초를 채워야 한다.
            Assert.AreEqual(PlacementResult.None, RestingTick(state));
            Assert.AreEqual(PlacementResult.None, RestingTick(state));
            Assert.AreEqual(PlacementResult.Placed, RestingTick(state));
        }

        [Test]
        public void WrongItem_ReportsOnceWhileInside_AndNeverPlaces()
        {
            var state = new PlacementZoneState(Settle);

            Assert.AreEqual(PlacementResult.WrongObject,
                state.Tick(acceptedInside: false, acceptedHeld: false, acceptedMoving: false, wrongInside: true, deltaTime: Frame));
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(PlacementResult.None,
                    state.Tick(acceptedInside: false, acceptedHeld: false, acceptedMoving: false, wrongInside: true, deltaTime: Frame));
            }
            Assert.IsFalse(state.IsPlaced);
        }

        [Test]
        public void WrongItem_ReenteringReportsAgain()
        {
            var state = new PlacementZoneState(Settle);
            state.Tick(false, false, false, true, Frame);
            state.Tick(false, false, false, false, Frame);

            Assert.AreEqual(PlacementResult.WrongObject, state.Tick(false, false, false, true, Frame));
        }

        [Test]
        public void AfterPlaced_WrongItemIsIgnored()
        {
            var state = new PlacementZoneState(Settle);
            RestingTick(state);
            RestingTick(state);
            RestingTick(state);

            Assert.AreEqual(PlacementResult.None, state.Tick(false, false, false, true, Frame));
        }

        [Test]
        public void Reset_AllowsPlacingAgain()
        {
            var state = new PlacementZoneState(Settle);
            RestingTick(state);
            RestingTick(state);
            RestingTick(state);
            Assert.IsTrue(state.IsPlaced);

            state.Reset();

            Assert.IsFalse(state.IsPlaced);
            Assert.AreEqual(PlacementResult.None, RestingTick(state));
            Assert.AreEqual(PlacementResult.None, RestingTick(state));
            Assert.AreEqual(PlacementResult.Placed, RestingTick(state));
        }

        [Test]
        public void NegativeSettleTime_PlacesOnFirstRestingTick()
        {
            var state = new PlacementZoneState(-1f);
            Assert.AreEqual(PlacementResult.Placed, RestingTick(state));
        }
    }
}
