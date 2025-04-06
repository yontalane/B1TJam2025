using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

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

    [System.Serializable]
    public struct PerpSettings
    {
        public PerpType type;

        public PerpBehavior behavior;

        [Range(1, 5)]
        public int hp;

        [Min(0f)]
        public float alertRadius;

        [Min(0f)]
        public float alertTimeBeforeAction;
    }

    public enum PerpType
    {
        Unassuming = 0,
        Protester = 1,
        SittingOnGround = 2,
        Graffiti = 3,
        WalkWIP = 4,
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    [AddComponentMenu("B1TJam2025/Perp")]
    public class Perp : MonoBehaviour
    {
        public delegate void PerpEventHandler(Perp perp);
        public static PerpEventHandler OnPerpSpawn = null;
        public static PerpEventHandler OnPerpKO = null;
        public static PerpEventHandler OnPerpEscape = null;


        private const float PERCENT_OF_RADIUS_BEFORE_ACTION = 0.5f;


        private bool m_firstPass = true;
        private PerpSettings m_settings;
        private NavMeshAgent m_navMeshAgent;
        private bool m_playerWasInAlertZone;
        private PerpState m_state;
        private float m_alertStartTime;


        [Header("References")]

        [SerializeField]
        private Renderer m_body;

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


        public bool IsRandom { get; set; }

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
                        StartRunning();
                        break;

                    case PerpState.KO:
                        m_navMeshAgent.enabled = false;

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
            m_body = GetComponentInChildren<Renderer>();
            m_animator = GetComponentInChildren<Animator>();
            m_canvas = GetComponentInChildren<Canvas>();
            m_alert = GetComponentInChildren<TMP_Text>();
            m_dialogLocation = null;

            m_koEffect = null;
        }


        private void Start()
        {
            m_navMeshAgent = GetComponent<NavMeshAgent>();

            m_alert.gameObject.SetActive(false);

            OnPerpSpawn?.Invoke(this);
        }

        public void Initialize(PerpSettings settings)
        {
            m_settings = settings;
        }

        public void EnableNavMeshAgent()
        {
            GetComponent<NavMeshAgent>().enabled = true;
        }


        private void LateUpdate()
        {
            if (m_firstPass)
            {
                m_animator.SetInteger("PerpType", (int)m_settings.type);
                m_firstPass = false;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, GameManager.Player.transform.position);
            bool playerIsInAlertZone = distanceToPlayer < m_settings.alertRadius;
            bool playerJustEnteredAlertZone = !m_playerWasInAlertZone && playerIsInAlertZone;
            bool playerIsTooClose = distanceToPlayer < m_settings.alertRadius * PERCENT_OF_RADIUS_BEFORE_ACTION;
            bool perpHasBeenAlertForTooLong = State == PerpState.Alert && Time.time - m_alertStartTime > m_settings.alertTimeBeforeAction;

            if (State == PerpState.Idle || State == PerpState.Alert)
            {
                if (playerIsTooClose || (perpHasBeenAlertForTooLong && playerIsInAlertZone))
                {
                    State = m_settings.behavior == PerpBehavior.Dialog ? PerpState.Dialog : m_settings.behavior == PerpBehavior.Run ? PerpState.Run : PerpState.Cower;
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

            if (State == PerpState.Run && Mathf.Approximately(Vector3.Distance(transform.position, m_navMeshAgent.destination), 0f))
            {
                OnPerpEscape?.Invoke(this);
                Destroy(gameObject);
            }
        }


        public void GetHit()
        {
            m_settings.hp--;

            if (m_settings.hp > 0)
            {
                State = PerpState.Cower;
                m_navMeshAgent.isStopped = true;
                m_animator.SetInteger("Variance", Random.Range(0, 2));
                m_animator.SetTrigger("Hit");
            }
            else
            {
                transform.position += 0.1f * Vector3.up;
                State = PerpState.KO;
                m_animator.SetTrigger("KO");
                OnPerpKO?.Invoke(this);
                StartCoroutine(RunDeathSequence());
            }
        }

        private IEnumerator RunDeathSequence()
        {
            yield return new WaitForSeconds(3f);

            for (int i = 0; i < 20; i++)
            {
                m_body.enabled = !m_body.enabled;
                yield return new WaitForSeconds(0.0333f);
            }

            Destroy(gameObject);
        }


        private void StartRunning()
        {
            if (!GameManager.TryGetRandomFleeTarget(transform, 20f, 40f, out Transform target))
            {
                return;
            }

            m_animator.SetBool("Run", true);
            m_navMeshAgent.SetDestination(target.position);
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_settings.alertRadius);

            if (Application.isPlaying)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(transform.position, 3f);
            }

            if (Application.isPlaying && m_navMeshAgent.hasPath)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(m_navMeshAgent.destination, 3f);
            }
        }
    }
}
