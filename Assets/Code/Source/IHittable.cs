using UnityEngine;

namespace B1TJam2025
{
    public interface IHittable
    {
        public Transform transform { get; }
        public bool IsDead { get; }
        public void GetHit();
        public void GetKilled();
    }

    public struct HittableEvent
    {
        public IHittable hittable;
        public bool isKilled;
    }

    public static class HittableHelper
    {
        public delegate void HittableEventHandler(HittableEvent hittableEvent);
        public static HittableEventHandler OnHit = null;
    }
}
