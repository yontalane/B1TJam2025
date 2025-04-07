using System.Collections.Generic;
using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Guide Arrow Manager")]
    public sealed class GuideArrowManager : MonoBehaviour
    {
        private const float SMOOTH_TIME = 0.3f;


        private readonly List<Perp> m_perps = new();
        private Player m_player;
        private float m_currentVelocity;
        private float m_targetAngle;


        [SerializeField]
        private Transform m_guideArrow;


        private float Angle
        {
            get
            {
                return m_guideArrow.localEulerAngles.z;
            }

            set
            {
                m_guideArrow.localEulerAngles = new()
                {
                    x = m_guideArrow.localEulerAngles.x,
                    y = m_guideArrow.localEulerAngles.y,
                    z = value,
                };
            }
        }


        private void Reset()
        {
            m_guideArrow = null;
        }


        private void OnEnable()
        {
            GameManager.OnGameStart += OnGameStart;
            Perp.OnPerpSpawn += OnPerpSpawn;
        }

        private void OnDisable()
        {
            GameManager.OnGameStart -= OnGameStart;
            Perp.OnPerpSpawn -= OnPerpSpawn;
        }


        private void OnGameStart()
        {
            m_player = GameManager.Player;
            m_perps.AddRange(FindObjectsByType<Perp>(FindObjectsInactive.Exclude, FindObjectsSortMode.None));
            m_targetAngle = Angle;
        }

        private void OnPerpSpawn(Perp perp)
        {
            m_perps.Add(perp);
        }


        private bool TryGetClosestPerp(out Perp perp)
        {
            float distance = float.MaxValue;
            perp = null;

            for (int i = m_perps.Count - 1; i >= 0; i--)
            {
                if (m_perps[i] == null || m_perps[i].IsDead)
                {
                    m_perps.RemoveAt(i);
                    continue;
                }

                float d = Vector3.Distance(m_perps[i].transform.position, m_player.transform.position);

                if (d < distance)
                {
                    distance = d;
                    perp = m_perps[i];
                }
            }

            return perp != null;
        }


        private void Update()
        {
            Angle = Mathf.SmoothDampAngle(Angle, m_targetAngle, ref m_currentVelocity, SMOOTH_TIME);
        }

        private void LateUpdate()
        {
            if (!TryGetClosestPerp(out Perp perp))
            {
                return;
            }

            Vector3 p = perp.transform.position - m_player.transform.position;

            m_targetAngle = Mathf.Atan2(p.z, p.x) * Mathf.Rad2Deg + 270f;
        }
    }
}
