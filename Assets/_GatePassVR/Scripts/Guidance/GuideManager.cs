// 현재 단계의 안내 텍스트/음성 표시와 무반응 시 재안내를 담당하는 단일 진입점
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GatePassVR.Guidance
{
    public class GuideManager : MonoBehaviour
    {
        public static GuideManager Instance { get; private set; }

        [SerializeField] private TMP_Text mainText;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float shortThreshold = 5f;
        [SerializeField] private float longThreshold = 10f;
        [SerializeField] private string initialMainText;
        [SerializeField] private string initialHintText;

        [SerializeField] private UnityEvent onNoProgressShort;
        [SerializeField] private UnityEvent onNoProgressLong;

        private GuideReguideTimer timer;
        private AudioClip currentVoice;
        private bool hasActiveGuide;

        private void Awake()
        {
            Instance = this;
            timer = new GuideReguideTimer(shortThreshold, longThreshold);

            if (!string.IsNullOrEmpty(initialMainText))
            {
                SetGuide(initialMainText, initialHintText);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (!hasActiveGuide)
            {
                return;
            }

            var (shortJustFired, longJustFired) = timer.Tick(Time.deltaTime);

            if (shortJustFired)
            {
                onNoProgressShort?.Invoke();
            }

            if (longJustFired)
            {
                onNoProgressLong?.Invoke();
                ReplayVoice();
            }
        }

        // 새 안내를 표시하고 재안내 타이머를 초기화한다. 다른 스크립트는 UI Text를 직접 건드리지 말고 이 메서드만 호출한다.
        public void SetGuide(string main, string hint = null, AudioClip voice = null)
        {
            if (mainText != null)
            {
                mainText.text = main;
            }

            if (hintText != null)
            {
                hintText.text = hint ?? string.Empty;
            }

            currentVoice = voice;
            hasActiveGuide = true;
            timer.Reset();

            if (voice != null && audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = voice;
                audioSource.Play();
            }
        }

        // 진행 신호를 받으면 재안내 타이머만 초기화한다 (텍스트/음성은 바꾸지 않음).
        public void ReportProgress()
        {
            timer.Reset();
        }

        // UnityEvent Persistent Listener는 정적 파라미터 1개짜리 메서드만 연결할 수 있어서 만든 편의 메서드.
        // 힌트 텍스트/음성은 그대로 둔다.
        public void SetMainText(string main)
        {
            if (mainText != null)
            {
                mainText.text = main;
            }

            hasActiveGuide = true;
            timer.Reset();
        }

        // 보조 힌트만 바꾼다(틀린 행동 힌트 등). 메인 안내·음성·재안내 타이머는 그대로 둔다.
        // (이씨 2026-11-23 추가, 김씨 부재로 이씨 수정)
        public void SetHintText(string hint)
        {
            if (hintText != null)
            {
                hintText.text = hint ?? string.Empty;
            }
        }

        public void ClearGuide()
        {
            hasActiveGuide = false;
            currentVoice = null;

            if (mainText != null)
            {
                mainText.text = string.Empty;
            }

            if (hintText != null)
            {
                hintText.text = string.Empty;
            }

            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void ReplayVoice()
        {
            if (currentVoice != null && audioSource != null)
            {
                audioSource.Stop();
                audioSource.clip = currentVoice;
                audioSource.Play();
            }
        }
    }
}
