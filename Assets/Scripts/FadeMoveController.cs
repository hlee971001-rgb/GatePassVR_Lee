// Fade Out -> XR Origin 이동 -> Fade In 순서로 씬 간 이동 시 화면 전환을 담당하는 컨트롤러
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace GatePassVR.VR
{
    public class FadeMoveController : MonoBehaviour
    {
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private Transform originToMove;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float fadeInDuration = 0.3f;

        [SerializeField] private UnityEvent onMoveCompleted;

        public bool IsMoving { get; private set; }

        // destination의 위치와 Y축 회전(시선 방향)으로 XR Origin을 이동시킨다.
        // 갑작스러운 카메라 회전을 막기 위해 Pitch/Roll은 반영하지 않는다.
        public void MoveTo(Transform destination)
        {
            if (IsMoving || destination == null || originToMove == null)
            {
                return;
            }

            StartCoroutine(MoveRoutine(destination));
        }

        private IEnumerator MoveRoutine(Transform destination)
        {
            IsMoving = true;

            yield return Fade(0f, 1f, fadeOutDuration);

            var yawOnly = Quaternion.Euler(0f, destination.eulerAngles.y, 0f);
            originToMove.SetPositionAndRotation(destination.position, yawOnly);

            yield return Fade(1f, 0f, fadeInDuration);

            IsMoving = false;
            onMoveCompleted?.Invoke();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (fadeCanvasGroup == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                fadeCanvasGroup.alpha = to;
                yield break;
            }

            float elapsed = 0f;
            fadeCanvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;
        }
    }
}