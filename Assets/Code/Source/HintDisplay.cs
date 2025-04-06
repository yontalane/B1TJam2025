using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Hint Display")]
    public class HintDisplay : MonoBehaviour
    {
        private bool m_isVisible;
        private bool m_isEnabled = true;


        [SerializeField]
        private GameObject m_target;


        public bool IsVisible
        {
            get
            {
                return m_isVisible;
            }

            set
            {
                m_isVisible = value;
                Refresh();
            }
        }

        public bool IsEnabled
        {
            get
            {
                return m_isEnabled;
            }

            set
            {
                m_isEnabled = value;
                Refresh();
            }
        }


        private void Reset()
        {
            m_target = null;
        }


        private void LateUpdate()
        {
            if (!m_isVisible || !m_isEnabled)
            {
                return;
            }

            m_target.transform.eulerAngles = Camera.main.transform.eulerAngles;
        }


        private void Refresh()
        {
            m_target.SetActive(m_isEnabled && m_isVisible);
        }
    }
}
