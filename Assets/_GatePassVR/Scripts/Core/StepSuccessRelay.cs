// 물건 집기·시간 경과처럼 전용 상호작용 컴포넌트가 없는 성공을 ScenarioManager에 알리고,
// Step에 들어왔을 때 이미 조건을 만족한 경우(미리 집었거나 미리 놓은 경우)를 따라잡아 진행이 막히지 않게 하는 연결 컴포넌트 (이씨)
using System;
using System.Collections;
using GatePassVR.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace GatePassVR.Core
{
    public class StepSuccessRelay : MonoBehaviour
    {
        [Serializable]
        public class GrabEntry
        {
            public XRGrabInteractable item;
            [Tooltip("이 물건을 집으면 보낼 성공 ID")]
            public string successId;
        }

        [Serializable]
        public class TimerEntry
        {
            [Tooltip("이 성공 ID를 기다리는 Step에 들어가면 시간을 잰다")]
            public string successId;
            [Tooltip("기다릴 시간(초). 안내 음성을 끝까지 듣게 하려면 음성 길이보다 길게 둔다.")]
            public float delay = 3f;
        }

        [SerializeField] private ScenarioManager scenario;
        [SerializeField] private GrabEntry[] grabs = new GrabEntry[0];
        [SerializeField] private TimerEntry[] timers = new TimerEntry[0];
        [Tooltip("Step에 들어왔을 때 이미 놓여 있으면 성공으로 따라잡을 Placement Zone")]
        [SerializeField] private PlacementZone[] zones = new PlacementZone[0];

        private void OnEnable()
        {
            foreach (var grab in grabs)
            {
                if (grab.item != null)
                {
                    grab.item.selectEntered.AddListener(OnItemSelected);
                }
            }
        }

        private void OnDisable()
        {
            foreach (var grab in grabs)
            {
                if (grab.item != null)
                {
                    grab.item.selectEntered.RemoveListener(OnItemSelected);
                }
            }
        }

        // ScenarioManager.onStepEnteredId에 연결한다.
        // Step 진입 도중에 바로 다음 Step으로 넘기면 진입 처리가 겹치므로 한 프레임 뒤에 확인한다.
        public void OnStepEntered(string successId)
        {
            if (string.IsNullOrEmpty(successId) || !isActiveAndEnabled)
            {
                return;
            }

            StartCoroutine(CatchUpNextFrame(successId));

            foreach (var timer in timers)
            {
                if (timer.successId == successId)
                {
                    StartCoroutine(ReportAfter(timer));
                }
            }
        }

        private void OnItemSelected(SelectEnterEventArgs args)
        {
            var item = args.interactableObject as XRGrabInteractable;
            foreach (var grab in grabs)
            {
                if (grab.item == item)
                {
                    scenario.ReportSuccess(grab.successId);
                }
            }
        }

        private IEnumerator CatchUpNextFrame(string successId)
        {
            yield return null;

            if (!IsWaitingFor(successId))
            {
                yield break;
            }

            // 이미 손에 들고 있으면 집기 성공으로 본다(놓았다면 다시 집으면 된다).
            foreach (var grab in grabs)
            {
                if (grab.successId == successId && grab.item != null && grab.item.isSelected)
                {
                    scenario.ReportSuccess(successId);
                    yield break;
                }
            }

            // 앞 Step 중에 미리 놓아 성공 신호가 무시된 경우.
            foreach (var zone in zones)
            {
                if (zone != null && zone.ZoneId == successId && zone.IsPlaced)
                {
                    scenario.ReportSuccess(successId);
                    yield break;
                }
            }
        }

        private IEnumerator ReportAfter(TimerEntry timer)
        {
            yield return new WaitForSeconds(timer.delay);

            if (IsWaitingFor(timer.successId))
            {
                scenario.ReportSuccess(timer.successId);
            }
        }

        private bool IsWaitingFor(string successId)
        {
            var step = scenario != null ? scenario.CurrentStep : null;
            return step != null && step.successId == successId;
        }
    }
}
