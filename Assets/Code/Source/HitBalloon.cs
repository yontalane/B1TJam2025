using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("B1TJam2025/Hit Balloon")]
    public class HitBalloon : MonoBehaviour
    {
        private const float DURATION = 0.5f;


        private Vector3 m_worldPosition;


        [Header("Settings")]

        [SerializeField]
        private string[] m_possibleLines;

        [Header("References")]

        [SerializeField]
        private TMP_Text m_text;


        private void Reset()
        {
            m_possibleLines = new string[]
            {
                "THWACK!",
                "CRACK!",
                "BREAK!",
                "BEAT!",
            };

            m_text = GetComponentInChildren<TMP_Text>();
        }


        public void Initialize(Canvas canvas, HittableEvent hittableEvent)
        {
            if (m_possibleLines.Length == 0)
            {
                Destroy(gameObject);
                return;
            }

            m_text.text = m_possibleLines[Mathf.FloorToInt(m_possibleLines.Length * Random.value)];

            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.SetParent(canvas.transform);

            rectTransform.localPosition = Vector3.zero;
            rectTransform.localEulerAngles = new()
            {
                x = 0f,
                y = 0f,
                z = Random.Range(-15f, 15f),
            };
            rectTransform.localScale = Vector3.one;

            m_worldPosition = Vector3.Lerp(GameManager.Player.transform.position, hittableEvent.hittable.transform.position, 0.5f) + (1.667f * Vector3.up);
            Vector2 normalizedScreenPosition = Camera.main.WorldToViewportPoint(m_worldPosition, Camera.MonoOrStereoscopicEye.Mono);

            Vector2 sizeDelta = rectTransform.sizeDelta;
            rectTransform.anchorMin = normalizedScreenPosition;
            rectTransform.anchorMax = normalizedScreenPosition;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.sizeDelta = sizeDelta;

            StartCoroutine(Flash());
        }


        private IEnumerator Flash()
        {
            Image image = GetComponent<Image>();
            bool flashOn = true;
            float startTime = Time.time;

            while (Time.time - startTime < DURATION)
            {
                yield return new WaitForSeconds(0.025f);
                flashOn = !flashOn;
                image.color = flashOn ? Color.white : new Color(0.125f, 0.125f, 0.125f, 1f);
            }

            Destroy(gameObject);
        }


        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_worldPosition, 0.333f);
            Gizmos.DrawWireSphere(m_worldPosition, 0.5f);
        }
    }
}
