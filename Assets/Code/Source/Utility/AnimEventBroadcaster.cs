using UnityEngine;

namespace B1TJam2025.Utility
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("B1TJam2025/Utility/Animation Event Broadcaster")]
    public class AnimEventBroadcaster : MonoBehaviour
    {
        public delegate void AnimEventBroadcasterHandler(AnimationEvent animationEvent);
        public AnimEventBroadcasterHandler OnAnimEvent = null;

        public void AnimationEvent(AnimationEvent animationEvent)
        {
            OnAnimEvent?.Invoke(animationEvent);
        }
    }
}
