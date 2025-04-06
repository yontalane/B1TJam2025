using System.Collections.Generic;
using System.Collections;
using B1TJam2025.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(CharacterController))]
    [AddComponentMenu("B1TJam2025/Player")]
    public class Player : MonoBehaviour
    {
        private const float SPEED_REDUCTION_WHEN_WALKING = 0.5f;

        private CharacterController m_characterController;
        private Vector3 m_input;
        private bool m_canRun = true;
        private bool m_canBeat = true;
        private Vector3 m_currentVelocity;
        private bool m_ridingVehicle;
        private Vehicle m_vehicle;
        private bool m_cheatSprint;
        private readonly List<Perp> m_perps = new();


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
        private TriggerOverlapChecker m_club;

        [SerializeField]
        private Camera m_dialogCamera;

        [SerializeField]
        private TriggerOverlapChecker m_interactionTrigger;

        [SerializeField]
        private TriggerOverlapChecker m_nearbyPerpTrigger;

        [Header("Prefabs")]

        [SerializeField]
        private ParticleSystem m_hitEffect;


        public bool DialogCameraIsActive
        {
            get
            {
                return m_dialogCamera.gameObject.activeSelf;
            }

            set
            {
                if (value)
                {
                    DialougeManager.Canvas.worldCamera = m_dialogCamera;
                }

                m_dialogCamera.gameObject.SetActive(value);
                GameManager.IsPaused = value;
            }
        }


        private void Reset()
        {
            m_speed = 5f;
            m_rotationSpeed = 0.3f;
            m_boredIntervalMin = 6f;
            m_boredIntervalMax = 8f;

            m_animator = GetComponentInChildren<Animator>();
            m_animBroadcaster = GetComponentInChildren<AnimEventBroadcaster>();
            m_club = GetComponentInChildren<TriggerOverlapChecker>();
            m_dialogCamera = GetComponentInChildren<Camera>();
            m_interactionTrigger = GetComponentInChildren<TriggerOverlapChecker>();
            m_nearbyPerpTrigger = GetComponentInChildren<TriggerOverlapChecker>();

            m_hitEffect = null;
        }


        private void OnEnable()
        {
            m_animBroadcaster.OnAnimEvent += OnAnimEvent;
            m_interactionTrigger.OnOverlapEnter += OnPossibleInteractionEnter;
            m_interactionTrigger.OnOverlapExit += OnPossibleInteractionExit;
        }

        private void OnDisable()
        {
            m_animBroadcaster.OnAnimEvent -= OnAnimEvent;
            m_interactionTrigger.OnOverlapEnter -= OnPossibleInteractionEnter;
            m_interactionTrigger.OnOverlapExit -= OnPossibleInteractionExit;
        }


        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();

            if (Camera.main.TryGetComponent(out SmoothFollow smoothFollow))
            {
                smoothFollow.transform.position = new()
                {
                    x = transform.position.x,
                    y = smoothFollow.transform.position.y,
                    z = transform.position.z - 2.5f,
                };
                smoothFollow.Initialize(transform);
            }

            transform.eulerAngles = new()
            {
                x = 0f,
                y = 180f,
                z = 0f,
            };

            transform.position = new()
            {
                x = transform.position.x,
                y = 0f,
                z = transform.position.z,
            };

            StartCoroutine(BoredTimer());
        }


        private void Update()
        {
            if (GameManager.IsPaused)
            {
                m_input = Vector3.zero;
            }

            if (!m_ridingVehicle)
            {
                UpdateMovementOnFoot();
            }
            else
            {
                UpdateMovementInVehicle();
            }

            transform.position = new()
            {
                x = transform.position.x,
                y = 0f,
                z = transform.position.z,
            };
        }

        private void UpdateMovementOnFoot()
        {
            bool movementInputExists = m_canRun && GetMovementInputExists();

            float speed = m_speed;

            m_animator.SetBool("Run", movementInputExists);

            if (movementInputExists)
            {
                if (m_cheatSprint)
                {
                    speed *= 5;
                }
                else if (m_animator.GetInteger("Speed") == 0)
                {
                    speed *= SPEED_REDUCTION_WHEN_WALKING;
                }

                Vector3 targetAngles = Quaternion.LookRotation(m_input.normalized, Vector3.up).eulerAngles;
                transform.eulerAngles = new()
                {
                    x = Mathf.SmoothDampAngle(transform.eulerAngles.x, targetAngles.x, ref m_currentVelocity.x, m_rotationSpeed),
                    y = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngles.y, ref m_currentVelocity.y, m_rotationSpeed),
                    z = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngles.z, ref m_currentVelocity.z, m_rotationSpeed),
                };

                m_characterController.Move(speed * Time.deltaTime * m_input);
            }
        }

        private void UpdateMovementInVehicle()
        {
            m_vehicle.Gear = Mathf.Approximately(m_input.z, 0f) ? 0 : m_input.z > 0f ? 1 : -1;

            transform.eulerAngles += new Vector3()
            {
                y = Time.deltaTime * m_vehicle.RotationSpeed * 100f * m_input.x,
            };

            m_characterController.Move(Time.deltaTime * m_vehicle.CurrentSpeed * transform.forward);
        }


        private IEnumerator BoredTimer()
        {
            while (true)
            {
                float interval = Random.Range(m_boredIntervalMin, m_boredIntervalMax);

                yield return new WaitForSeconds(interval);

                if (GameManager.IsPaused)
                {
                    continue;
                }

                if (!m_canRun || !m_canBeat)
                {
                    continue;
                }

                if (GetMovementInputExists())
                {
                    continue;
                }

                SetRandomVariation();
                m_animator.SetTrigger("Bored");
            }
        }


        public void MoveTo(Transform target)
        {
            m_characterController.Move(target.position - transform.position);
            transform.eulerAngles = target.eulerAngles;
        }

        private bool GetMovementInputExists()
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
                case "CanRun":
                    m_canRun = animationEvent.intParameter > 0;
                    break;

                case "CanBeat":
                    m_canBeat = animationEvent.intParameter > 0;
                    break;

                case "Club":
                    m_club.IsEnabled = animationEvent.intParameter > 0;
                    m_club.gameObject.SetActive(m_club.IsEnabled);
                    break;

                case "Step":
                    SFXManager.Play("Step", transform.position, animationEvent.floatParameter);
                    break;

                case "Hit" when m_club.IsEnabled:
                    if (m_club.TryGetOverlapByType(out Perp perp) && perp.State != PerpState.KO)
                    {
                        SFXManager.Play("HitPerp", transform.position);

                        perp.GetHit();

                        ParticleSystem effect = Instantiate(m_hitEffect);
                        effect.transform.position = m_club.Collider.bounds.center;
                        effect.transform.localEulerAngles = Vector3.zero;
                        effect.transform.localScale = Vector3.one;

                        Camera.main.GetComponent<Shake>().Activate();
                    }
                    else
                    {
                        SFXManager.Play("Whiff", transform.position);
                    }
                    break;
            }
        }

        private void OnPossibleInteractionEnter(Collider collider)
        {
            if (!collider.TryGetComponent(out HintDisplay hintDisplay))
            {
                return;
            }

            hintDisplay.IsVisible = true;
        }

        private void OnPossibleInteractionExit(Collider collider)
        {
            if (!collider.TryGetComponent(out HintDisplay hintDisplay))
            {
                return;
            }

            hintDisplay.IsVisible = false;
        }

        private void RotateTowardClosest()
        {
            int count = m_nearbyPerpTrigger.GetOverlapsByType(m_perps);

            if (count == 0)
            {
                return;
            }

            float distance = float.MaxValue;
            int index = -1;

            for (int i = 0; i < count; i++)
            {
                float newDistance = Vector3.Distance(transform.position, m_perps[i].transform.position);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    index = i;
                }
            }

            if (index == -1)
            {
                return;
            }

            transform.LookAt(m_perps[index].transform, Vector3.up);
        }

        private bool TryInteract()
        {
            if (m_interactionTrigger.TryGetOverlapByType(out Vehicle vehicle))
            {
                m_interactionTrigger.IsEnabled = false;

                m_animator.SetBool("Ride", true);

                vehicle.StartRiding(m_characterController);

                m_ridingVehicle = true;
                m_vehicle = vehicle;

                return true;
            }
            else if (m_interactionTrigger.TryGetOverlapByType(out SubwayStop subwayStop))
            {
                SubwayManager.InteractWithSubwayStop(subwayStop);

                return true;
            }

            return false;
        }

        private bool TryExitVehicle()
        {
            if (!m_ridingVehicle)
            {
                return false;
            }

            m_animator.SetBool("Ride", false);

            m_vehicle.StopRiding();

            transform.position = m_vehicle.OutsideSpot;

            m_interactionTrigger.IsEnabled = true;
            m_interactionTrigger.Clear();

            m_ridingVehicle = false;
            m_vehicle = null;

            return true;
        }


        public void OnMove(InputValue inputValue)
        {
            if (GameManager.IsPaused)
            {
                return;
            }

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
            if (GameManager.IsPaused)
            {
                return;
            }

            if (!m_canBeat)
            {
                return;
            }

            if (!inputValue.isPressed)
            {
                return;
            }

            if (TryInteract())
            {
                return;
            }

            if (TryExitVehicle())
            {
                return;
            }

            RotateTowardClosest();

            SetRandomVariation();
            m_animator.SetTrigger("Beat");
        }

        public void OnSprint(InputValue inputValue)
        {
            if (GameManager.IsPaused)
            {
                return;
            }

            m_animator.SetInteger("Speed", inputValue.isPressed ? 1 : 0);
        }

        public void OnCheatSprint(InputValue inputValue)
        {
            m_cheatSprint = inputValue.isPressed;
        }
    }
}
