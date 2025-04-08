using TMPro;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("B1TJam2025/Scoring System/Contextual Score Display")]
    public class ContextualScoreDisplay : MonoBehaviour
    {
        private bool m_readyToGo = false;
        private float m_startTime;


        [SerializeField]
        private TMP_Text m_text;


        private void Reset()
        {
            m_text = GetComponentInChildren<TMP_Text>();
        }


        public void Initialize(Canvas canvas, int scoreDelta, ScoringSystem.QualityOfScoreChange quality)
        {
            m_text.text = scoreDelta > 0 ? $"+{scoreDelta}" : scoreDelta.ToString();
            Animator animator = GetComponent<Animator>();
            animator.SetInteger("Quality", quality == ScoringSystem.QualityOfScoreChange.Positive ? 1 : quality == ScoringSystem.QualityOfScoreChange.Negative ? -1 : 0);

            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform);

            rectTransform.localPosition = Vector3.zero;
            rectTransform.localEulerAngles = Vector3.zero;
            rectTransform.localScale = Vector3.one;

            Vector3 worldPosition = GameManager.Player.transform.position + (1.667f * Vector3.up);
            Vector2 normalizedScreenPosition = Camera.main.WorldToViewportPoint(worldPosition, Camera.MonoOrStereoscopicEye.Mono);

            Vector2 sizeDelta = rectTransform.sizeDelta;
            rectTransform.anchorMin = normalizedScreenPosition;
            rectTransform.anchorMax = normalizedScreenPosition;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;

            DialougeManager.OnDialougeComplete += OnDialogComplete;

            m_startTime = Time.unscaledTime;
        }

        private void OnDialogComplete()
        {
            m_readyToGo = true;
        }

        private void LateUpdate()
        {
            if (Time.unscaledTime - m_startTime > 0.5f && !GameManager.IsPaused)
            {
                m_readyToGo = true;
            }

            if (!m_readyToGo)
            {
                return;
            }

            if (GameManager.IsPaused)
            {
                return;
            }

            GetComponent<Animator>().SetTrigger("Activate");
        }

        public void DestroyScoreDisplay(AnimationEvent _)
        {
            DialougeManager.OnDialougeComplete -= OnDialogComplete;
            Destroy(gameObject);
        }
    }
}
