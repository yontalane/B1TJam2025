using UnityEngine;

namespace B1TJam2025.Utility
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Utility/Smooth Follow")]
    public class SmoothFollow : MonoBehaviour
    {
        private bool m_initialized = false;
        private Vector3 m_offset;
        private Vector3 m_currentVelocity;


        [Header("Settings")]

        [SerializeField]
        private float m_smoothTime;

        [Header("References")]

        [SerializeField]
        private Transform m_target;


        private void Reset()
        {
            m_smoothTime = 0.3f;

            m_target = null;
        }


        private void Start()
        {
            Initialize(m_target);
        }

        public void Initialize(Transform target)
        {
            if (target == null)
            {
                return;
            }

            m_target = target;
            m_offset = transform.position - m_target.position;

            m_initialized = true;
        }

        private void Update()
        {
            if (!m_initialized)
            {
                return;
            }

            Vector3 desiredPosition = m_target.position + m_offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref m_currentVelocity, m_smoothTime);
        }
    }
}
