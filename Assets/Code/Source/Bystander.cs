using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    [AddComponentMenu("B1TJam2025/Bystander")]
    public class Bystander : MonoBehaviour, IHittable
    {
        public delegate void BystanderEventHandler(Bystander bystander);
        public static BystanderEventHandler OnBystanderKilled = null;


        private const float MIN_DISTANCE = 5f;
        private const float CLOSE_ENOUGH_TO_DESTINATION = 0.1f;
        private const float TIME_TO_REMAIN_AFTER_KO = 5f;

        private readonly List<GameObject> m_list = new();
        private NavMeshAgent m_navMeshAgent;
        private Vector3 m_pointA;
        private Vector3 m_pointB;
        private bool m_comingFromA;
        private bool m_isDead = false;


        [SerializeField]
        private ParticleSystem m_koEffect;


        public Vector3 Position
        {
            get
            {
                return transform.position;
            }

            set
            {
                GetComponent<NavMeshAgent>().Warp(value);
            }
        }

        private Vector3 StartingPoint => m_comingFromA ? m_pointA : m_pointB;

        private Vector3 Destination => m_comingFromA ? m_pointB : m_pointA;

        public bool IsDead => m_isDead;


        private void Reset()
        {
            m_koEffect = null;
        }


        public void Initialize(IReadOnlyList<GameObject> targets)
        {
            m_list.AddRange(targets);

            m_navMeshAgent = GetComponent<NavMeshAgent>();
            NavMeshPath path = new();
            m_pointA = transform.position;
            m_pointB = m_pointA;
            bool foundTarget = false;

            while (!foundTarget && m_list.Count > 0)
            {
                int index = Mathf.FloorToInt(m_list.Count * Random.value);
                m_pointB = m_list[index].transform.position;
                m_list.RemoveAt(index);

                if (Vector3.Distance(m_pointA, m_pointB) <= MIN_DISTANCE)
                {
                    continue;
                }

                if (!m_navMeshAgent.isOnNavMesh)
                {
                    continue;
                }

                if (!m_navMeshAgent.CalculatePath(m_pointB, path))
                {
                    continue;
                }

                if (path.status != NavMeshPathStatus.PathComplete)
                {
                    continue;
                }

                foundTarget = true;
            }

            m_comingFromA = true;
            if (m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.SetDestination(m_pointB);
            }
        }


        public void ToggleMovement(bool movementOn)
        {
            if (m_navMeshAgent.hasPath && m_navMeshAgent.isActiveAndEnabled && m_navMeshAgent.isOnNavMesh)
            {
                m_navMeshAgent.isStopped = !movementOn;
            }

            GetComponentInChildren<Animator>().enabled = movementOn;
        }


        private void LateUpdate()
        {
            if (m_isDead)
            {
                return;
            }

            if (Vector3.Distance(transform.position, Destination) > CLOSE_ENOUGH_TO_DESTINATION)
            {
                return;
            }
            
            m_navMeshAgent.SetDestination(StartingPoint);
            m_comingFromA = !m_comingFromA;
        }


        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (m_navMeshAgent == null || m_navMeshAgent.path == null || m_navMeshAgent.path.corners.Length <= 1)
            {
                return;
            }

            float r = 0.15f;

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(m_navMeshAgent.path.corners[0], r);

            for (int i = 1; i < m_navMeshAgent.path.corners.Length; i++)
            {
                Gizmos.DrawLine(m_navMeshAgent.path.corners[i - 1], m_navMeshAgent.path.corners[i]);
                Gizmos.DrawSphere(m_navMeshAgent.path.corners[i], r);
            }
        }

        public void GetHit() => GetKilled();

        public void GetKilled()
        {
            HittableHelper.OnHit?.Invoke(new()
            {
                hittable = this,
                isKilled = true,
            });

            ParticleSystem effect = Instantiate(m_koEffect);
            effect.transform.position = transform.position;
            effect.transform.eulerAngles = transform.eulerAngles;
            effect.transform.localScale = Vector3.one;

            m_isDead = true;
            GetComponentInChildren<Animator>().SetTrigger("Die");
            GetComponent<Collider>().enabled = false;
            m_navMeshAgent.isStopped = true;
            m_navMeshAgent.enabled = false;

            OnBystanderKilled?.Invoke(this);

            StartCoroutine(Die());
        }

        private IEnumerator Die()
        {
            yield return new WaitForSeconds(TIME_TO_REMAIN_AFTER_KO);
            Destroy(gameObject);
        }
    }
}
