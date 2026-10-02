// Play 모드에서 Dest_ 지점을 순서대로 FadeMoveController로 이동해 보는 동선 점검용 Editor 창
using GatePassVR.VR;
using UnityEditor;
using UnityEngine;

namespace GatePassVR.EditorTools
{
    public class DestTourWindow : EditorWindow
    {
        // plan.md의 7개 구역 순서. Boarding 다음은 비행(Fade)으로 Arrival로 넘어간다.
        static readonly string[] TourOrder =
        {
            "Dest_Start", "Dest_CheckIn", "Dest_Security", "Dest_Boarding",
            "Dest_Arrival", "Dest_Immigration", "Dest_Baggage",
        };

        int currentIndex = -1;

        // Point & Hold(김씨 담당)가 붙기 전 임시 점검용이다. 이동은 기존 FadeMoveController.MoveTo를 그대로 쓴다.
        [MenuItem("GatePass/Layout/Dest Tour Window")]
        static void Open() => GetWindow<DestTourWindow>("Dest Tour");

        void OnGUI()
        {
            if (!EditorApplication.isPlaying)
            {
                currentIndex = -1;
                EditorGUILayout.HelpBox("Play 모드에서 사용합니다. Airport_Lee Scene을 열고 Play를 눌러 주세요.", MessageType.Info);
                return;
            }

            var mover = Object.FindFirstObjectByType<FadeMoveController>();
            if (mover == null)
            {
                EditorGUILayout.HelpBox("Scene에서 FadeMoveController를 찾을 수 없습니다.", MessageType.Error);
                return;
            }

            string current = currentIndex < 0 ? "아직 이동 안 함" : $"{currentIndex + 1}/{TourOrder.Length}  {TourOrder[currentIndex]}";
            EditorGUILayout.LabelField("현재 지점", current);
            if (mover.IsMoving)
            {
                EditorGUILayout.LabelField("이동 중...");
            }

            using (new EditorGUI.DisabledScope(mover.IsMoving))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(currentIndex <= 0))
                    {
                        if (GUILayout.Button("◀ 이전", GUILayout.Height(30)))
                        {
                            Go(mover, currentIndex - 1);
                        }
                    }
                    using (new EditorGUI.DisabledScope(currentIndex >= TourOrder.Length - 1))
                    {
                        if (GUILayout.Button("다음 ▶", GUILayout.Height(30)))
                        {
                            Go(mover, currentIndex + 1);
                        }
                    }
                }

                EditorGUILayout.Space();
                for (int i = 0; i < TourOrder.Length; i++)
                {
                    if (GUILayout.Button($"{i + 1}. {TourOrder[i]}"))
                    {
                        Go(mover, i);
                    }
                }
            }
        }

        void Go(FadeMoveController mover, int index)
        {
            var dest = GameObject.Find(TourOrder[index]);
            if (dest == null)
            {
                Debug.LogError($"[DestTourWindow] '{TourOrder[index]}' 지점을 찾을 수 없습니다.");
                return;
            }

            currentIndex = index;
            mover.MoveTo(dest.transform);
        }

        // 이동 중 표시와 버튼 활성 상태를 갱신한다.
        void OnInspectorUpdate() => Repaint();
    }
}
