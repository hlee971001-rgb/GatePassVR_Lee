// 정해진 Step(예: END)에 있는 동안만 오브젝트를 보여 주는 컴포넌트 (이씨). END 화면 크레딧 글씨에 쓴다.
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
            target.SetActive(step != null && Array.IndexOf(stepIds, step.stepId) >= 0);
        }
    }
}
