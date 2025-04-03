using B1TJam2025.Utility;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("B1TJam2025/Vehicle")]
    public class Vehicle : MonoBehaviour
    {
        private const float SLOW_ENOUGH_TO_STOP = 0.01f;


        private BoxCollider m_collider;
        private bool m_beingDriven;
        private int m_gear;
        private float m_currentVelocity;
        private bool m_sliding;


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


        private void Reset()
        {
            m_topSpeed = 10f;
            m_acceleration = 1f;
        }


        private void Start()
        {
            m_collider = GetComponent<BoxCollider>();
            m_collider.isTrigger = false;
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


        public void StartRiding(CharacterController rider)
        {
            m_collider.enabled = false;

            rider.transform.eulerAngles = transform.eulerAngles;
            Vector3 riderPosition = transform.position;
            riderPosition.y = rider.transform.position.y;
            rider.Move(riderPosition - rider.transform.position);

            transform.SetParent(rider.transform, true);
            m_sliding = false;
            ResetSpeed();
            m_beingDriven = true;
        }

        public void StopRiding()
        {
            transform.SetParent(null, true);
            m_sliding = true;
            m_collider.enabled = true;
            m_collider.isTrigger = true;
            m_beingDriven = false;
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

            // HACK: The following two checks should be handled with layers.
            if (collider.TryGetComponent(out Player _))
            {
                return;
            }

            if (collider.TryGetComponent(out TriggerOverlapChecker _))
            {
                return;
            }

            CurrentSpeed *= -0.2f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(OutsideSpot, 0.1f);
        }
    }
}
