// 안내 HUD를 항상 앞에 그리는 UIAlwaysOnTop이 이미지 머티리얼을 사본으로 바꾸고 한 번만 적용하는지 검증하는 EditMode 테스트
using GatePassVR.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GatePassVR.Tests.EditMode
{
    public class UIAlwaysOnTopTests
    {
        private GameObject root;

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
        }

        [Test]
        public void Apply_Image_GetsOwnMaterialWithZTestAlways()
        {
            root = new GameObject("Canvas", typeof(Canvas));
            var image = new GameObject("Background", typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(root.transform);
            var shared = image.materialForRendering;

            root.AddComponent<UIAlwaysOnTop>().Apply();

            Assert.AreNotSame(shared, image.material);
            Assert.AreEqual((int)CompareFunction.Always, image.material.GetInt("unity_GUIZTestMode"));
            Assert.AreEqual((int)RenderQueue.Overlay, image.material.renderQueue);
        }

        [Test]
        public void Apply_Twice_KeepsSameMaterial()
        {
            root = new GameObject("Canvas", typeof(Canvas));
            var image = new GameObject("Background", typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(root.transform);
            var onTop = root.AddComponent<UIAlwaysOnTop>();

            onTop.Apply();
            var first = image.material;
            onTop.Apply();

            Assert.AreSame(first, image.material);
        }
    }
}
