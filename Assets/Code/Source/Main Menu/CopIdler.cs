using System.Collections;
using UnityEngine;

namespace B1TJam2025.MainMenu
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("B1TJam2025/Main Menu/Cop Idler")]
    /// <summary>
    /// Play's Beat Cop's random idle animations.
    /// </summary>
    public class CopIdler : MonoBehaviour
    {
        private const int MAXIMUM_VARIATIONS = 2;


        private Animator m_animator;


        [SerializeField]
        [Min(0f)]
        [Tooltip("Minimum interval between idle actions.")]
        private float m_boredIntervalMin;

        [SerializeField]
        [Min(0f)]
        [Tooltip("Maximum interval between idle actions.")]
        private float m_boredIntervalMax;


        private void Start()
        {
            m_animator = GetComponent<Animator>();

            StartCoroutine(BoredTimer());
        }


        private IEnumerator BoredTimer()
        {
            while (true)
            {
                float interval = Random.Range(m_boredIntervalMin, m_boredIntervalMax);

                yield return new WaitForSeconds(interval);

                m_animator.SetInteger("Variation", Random.Range(0, MAXIMUM_VARIATIONS));
                m_animator.SetTrigger("Bored");
            }
        }
    }
}
