using System.Collections;
using B1TJam2025.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    [AddComponentMenu("B1TJam2025/Player")]
    public class Player : MonoBehaviour
    {
        private const float WALK_DURATION = 0.75f;
        private const float SPEED_REDUCTION_WHEN_WALKING = 0.5f;

        private Vector3 m_input;
        private bool m_isBusy;
        private float m_startedWalkingTime;
        private Vector3 m_currentVelocity;


        [Header("Settings")]

        [SerializeField]
        [Min(0f)]
        private float m_speed;

        [SerializeField]
        [Min(0f)]
        private float m_rotationSpeed;

        [SerializeField]
        [Min(0f)]
        private float m_boredIntervalMin;

        [SerializeField]
        [Min(0f)]
        private float m_boredIntervalMax;

        [Header("References")]

        [SerializeField]
        private Animator m_animator;

        [SerializeField]
        private AnimEventBroadcaster m_animBroadcaster;

        [SerializeField]
        private GameObject m_club;


        private void Reset()
        {
            m_speed = 5f;
            m_rotationSpeed = 0.3f;
            m_boredIntervalMin = 6f;
            m_boredIntervalMax = 8f;

            m_animator = GetComponentInChildren<Animator>();
            m_animBroadcaster = GetComponentInChildren<AnimEventBroadcaster>();
            m_club = null;
        }


        private void OnEnable()
        {
            m_animBroadcaster.OnAnimEvent += OnAnimEvent;
        }

        private void OnDisable()
        {
            m_animBroadcaster.OnAnimEvent -= OnAnimEvent;
        }


        private void Start()
        {
            StartCoroutine(BoredTimer());
        }


        private void LateUpdate()
        {
            bool running = !m_isBusy && GetIsRunning();

            float speed = m_speed;

            if (running && !m_animator.GetBool("Run"))
            {
                m_startedWalkingTime = Time.time;
                m_animator.SetInteger("Speed", 0);
                speed *= SPEED_REDUCTION_WHEN_WALKING;
            }
            else if (Time.time - m_startedWalkingTime <= WALK_DURATION)
            {
                speed *= SPEED_REDUCTION_WHEN_WALKING;
            }
            else
            {
                m_animator.SetInteger("Speed", 1);
            }

            m_animator.SetBool("Run", running);

            if (running)
            {
                transform.Translate(speed * Time.deltaTime * m_input, Space.World);

                Vector3 targetAngles = Quaternion.LookRotation(m_input.normalized, Vector3.up).eulerAngles;
                transform.eulerAngles = new()
                {
                    x = Mathf.SmoothDampAngle(transform.eulerAngles.x, targetAngles.x, ref m_currentVelocity.x, m_rotationSpeed),
                    y = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngles.y, ref m_currentVelocity.y, m_rotationSpeed),
                    z = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngles.z, ref m_currentVelocity.z, m_rotationSpeed),
                };
            }
        }


        private IEnumerator BoredTimer()
        {
            while (true)
            {
                float interval = Random.Range(m_boredIntervalMin, m_boredIntervalMax);

                yield return new WaitForSeconds(interval);

                if (m_isBusy)
                {
                    continue;
                }

                if (GetIsRunning())
                {
                    continue;
                }

                SetRandomVariation();
                m_animator.SetTrigger("Bored");
            }
        }


        private bool GetIsRunning()
        {
            return !Mathf.Approximately(m_input.magnitude, 0f);
        }

        private void SetRandomVariation()
        {
            m_animator.SetInteger("Variation", Random.Range(0, 2));
        }

        private void OnAnimEvent(AnimationEvent animationEvent)
        {
            switch (animationEvent.stringParameter)
            {
                case "Busy":
                    m_isBusy = animationEvent.intParameter > 0;
                    break;

                case "Club":
                    m_club.SetActive(animationEvent.intParameter > 0);
                    break;
            }
        }


        public void OnMove(InputValue inputValue)
        {
            Vector2 input = inputValue.Get<Vector2>();

            m_input = new()
            {
                x = input.x,
                y = 0f,
                z = input.y,
            };
        }

        public void OnAttack(InputValue inputValue)
        {
            if (m_isBusy)
            {
                return;
            }

            SetRandomVariation();
            m_animator.SetTrigger("Beat");
        }
    }
}
