// World Space Canvas의 글자·이미지를 다른 물체보다 항상 앞에 그려서 카운터 등에 가려지지 않게 한다 (이씨 11/25)
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace GatePassVR.UI
{
    public class UIAlwaysOnTop : MonoBehaviour
    {
        private const string ZTestProperty = "unity_GUIZTestMode";

        private bool applied;

        private void Awake()
        {
            Apply();
        }

        // 자식의 모든 Graphic(꺼진 것 포함)에 깊이 검사 없이 그리는 머티리얼 사본을 한 번만 입힌다.
        public void Apply()
        {
            if (applied)
            {
                return;
            }

            applied = true;
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                Material material;
                if (graphic is TMP_Text text)
                {
                    material = text.fontMaterial; // TMP가 글꼴 머티리얼 사본을 만들어 이 글자에만 쓴다
                }
                else
                {
                    material = new Material(graphic.materialForRendering);
                    graphic.material = material;
                }

                MakeOnTop(material);
            }
        }

        public static void MakeOnTop(Material material)
        {
            material.SetInt(ZTestProperty, (int)CompareFunction.Always);
            material.renderQueue = (int)RenderQueue.Overlay;
        }
    }
}
