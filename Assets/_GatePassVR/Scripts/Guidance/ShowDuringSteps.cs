// 정해진 Step(예: END)에 있는 동안만 오브젝트를 보여 주거나, 반대로 그 Step 동안만 숨기는 컴포넌트 (이씨). END 크레딧, START 화면 정리에 쓴다.
using System;
using GatePassVR.Core;
using UnityEngine;

namespace GatePassVR.Guidance
{
    public class ShowDuringSteps : MonoBehaviour
    {
        [SerializeField] private ScenarioManager scenario;
        [Tooltip("이 Step ID(guide-texts.md)에 있는 동안만 보여 준다")]
        [SerializeField] private string[] stepIds = { "END" };
        [SerializeField] private GameObject target;
        [Tooltip("켜면 반대로 목록의 Step 동안만 숨기고 나머지에서는 보여 준다")]
        [SerializeField] private bool hideDuringSteps;

        private void Start()
        {
            Refresh();
        }

        // ScenarioManager.onStepEntered에 연결한다.
        public void Refresh()
        {
            if (target == null)
            {
                return;
            }

            var step = scenario != null ? scenario.CurrentStep : null;
            target.SetActive(ShouldShow(step?.stepId, stepIds, hideDuringSteps));
        }

        // 현재 Step ID가 목록에 있는지로 보일지 정한다. Step이 없으면(시작 전·완료 후) 목록에 없는 것으로 본다.
        public static bool ShouldShow(string currentStepId, string[] ids, bool hideDuringListed)
        {
            bool listed = currentStepId != null && ids != null && Array.IndexOf(ids, currentStepId) >= 0;
            return listed != hideDuringListed;
        }
    }
}
