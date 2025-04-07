using B1TJam2025.Utility;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HintDisplay))]
    [AddComponentMenu("B1TJam2025/Vehicle")]
    public class Vehicle : MonoBehaviour
    {
        public delegate void VehicleEventHandler(Vehicle vehicle);
        public static VehicleEventHandler OnVehicleEntered = null;
        public static VehicleEventHandler OnVehicleExited = null;


        private const float SLOW_ENOUGH_TO_STOP = 0.01f;


        private BoxCollider m_collider;
        private HintDisplay m_hintDisplay;
        private bool m_beingDriven;
        private int m_gear;
        private float m_currentVelocity;
        private bool m_sliding;
        private float m_y;


        [SerializeField]
        [Min(0f)]
        private float m_topSpeed;

        [SerializeField]
        [Min(0.001f)]
        private float m_acceleration;

        [SerializeField]
        [Min(0.001f)]
        private float m_rotationSpeed;

        [SerializeField]
        private Vector2 m_outsideSpot;

        [SerializeField]
        private int m_animationIndex;

        [Space]

        [SerializeField]
        private TriggerOverlapChecker m_crashDetector;


        public float CurrentSpeed { get; private set; }

        public Vector3 OutsideSpot
        {
            get
            {
                Vector3 v = new()
                {
                    x = m_outsideSpot.x,
                    y = 0f,
                    z = m_outsideSpot.y,
                };
                v = transform.TransformPoint(v);
                return v;
            }
        }


        public float RotationSpeed => m_rotationSpeed * Mathf.Abs(CurrentSpeed / m_topSpeed);

        public int Gear
        {
            get => m_gear;
            set => m_gear = value;
        }

        public int AnimationIndex => m_animationIndex;


        private void Reset()
        {
            m_topSpeed = 10f;
            m_acceleration = 1f;
            m_rotationSpeed = 1f;
            m_outsideSpot = default;
            m_animationIndex = 0;

            m_crashDetector = GetComponentInChildren<TriggerOverlapChecker>();
        }


        private void OnEnable()
        {
            if (m_crashDetector != null)
            {
                m_crashDetector.OnOverlapEnter += OnOverlapEnter;
            }
        }

        private void OnDisable()
        {
            if (m_crashDetector != null)
            {
                m_crashDetector.OnOverlapEnter -= OnOverlapEnter;
            }
        }


        private void Start()
        {
            m_collider = GetComponent<BoxCollider>();
            m_hintDisplay = GetComponent<HintDisplay>();
            m_collider.isTrigger = false;
            m_y = transform.position.y;
        }

        private void Update()
        {
            if (m_beingDriven)
            {
                float targetSpeed = m_gear * m_topSpeed;
                CurrentSpeed = Mathf.SmoothDamp(CurrentSpeed, targetSpeed, ref m_currentVelocity, m_acceleration);
            }
            else if (m_sliding)
            {
                CurrentSpeed = Mathf.SmoothDamp(CurrentSpeed, 0f, ref m_currentVelocity, m_acceleration);

                if (Mathf.Abs(CurrentSpeed) < SLOW_ENOUGH_TO_STOP)
                {
                    m_sliding = false;
                    m_collider.isTrigger = false;
                    return;
                }

                transform.Translate(Time.deltaTime * CurrentSpeed * transform.forward, Space.World);
            }
        }

        private void LateUpdate()
        {
            transform.position = new()
            {
                x = transform.position.x,
                y = m_y,
                z = transform.position.z,
            };
        }


        private void OnOverlapEnter(Collider collider)
        {
            if (!m_beingDriven)
            {
                return;
            }

            if (collider.TryGetComponent(out IHittable hittable))
            {
                hittable.GetKilled();
            }
            else
            {
                SFXManager.Play("HitWall", transform.position);
            }
        }


        public void StartRiding(CharacterController rider)
        {
            m_collider.enabled = false;
            m_hintDisplay.IsEnabled = false;

            rider.transform.eulerAngles = transform.eulerAngles;
            Vector3 riderPosition = transform.position;
            riderPosition.y = rider.transform.position.y;
            rider.Move(riderPosition - rider.transform.position);

            transform.SetParent(rider.transform, true);
            m_sliding = false;
            ResetSpeed();
            m_beingDriven = true;

            OnVehicleEntered?.Invoke(this);
        }

        public void StopRiding()
        {
            m_hintDisplay.IsEnabled = true;
            m_hintDisplay.IsVisible = false;

            transform.SetParent(null, true);
            m_sliding = true;
            m_collider.enabled = true;
            m_collider.isTrigger = true;
            m_beingDriven = false;

            OnVehicleExited?.Invoke(this);
        }

        private void ResetSpeed()
        {
            m_gear = 0;
            CurrentSpeed = 0f;
            m_currentVelocity = 0f;
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (!m_sliding)
            {
                return;
            }

            // HACK: The following checks should be handled with layers. Or just something that's not this.
            if (collider.TryGetComponent(out Player _))
            {
                return;
            }

            if (collider.TryGetComponent(out TriggerOverlapChecker _))
            {
                return;
            }

            if (LayerMask.LayerToName(collider.gameObject.layer) == "Surface")
            {
                return;
            }

            if (LayerMask.LayerToName(collider.gameObject.layer) == "SurfaceBlocked")
            {
                return;
            }

            if (collider.TryGetComponent(out CityMap _))
            {
                return;
            }

            SFXManager.Play("HitWall", transform.position);

            CurrentSpeed *= -0.2f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(OutsideSpot, 0.1f);
        }
    }
}
