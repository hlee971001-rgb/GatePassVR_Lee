// ControllerHandPose가 손가락 관절만 한 번 굽히는지 검증하는 EditMode 테스트
using GatePassVR.VR;
using NUnit.Framework;
using UnityEngine;

namespace GatePassVR.Tests.EditMode
{
    public class ControllerHandPoseTests
    {
        private GameObject root;
        private Transform wrist;
        private Transform middleProximal;
        private Transform middleTip;
        private ControllerHandPose pose;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("Hand");
            root.SetActive(false); // Awake에서 바로 적용되지 않게 막는다
            wrist = new GameObject("R_Wrist").transform;
            wrist.SetParent(root.transform);
            middleProximal = new GameObject("R_MiddleProximal").transform;
            middleProximal.SetParent(wrist);
            middleTip = new GameObject("R_MiddleTip").transform;
            middleTip.SetParent(middleProximal);
            pose = root.AddComponent<ControllerHandPose>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Apply_CurlsFingerJointOnly()
        {
            pose.Apply();

            Assert.Greater(Quaternion.Angle(Quaternion.identity, middleProximal.localRotation), 1f);
            Assert.AreEqual(Quaternion.identity, wrist.localRotation);
            Assert.AreEqual(Quaternion.identity, middleTip.localRotation);
        }

        [Test]
        public void Apply_Twice_CurlsOnlyOnce()
        {
            pose.Apply();
            Quaternion once = middleProximal.localRotation;

            pose.Apply();

            Assert.AreEqual(once, middleProximal.localRotation);
        }
    }
}
