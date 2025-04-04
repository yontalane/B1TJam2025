using TMPro;
using UnityEngine;

namespace B1TJam2025
{
    public enum PerpBehavior
    {
        None = 0,
        Dialog = 10,
        Run = 20,
    }

    public enum PerpState
    {
        Idle = 0,
        Alert = 10,
        Dialog = 20,
        Run = 30,
        Cower = 40,
        KO = 50,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("B1TJam2025/Perp")]
    public class Perp : MonoBehaviour
    {
        private const float PERCENT_OF_RADIUS_BEFORE_ACTION = 0.5f;


        private CharacterController m_characterController;
        private bool m_playerWasInAlertZone;
        private PerpState m_state;
        private float m_alertStartTime;


        [Header("Settings")]

        [SerializeField]
        private PerpBehavior m_behavior;

        [SerializeField]
        [Range(1, 5)]
        private int m_hp;

        [SerializeField]
        [Min(0f)]
        private float m_alertRadius;

        [SerializeField]
        [Min(0f)]
        private float m_alertTimeBeforeAction;

        [Header("References")]

        [SerializeField]
        private Animator m_animator;

        [SerializeField]
        private Canvas m_canvas;

        [SerializeField]
        private TMP_Text m_alert;

        [SerializeField]
        private Transform m_dialogLocation;

        [SerializeField]
        private Camera m_dialogCamera;

        [Header("Prefabs")]

        [SerializeField]
        private ParticleSystem m_koEffect;


        public PerpState State
        {
            get
            {
                return m_state;
            }

            private set
            {
                if (m_state == value)
                {
                    return;
                }

                m_state = value;

                bool alertVisible = false;

                switch(m_state)
                {
                    case PerpState.Idle:
                        m_animator.SetInteger("StandState", 0);
                        break;

                    case PerpState.Alert:
                        m_animator.SetInteger("StandState", 0);
                        alertVisible = true;
                        break;

                    case PerpState.Dialog:
                        m_animator.SetInteger("StandState", 1);
                        GameManager.Player.MoveTo(m_dialogLocation);
                        m_dialogCamera.gameObject.SetActive(true);
                        GameManager.IsPaused = true;

                        RendererManager.SetColorsByName("Dialog");
                        break;

                    case PerpState.Cower:
                        m_animator.SetInteger("StandState", 2);
                        break;

                    case PerpState.Run:
                        m_animator.SetBool("Run", true);
                        break;

                    case PerpState.KO:
                        m_characterController.enabled = false;

                        ParticleSystem effect = Instantiate(m_koEffect);
                        effect.transform.position = transform.position;
                        effect.transform.eulerAngles = transform.eulerAngles;
                        effect.transform.localScale = Vector3.one;
                        break;
                }

                m_alert.gameObject.SetActive(alertVisible);
            }
        }


        private void Reset()
        {
            m_behavior = PerpBehavior.Dialog;
            m_hp = 3;
            m_alertRadius = 2.25f;
            m_alertTimeBeforeAction = 4f;

            m_animator = GetComponentInChildren<Animator>();
            m_canvas = GetComponentInChildren<Canvas>();
            m_alert = GetComponentInChildren<TMP_Text>();
            m_dialogLocation = null;

            m_koEffect = null;
        }


        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();

            m_alert.gameObject.SetActive(false);
        }


        private void LateUpdate()
        {
            float distanceToPlayer = Vector3.Distance(transform.position, GameManager.Player.transform.position);
            bool playerIsInAlertZone = distanceToPlayer < m_alertRadius;
            bool playerJustEnteredAlertZone = !m_playerWasInAlertZone && playerIsInAlertZone;
            bool playerIsTooClose = distanceToPlayer < m_alertRadius * PERCENT_OF_RADIUS_BEFORE_ACTION;
            bool perpHasBeenAlertForTooLong = State == PerpState.Alert && Time.time - m_alertStartTime > m_alertTimeBeforeAction;

            if (State == PerpState.Idle || State == PerpState.Alert)
            {
                if (playerIsTooClose || (perpHasBeenAlertForTooLong && playerIsInAlertZone))
                {
                    State = m_behavior == PerpBehavior.Dialog ? PerpState.Dialog : m_behavior == PerpBehavior.Run ? PerpState.Run : PerpState.Cower;
                }
                else if (perpHasBeenAlertForTooLong && !playerIsInAlertZone)
                {
                    State = PerpState.Idle;
                }
                else if (State == PerpState.Idle && playerJustEnteredAlertZone)
                {
                    State = PerpState.Alert;
                    m_alertStartTime = Time.time;
                }
            }

            if (State == PerpState.Dialog || State == PerpState.Cower)
            {
                transform.LookAt(GameManager.Player.transform, Vector3.up);
            }

            if (m_alert.gameObject.activeSelf)
            {
                m_canvas.transform.eulerAngles = Camera.main.transform.eulerAngles;
                m_alert.color = Random.value < 0.5f ? Color.black : Color.white;
            }

            m_playerWasInAlertZone = playerIsInAlertZone;
        }


        public void GetHit()
        {
            m_hp--;

            if (m_hp > 0)
            {
                State = PerpState.Cower;
                m_animator.SetInteger("Variance", Random.Range(0, 2));
                m_animator.SetTrigger("Hit");
            }
            else
            {
                transform.position += 0.1f * Vector3.up;
                State = PerpState.KO;
                m_animator.SetTrigger("KO");
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_alertRadius);
        }
    }
}
