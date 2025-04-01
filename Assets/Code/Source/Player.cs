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
        private const float ROTATION_SPEED = 0.3f;
        private const float BORED_INTERVAL = 6f;

        private Vector3 m_input;
        private bool m_isBusy;


        [Header("Settings")]

        [SerializeField]
        [Min(0f)]
        private float m_speed;

        [Header("References")]

        [SerializeField]
        private Animator m_animator;

        [SerializeField]
        private AnimEventBroadcaster m_animBroadcaster;


        private void Reset()
        {
            m_speed = 5f;

            m_animator = GetComponentInChildren<Animator>();
            m_animBroadcaster = GetComponentInChildren<AnimEventBroadcaster>();
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

            m_animator.SetBool("Run", running);

            if (running)
            {
                transform.Translate(m_speed * Time.deltaTime * m_input, Space.World);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(m_input.normalized, Vector3.up), ROTATION_SPEED);
            }
        }


        private IEnumerator BoredTimer()
        {
            while (true)
            {
                yield return new WaitForSeconds(BORED_INTERVAL);

                if (m_isBusy)
                {
                    continue;
                }

                if (GetIsRunning())
                {
                    continue;
                }

                m_animator.SetTrigger("Bored");
            }
        }


        private bool GetIsRunning()
        {
            return !Mathf.Approximately(m_input.magnitude, 0f);
        }

        private void OnAnimEvent(AnimationEvent animationEvent)
        {
            switch (animationEvent.stringParameter)
            {
                case "Busy":
                    m_isBusy = animationEvent.intParameter > 0;
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

            m_animator.SetTrigger("Beat");
        }
    }
}
