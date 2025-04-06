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


        public Collider Collider { get; private set; }

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
            Collider = GetComponent<Collider>();

            Collider.isTrigger = true;
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
            for (int i = m_colliders.Count - 1; i >= 0; i--)
            {
                if (m_colliders[i] == null)
                {
                    m_colliders.RemoveAt(i);
                    continue;
                }

                if (m_colliders[i].TryGetComponent(out overlap))
                {
                    return true;
                }
            }

            overlap = null;
            return false;
        }

        public int GetOverlapsByType<T>(List<T> overlaps) where T : Component
        {
            overlaps ??= new();
            overlaps.Clear();
            int count = 0;

            for (int i = m_colliders.Count - 1; i >= 0; i--)
            {
                if (m_colliders[i] == null)
                {
                    m_colliders.RemoveAt(i);
                    continue;
                }

                if (m_colliders[i].TryGetComponent(out T overlap))
                {
                    overlaps.Add(overlap);
                    count++;
                }
            }

            return count;
        }
    }
}
