// Step 진행 효과음(성공·완료)과 틀린 행동 힌트(문구 + 재시도음), 막혔을 때 Reset 안내를 현재 Step 기준으로 보여 주는 컴포넌트 (이씨)
using System;
using GatePassVR.Core;
using UnityEngine;

namespace GatePassVR.Guidance
{
    [RequireComponent(typeof(AudioSource))]
    public class StepFeedback : MonoBehaviour
    {
        [Serializable]
        public class WrongHint
        {
            [Tooltip("guide-texts.md의 Step ID")]
            public string stepId;
            [Tooltip("guide-texts.md '틀린 행동 힌트' 칸")]
            public string hint;
        }

        [SerializeField] private ScenarioManager scenario;
        [SerializeField] private GuideManager guideManager;
        [SerializeField] private AudioClip successClip;
        [SerializeField] private AudioClip retryClip;
        [SerializeField] private AudioClip completeClip;
        [Tooltip("이 Step에 들어가면 성공음 대신 완료음을 낸다")]
        [SerializeField] private string completeStepId = "END";
        [SerializeField] private WrongHint[] wrongHints = new WrongHint[0];
        [Tooltip("약 10초 무반응일 때 보조 힌트에 보여 줄 Reset 안내(guide-texts.md C-RESET)")]
        [SerializeField] private string resetHint;
        [Tooltip("틀린 행동이 연달아 와도 재시도음은 이 간격(초)보다 자주 내지 않는다")]
        [SerializeField] private float retryCooldown = 1f;

        private AudioSource audioSource;
        private int lastStepIndex = -1;
        private float lastRetryTime = -100f;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        // ScenarioManager.onStepEntered에 연결한다. 앞으로 나아갔을 때만 소리를 낸다(Reset 재진입·처음부터 다시는 조용히).
        public void OnStepEntered()
        {
            if (scenario == null)
            {
                return;
            }

            int index = scenario.CurrentStepIndex;
            bool advanced = index > lastStepIndex && lastStepIndex >= 0;
            lastStepIndex = index;

            if (!advanced)
            {
                return;
            }

            var step = scenario.CurrentStep;
            var clip = step != null && step.stepId == completeStepId ? completeClip : successClip;
            Play(clip);
        }

        // PlacementZone.onWrongObject, HandOverZone.onWrongObject, ScannerZone.onWrongItem에 연결한다.
        // 지금 Step에 맞는 힌트가 있을 때만 보조 힌트를 바꾸고 재시도음을 낸다.
        public void ShowWrongHint()
        {
            var step = scenario != null ? scenario.CurrentStep : null;
            if (step == null)
            {
                return;
            }

            foreach (var entry in wrongHints)
            {
                if (entry.stepId != step.stepId)
                {
                    continue;
                }

                SetHint(entry.hint);
                if (Time.time - lastRetryTime >= retryCooldown)
                {
                    lastRetryTime = Time.time;
                    Play(retryClip);
                }

                return;
            }
        }

        // GuideManager.onNoProgressLong(약 10초 무반응)에 연결한다.
        public void ShowResetHint()
        {
            if (!string.IsNullOrEmpty(resetHint) && scenario != null && scenario.CurrentStep != null)
            {
                SetHint(resetHint);
            }
        }

        private void SetHint(string hint)
        {
            var guide = guideManager != null ? guideManager : GuideManager.Instance;
            if (guide != null)
            {
                guide.SetHintText(hint);
            }
        }

        private void Play(AudioClip clip)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}
