using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Flee Alert Manager")]
    public class FleeAlertManager : MonoBehaviour
    {
        private const float EARLY_FLASH_INTERVAL = 1f;
        private const float LATE_FLASH_INTERVAL = 0.02f;
        private const float FLASH_DURATION = 0.02f;

        private bool m_currentlyTracking;
        private Perp m_trackedPerp;
        private float m_lastFlash;
        private bool m_inFlash;


        [SerializeField]
        private Image m_frame;

        [SerializeField]
        private TMP_Text m_percent;


        private void OnEnable()
        {
            Perp.OnPerpStartEscaping += OnPerpStartEscaping;
            Perp.OnPerpEscapePrevented += OnPerpEscapePrevented;
        }

        private void OnDisable()
        {
            Perp.OnPerpStartEscaping -= OnPerpStartEscaping;
            Perp.OnPerpEscapePrevented -= OnPerpEscapePrevented;
        }


        private void Start()
        {
            m_frame.gameObject.SetActive(false);
        }


        private void OnPerpStartEscaping(Perp perp)
        {
            if (m_currentlyTracking)
            {
                return;
            }

            Debug.Log($"Showing flee alert on new perp.");
            m_currentlyTracking = true;
            m_trackedPerp = perp;
            UpdatePercentLabel(0f);
            m_frame.color = Color.black;
            m_frame.gameObject.SetActive(true);

            m_inFlash = false;
            m_lastFlash = Time.time;
        }

        private void OnPerpEscapePrevented(Perp perp)
        {
            if (!m_currentlyTracking)
            {
                return;
            }

            Debug.Log($"Hiding flee alert due to escape prevented.");
            m_currentlyTracking = false;
            m_trackedPerp = null;
            m_frame.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (!m_currentlyTracking)
            {
                return;
            }

            if (m_trackedPerp == null || m_trackedPerp.IsDead)
            {
                Debug.Log($"Hiding flee alert due to perp not found.");
                m_currentlyTracking = false;
                m_frame.gameObject.SetActive(false);
                return;
            }

            float closenessToEscape = m_trackedPerp.ClosenessToEscape;
            UpdatePercentLabel(closenessToEscape);

            if (m_inFlash && Time.time - m_lastFlash >= FLASH_DURATION)
            {
                m_frame.color = Color.black;
                m_inFlash = false;
                m_lastFlash = Time.time;
            }
            else if (!m_inFlash && Time.time - m_lastFlash >= Mathf.Lerp(EARLY_FLASH_INTERVAL, LATE_FLASH_INTERVAL, closenessToEscape))
            {
                m_frame.color = Color.white;
                m_inFlash = true;
                m_lastFlash = Time.time;
            }
        }

        private void UpdatePercentLabel(float closenessToEscape)
        {
            if (!m_currentlyTracking || m_trackedPerp == null)
            {
                return;
            }

            m_percent.text = $"{Mathf.RoundToInt(closenessToEscape * 100f)}%";
        }
    }
}
