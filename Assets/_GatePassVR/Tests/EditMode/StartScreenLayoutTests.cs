// 시작 화면 정리에 쓰는 ShowDuringSteps 숨기기 옵션과 GuideUIFollow 느슨한 따라오기 계산을 검증하는 EditMode 테스트
using GatePassVR.Guidance;
using GatePassVR.UI;
using NUnit.Framework;

namespace GatePassVR.Tests.EditMode
{
    public class StartScreenLayoutTests
    {
        private static readonly string[] StartOnly = { "START" };

        [Test]
        public void ShouldShow_Default_ShowsOnlyListedStep()
        {
            Assert.IsTrue(ShowDuringSteps.ShouldShow("START", StartOnly, false));
            Assert.IsFalse(ShowDuringSteps.ShouldShow("S1-0", StartOnly, false));
            Assert.IsFalse(ShowDuringSteps.ShouldShow(null, StartOnly, false));
        }

        [Test]
        public void ShouldShow_Hide_HidesOnlyListedStep()
        {
            Assert.IsFalse(ShowDuringSteps.ShouldShow("START", StartOnly, true));
            Assert.IsTrue(ShowDuringSteps.ShouldShow("S1-0", StartOnly, true));
            Assert.IsTrue(ShowDuringSteps.ShouldShow(null, StartOnly, true));
        }

        [Test]
        public void NextYaw_SmallTurn_StaysPut()
        {
            bool recentering = false;
            float yaw = GuideUIFollow.NextYaw(0f, 20f, ref recentering, 30f, 90f, 0.1f);

            Assert.AreEqual(0f, yaw);
            Assert.IsFalse(recentering);
        }

        [Test]
        public void NextYaw_BigTurn_FollowsUntilCentered()
        {
            bool recentering = false;
            float yaw = 0f;

            yaw = GuideUIFollow.NextYaw(yaw, 40f, ref recentering, 30f, 90f, 0.1f);
            Assert.AreEqual(9f, yaw, 0.001f);
            Assert.IsTrue(recentering);

            for (int i = 0; i < 10; i++)
            {
                yaw = GuideUIFollow.NextYaw(yaw, 40f, ref recentering, 30f, 90f, 0.1f);
            }

            Assert.AreEqual(40f, yaw, 1f);
            Assert.IsFalse(recentering);
        }

        [Test]
        public void PulseAlpha_GoesFromMinToMax()
        {
            Assert.AreEqual(0.15f, PulseGraphic.AlphaAt(0f, 0.15f, 0.5f, 2f), 0.001f);
            Assert.AreEqual(0.5f, PulseGraphic.AlphaAt(1f, 0.15f, 0.5f, 2f), 0.001f);
            Assert.AreEqual(0.15f, PulseGraphic.AlphaAt(2f, 0.15f, 0.5f, 2f), 0.001f);
        }

        [Test]
        public void NextYaw_WrapsAround360()
        {
            bool recentering = false;
            float yaw = GuideUIFollow.NextYaw(350f, 30f, ref recentering, 30f, 90f, 0.1f);

            // 350도에서 30도까지는 40도 차이라 따라가기 시작하고, 반대 방향(320도)으로 돌지 않는다
            Assert.IsTrue(recentering);
            Assert.AreEqual(359f, yaw, 0.001f);
        }
    }
}
