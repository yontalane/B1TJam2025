using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025.Utility
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("B1TJam2025/Utility/Trigger Overlap Checker")]
    public class TriggerOverlapChecker : MonoBehaviour
    {
        private readonly List<Collider> m_colliders = new();


        public bool IsEnabled
        {
            get
            {
                return enabled;
            }

            set
            {
                if (value == enabled)
                {
                    return;
                }

                Clear();
                enabled = value;
            }
        }


        private void Start()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        public void Clear() => m_colliders.Clear();

        private void OnTriggerEnter(Collider other)
        {
            if (m_colliders.Contains(other))
            {
                return;
            }

            m_colliders.Add(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!m_colliders.Contains(other))
            {
                return;
            }

            m_colliders.Remove(other);
        }

        public bool TryGetOverlapByType<T>(out T overlap) where T : Component
        {
            foreach (Collider collider in m_colliders)
            {
                if (collider.TryGetComponent(out overlap))
                {
                    return true;
                }
            }

            overlap = null;
            return false;
        }
    }
}
