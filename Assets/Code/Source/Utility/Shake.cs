using System.Collections;
using UnityEngine;

namespace B1TJam2025.Utility
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Utility/Shake")]
    public class Shake : MonoBehaviour
    {
        [SerializeField]
        [Min(0f)]
        private float m_amount;

        [SerializeField]
        [Min(0f)]
        private float m_duration;

        [SerializeField]
        private AnimationCurve m_falloff;


        private void Reset()
        {
            m_amount = 0.1f;
            m_duration = 0.5f;
            m_falloff = new(new(0f, 1f), new(1f, 0f));
        }


        public void Activate()
        {
            StartCoroutine(DoShake());
        }

        private IEnumerator DoShake()
        {
            Vector3 position = transform.position;
            float startTime = Time.time;
            float t = 0f;

            while (t < 1f)
            {
                t = (Time.time - startTime) / m_duration;

                transform.position = position + m_falloff.Evaluate(t) * new Vector3()
                {
                    x = Random.Range(-m_amount, m_amount),
                    y = Random.Range(-m_amount, m_amount),
                    z = Random.Range(-m_amount, m_amount),
                };

                yield return new WaitForEndOfFrame();
            }

            transform.position = position;
        }
    }
}
