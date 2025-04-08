using UnityEngine;

namespace B1TJam2025
{
    [DisallowMultipleComponent]
    [AddComponentMenu("B1TJam2025/Hit Balloon Manager")]
    public sealed class HitBalloonManager : MonoBehaviour
    {
        [Header("Scene References")]

        [SerializeField]
        private Canvas m_canvas;

        [Header("Prefabs")]

        [SerializeField]
        private HitBalloon m_prefab;


        private void Reset()
        {
            m_canvas = FindAnyObjectByType<Canvas>();

            m_prefab = null;
        }


        private void OnEnable()
        {
            HittableHelper.OnHit += OnHittableHit;
        }

        private void OnDisable()
        {
            HittableHelper.OnHit -= OnHittableHit;
        }


        private void OnHittableHit(HittableEvent hittableEvent)
        {
            HitBalloon instance = Instantiate(m_prefab);
            instance.Initialize(m_canvas, hittableEvent);
        }
    }
}
